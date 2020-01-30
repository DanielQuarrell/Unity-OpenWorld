using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    List<GameObject> objectsInChunk;
    
    GameObject terrainObject;
    
    bool chunkActive = false;

    private void Awake()
    {
        objectsInChunk = new List<GameObject>();
    }

    public void FindObjectsInChunk()
    {
        objectsInChunk = new List<GameObject>();

        foreach (Transform child in this.transform)
        {
            objectsInChunk.Add(child.gameObject);
        }
    }

    public void SetTerrain(GameObject newTerrainObject)
    {
        terrainObject = newTerrainObject;
        terrainObject.transform.SetParent(this.transform);
        terrainObject.transform.localPosition = new Vector3(-16, 0, -16);
    }

    public void SetChunkActive(bool active)
    {
        chunkActive = active;
    }

    public bool IsChunkActive()
    {
        return chunkActive;
    }

    public void AddObject(GameObject newObject)
    {
        newObject.transform.SetParent(this.transform);
        objectsInChunk.Add(newObject);
    }

    public List<GameObject> GetObjectsInChunk()
    {
        if(objectsInChunk == null || objectsInChunk.Count == 0)
        {
            FindObjectsInChunk();
        }

        return objectsInChunk;
    }

    public Vector2 GetCoordinate()
    {
        Vector3 vec3Coordinate = (this.transform.position - new Vector3(16, 0, 16)) / 32;

        return new Vector2((int)vec3Coordinate.x, (int)vec3Coordinate.z);
    }

    public bool ObjectInChunk(Transform worldObject)
    {
        foreach (GameObject chunkObject in objectsInChunk)
        {
            if(worldObject.gameObject == chunkObject)
            {
                return false;
            }
        }

        Vector3 worldPosition = this.transform.position - new Vector3(16, 0, 16);

        return worldPosition.x <= worldObject.position.x && worldObject.position.x <= worldPosition.x + 32 &&
               worldPosition.z <= worldObject.position.z && worldObject.position.z <= worldPosition.z + 32;
    }

    public void Unload()
    {
        Destroy(terrainObject);

        foreach (GameObject worldObject in objectsInChunk)
        {
            Destroy(worldObject);
        }

        objectsInChunk.Clear();
    }
}
