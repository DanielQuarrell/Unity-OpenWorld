using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

#if UNITY_EDITOR
public class SaveLevelToFile: MonoBehaviour
{
    [SerializeField] GameObject enemiesHolder;
    [SerializeField] GameObject world;
    [SerializeField] List<WorldChunk> chunks;
    [SerializeField] List<EnemyController> enemies;

    WorldData worldData;
    WorldEnemyData worldEnemyData;

    public void SortObjectsIntoWorld()
    {
        foreach (Transform child in world.transform)
        {
            if (child.GetComponent<WorldChunk>())
            {
                if(!ChunkExists(child.GetComponent<WorldChunk>()))
                {
                    chunks.Add(child.GetComponent<WorldChunk>());
                }
            }
            else
            {
                CheckObjectInChunks(child);
            }
        }

        foreach (Transform child in enemiesHolder.transform)
        {
            if (child.GetComponent<EnemyController>())
            {
                if(!EnemyExists(child.GetComponent<EnemyController>()))
                {
                    enemies.Add(child.GetComponent<EnemyController>());
                }
            }
        }

        AssignEnemiesToChunk();
    }

    bool ChunkExists(WorldChunk newChunk)
    {
        foreach (WorldChunk chunk in chunks)
        {
            if (newChunk == chunk)
            {
                return true;
            }
        }

        return false;
    }

    bool EnemyExists(EnemyController newEnemy)
    {
        foreach (EnemyController enemy in enemies)
        {
            if (newEnemy == enemy)
            {
                return true;
            }
        }

        return false;
    }

    void CheckObjectInChunks(Transform worldObject)
    {
        foreach (WorldChunk chunk in chunks)
        {
            chunk.FindObjectsInChunk();

            if (chunk.ObjectInChunk(worldObject))
            {
                chunk.AddObject(worldObject.gameObject);
            }
        }
    }

    void AssignEnemiesToChunk()
    {
        foreach (EnemyController enemy in enemies)
        {
            foreach (WorldChunk chunk in chunks)
            {
                if (chunk.ObjectInChunk(enemy.transform))
                {
                    enemy.coordinate = chunk.GetCoordinate();
                    break;
                }
            }
        }
    }

    public void Save()
    {
        worldData = new WorldData();
        worldData.chunks = new List<ChunkData>();
        worldEnemyData = new WorldEnemyData();
        worldEnemyData.enemies = new List<EnemyData>();

        int enemyId = 0;

        foreach (WorldChunk chunk in chunks)
        {
            ChunkData chunkData = new ChunkData();
            chunkData.coordinate = new SerializableVector2(chunk.GetCoordinate());
            chunkData.worldObjects = new List<WorldObjectData>();

            foreach (GameObject chunkObject in chunk.GetObjectsInChunk())
            {
                if (chunkObject.GetComponent<Terrain>())
                {
                    chunkData.terrainObject = ProcessTerrain(chunkObject);
                }
                else
                {
                    chunkData.worldObjects.Add(ProcessChunkObject(chunkObject, false));
                }
            }

            worldData.chunks.Add(chunkData);
        }

        foreach (EnemyController enemy in enemies)
        {
            worldEnemyData.enemies.Add(ProcessEnemy(enemy, enemyId));
            enemyId++;
        }

        SaveToBinary();
    }

    TerrainObjectData ProcessTerrain(GameObject chunkObject)
    {
        TerrainObjectData terrainObject = new TerrainObjectData();

        Terrain terrainChunk = chunkObject.GetComponent<Terrain>();

        terrainObject.terrainName = chunkObject.GetComponent<Terrain>().terrainData.name;

        terrainObject.heightmapHeight = terrainChunk.terrainData.heightmapHeight;
        terrainObject.heightmapWidth = terrainChunk.terrainData.heightmapWidth;
        terrainObject.heightmapResolution = terrainChunk.terrainData.heightmapResolution;
        terrainObject.terrainSize = new SerializableVector3(terrainChunk.terrainData.size);

        terrainObject.alphamapLayers = terrainChunk.terrainData.alphamapLayers;
        terrainObject.alphamapResolution = terrainChunk.terrainData.alphamapResolution;
        terrainObject.alphamapHeight = terrainChunk.terrainData.alphamapHeight;
        terrainObject.alphamapWidth = terrainChunk.terrainData.alphamapWidth;

        float[,] heightData = terrainChunk.terrainData.GetHeights(0, 0, terrainChunk.terrainData.heightmapWidth, terrainChunk.terrainData.heightmapHeight);
        terrainObject.terrainHeightData = new float[terrainChunk.terrainData.heightmapWidth * terrainChunk.terrainData.heightmapHeight];

        int heightIndex = 0;

        for (int i = 0; i < terrainChunk.terrainData.heightmapWidth; i++)
        {
            for (int j = 0; j < terrainChunk.terrainData.heightmapHeight; j++)
            {
                terrainObject.terrainHeightData[heightIndex] = heightData[i, j];
                heightIndex++;
            }
        }

        terrainObject.terrainLayers = new List<TerrainLayerData>();
        foreach (TerrainLayer layer in chunkObject.GetComponent<Terrain>().terrainData.terrainLayers)
        {
            TerrainLayerData terrainLayerData = new TerrainLayerData();
            terrainLayerData.diffuseTexture = layer.diffuseTexture.name;
            terrainLayerData.size = new SerializableVector2(layer.tileSize);
            terrainLayerData.offset = new SerializableVector2(layer.tileOffset);

            terrainObject.terrainLayers.Add(terrainLayerData);
        }

        return terrainObject;
    }

