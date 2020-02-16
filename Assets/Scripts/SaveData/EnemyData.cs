using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public SerializableVector2 coordinate;

    public string prefabName;
    public int id;
    public SerializableVector3 spawnPosition;
    public SerializableQuaternion spawnRotation;
    public SerializableVector3 spawnScale;

    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public SerializableVector3 scale;

    public bool dead;
    public int maxHealth;
    public int health;

    public float speed;
    public float attackingRange;
    public float exploringRange;
    public float idleTime;
}

[System.Serializable]
public class WorldEnemyData
{
    public List<EnemyData> enemies;
}

