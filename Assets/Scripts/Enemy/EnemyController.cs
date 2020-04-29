using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    enum EnemyState
    {
        IDLE,
        ROAMING,
        CHASING
    }

    EnemyState currentState;
    NavMeshAgent agent;
    [HideInInspector] public Animator animator;

    public int id;
    public Vector2 coordinate;

    public float attackingRange = 20;
    public float exploringRange = 30; 
    public float idleTime = 4f;

    [SerializeField] private int health = 3;

    private float idleTimer = 0f;
    private float weaponRange = 2;
    private bool canDealDamage;
    private bool dead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        EnterIdle();
    }

    private void Update()
    {
        if(!dead && agent.isOnNavMesh)
        {
            if (PlayerInRange())
            {
                currentState = EnemyState.CHASING;
            }

            switch (currentState)
            {
                case EnemyState.IDLE:
                    IdleState();
                    break;

                case EnemyState.ROAMING:
                    RoamingState();
                    break;

                case EnemyState.CHASING:
                    ChasingState();
                    break;
            }

            animator.SetFloat("Velocity", agent.velocity.magnitude);
            animator.SetFloat("AnimationSpeed", agent.velocity.magnitude / agent.speed);
        }
    }

    private void EnterIdle()
    {
        idleTimer = Random.Range(1.0f, idleTime);
        currentState = EnemyState.IDLE;
    }

    private void IdleState()
    {
        if (idleTimer > 0)
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                FindNewLocation();
            }
        }
    }

    private void FindNewLocation()
    {
        agent.SetDestination(GetRandomPoint(this.transform.position, exploringRange));
        currentState = EnemyState.ROAMING;
    }

    private Vector3 GetRandomPoint(Vector3 center, float maxDistance)
    {
        Vector3 randomPos = Random.insideUnitSphere * maxDistance + center;

        NavMeshHit hit;

        NavMesh.SamplePosition(randomPos, out hit, maxDistance, NavMesh.AllAreas);

        return hit.position;
    }

    private void RoamingState()
    {
        if (!agent.hasPath)
        {
            EnterIdle();
        }
    }

    private bool PlayerInRange()
    {
        float distance = DistanceFromPlayer();

        bool inRange = distance < attackingRange;

        return inRange && PlayerInSight();
    }

    public bool PlayerInSight()
    {
        RaycastHit hit;

        Vector3 playerSight = Player.instance.transform.position + Vector3.up;
        Vector3 enemySight = this.transform.position + Vector3.up;

        if (Physics.Linecast(enemySight, playerSight, out hit))
        {
            if (hit.transform == Player.instance.transform)
            {
                Debug.DrawLine(enemySight, playerSight, Color.green);
                return true;
            }
            else
            {
                Debug.DrawLine(enemySight, playerSight, Color.red);
                return false;
            }
        }

        return false;
    }

    float DistanceFromPlayer()
    {
        return Vector3.Distance(Player.instance.transform.position, this.transform.position);
    }

    private void ChasingState()
    {
        if (PlayerInRange())
        {
            EnterIdle();
        }

        if (DistanceFromPlayer() <= weaponRange && !animator.GetBool("Attacking") && !animator.GetBool("Hit"))
        {
            agent.isStopped = true;
            animator.SetTrigger("Attack");
            canDealDamage = true;
        }
        else if (DistanceFromPlayer() > weaponRange)
        {
            if (agent.isStopped)
            {
                agent.isStopped = false;
            }

            agent.SetDestination(Player.instance.transform.position);
        }
    }

    public int GetHealth()
    {
        return health;
    }

    public void SetHealth(int _health)
    {
        health = _health;
    }

    public bool IsDead()
    {
        return dead;
    }

    public void DealDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;

            if (health <= 0)
            {
                dead = true;
                agent.enabled = false;
                animator.SetBool("TriggerDeath", true);
                animator.SetBool("Dead", true);
            }
            else
            {
                animator.SetTrigger("Impact");
            }
        }
    }

    public void OnSwordCollision(Player player)
    {
        if(canDealDamage)
        {
            player.DealDamage(1);
            canDealDamage = false;
        }
    }
}
