using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySwordCollider : MonoBehaviour
{
    [SerializeField] EnemyController enemyController;

    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        boxCollider.enabled = enemyController.animator.GetBool("Attacking");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemyController.OnSwordCollision(other.GetComponent<Player>());
        }
    }
}
