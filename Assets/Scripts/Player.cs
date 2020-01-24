using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }
}
