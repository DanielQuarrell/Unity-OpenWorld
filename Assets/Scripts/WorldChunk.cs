using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    [SerializeField] int terrainIndex;

    GameObject terrainObject;

    [SerializeField] bool chunkActive = false;

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
}
