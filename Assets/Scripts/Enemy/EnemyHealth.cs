using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyFSM fsm;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Base Health Stats")]
    public int baseMaxHealth = 3;

    private int scaledMaxHealth;
    public int currentHealth;

    //public int maxHealth;

    private bool isStunned;
    public float damagedStunTime = 5f;    
   
    void Start()
    {
        // Apply difficulty scaling to health
        if (DifficultyManager.Instance != null)
        {
            scaledMaxHealth = DifficultyManager.Instance.GetScaledEnemyHealth(baseMaxHealth);

            // Subscribe to difficulty changes
            DifficultyManager.OnDifficultyChanged += OnDifficultyChanged;
        }
        else
        {
            scaledMaxHealth = baseMaxHealth;
        }

        currentHealth = scaledMaxHealth;
        fsm = GetComponent<EnemyFSM>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (DifficultyManager.Instance != null && DifficultyManager.Instance.showDebugInfo)
        {
            Debug.Log($"Enemy {gameObject.name} spawned with {scaledMaxHealth} health (base: {baseMaxHealth})");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
        }
    }

    private void OnDifficultyChanged(float newMultiplier)
    {
        // Update scaled health when difficulty changes (for newly spawned enemies)
        int newScaledMaxHealth = DifficultyManager.Instance.GetScaledEnemyHealth(baseMaxHealth);

        // Scale current health proportionally
        float healthRatio = (float)currentHealth / scaledMaxHealth;
        scaledMaxHealth = newScaledMaxHealth;
        currentHealth = Mathf.RoundToInt(scaledMaxHealth * healthRatio);

        if (DifficultyManager.Instance.showDebugInfo)
        {
            Debug.Log($"Enemy {gameObject.name} health updated: {currentHealth}/{scaledMaxHealth}");
        }
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
            OnEnemyDefeated();
            Destroy(gameObject);
        }
    }

    private void OnEnemyDefeated()
    {
        // Notify difficulty manager that player has made progress
        if (DifficultyManager.Instance != null)
        {
            // You can adjust how much progress each enemy gives
            // For example, stronger enemies or bosses could give more progress
            int progressAmount = GetProgressValue();
            DifficultyManager.Instance.IncreaseProgress(progressAmount);
        }
    }

    private int GetProgressValue()
    {
        // Base progress value based on enemy health
        // Stronger enemies (higher base health) give more progress
        if (baseMaxHealth >= 5)
        {
            return 2; // Strong enemy
        }
        else if (baseMaxHealth >= 3)
        {
            return 1; // Normal enemy
        }
        else
        {
            return 1; // Weak enemy, but still gives progress
        }
    }

    IEnumerator StunTimer(float time)
    {        
        isStunned = true;
        fsm.ChangeState(EnemyState.Inactive);        

        yield return new WaitForSeconds(time);
        
        isStunned = false;
        fsm.ChangeState(EnemyState.Idle);
    }

    // Getters for other systems
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return scaledMaxHealth;
    }

    public int GetBaseMaxHealth()
    {
        return baseMaxHealth;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / scaledMaxHealth;
    }
}
