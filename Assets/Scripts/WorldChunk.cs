using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    [SerializeField] GameObject model;

    public void SetModelActive(bool active)
    {
        model.SetActive(active);
    }
}
