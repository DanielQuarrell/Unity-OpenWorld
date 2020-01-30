using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class SaveLevelToJson : MonoBehaviour
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
            chunkData.coordinate = chunk.GetCoordinate();
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

        // Write the text to save data.
        string data = string.Empty;
        data = JsonUtility.ToJson(worldData, true);
        File.WriteAllText(Application.persistentDataPath + "/worldJSON.json", data);
        Debug.Log("Saved world to: " + Application.persistentDataPath + "/worldJSON.json");
    }

    TerrainObjectData ProcessTerrain(GameObject chunkObject)
    {
        TerrainObjectData terrainObject = new TerrainObjectData();

        terrainObject.terrainName = chunkObject.GetComponent<Terrain>().terrainData.name;

        terrainObject.terrainLayers = new List<string>();
        foreach (TerrainLayer layer in chunkObject.GetComponent<Terrain>().terrainData.terrainLayers)
        {
            string layerString = layer.name;
            terrainObject.terrainLayers.Add(layer.name);
        }

        return terrainObject;
    }

    WorldObjectData ProcessChunkObject(GameObject chunkObject, bool childObject)
    {
        WorldObjectData worldObject = new WorldObjectData();

        worldObject.objectName = chunkObject.name;
        worldObject.position = childObject ? chunkObject.transform.localPosition : chunkObject.transform.position;
        worldObject.rotation = childObject ? chunkObject.transform.localRotation : chunkObject.transform.rotation;
        worldObject.scale = chunkObject.transform.localScale;

        if (chunkObject.GetComponent<MeshRenderer>())
        {
            worldObject.hadModel = true;

            Mesh mesh = chunkObject.GetComponent<MeshFilter>().sharedMesh;

            if(AssetDatabase.IsSubAsset(mesh.GetInstanceID()))
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
            worldObject.hadModel = false;
        }

        worldObject.childObjects = new List<WorldObjectData>();
        foreach (Transform child in chunkObject.transform)
        {
            worldObject.childObjects.Add(ProcessChunkObject(child.gameObject, true));
        }

        return worldObject;
    }
}
