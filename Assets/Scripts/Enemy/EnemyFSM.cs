using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Patrolling,
    Chasing,
    Attacking,
    Dead,
    Inactive
}

public class EnemyFSM : MonoBehaviour
{
    private DungeonGenerator dungeonGenerator;
    private HashSet<Vector2Int> allFloorTiles;

    [Header("Room Management")]
    private int myRoomIndex = -1;
    private bool isRoomActive = true;
    private EnemyState stateBeforeInactive;

    [Header("Idle Settings")]
    [Range(0f, 1f)]
    public float patrolChancePerSecond = 0.5f;

    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;     
    
    private AStarPathfinding pathfinding;
    private List<Vector2Int> patrolPath;
    private int currentPatrolPointIndex;
    private bool isPatrolling = false;

    [Header("Chase Settings")]
    public float chaseSpeed;
    public float pathUpdateInterval = 0.5f;
    public float chasePathThreshold = 0.3f;

    private List<Vector2Int> chasePath;
    private int currentChasePointIndex;
    private float lastChasePathUpdate;
    private bool isChasing = false;

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

    // Initialize components and set initial state to Idle.
    private void Start()
    {
        dungeonGenerator = FindObjectOfType<DungeonGenerator>();
        if (dungeonGenerator != null)
        {
            allFloorTiles = dungeonGenerator.GetAllFloorTiles();
            Debug.Log($"DungeonGenerator found {allFloorTiles.Count}");
        }

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pathfinding = GetComponent<AStarPathfinding>();

        ChangeState(EnemyState.Idle);
    }