    WorldObjectData ProcessChunkObject(GameObject chunkObject, bool childObject)
    {
        WorldObjectData worldObject = new WorldObjectData();

        worldObject.objectName = chunkObject.name;
        worldObject.isStatic = chunkObject.isStatic;

        worldObject.position = new SerializableVector3(childObject ? chunkObject.transform.localPosition : chunkObject.transform.position);
        worldObject.rotation = new SerializableQuaternion(childObject ? chunkObject.transform.localRotation : chunkObject.transform.rotation);
        worldObject.scale = new SerializableVector3(chunkObject.transform.localScale);

        if (chunkObject.GetComponent<MeshRenderer>())
        {
            worldObject.hasModel = true;

            Mesh mesh = chunkObject.GetComponent<MeshFilter>().sharedMesh;

            
            if (AssetDatabase.IsSubAsset(mesh.GetInstanceID()) && chunkObject.transform.parent.name != "Chunk")
            {
                string modelString = chunkObject.transform.parent.name;
                worldObject.model = modelString.Replace(" Instance", "");
            }
            else
            {
                string modelString = mesh.name;
                worldObject.model = modelString.Replace(" Instance", "");
            }

            string meshString = mesh.name;
            worldObject.mesh = meshString.Replace(" Instance", "");

            worldObject.materials = new List<string>();
            foreach (Material mat in chunkObject.GetComponent<MeshRenderer>().sharedMaterials)
            {
                string matString = mat.name;
                worldObject.materials.Add(matString.Replace(" (Instance)", ""));
            }
        }
        else
        {
            worldObject.hasModel = false;
        }
        
        if(chunkObject.GetComponent<NavMeshObstacle>())
        {
            worldObject.isNavMeshObstacle = true;

            NavMeshObstacle navMeshObstacle = chunkObject.GetComponent<NavMeshObstacle>();

            worldObject.size = new SerializableVector3(navMeshObstacle.size);
            worldObject.center = new SerializableVector3(navMeshObstacle.center);
        }
        else
        {
            worldObject.isNavMeshObstacle = false;
        }

        worldObject.childObjects = new List<WorldObjectData>();
        foreach (Transform child in chunkObject.transform)
        {
            worldObject.childObjects.Add(ProcessChunkObject(child.gameObject, true));
        }

        return worldObject;
    }

    EnemyData ProcessEnemy(EnemyController enemyInChunk, int id)
    {
        EnemyData enemy = new EnemyData();

        enemy.id = id;
        enemy.coordinate = new SerializableVector2(enemyInChunk.coordinate);

        enemy.prefabName = enemyInChunk.gameObject.name;
        enemy.position = new SerializableVector3(enemyInChunk.transform.position);
        enemy.rotation = new SerializableQuaternion(enemyInChunk.transform.rotation);
        enemy.scale = new SerializableVector3(enemyInChunk.transform.localScale);

        enemy.spawnPosition = new SerializableVector3(enemyInChunk.transform.position);
        enemy.spawnRotation = new SerializableQuaternion(enemyInChunk.transform.rotation);
        enemy.spawnScale = new SerializableVector3(enemyInChunk.transform.localScale);

        EnemyController enemyController = enemyInChunk.GetComponent<EnemyController>();
        NavMeshAgent enemyAgent = enemyInChunk.GetComponent<NavMeshAgent>();

        enemy.dead = false;
        enemy.maxHealth = enemyController.GetHealth();
        enemy.health = enemyController.GetHealth();

        enemy.speed = enemyAgent.speed;
        enemy.attackingRange = enemyController.attackingRange;
        enemy.exploringRange = enemyController.exploringRange;
        enemy.idleTime = enemyController.idleTime;

        return enemy;
    }

    void SaveToBinary()
    {
        string worldDataPath = "Assets/WorldData/";

        BinaryFormatter bf = new BinaryFormatter();
        FileStream worldFile = File.Create(worldDataPath + "worldData.dat");
        bf.Serialize(worldFile, worldData);
        worldFile.Close();
        Debug.Log("Saved world to: " + worldDataPath + "worldData.dat");

        FileStream enemiesFile = File.Create(worldDataPath + "enemiesData.dat");
        bf.Serialize(enemiesFile, worldEnemyData);
        enemiesFile.Close();
        Debug.Log("Saved enemies to: " + worldDataPath + "enemiesData.dat");
    }
}
#endif