using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLevelToJson : MonoBehaviour
{
    [SerializeField] WorldChunk[] chunks;

    WorldData worldData;

    void Start()
    {
        worldData = new WorldData();
        worldData.chunks = new List<ChunkData>();

        foreach (WorldChunk chunk in chunks)
        {
            ChunkData chunkData = new ChunkData();
            chunkData.coordinate = chunk.GetCoordinates();
            chunkData.worldObjects = new List<WorldObject>();

            foreach (GameObject objectInChunk in chunk.GetObjectsInChunk())
            {
                if (objectInChunk.GetComponent<Terrain>())
                {
                    TerrainObject terrainObject = new TerrainObject();

                    terrainObject.terrainLayer = objectInChunk.GetComponent<Terrain>().terrainData.terrainLayers[0].name;
                    terrainObject.terrainName = objectInChunk.GetComponent<TerrainCollider>().terrainData.name;

                    chunkData.terrainObject = terrainObject;
                }
                else
                {
                    WorldObject worldObject = new WorldObject();

                    worldObject.objectName = objectInChunk.name;
                    worldObject.position = objectInChunk.transform.position;
                    worldObject.rotation = objectInChunk.transform.rotation;
                    worldObject.scale = objectInChunk.transform.localScale;

                    if (objectInChunk.GetComponent<MeshRenderer>())
                    {
                        worldObject.model = objectInChunk.GetComponent<MeshFilter>().mesh.name;

                        worldObject.materials = new List<string>();
                        foreach (Material mat in objectInChunk.GetComponent<MeshRenderer>().materials)
                        {
                            worldObject.materials.Add(mat.name);
                        }
                    }

                    chunkData.worldObjects.Add(worldObject);
                }
            }

            worldData.chunks.Add(chunkData);
        }

        Save();
    }

    public void Save()
    {
        // Write the text to save data.
        string data = string.Empty;
        data = JsonUtility.ToJson(worldData, true);
        File.WriteAllText(Application.persistentDataPath + "/worldJSON.json", data);
        Debug.Log(Application.persistentDataPath);
    }
}
