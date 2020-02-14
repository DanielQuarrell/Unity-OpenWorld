using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;


public class SaveLevelToFile: MonoBehaviour
{
    [SerializeField] GameObject world;
    [SerializeField] List<WorldChunk> chunks;

    WorldData worldData;

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

    public void Save()
    {
        worldData = new WorldData();
        worldData.chunks = new List<ChunkData>();

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
        worldObject.position = new SerializableVector3(childObject ? chunkObject.transform.localPosition : chunkObject.transform.position);
        worldObject.rotation = new SerializableQuaternion(childObject ? chunkObject.transform.localRotation : chunkObject.transform.rotation);
        worldObject.scale = new SerializableVector3(chunkObject.transform.localScale);

        if (chunkObject.GetComponent<MeshRenderer>())
        {
            worldObject.hasModel = true;

            Mesh mesh = chunkObject.GetComponent<MeshFilter>().sharedMesh;

            if(AssetDatabase.IsSubAsset(mesh.GetInstanceID()) && chunkObject.transform.parent.name != "Chunk")
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

    void SaveToBinary()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/worldData.dat");
        bf.Serialize(file, worldData);
        file.Close();
        Debug.Log("Saved world to: " + Application.persistentDataPath + "/worldData.dat");
    }
}
