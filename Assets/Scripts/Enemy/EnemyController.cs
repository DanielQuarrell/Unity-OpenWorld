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
    Animator animator;

    GameObject player;

    [SerializeField] private float attackingRange = 20;
    [SerializeField] private float exploringRange = 30; 
    [SerializeField] private float idleTime = 4f;

    private float idleTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        player = GameObject.FindWithTag("Player");
    }

    private void Start()
    {
        EnterIdle();
    }

    private void Update()
    {
        if(PlayerInRange())
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

        if (Input.GetKeyDown(KeyCode.H))
        {
            animator.SetTrigger("Impact");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            animator.SetBool("TriggerDeath", true);
            animator.SetBool("Dead", true);
        }

        animator.SetFloat("Velocity", agent.velocity.magnitude);
        animator.SetFloat("AnimationSpeed", agent.velocity.magnitude / agent.speed);
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
        float distance = Vector3.Distance(player.transform.position, this.transform.position);

        bool inRange = distance < attackingRange;

        return inRange && PlayerInSight();
    }

    public bool PlayerInSight()
    {
        RaycastHit hit;

        Vector3 playerSight = player.transform.position + Vector3.up;
        Vector3 enemySight = this.transform.position + Vector3.up;

        if (Physics.Linecast(enemySight, playerSight, out hit))
        {
            if (hit.transform == player.transform)
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

    private void ChasingState()
    {
        if (PlayerInRange())
        {
            currentState = EnemyState.IDLE;
        }

        agent.SetDestination(player.transform.position);

        if(agent.remainingDistance < 2)
        {
            animator.SetTrigger("Attack");
        }
    }
}
