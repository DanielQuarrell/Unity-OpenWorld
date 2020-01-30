using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldChunkLoader : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] WorldChunk[] chunks;

    [SerializeField] float distanceToLoadChunk = 100;

    void Update()
    {
        foreach (WorldChunk chunk in chunks)
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
                    chunk.Unload();
                }
            }
        }
    }

    void LoadChunk(WorldChunk chunk)
    {
        if (File.Exists(Application.persistentDataPath + "/worldJSON.json"))
        {
            string data = File.ReadAllText(Application.persistentDataPath + "/worldJSON.json");

            WorldData worldData = JsonUtility.FromJson<WorldData>(data);

            foreach(ChunkData chunkData in worldData.chunks)
            {
                if (chunkData.coordinate == chunk.GetCoordinate())
                {
                    chunk.SetTerrain(LoadTerrain(chunkData));

                    foreach (WorldObjectData worldObjectData in chunkData.worldObjects)
                    {
                        chunk.AddObject(LoadWorldObject(worldObjectData));
                    }
                }
            }
        }
    }

    GameObject LoadWorldObject(WorldObjectData worldObjectData)
    {
        GameObject worldObject;
        worldObject = new GameObject(worldObjectData.objectName);

        foreach (WorldObjectData childObjectData in worldObjectData.childObjects)
        {
            LoadWorldObject(childObjectData).transform.SetParent(worldObject.transform);
        }

        if(worldObjectData.hadModel)
        {
            worldObject.AddComponent<MeshFilter>();
            worldObject.AddComponent<MeshRenderer>();
            worldObject.AddComponent<MeshCollider>();

            Mesh mesh = new Mesh();
            foreach (Mesh subMesh in Resources.LoadAll<Mesh>("3D_Models/" + worldObjectData.model))
            {
                if (subMesh.name == worldObjectData.mesh)
                {
                    mesh = subMesh;
                }
            }
            
            worldObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            worldObject.GetComponent<MeshCollider>().sharedMesh = mesh;

            List<Material> objectMaterials = new List<Material>();
            for (int i = 0; i < worldObjectData.materials.Count; i++)
            {
                Material material = Resources.Load("Materials/" + worldObjectData.materials[i]) as Material;
                objectMaterials.Add(material);
            }

            worldObject.GetComponent<MeshRenderer>().sharedMaterials = objectMaterials.ToArray();
        }

        worldObject.transform.position = worldObjectData.position;
        worldObject.transform.rotation = worldObjectData.rotation;
        worldObject.transform.localScale = worldObjectData.scale;

        return worldObject;
    }

    GameObject LoadTerrain(ChunkData chunkData)
    {
        TerrainObjectData terrainObjectData = chunkData.terrainObject;

        GameObject terrainObject;
        terrainObject = new GameObject(terrainObjectData.terrainName);
        terrainObject.AddComponent<Terrain>();
        terrainObject.AddComponent<TerrainCollider>();

        TerrainData terrainData = Resources.Load("Terrain/Data/" + terrainObjectData.terrainName) as TerrainData;

        terrainObject.GetComponent<Terrain>().terrainData = terrainData;
        terrainObject.GetComponent<TerrainCollider>().terrainData = terrainData;

        List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

        for (int i = 0; i < terrainObjectData.terrainLayers.Count; i++)
        {
            TerrainLayer terrainLayer = Resources.Load("Terrain/Layers/" + terrainObjectData.terrainLayers[i]) as TerrainLayer;
            terrainLayers.Add(terrainLayer);
        }

        terrainObject.GetComponent<Terrain>().terrainData.terrainLayers = terrainLayers.ToArray();
        terrainObject.GetComponent<Terrain>().materialTemplate = Resources.Load("Materials/terrain_standard") as Material;
        terrainObject.GetComponent<Terrain>().allowAutoConnect = true;

        return terrainObject;
    }
}
