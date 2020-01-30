using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

            foreach (GameObject objectInChunk in chunk.GetObjectsInChunk())
            {
                if (objectInChunk.GetComponent<Terrain>())
                {
                    TerrainObjectData terrainObject = new TerrainObjectData();

                    terrainObject.terrainName = objectInChunk.GetComponent<Terrain>().terrainData.name;

                    terrainObject.terrainLayers = new List<string>();
                    foreach (TerrainLayer layer in objectInChunk.GetComponent<Terrain>().terrainData.terrainLayers)
                    {
                        string layerString = layer.name;
                        terrainObject.terrainLayers.Add(layer.name);
                    }

                    chunkData.terrainObject = terrainObject;
                }
                else
                {
                    WorldObjectData worldObject = new WorldObjectData();

                    worldObject.objectName = objectInChunk.name;
                    worldObject.position = objectInChunk.transform.position;
                    worldObject.rotation = objectInChunk.transform.rotation;
                    worldObject.scale = objectInChunk.transform.localScale;

                    if (objectInChunk.GetComponent<MeshRenderer>())
                    {
                        string modelString = objectInChunk.GetComponent<MeshFilter>().sharedMesh.name;
                        worldObject.model = modelString.Replace(" Instance", "");

                        worldObject.materials = new List<string>();
                        foreach (Material mat in objectInChunk.GetComponent<MeshRenderer>().sharedMaterials)
                        {
                            string matString = mat.name;
                            worldObject.materials.Add(matString.Replace(" (Instance)", ""));
                        }
                    }

                    chunkData.worldObjects.Add(worldObject);
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
}
