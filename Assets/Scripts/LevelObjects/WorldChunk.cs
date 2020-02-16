using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    public Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 size = new Vector3(32.0f, 100.0f, 32.0f);

    public static List<Bounds> activeChunkBounds = new List<Bounds>();

    Bounds chunkBoundary;

    List<GameObject> objectsInChunk;
    GameObject terrainObject;
    
    bool chunkActive = false;

    private void Awake()
    {
        objectsInChunk = new List<GameObject>();

        chunkBoundary = GetChunkBounds();
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
        terrainObject.transform.localPosition = new Vector3(-size.x / 2, 0, -size.z / 2);
    }

    public void SetChunkActive(bool active)
    {
        chunkActive = active;

        if(chunkActive)
        {
            activeChunkBounds.Add(chunkBoundary);
        }
        else
        {
            activeChunkBounds.Remove(chunkBoundary);
        }
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

        if(objectsInChunk == null)
        {
            return new List<GameObject>();
        }

        return objectsInChunk;
    }

    public Vector2 GetCoordinate()
    {
        Vector3 vec3Coordinate = (this.transform.position - new Vector3(size.x / 2, 0, size.z / 2)) / 32;

        return new Vector2((int)vec3Coordinate.x, (int)vec3Coordinate.z);
    }

    public bool ObjectInChunk(Transform worldObject)
    {
        if(objectsInChunk != null)
        {
            foreach (GameObject chunkObject in objectsInChunk)
            {
                if (worldObject.gameObject == chunkObject)
                {
                    return false;
                }
            }
        }

        Vector3 worldPosition = this.transform.position - new Vector3(size.x / 2, 0, size.z / 2);

        return worldPosition.x <= worldObject.position.x && worldObject.position.x <= worldPosition.x + size.x &&
               worldPosition.z <= worldObject.position.z && worldObject.position.z <= worldPosition.z + size.z;
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

    public static Bounds GetWorldBounds()
    {
        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float minZ = Mathf.Infinity;
                     
        float maxX = Mathf.NegativeInfinity;
        float maxY = Mathf.NegativeInfinity;
        float maxZ = Mathf.NegativeInfinity;

        //Loop through the 8 vertices describing the bounding box
        for (int i = 0; i < activeChunkBounds.Count; i++)
        {
            //Get the smallest vertex 
            minX = Mathf.Min(minX, activeChunkBounds[i].center.x);
            minY = Mathf.Min(minY, activeChunkBounds[i].center.y);
            minZ = Mathf.Min(minZ, activeChunkBounds[i].center.z);

            //Get the largest vertex 
            maxX = Mathf.Max(maxX, activeChunkBounds[i].center.x);
            maxY = Mathf.Max(maxY, activeChunkBounds[i].center.y);
            maxZ = Mathf.Max(maxZ, activeChunkBounds[i].center.z);
        }

        Vector3 size = new Vector3(maxX - minX + 32, 100, maxZ - minZ + 32);
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);

        if(minX == Mathf.Infinity ||
           minY == Mathf.Infinity ||
           minZ == Mathf.Infinity ||
           maxX == Mathf.NegativeInfinity || 
           maxY == Mathf.NegativeInfinity ||
           maxZ == Mathf.NegativeInfinity)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        return new Bounds(center, size);
    }

    Bounds GetChunkBounds()
    {
        Vector3 transformCenter = transform.position + center;
        return new Bounds(transformCenter, size);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Bounds bounds = GetChunkBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