    // Continuously update the enemy state and handle actions based on the current state.
    private void Update()
    {
        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        // Only update AI if room is active
        if (!isRoomActive)
        {
            HandleInactiveState();
            return;
        }

        // Check for player only if room is active
        CheckForPlayer();

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
            case EnemyState.Inactive:
                HandleInactiveState();
                break;
        }
    }

    // Flip the enemy’s facing direction.
    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }


    // -----------------------------------------------   ENEMY INACTIVE STATE - START  ----------------------------------------------- //


    // Stop movement and animations when the room is inactive.
    private void HandleInactiveState()
    {
        Debug.Log("No Moving!");

        // Stop all movement and animations
        rb.velocity = Vector2.zero;

        // Set idle animation
        if (anim != null)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isMoving", false);
            anim.SetBool("isAttacking", false);
        }
    }

    // -----------------------------------------------   ENEMY INACTIVE STATE - END  ----------------------------------------------- //


    // -----------------------------------------------   ENEMY IDLE STATE - START  ----------------------------------------------- //


    // Handle transition from Idle to Patrolling based on random chance.
    private void HandleIdleState()
    {
        float randomChance = UnityEngine.Random.Range(0f, 1f);
        float frameChance = patrolChancePerSecond * Time.deltaTime;

        if (randomChance < frameChance)
        {
            ChangeState(EnemyState.Patrolling);
        }
    }


    // -----------------------------------------------   ENEMY IDLE STATE - END  ----------------------------------------------- //


    // -----------------------------------------------   ENEMY PATROL STATE - START  ----------------------------------------------- //


    // Transition to Patrolling state, choose random patrol point, and start pathfinding.
    public void EnterPatrollingState()
    {
        if (isPatrolling || !isRoomActive) return;
  
        // Start patrolling immediately if the room is active and enemy isn't already patrolling        

        isPatrolling = true;

        // Choose a random patrol point on the floor tiles
        Vector2Int randomPatrolPoint = GetRandomPatrolPoint();
        Vector2Int startPosition = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        // Get the path using the pathfinding algorithm
        List<Vector2Int> path = pathfinding.FindPath(startPosition, randomPatrolPoint);

        // Ensure we got a valid path
        if (path == null || path.Count == 0)
        {            
            ChangeState(EnemyState.Idle);
            return;
        }

        patrolPath = path;        

        currentPatrolPointIndex = 0;

        // Start patrol coroutine
        StartCoroutine(PatrolAlongPath(patrolPath));
    }

    // Move the enemy along the patrol path, update direction when necessary, and stop at each patrol point.
    private IEnumerator PatrolAlongPath(List<Vector2Int> path)
    {
        while (currentPatrolPointIndex < path.Count && isRoomActive)
        {
            Vector3 targetPosition = new Vector3(path[currentPatrolPointIndex].x + 0.5f, path[currentPatrolPointIndex].y + 0.25f, transform.position.z);
            Vector3 startPosition = transform.position;

            Vector2 directionToTarget = (targetPosition - startPosition).normalized;
            if (Mathf.Abs(directionToTarget.x) > 0.3f)
            {
                int desiredDirection = directionToTarget.x > 0 ? -1 : 1;
                if (facingDirection != desiredDirection)
                {
                    Flip();
                }
            }

            // Move towards the target point
            while (Vector2.Distance(transform.position, targetPosition) > 0.1f && isRoomActive)
            {
                Vector2 direction = (targetPosition - transform.position).normalized;
                rb.velocity = direction * patrolSpeed;                
                yield return null;
            }

            // Stop movement when close to target point
            rb.velocity = Vector2.zero;            
            currentPatrolPointIndex++;           
        }

        // End patrolling and switch to idle state
        isPatrolling = false;
        
        if (isRoomActive)
        {
            ChangeState(EnemyState.Idle);            
        }
    }

    // Return a random patrol point from the available floor tiles.
    private Vector2Int GetRandomPatrolPoint()
    {
        List<Vector2Int> floorTilesList = new List<Vector2Int>(allFloorTiles);
        // Ensure we get a random point from the floor tiles
        return floorTilesList[UnityEngine.Random.Range(0, floorTilesList.Count)];
    }


    // -----------------------------------------------   ENEMY PATROL STATE - END  ----------------------------------------------- //


    // -----------------------------------------------   ENEMY CHASING STATE - START  ----------------------------------------------- //


    // Move the enemy towards the player in Chasing state.
    private void HandleChasingState()
    {
        if (!isRoomActive || player == null) return;

        // Check if need to update chasing path
        bool shouldUpdatePath = false;

        // if no path or over the path update interval
        if (chasePath == null || chasePath.Count == 0 ||
        Time.time - lastChasePathUpdate > pathUpdateInterval)
        {
            shouldUpdatePath = true;
        }

        // if current path is finished
        if (chasePath != null && currentChasePointIndex >= chasePath.Count)
        {
            shouldUpdatePath = true;
        }

        if (shouldUpdatePath)
        {
            UpdateChasePath();
        }

        // Chase follow path or backup plan (direct chasing)
        if (chasePath != null && chasePath.Count > 0 && currentChasePointIndex < chasePath.Count)
        {
            ChaseAlongPath();
        }
        else
        {            
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * chaseSpeed;
            HandleChaseFlip(direction);
        }
    }

    // Recalculate and update the chase path to follow the player.
    private void UpdateChasePath()
    {
        if (pathfinding == null) return;

        Vector2Int enemyPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
        Vector2Int playerPos = new Vector2Int(Mathf.FloorToInt(player.position.x), Mathf.FloorToInt(player.position.y));

        List<Vector2Int> newPath = pathfinding.FindPath(enemyPos, playerPos);

        if (newPath != null && newPath.Count > 0)
        {
            chasePath = newPath;
            currentChasePointIndex = 0;
            lastChasePathUpdate = Time.time;
        }
        else
        {            
            chasePath = null;
        }
    }

    // Move the enemy along the chase path and update the current chase point.
    private void ChaseAlongPath()
    {
        Vector3 targetPosition = new Vector3(
            chasePath[currentChasePointIndex].x + 0.5f,
            chasePath[currentChasePointIndex].y + 0.25f,
            transform.position.z
        );

        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * chaseSpeed;        

        HandleChaseFlip(direction);
        
        if (Vector2.Distance(transform.position, targetPosition) < chasePathThreshold)
        {
            currentChasePointIndex++;
        }
    }

    // Flip the enemy’s facing direction based on the chase movement.
    private void HandleChaseFlip(Vector2 moveDirection)
    {
        if (Mathf.Abs(moveDirection.x) > 0.1f)
        {
            if (player.position.x > transform.position.x && facingDirection == 1 ||
                player.position.x < transform.position.x && facingDirection == -1)
            {
                Flip();
            }
        }
    }


    // -----------------------------------------------   ENEMY CHASING STATE - END  ----------------------------------------------- //


    // -----------------------------------------------   ENEMY ATTACK STATE - START  ----------------------------------------------- //


    // Stop movement and set the attack animation when in Attacking state.
    private void HandleAttackingState()
    {
        if (!isRoomActive) return;
        rb.velocity = Vector2.zero;
    }


    // -----------------------------------------------   ENEMY ATTACK STATE - END  ----------------------------------------------- //


    // -----------------------------------------------   ENEMY DEAD STATE - START  ----------------------------------------------- //


    // Handle the enemy’s behavior when in Dead state
    private void HandleDeadState()
    {

    }


    // -----------------------------------------------   ENEMY DEAD STATE - END  ----------------------------------------------- //


    // Detect the player and change state to Attacking if in range, or Chasing if out of range.
    private void CheckForPlayer()
    {
        if (!isRoomActive) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);

        if (hits.Length > 0)
        {            
            player = hits[0].transform;

            if (Vector2.Distance(transform.position, player.transform.position) <= attackRange && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = attackCooldown;
                ChangeState(EnemyState.Attacking);
            }
            else if (Vector2.Distance(transform.position, player.position) > attackRange && currentState != EnemyState.Attacking && currentState != EnemyState.Chasing && currentState != EnemyState.Inactive)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            if (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking)
            {                
                rb.velocity = Vector2.zero;
                ChangeState(EnemyState.Idle);
            }
        }
    }

    // Change the enemy’s state, stop previous actions, and start corresponding animations.
    public void ChangeState(EnemyState newState)
    {
        // Don't change state if room is inactive unless it's to RoomInactive state
        if (!isRoomActive && newState != EnemyState.Inactive) return;

        // Clear previous state animations
        if (currentState == EnemyState.Idle)
        {
            //Debug.Log("ChangeState: Exit Idle");
            anim.SetBool("isIdle", false);
        }
        else if (currentState == EnemyState.Chasing)
        {
            anim.SetBool("isMoving", false);
            chasePath = null;
            currentChasePointIndex = 0;
            isChasing = false;
        }
        else if (currentState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", false);
        }
        else if (currentState == EnemyState.Patrolling)
        {
            anim.SetBool("isMoving", false);
            if (patrolPath != null) patrolPath.Clear();
            currentPatrolPointIndex = 0;
            StopAllCoroutines();
            isPatrolling = false;
        }

        currentState = newState;

        // Set new state animations
        if (currentState == EnemyState.Idle)
        {            
            anim.SetBool("isIdle", true);
        }
        else if (currentState == EnemyState.Chasing)
        {
            anim.SetBool("isMoving", true);
            isChasing = true;
            UpdateChasePath();
        }
        else if (currentState == EnemyState.Attacking)
        {
            anim.SetBool("isAttacking", true);
        }
        else if (currentState == EnemyState.Patrolling)
        {
            anim.SetBool("isMoving", true);
            EnterPatrollingState();
        }
    }


    // -----------------------------------------------   ROOM MANAGER PUBLIC METHOD  ----------------------------------------------- //

    // Set the index of the room the enemy is currently in.
    public void SetRoomIndex(int roomIndex)
    {
        myRoomIndex = roomIndex;
    }

    // Return the current room index.
    public int GetRoomIndex()
    {
        return myRoomIndex;
    }

    // Set the room’s active status and transition the enemy state if necessary.
    public void SetRoomActive(bool active)
    {
        if (isRoomActive == active) return;

        if (active)
        {
            // Room becomes active, resume previous state or idle state if no previous state was saved
            isRoomActive = true;

            if (currentState == EnemyState.Inactive)
            {
                ChangeState(stateBeforeInactive != EnemyState.Inactive ? stateBeforeInactive : EnemyState.Idle);
            }
        }
        else
        {
            // Room becomes inactive, stop patrol and move to inactive state
            isRoomActive = false;
            StopAllCoroutines();
            isPatrolling = false;
            rb.velocity = Vector2.zero;
            ChangeState(EnemyState.Inactive);
        }
    }

    // Return whether the room is active.
    public bool IsRoomActive()
    {
        return isRoomActive;
    }

    // Draw gizmos for visualizing the detection range and patrol path in the editor.
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

        if (chasePath != null && chasePath.Count > 0)
        {
            Gizmos.color = Color.red;
            for (int i = currentChasePointIndex; i < chasePath.Count - 1; i++)
            {
                Vector3 start = new Vector3(chasePath[i].x + 0.5f, chasePath[i].y + 0.25f, 0);
                Vector3 end = new Vector3(chasePath[i + 1].x + 0.5f, chasePath[i + 1].y + 0.25f, 0);
                Gizmos.DrawLine(start, end);
            }
            
            if (currentChasePointIndex < chasePath.Count)
            {
                Vector3 currentTarget = new Vector3(
                    chasePath[currentChasePointIndex].x + 0.5f,
                    chasePath[currentChasePointIndex].y + 0.25f,
                    0
                );
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(currentTarget, 0.2f);
            }
        }

        // Draw room activity status
        Gizmos.color = isRoomActive ? Color.green : Color.gray;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}