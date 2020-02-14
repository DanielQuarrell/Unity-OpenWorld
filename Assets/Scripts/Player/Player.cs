using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    [HideInInspector] public Animator animator;

    private bool canDealDamage;
    private int health = 6;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;

        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !animator.GetBool("Attacking"))
        {
            animator.SetTrigger("Attack");
            canDealDamage = true;
        }
    }

    public void DealDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;

            if (health <= 0)
            {
                animator.SetBool("TriggerDeath", true);
                animator.SetBool("Dead", true);
            }
        }
    }

    public void OnSwordCollision(EnemyController enemy)
    {
        if(canDealDamage)
        {
            enemy.DealDamage(1);
            canDealDamage = false;
        }
    }
}
