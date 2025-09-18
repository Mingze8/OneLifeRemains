using UnityEngine;

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
    [Header("Idle Settings")]
    public float idleDuration = 3f;
    private float idleTimer;

    [Header("Walking Speed")]
    public float patrolSpeed;
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
        rb = GetComponent<Rigidbody2D>();  
        anim = GetComponent<Animator>();
        ChangeState(EnemyState.Idle);
    }

    private void Update()
    {
        CheckForPlayer();

        if (attackCooldownTimer > 0)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Patrolling:
                HandlePatrollingState();
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
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleDuration)
        {            
            ChangeState(EnemyState.Patrolling);
        }

        //if (Vector2.Distance(transform.position, player.transform.position) <= playerDetectRange)
        //{
        //    Debug.Log("Turn from Idle to Chasing");
        //    ChangeState(EnemyState.Chasing);
        //}
    }

    private void HandlePatrollingState()
    {
        
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

        if(hits.Length > 0)
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionPoint.position, playerDetectRange);
    }
}
