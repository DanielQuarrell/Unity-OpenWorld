using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    [SerializeField] int terrainIndex;

    List<GameObject> objectsInChunk;
    
    GameObject terrainObject;

    [SerializeField] bool chunkActive = false;

    private void Awake()
    {
        FindObjectsInChunk();
    }

    private void FindObjectsInChunk()
    {
        objectsInChunk = new List<GameObject>();

        foreach (Transform child in this.transform)
        {
            objectsInChunk.Add(child.gameObject);
        }
    }

    public List<GameObject> GetObjectsInChunk()
    {
        if(objectsInChunk == null)
        {
            FindObjectsInChunk();
        }

        return objectsInChunk;
    }

    public Vector2 GetCoordinates()
    {
        Vector3 vec3Coordinate = (this.transform.position - new Vector3(16, 0, 16)) / 32;

        return new Vector2((int)vec3Coordinate.x, (int)vec3Coordinate.z);
    }
        

    /*
    public void SpawnChunk()
    {
        if(!chunkActive)
        {
            terrainObject = Instantiate(Resources.Load("TerrainChunks/Terrain " + terrainIndex) as GameObject, this.transform);
            chunkActive = true;
        } 
    }

    public void UnloadChunk()
    {
        if (chunkActive)
        {
            Destroy(terrainObject);
            chunkActive = false;
        }
    }

    public bool IsChunkActive()
    {
        return chunkActive;
    }
    */
}
