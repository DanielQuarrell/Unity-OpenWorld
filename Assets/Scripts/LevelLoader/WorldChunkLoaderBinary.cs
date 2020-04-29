using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class WorldChunkLoaderBinary : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] WorldNode[] chunks;
    [SerializeField] GameObject enemiesHolder;

    [SerializeField] float distanceToLoadChunk = 100;

    private List<EnemyController> loadedEnemies;

    private void Awake()
    {
        loadedEnemies = new List<EnemyController>();
    }

    private void Start()
    {
        foreach (WorldNode chunk in chunks)
        {
            chunk.SetChunkLoaded(false);
        }
    }

    void Update()
    {
        foreach (WorldNode chunk in chunks)
        {
            Vector3 vectorDistance = player.transform.position - chunk.transform.position;
            vectorDistance.y = 0;
            float distanceToPlayer = vectorDistance.magnitude;

            if (distanceToPlayer < distanceToLoadChunk)
            {
                if (!chunk.IsChunkActive())
                {
                    chunk.SetChunkActive(true);
                    LoadChunk(chunk);
                }
            }
            else
            {
                if (chunk.IsChunkActive())
                {
                    chunk.SetChunkActive(false);
                    UnloadChunk(chunk);
                }
            }
        }
    }

    void LoadChunk(WorldNode chunk)
    {
        if (File.Exists(Application.persistentDataPath + "/worldData.dat") && File.Exists(Application.persistentDataPath + "/enemiesData.dat"))
        {
            //Load files from binary
            BinaryFormatter bf = new BinaryFormatter();

            FileStream worldFile = File.Open(Application.persistentDataPath + "/worldData.dat", FileMode.Open);
            WorldData worldData = (WorldData)bf.Deserialize(worldFile);
            worldFile.Close();

            FileStream enemiesFile = File.Open(Application.persistentDataPath + "/enemiesData.dat", FileMode.Open);
            WorldEnemyData worldEnemyData = (WorldEnemyData)bf.Deserialize(enemiesFile);
            enemiesFile.Close();

            foreach (ChunkData chunkData in worldData.chunks)
            {
                if (chunkData.coordinate.vector2 == chunk.GetCoordinate())
                {
                    chunk.loadCorourtine = LoadChunkAsync(chunk, chunkData, worldEnemyData);
                    StartCoroutine(chunk.loadCorourtine);
                }
            }
        }
    }

    void UnloadChunk(WorldNode chunk)
    {
        //If chunk hasn't finished loaded
        if (!chunk.IsChunkLoaded() && chunk.loadCorourtine != null)
        {
            //Stop chunk loading
            StopCoroutine(chunk.loadCorourtine);
        }

        if (File.Exists(Application.persistentDataPath + "/enemiesData.dat"))
        {
            //Load enemy file
            BinaryFormatter bf = new BinaryFormatter();

            FileStream readFile = File.Open(Application.persistentDataPath + "/enemiesData.dat", FileMode.Open);
            WorldEnemyData worldEnemyData = (WorldEnemyData)bf.Deserialize(readFile);
            readFile.Close();

            if (loadedEnemies != null)
            {
                List<EnemyController> enemiesToRemove = new List<EnemyController>();

                foreach (EnemyController enemy in loadedEnemies)
                {
                    if (chunk.ObjectInChunk(enemy.transform))
                    {
                        //Rewrite the enemy data with the loaded enemy
                        OverideEnemy(ref worldEnemyData, enemy, chunk.GetCoordinate());

                        enemiesToRemove.Add(enemy);
                    }
                }

                foreach (EnemyController enemy in enemiesToRemove)
                {
                    loadedEnemies.Remove(enemy);
                    Destroy(enemy.gameObject);
                }
            }

            //Overwrite enemy file
            FileStream overwriteFile = File.Open(Application.persistentDataPath + "/enemiesData.dat", FileMode.Open);
            bf.Serialize(overwriteFile, worldEnemyData);
            overwriteFile.Close();
        }
        else
        {
            Debug.LogError("Enemies File doesn't exist");
        }

        //Remove loaded objects
        chunk.Unload();
    }

    bool IsEnemyLoaded(int id)
    {
        foreach (EnemyController loadedEnemy in loadedEnemies)
        {
            if (loadedEnemy.id == id)
            {
                return true;
            }
        }

        return false;
    }

    void OverideEnemy(ref WorldEnemyData worldEnemyData, EnemyController enemyInChunk, Vector2 coordinate)
    {
        //Ensure it rewrites the same enemy
        for (int i = 0; i < worldEnemyData.enemies.Count; i++)
        {
            if (worldEnemyData.enemies[i].id == enemyInChunk.id)
            {
                worldEnemyData.enemies[i].coordinate = new SerializableVector2(coordinate);

                worldEnemyData.enemies[i].position = new SerializableVector3(enemyInChunk.transform.position);
                worldEnemyData.enemies[i].rotation = new SerializableQuaternion(enemyInChunk.transform.rotation);
                worldEnemyData.enemies[i].scale = new SerializableVector3(enemyInChunk.transform.localScale);

                worldEnemyData.enemies[i].dead = enemyInChunk.IsDead();
                worldEnemyData.enemies[i].health = enemyInChunk.GetHealth();
            }
        }
    }

    IEnumerator LoadChunkAsync(WorldNode chunk, ChunkData chunkData, WorldEnemyData worldEnemyData)
    {
        yield return StartCoroutine(LoadTerrain(chunk, chunkData.terrainObject));

        foreach (WorldObjectData worldObjectData in chunkData.worldObjects)
        {
            yield return StartCoroutine(LoadWorldObject(chunk, worldObjectData, null));
        }

        yield return StartCoroutine(LoadEnemies(chunk.GetCoordinate(), worldEnemyData));

        chunk.SetChunkLoaded(true);
    }

    IEnumerator LoadTerrain(WorldNode chunk, TerrainObjectData terrainObjectData)
    {
        //Create new object to apply the terrain to
        GameObject terrainObject;
        terrainObject = new GameObject(terrainObjectData.terrainName);
        terrainObject.AddComponent<Terrain>();
        terrainObject.AddComponent<TerrainCollider>();

        //Create terrain data from height map
        TerrainData terrainData = new TerrainData();

        float[,] heightData = new float[terrainObjectData.heightmapWidth, terrainObjectData.heightmapHeight];

        int heightIndex = 0;

        for (int i = 0; i < terrainObjectData.heightmapWidth; i++)
        {
            for (int j = 0; j < terrainObjectData.heightmapHeight; j++)
            {
                heightData[i, j] = terrainObjectData.terrainHeightData[heightIndex];
                heightIndex++;
            }
        }

        terrainData.heightmapResolution = terrainObjectData.heightmapResolution;
        terrainData.SetHeights(0, 0, heightData);
        terrainData.size = terrainObjectData.terrainSize.vector3;

        //Add terrain layers
        List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

        foreach (TerrainLayerData terrainLayerData in terrainObjectData.terrainLayers)
        {
            TerrainLayer terrainLayer = new TerrainLayer();

            //Load Texture
            ResourceRequest diffuseTextureRequest = Resources.LoadAsync<Texture2D>("Textures/" + terrainLayerData.diffuseTexture);
            yield return new WaitWhile(() => diffuseTextureRequest.isDone == false);
            terrainLayer.diffuseTexture = diffuseTextureRequest.asset as Texture2D;

            //Set size and offset
            terrainLayer.tileSize = terrainLayerData.size.vector2;
            terrainLayer.tileOffset = terrainLayerData.offset.vector2;

            terrainLayers.Add(terrainLayer);
        }

        terrainData.terrainLayers = terrainLayers.ToArray();

        //Set alphamaps to display texture on terrain
        terrainData.alphamapResolution = terrainObjectData.alphamapResolution;

        float[,,] maps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                for (int l = 0; l < terrainData.alphamapLayers; l++)
                {
                    maps[x, y, l] = 1;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, maps);

        //Set Materials
        ResourceRequest materialRequest = Resources.LoadAsync<Material>("Materials/terrain_standard");
        yield return new WaitWhile(() => materialRequest.isDone == false);
        terrainObject.GetComponent<Terrain>().materialTemplate = materialRequest.asset as Material;

        //Connect terrain pieces
        terrainObject.GetComponent<Terrain>().allowAutoConnect = true;

        //Assign terrain data
        terrainObject.GetComponent<Terrain>().terrainData = terrainData;
        terrainObject.GetComponent<TerrainCollider>().terrainData = terrainData;

        //Add it as a source tag for the navMesh
        terrainObject.AddComponent<NavMeshSourceTag>();

        //Add terrain to chunk
        chunk.SetTerrain(terrainObject);

        yield return null;
    }

    IEnumerator LoadWorldObject(WorldNode chunk, WorldObjectData worldObjectData, Transform parent)
    {
        //Create object to place in the world
        GameObject worldObject;
        worldObject = new GameObject(worldObjectData.objectName);
        worldObject.isStatic = worldObjectData.isStatic;

        //Load mesh if it has one
        if (worldObjectData.hasModel)
        {
            worldObject.AddComponent<MeshFilter>();
            worldObject.AddComponent<MeshRenderer>();
            worldObject.AddComponent<MeshCollider>();

            //Load main and sub Meshes
            Mesh mesh = new Mesh();

            foreach (Mesh subMesh in Resources.LoadAll<Mesh>("3D_Models/" + worldObjectData.model))
            {
                if (subMesh.name == worldObjectData.mesh)
                {
                    mesh = subMesh;
                }
            }

            //If child object not a submesh
            if (mesh.vertexCount == 0)
            {
                ResourceRequest meshRequest = Resources.LoadAsync<Mesh>("3D_Models/" + worldObjectData.mesh);
                yield return new WaitWhile(() => meshRequest.isDone == false);
                mesh = meshRequest.asset as Mesh;
            }

            //Load and apply materials
            List<Material> objectMaterials = new List<Material>();
            for (int i = 0; i < worldObjectData.materials.Count; i++)
            {
                ResourceRequest materialRequest = Resources.LoadAsync<Material>("Materials/" + worldObjectData.materials[i]);
                yield return new WaitWhile(() => materialRequest.isDone == false);
                Material material = materialRequest.asset as Material;

                objectMaterials.Add(material);
            }

            worldObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            worldObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            worldObject.GetComponent<MeshRenderer>().sharedMaterials = objectMaterials.ToArray();

            //Add it as a source tag for the navMesh
            worldObject.AddComponent<NavMeshSourceTag>();
        }

        //Add nav mesh obstacles for lakes
        if (worldObjectData.isNavMeshObstacle)
        {
            worldObject.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            worldObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().size = worldObjectData.size.vector3;
            worldObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().center = worldObjectData.center.vector3;
            worldObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().carving = true;
        }

        //Loop through and create child objects first
        foreach (WorldObjectData childObjectData in worldObjectData.childObjects)
        {
            yield return StartCoroutine(LoadWorldObject(chunk, childObjectData, worldObject.transform));
        }

        //Set transform
        worldObject.transform.position = worldObjectData.position.vector3;
        worldObject.transform.rotation = worldObjectData.rotation.quaternion;
        worldObject.transform.localScale = worldObjectData.scale.vector3;

        if (parent)
        {
            //Set object parent
            worldObject.transform.SetParent(parent);
        }
        else
        {
            //Add object to chunk;
            chunk.AddObject(worldObject);
        }
    }

    IEnumerator LoadEnemies(Vector2 chunkCoordinate, WorldEnemyData worldEnemyData)
    {
        //Find enemies in chunk
        foreach (EnemyData enemyData in worldEnemyData.enemies)
        {
            if (enemyData.coordinate.vector2 == chunkCoordinate)
            {
                //If the enemy is not loaded, then spawn the enemy
                if (!IsEnemyLoaded(enemyData.id) && !enemyData.dead)
                {
                    yield return StartCoroutine(LoadEnemy(enemyData));
                }
            }
        }
    }

    IEnumerator LoadEnemy(EnemyData enemyData)
    {
        //Load enemy prefab
        ResourceRequest request = Resources.LoadAsync<GameObject>("Enemies/" + enemyData.prefabName);
        yield return new WaitWhile(() => request.isDone == false);
        GameObject enemyObject = Instantiate(request.asset as GameObject, enemiesHolder.transform);
        EnemyController enemy = enemyObject.GetComponent<EnemyController>();

        enemy.id = enemyData.id;
        enemy.coordinate = enemyData.coordinate.vector2;

        enemy.transform.position = enemyData.position.vector3;
        enemy.transform.rotation = enemyData.rotation.quaternion;
        enemy.transform.localScale = enemyData.spawnScale.vector3;

        UnityEngine.AI.NavMeshAgent enemyAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();

        enemy.SetHealth(enemyData.health);

        enemyAgent.speed = enemyData.speed;
        enemy.attackingRange = enemyData.attackingRange;
        enemy.exploringRange = enemyData.exploringRange;
        enemy.idleTime = enemyData.idleTime;

        //Keep reference the all the loaded enemies in the scene
        loadedEnemies.Add(enemy);
    }
}

