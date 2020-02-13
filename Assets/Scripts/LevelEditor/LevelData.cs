using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    public Vector2 vector2
    {
        get
        {
            return new Vector2(x, y);
        }
        set
        {
            x = value.x;
            y = value.y;
        }
    }

    public SerializableVector2(Vector2 vector2)
    {
        x = vector2.x;
        y = vector2.y;
    }
}

[System.Serializable]
public class SerializableVector3
{
    public float x;
    public float y;
    public float z;

    public Vector3 vector3
    {
        get
        {
            return new Vector3(x, y, z);
        }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
    }

    public SerializableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}

[System.Serializable]
public class SerializableQuaternion
{
    public float x;
    public float y;
    public float z;
    public float w;

    public Quaternion quaternion
    {
        get
        {
            return new Quaternion(x, y, z, w);
        }
        set
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }
    }

    public SerializableQuaternion(Quaternion quaternion)
    {
        x = quaternion.x;
        y = quaternion.y;
        z = quaternion.z;
        w = quaternion.w;
    }
}

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
