using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum EnemyState
{
    Idle,
    Patrolling,
    Chasing,
    Attacking,
    Dead
}

public class EnemyFSM : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private HashSet<Vector2Int> allFloorTiles;

    [Header("Idle Settings")]
    public float idleDuration = 3f;
    private float idleTimer;

    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolWaitTime = 3f;
    public float forceAwayFromEnemies = 0.5f;

    private float patrolTimer;
    private Vector2 currentTargetPosition;
    private AStarPathfinding pathfinding;
    private List<Vector2Int> patrolPath;
    private int currentPatrolPointIndex;
    private bool isPatrolling = false;

    [Header("Chase Settings")]
    public float chaseSpeed;

    [Header("Detection Area")]
    public float playerDetectRange = 5;
    public Transform detectionPoint;
    public LayerMask playerLayer;

    [Header("Attack Settings")]
    public float attackRange = 2;
    public float attackCooldown = 2;
    private float attackCooldownTimer;

    private EnemyState currentState;
    private int facingDirection = 1;
    private Rigidbody2D rb;
    private Transform player;

    private Animator anim;

    private void Start()
    {
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        if (dungeonGenerator != null)
        {
            allFloorTiles = dungeonGenerator.GetAllFloorTiles(); // Access the floor tiles from DungeonGenerator
            Debug.Log($"DungeonGenerator found {allFloorTiles.Count}");
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ChangeState(EnemyState.Idle);
        pathfinding = GetComponent<AStarPathfinding>();
    }

    private void Update()
    {
        //CheckForPlayer();

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Chasing:
                HandleChasingState();
                break;
            case EnemyState.Attacking:
                HandleAttackingState();
                break;
            case EnemyState.Dead:
                HandleDeadState();
                break;
        }
    }

    private void HandleIdleState()
    {
        // Start the idle timer if it's the first time entering Idle state
        if (idleTimer <= 0)
        {
            idleTimer = idleDuration; // Set the idle duration
        }

        // Countdown the idle timer
        idleTimer -= Time.deltaTime;

        // When the timer reaches zero, change to the Patrolling state
        if (idleTimer <= 0)
        {
            ChangeState(EnemyState.Patrolling);
        }
    }

    public void EnterPatrollingState()
    {
        if (isPatrolling) return;
        
        isPatrolling = true;

        // Select a random patrol point
        Vector2Int randomPatrolPoint = GetRandomPatrolPoint();

        // Perform A* pathfinding to find a path to that point
        Vector2Int startPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);
        List<Vector2Int> path = pathfinding.FindPath(startPosition, randomPatrolPoint);

        patrolPath = path;
        currentPatrolPointIndex = 0;

        // Start patrolling along the path
        StartCoroutine(PatrolAlongPath(patrolPath));
    }

    private IEnumerator PatrolAlongPath(List<Vector2Int> path)
    {
        while (currentPatrolPointIndex < path.Count)
        {
            Vector3 targetPosition = new Vector3(path[currentPatrolPointIndex].x + 0.5f, path[currentPatrolPointIndex].y + 0.25f, transform.position.z);
            //targetPosition = AvoidOtherEnemies(targetPosition);

            // Move towards the target position
            while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
            {
                Vector2 direction = (targetPosition - transform.position).normalized;
                rb.velocity = direction * patrolSpeed;

                yield return null;
            }

            // Stop at the patrol point and wait
            rb.velocity = Vector2.zero;
            currentPatrolPointIndex++;

            // Optionally, add a waiting time at each patrol point before moving to the next
            if (currentPatrolPointIndex < path.Count)
            {
                yield return new WaitForSeconds(patrolWaitTime);
            }
        }

        // Once the patrol path is complete, return to idle
        isPatrolling = false;
        ChangeState(EnemyState.Idle);
    }

    Vector3 AvoidOtherEnemies(Vector2 targetPosition)
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 1f, LayerMask.GetMask("Enemy"));

        if (nearbyEnemies.Length > 0)
        {
            Debug.Log(nearbyEnemies.Length);

            foreach (var enemy in nearbyEnemies)
            {
                if (enemy.gameObject != this.gameObject) // Avoid the enemy self
                {
                    Debug.Log($"enemies closing");
                    Vector2 directionAwayFromEnemy = (transform.position - enemy.transform.position).normalized;
                    targetPosition += directionAwayFromEnemy * forceAwayFromEnemies;
                }
            }

            Vector2Int targetGridPos = new Vector2Int(Mathf.FloorToInt(targetPosition.x), Mathf.FloorToInt(targetPosition.y));

            if (!IsValidFloorTile(targetGridPos))
            {
                targetPosition = FindNearestValidPosition(targetPosition);
                RecalculatePathToNewTarget(targetPosition);
            }
        }

        return targetPosition;
    }

    bool IsValidFloorTile(Vector2Int gridPos)
    {
        return allFloorTiles.Contains(gridPos);
    }

    void RecalculatePathToNewTarget(Vector2 targetPosition)
    {
        Vector2Int targetGridPos = new Vector2Int(Mathf.FloorToInt(targetPosition.x), Mathf.FloorToInt(targetPosition.y));

        if (IsValidFloorTile(targetGridPos))
        {
            Vector2Int startPosition = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
            List<Vector2Int> newPath = pathfinding.FindPath(startPosition, targetGridPos);

            Debug.Log("New Path Length: " + newPath.Count);

            patrolPath = newPath; // Update the patrol path to the new path
            currentPatrolPointIndex = 0; // Restart patrolling from the new path
        }
    }

    Vector2 FindNearestValidPosition(Vector2 invalidPos)
    {
        Vector2 nearestValidPosition = invalidPos;
        float closestDistance = float.MaxValue;

        foreach (Vector2Int validPos in allFloorTiles)
        {
            float distance = Vector2.Distance(invalidPos, validPos);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestValidPosition = validPos;
            }
        }

        Debug.Log("Nearest Valid Position Found");
        return nearestValidPosition;
    }

    // Select a random patrol point from allFloorTiles
    private Vector2Int GetRandomPatrolPoint()
    {
        List<Vector2Int> floorTilesList = new List<Vector2Int>(allFloorTiles);
        return floorTilesList[UnityEngine.Random.Range(0, floorTilesList.Count)];
    }

    private void HandleChasingState()
    {
        if (player.position.x > transform.position.x && facingDirection == 1 ||
            player.position.x < transform.position.x && facingDirection == -1)
        {
            Flip();
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * chaseSpeed;
    }

    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    private void HandleAttackingState()
    {
        rb.velocity = Vector2.zero;
    }

    private void HandleDeadState()
    {

    }

    private void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);

        if (hits.Length > 0)
        {
            player = hits[0].transform;

            if (Vector2.Distance(transform.position, player.transform.position) <= attackRange && attackCooldownTimer <= 0)
            {
                // if the player is in attack range and cooldown is ready
                attackCooldownTimer = attackCooldown;
                ChangeState(EnemyState.Attacking);
            }
            else if (Vector2.Distance(transform.position, player.position) > attackRange && currentState != EnemyState.Attacking)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
        }
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == EnemyState.Idle)
        {
            anim.SetBool("isIdle", false);
        }
        else if (currentState == EnemyState.Chasing)
        {
            anim.SetBool("isMoving", false);
        }
        else if (currentState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", false);
        }
        else if (currentState == EnemyState.Patrolling)
        {
            patrolPath.Clear();
            currentPatrolPointIndex = 0;
        }

        currentState = newState;

        if (currentState == EnemyState.Idle)
        {
            anim.SetBool("isIdle", true);
        }
        else if (currentState == EnemyState.Chasing)
        {
            anim.SetBool("isMoving", true);
        }
        else if (currentState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", true);
        }

        if (currentState == EnemyState.Patrolling)
        {
            EnterPatrollingState();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionPoint.position, playerDetectRange);

        if (patrolPath != null && patrolPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < patrolPath.Count - 1; i++)
            {
                Vector3 start = new Vector3(patrolPath[i].x + 0.5f, patrolPath[i].y + 0.25f, 0);
                Vector3 end = new Vector3(patrolPath[i + 1].x + 0.5f, patrolPath[i + 1].y + 0.25f, 0);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}