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