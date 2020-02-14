using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwordCollider : MonoBehaviour
{
    [SerializeField] Player player;

    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        boxCollider.enabled = player.animator.GetBool("Attacking");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            player.OnSwordCollision(other.GetComponent<EnemyController>());
        }
    }
}
