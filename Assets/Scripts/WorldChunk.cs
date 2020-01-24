using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    [SerializeField] GameObject model;

    // Update is called once per frame
    void Update()
    {
        model.SetActive(Vector3.Distance(this.transform.position, Player.instance.transform.position) < 10);
    }
}
