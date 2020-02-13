using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using System.Runtime.Serialization.Formatters.Binary;

public class WorldChunkLoader : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] NavMeshSurface navMeshSurface;
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
        if (File.Exists(Application.persistentDataPath + "/worldData.dat"))
        {
            //Load from binary
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/worldData.dat", FileMode.Open);
            WorldData worldData = (WorldData)bf.Deserialize(file);
            file.Close();

            //Load from JSON
            //string data = File.ReadAllText(Application.persistentDataPath + "/worldData.dat");
            //WorldData worldData = JsonUtility.FromJson<WorldData>(data);

            foreach (ChunkData chunkData in worldData.chunks)
            {
                if (chunkData.coordinate.vector2 == chunk.GetCoordinate())
                {
                    IEnumerator loadChunkAsync = LoadChunkAsync(chunk, chunkData);
                    StartCoroutine(loadChunkAsync);
                }
            }
        }
    }

    IEnumerator LoadChunkAsync(WorldChunk chunk, ChunkData chunkData)
    {
        yield return StartCoroutine(LoadTerrain(chunk, chunkData.terrainObject));

        foreach (WorldObjectData worldObjectData in chunkData.worldObjects)
        {
            yield return StartCoroutine(LoadWorldObject(chunk, worldObjectData, null));
        }
    }

    IEnumerator LoadTerrain(WorldChunk chunk, TerrainObjectData terrainObjectData)
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
                    maps[x, y, 0] = 1;
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

    IEnumerator LoadWorldObject(WorldChunk chunk, WorldObjectData worldObjectData, Transform parent)
    {
        //Create object to place in the world
        GameObject worldObject;
        worldObject = new GameObject(worldObjectData.objectName);

        //Load mesh if it has one
        if(worldObjectData.hasModel)
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

        if(worldObjectData.isNavMeshObstacle)
        {
            worldObject.AddComponent<NavMeshObstacle>();
            worldObject.GetComponent<NavMeshObstacle>().size = worldObjectData.size.vector3;
            worldObject.GetComponent<NavMeshObstacle>().center = worldObjectData.center.vector3;
            worldObject.GetComponent<NavMeshObstacle>().carving = true;
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

        if(parent)
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

    IEnumerator BuildNavmesh(NavMeshSurface surface)
    {
        // get the data for the surface
        var data = InitializeBakeData(surface);

        // start building the navmesh
        var async = surface.UpdateNavMesh(data);

        // wait until the navmesh has finished baking
        yield return async;

        Debug.Log("finished");

        // you need to save the baked data back into the surface
        surface.navMeshData = data;

        // call AddData() to finalize it
        surface.AddData();
    }

    // creates the navmesh data
    private NavMeshData InitializeBakeData(NavMeshSurface surface)
    {
        var emptySources = new List<NavMeshBuildSource>();
        var emptyBounds = new Bounds();

        return NavMeshBuilder.BuildNavMeshData(surface.GetBuildSettings(), emptySources, emptyBounds, surface.transform.position, surface.transform.rotation);
    }
}
