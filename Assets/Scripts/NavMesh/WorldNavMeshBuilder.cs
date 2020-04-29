using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            yield return UpdateNavMeshAsync().AsIEnumerator();

            yield return new WaitWhile(() => asyncOperation.isDone == false);
            yield return new WaitForSeconds(1);
        }
    }

    void OnEnable()
    {
        // Construct and add navmesh
        navMesh = new NavMeshData();
        instance = NavMesh.AddNavMeshData(navMesh);

        UpdateNavMesh();
    }

    void OnDisable()
    {
        // Unload navmesh and clear handle
        instance.Remove();
    }

    void UpdateNavMesh()
    {
        NavMeshSourceTag.Collect(ref sources);

        List<NavMeshBuildSource> navMeshSources = sources;
        NavMeshBuildSettings defaultBuildSettings = NavMesh.GetSettingsByID(0);
        Bounds bounds = WorldNode.GetWorldBounds();

        NavMeshBuilder.UpdateNavMeshData(navMesh, defaultBuildSettings, sources, bounds);
    }

    private async Task UpdateNavMeshAsync()
    {
        NavMeshSourceTag.Collect(ref sources);
        List<NavMeshBuildSource> navMeshSources = sources;
        NavMeshBuildSettings defaultBuildSettings = NavMesh.GetSettingsByID(0);
        Bounds bounds = new Bounds();

        await Task.Run(() =>
        {
            bounds = WorldNode.GetWorldBounds();
        });

        asyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(navMesh, defaultBuildSettings, navMeshSources, bounds);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Bounds bounds = WorldNode.GetWorldBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(bounds.center, 3);
    }
}
