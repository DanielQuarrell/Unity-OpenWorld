using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[DefaultExecutionOrder(-200)]
public class NavMeshSourceTag : MonoBehaviour
{
    // Global containers for all active mesh/terrain tags
    public static List<MeshFilter> meshes = new List<MeshFilter>();
    public static List<Terrain> terrains = new List<Terrain>();

    void OnEnable()
    {
        MeshFilter mesh = GetComponent<MeshFilter>();
        if (mesh != null)
        {
            meshes.Add(mesh);
        }

        Terrain terrain = GetComponent<Terrain>();
        if (terrain != null)
        {
            terrains.Add(terrain);
        }
    }

    void OnDisable()
    {
        MeshFilter mesh = GetComponent<MeshFilter>();
        if (mesh != null)
        {
            meshes.Remove(mesh);
        }

        Terrain terrain = GetComponent<Terrain>();
        if (terrain != null)
        {
            terrains.Remove(terrain);
        }
    }

    // Collect all the navmesh build sources for enabled objects tagged by this component
    public static void Collect(ref List<NavMeshBuildSource> sources)
    {
        sources.Clear();

        for (int i = 0; i < meshes.Count; ++i)
        {
            MeshFilter mesh = meshes[i];
            if (mesh == null) continue;

            var m = mesh.sharedMesh;
            if (m == null) continue;

            NavMeshBuildSource source = new NavMeshBuildSource();
            source.shape = NavMeshBuildSourceShape.Mesh;
            source.sourceObject = m;
            source.transform = mesh.transform.localToWorldMatrix;
            source.area = 0;
            sources.Add(source);
        }

        for (int i = 0; i < terrains.Count; ++i)
        {
            Terrain terrain = terrains[i];
            if (terrain == null) continue;

            NavMeshBuildSource source = new NavMeshBuildSource();
            source.shape = NavMeshBuildSourceShape.Terrain;
            source.sourceObject = terrain.terrainData;

            // Terrain system only supports translation
            source.transform = Matrix4x4.TRS(terrain.transform.position, Quaternion.identity, Vector3.one);
            source.area = 0;
            sources.Add(source);
        }
    }
}
