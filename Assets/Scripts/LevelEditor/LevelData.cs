using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldObject
{
    public string objectName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public string model;

    public List<string> materials;
}

[System.Serializable]
public class TerrainObject
{
    public string terrainName;
    public string terrainLayer;
}

[System.Serializable]
public class ChunkData
{
    public Vector2 coordinate;

    public List<WorldObject> worldObjects;
    public TerrainObject terrainObject;
}

[System.Serializable]
public class WorldData
{
    public List<ChunkData> chunks;
}
