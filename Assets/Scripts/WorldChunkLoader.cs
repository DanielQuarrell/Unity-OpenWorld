using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunkLoader : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] WorldChunk[] chunks;

    [SerializeField] float distanceToLoadChunk = 100;

    /*
    void Update()
    {
        foreach(WorldChunk chunk in chunks)
        {
            Vector3 vectorDistance = player.transform.position - chunk.transform.position;
            vectorDistance.y = 0;
            float distanceToPlayer = vectorDistance.magnitude;

            if (distanceToPlayer < distanceToLoadChunk)
            {
                if (!chunk.IsChunkActive())
                {
                    chunk.SpawnChunk();
                }
            }
            else
            {
                if (chunk.IsChunkActive())
                {
                    chunk.UnloadChunk();
                }
            }
        }
    }
    */
}
