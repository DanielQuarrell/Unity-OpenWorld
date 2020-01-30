﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldObjectData
{
    public string objectName;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public string model;

    public List<string> materials;
}

[System.Serializable]
public class TerrainObjectData
{
    public string terrainName;
    public List<string> terrainLayers;
}

[System.Serializable]
public class ChunkData
{
    public Vector2 coordinate;

    public List<WorldObjectData> worldObjects;
    public TerrainObjectData terrainObject;
}

[System.Serializable]
public class WorldData
{
    public List<ChunkData> chunks;
}
