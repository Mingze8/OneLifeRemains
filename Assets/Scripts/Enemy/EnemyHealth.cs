using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyFSM fsm;
    private Animator anim;
    private Rigidbody2D rb;

    public int currentHealth;
    public int maxHealth;

    private bool isStunned;
    public float damagedStunTime = 5f;    

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        fsm = GetComponent<EnemyFSM>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int amount)
    {
        anim.SetTrigger("isDamaged");
        currentHealth -= amount;

        if (!isStunned)
        {            
            StartCoroutine(StunTimer(damagedStunTime));
        }        

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator StunTimer(float time)
    {        
        isStunned = true;
        fsm.ChangeState(EnemyState.Inactive);        

        yield return new WaitForSeconds(time);

        Debug.Log("Can Moving!");
        isStunned = false;
        fsm.ChangeState(EnemyState.Idle);
    }
}
