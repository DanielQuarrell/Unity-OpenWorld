using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldObjectData
{
    public string objectName;
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;

    public bool hasModel;
    public string model;
    public string mesh;
    public List<string> materials;

    public bool isNavMeshObstacle;
    public SerializableVector3 size;
    public SerializableVector3 center;

    public List<WorldObjectData> childObjects;
}

[System.Serializable]
public class TerrainObjectData
{
    public string terrainName;
    public int heightmapWidth;
    public int heightmapHeight;
    public int heightmapResolution;

    public int alphamapLayers;
    public int alphamapResolution;
    public int alphamapWidth;
    public int alphamapHeight;

    public SerializableVector3 terrainSize;
    public float[] terrainHeightData;
    public List<TerrainLayerData> terrainLayers;
}

[System.Serializable]
public class TerrainLayerData
{
    public string diffuseTexture;
    public SerializableVector2 size;
    public SerializableVector2 offset;
}

[System.Serializable]
public class ChunkData
{
    public SerializableVector2 coordinate;

    public List<WorldObjectData> worldObjects;
    public TerrainObjectData terrainObject;
}

[System.Serializable]
public class WorldData
{
    public List<ChunkData> chunks;
}
