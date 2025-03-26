using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WheelAI : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private Transform playerTransform;
    
    public float attackRange = 2f;
    public float moveSpeed = 50f;
    public float attackSpeed = 100f;
    private bool isAttacking = false;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;
        navAgent.acceleration = 12f;       
        navAgent.stoppingDistance = 0f; 
        
        FindPlayer(); // in game  find the player when the enemy spawns
    }

    void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer(); 
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            StartCoroutine(AttackPlayer());
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {         Debug.Log("find player ");
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning(" Player not found!.");
        }
    }

    private void ChasePlayer()
    {
        if (isAttacking) return; // Stop moving if attacking

        if (navAgent.isStopped)
        {       Debug.Log("chasing");
            navAgent.isStopped = false; // Resume movement
        }

        if (playerTransform != null)
        {
            navAgent.SetDestination(playerTransform.position);
        }
    }

    public IEnumerator AttackPlayer()
    {
        if (isAttacking) yield break;

        isAttacking = true;
        navAgent.isStopped = true; 
        
        // Attack logic: Increase Z-speed
        Vector3 attackMove = transform.forward * attackSpeed * Time.deltaTime;
        transform.position += attackMove;

        yield return new WaitForSeconds(1.5f);

        isAttacking = false;
    }

    public void StartChasing()
    {
        enabled = true; 
        FindPlayer(); 
        Debug.Log("Enemy started chasing the player!");
    }
}
