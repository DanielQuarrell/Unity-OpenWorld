using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;

//Build and update a localized navmesh from the sources marked by NavMeshSourceTag
[DefaultExecutionOrder(-102)]
public class WorldNavMeshBuilder : MonoBehaviour
{
    NavMeshData navMesh;
    AsyncOperation asyncOperation;
    NavMeshDataInstance instance;
    List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

    IEnumerator Start()
    {
        while (true)
        {
            UpdateNavMesh(true);
            yield return asyncOperation;
            yield return new WaitForSeconds(1);
        }
    }

    void OnEnable()
    {
        // Construct and add navmesh
        navMesh = new NavMeshData();
        instance = NavMesh.AddNavMeshData(navMesh);

        UpdateNavMesh(false);
    }

    void OnDisable()
    {
        // Unload navmesh and clear handle
        instance.Remove();
    }

    void UpdateNavMesh(bool asyncUpdate = false)
    {
        if(asyncOperation != null)
        {
            if(!asyncOperation.isDone)
            {
                NavMeshBuilder.Cancel(navMesh);
            }
        }

        NavMeshSourceTag.Collect(ref sources);

        List<NavMeshBuildSource> navMeshSources = sources;
        NavMeshBuildSettings defaultBuildSettings = NavMesh.GetSettingsByID(0);
        Bounds bounds = WorldChunk.GetWorldBounds();

        if (asyncUpdate)
        {
            asyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(navMesh, defaultBuildSettings, navMeshSources, bounds);
        }
        else
        {
            NavMeshBuilder.UpdateNavMeshData(navMesh, defaultBuildSettings, sources, bounds);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Bounds bounds = WorldChunk.GetWorldBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(bounds.center, 3);
    }
}
