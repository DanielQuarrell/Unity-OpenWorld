using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldObjectPool : MonoBehaviour
{
    struct WorldObject
    {
        public GameObject gameObject;
        public bool active;
    }
    
    private WorldObject[] objectPool;

    private void Awake()
    {
        objectPool = new WorldObject[this.transform.childCount];

        int i = 0;
        foreach (Transform child in transform)
        {
            objectPool[i].gameObject = child.gameObject;
            objectPool[i].active = false;
            i++;
        }
    }

    public GameObject GetNewWorldObject()
    {
        for (int i = 0; i < objectPool.Length; i++)
        {
            if(objectPool[i].active == false)
            {
                objectPool[i].active = true;
                return objectPool[i].gameObject;
            }
        }

        return null;
    }

    public void RemoveWorldObject(GameObject _worldObject)
    {
        for (int i = 0; i < objectPool.Length; i++)
        {
            if (objectPool[i].gameObject == _worldObject)
            {
                objectPool[i].active = false;

                objectPool[i].gameObject.transform.parent = this.transform;

                if(objectPool[i].gameObject.GetComponent<MeshFilter>().mesh)
                {
                    objectPool[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                    objectPool[i].gameObject.GetComponent<MeshRenderer>().materials = new Material[0];
                    objectPool[i].gameObject.GetComponent<MeshFilter>().mesh = null;
                    objectPool[i].gameObject.GetComponent<MeshCollider>().enabled = false;
                    objectPool[i].gameObject.GetComponent<MeshCollider>().sharedMesh = null;
                }
                
                objectPool[i].gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>().enabled = false;
            }
        }
    }
}
