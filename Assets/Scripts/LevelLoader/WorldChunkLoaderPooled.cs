using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Threading.Tasks;

public class LoadedAssets
{
    public Mesh[] meshes;
    public Material[] materials;
    public Texture2D[] textures;
    public GameObject[] prefabs;
}

public class WorldChunkLoaderPooled : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] WorldNode[] chunks;
    [SerializeField] GameObject enemiesHolder;

    [SerializeField] float distanceToLoadChunk = 100;

    [SerializeField] WorldObjectPool worldObjectPool;

    private LoadedAssets loadedAssets;

    private List<EnemyController> loadedEnemies;

    private AssetBundle worldAssetBundle = null;
    private AssetBundle enemyAssetBundle = null;
    private bool assetBundlesLoaded;

    bool loadingFile = false;

    private void Awake()
    {
        loadedAssets = new LoadedAssets();

        loadedEnemies = new List<EnemyController>();
    }

    private void Start()
    {
        foreach (WorldNode chunk in chunks)
        {
            chunk.SetChunkLoaded(false);
        }

        player.SetActive(false);

        StartCoroutine(LoadAssetBundles());
    }

    private void LoadStartingChunks()
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
            }
        }

        player.SetActive(true);
    }

    void Update()
    {
        if(assetBundlesLoaded)
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
                        StartCoroutine(LoadChunk(chunk));
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
    }

    IEnumerator LoadChunk(WorldNode chunk)
    {
        yield return LoadChunkFromFileAsync(chunk).AsIEnumerator();
    }

    private async Task LoadChunkFromFileAsync(WorldNode chunk)
    {
        if (File.Exists(Application.persistentDataPath + "/worldData.dat") && File.Exists(Application.persistentDataPath + "/enemiesData.dat"))
        {
            //Load files from binary
            BinaryFormatter bf = new BinaryFormatter();

            while (loadingFile)
            {
                await Task.Delay(25);
            }

            WorldData worldData = await DeserialiseWorldFileAsync(bf);

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

    private async Task<WorldData> DeserialiseWorldFileAsync(BinaryFormatter bf)
    {
        string persistantDataPath = Application.persistentDataPath;
        WorldData worldData = new WorldData();

        if (!loadingFile)
        {
            loadingFile = true;

            await Task.Run(() =>
            {
                FileStream worldFile = File.Open(persistantDataPath + "/worldData.dat", FileMode.Open);
                worldData = (WorldData)bf.Deserialize(worldFile);
                worldFile.Close();
                loadingFile = false;
            });
        }

        return worldData;
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
        chunk.Unload(ref worldObjectPool);
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
            terrainLayer.diffuseTexture = loadedAssets.textures.FirstOrDefault(t => t.name == terrainLayerData.diffuseTexture);

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
        terrainObject.GetComponent<Terrain>().materialTemplate = loadedAssets.materials.FirstOrDefault(m => m.name == "terrain_standard");

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
        GameObject worldObject = worldObjectPool.GetNewWorldObject();

        //Load mesh if it has one
        if (worldObjectData.hasModel)
        {
            //Load mesh
            Mesh mesh = new Mesh();

            mesh = loadedAssets.meshes.FirstOrDefault(m => m.name == worldObjectData.mesh);

            //Load and apply materials
            List<Material> objectMaterials = new List<Material>();
            for (int i = 0; i < worldObjectData.materials.Count; i++)
            {
                Material material = loadedAssets.materials.FirstOrDefault(m => m.name == worldObjectData.materials[i]);

                objectMaterials.Add(material);
            }

            worldObject.GetComponent<MeshCollider>().enabled = true;
            worldObject.GetComponent<MeshRenderer>().enabled = true;

            worldObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            worldObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            worldObject.GetComponent<MeshRenderer>().sharedMaterials = objectMaterials.ToArray();

            //Add it as a source tag for the navMesh
        }

        //Add nav mesh obstacles for lakes
        if (worldObjectData.isNavMeshObstacle)
        {
            worldObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = true;
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
        GameObject enemyObject = Instantiate(loadedAssets.prefabs.FirstOrDefault(m => m.name == enemyData.prefabName), enemiesHolder.transform);
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

        yield return null;
    }

    private IEnumerator LoadAssetBundles()
    {
        string worldBundlePath = "";
#if UNITY_STANDALONE || UNITY_EDITOR
        worldBundlePath = Application.streamingAssetsPath + "/StandaloneWindows/" + "world";
#endif
        byte[] worldBundleData = System.IO.File.ReadAllBytes(worldBundlePath);

        AssetBundleCreateRequest resultWorldAssetBundle = AssetBundle.LoadFromMemoryAsync(worldBundleData);
        yield return new WaitWhile(() => resultWorldAssetBundle.isDone == false);
        worldAssetBundle = resultWorldAssetBundle.assetBundle;

        string enemyBundlePath = "";
#if UNITY_STANDALONE || UNITY_EDITOR
        enemyBundlePath = Application.streamingAssetsPath + "/StandaloneWindows/" + "enemies";
#endif
        byte[] enemyBundleData = System.IO.File.ReadAllBytes(enemyBundlePath);

        AssetBundleCreateRequest resultEnemyAssetBundle = AssetBundle.LoadFromMemoryAsync(enemyBundleData);
        yield return new WaitWhile(() => resultEnemyAssetBundle.isDone == false);
        enemyAssetBundle = resultEnemyAssetBundle.assetBundle;

        loadedAssets.meshes = worldAssetBundle.LoadAllAssets<Mesh>();
        loadedAssets.materials = worldAssetBundle.LoadAllAssets<Material>();
        loadedAssets.textures = worldAssetBundle.LoadAllAssets<Texture2D>();
        loadedAssets.prefabs = enemyAssetBundle.LoadAllAssets<GameObject>();

        worldAssetBundle.Unload(false);
        enemyAssetBundle.Unload(false);

        assetBundlesLoaded = true;

        LoadStartingChunks();
    }
}