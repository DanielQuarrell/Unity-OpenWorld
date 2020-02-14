using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    NavMeshAgent agent;

    GameObject Player;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        Player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            agent.SetDestination(Player.transform.position);
        }
    }
}
