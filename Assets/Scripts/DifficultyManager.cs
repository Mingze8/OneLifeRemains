using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("Difficulty Settings")]
    [Range(1f, 3f)]
    public float maxDifficultyMultiplier = 3f;

    [Range(1, 50)]
    public int maxProgressLevel = 50; // Max progress level to reach full difficulty

    private int currentProgressLevel = 0;
    private float currentDifficultyMultiplier = 1f;

    [Header("Scaling Parameters")]

    [Tooltip("How enemy damage scales with difficulty")]
    public AnimationCurve enemyDamageScaling = AnimationCurve.Linear(0f, 1f, 1f, 2f);

    [Tooltip("How enemy health scales with difficulty")]
    public AnimationCurve enemyHealthScaling = AnimationCurve.Linear(0f, 1f, 1f, 2f);

    [Tooltip("How rare loot chance scales with difficulty")]
    public AnimationCurve rareLootChanceScaling = AnimationCurve.Linear(0f, 1f, 1f, 2f);

    [Tooltip("How enemy spawn count scales with difficulty")]
    public AnimationCurve enemySpawnScaling = AnimationCurve.Linear(0f, 1f, 1f, 1.3f);

    [Header("Debug")]
    public bool showDebugInfo = true;


    // Singleton pattern for easy access
    public static DifficultyManager Instance { get; private set; }

    // Events for difficulty changes
    public static event Action<float> OnDifficultyChanged;
    public static event Action<int> OnProgressLevelChanged;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateDifficultyMultiplier();
    }

    
    // -----------------------------------------------   PROGRESS MANAGEMENT PART - START  ----------------------------------------------- //


    // Increase progress level - when player completes rooms, defeats bosses, etc.)
    public void IncreaseProgress(int amount = 1)
    {
        int oldLevel = currentProgressLevel;
        currentProgressLevel = Mathf.Min(currentProgressLevel + amount, maxProgressLevel);

        if (oldLevel != currentProgressLevel)
        {
            UpdateDifficultyMultiplier();
            OnProgressLevelChanged?.Invoke(currentProgressLevel);

            if (showDebugInfo)
            {
                Debug.Log($"Progress Level: {currentProgressLevel}/{maxProgressLevel} | " +
                         $"Difficulty Multiplier: {currentDifficultyMultiplier:F2}x");
            }
        }
    }

    // Set progress level directly
    public void SetProgressLevel(int level)
    {
        int oldLevel = currentProgressLevel;
        currentProgressLevel = Mathf.Clamp(level, 0, maxProgressLevel);

        if (oldLevel != currentProgressLevel)
        {
            UpdateDifficultyMultiplier();
            OnProgressLevelChanged?.Invoke(currentProgressLevel);
        }
    }

    // Reset progress to beginning
    public void ResetProgress()
    {
        currentProgressLevel = 0;
        UpdateDifficultyMultiplier();
        OnProgressLevelChanged?.Invoke(currentProgressLevel);
    }


    // -----------------------------------------------   PROGRESS MANAGEMENT PART - END  ----------------------------------------------- //


    // -----------------------------------------------   DIFFICULTY CALCULATION PART - START  ----------------------------------------------- //

    private void UpdateDifficultyMultiplier()
    {
        float progressRatio = (float)currentProgressLevel / maxProgressLevel;
        float oldMultiplier = currentDifficultyMultiplier;

        currentDifficultyMultiplier = Mathf.Lerp(1f, maxDifficultyMultiplier, progressRatio);

        if (Math.Abs(oldMultiplier - currentDifficultyMultiplier) > 0.01f)
        {
            OnDifficultyChanged?.Invoke(currentDifficultyMultiplier);

            if (showDebugInfo)
            {
                Debug.Log($"Difficulty Updated: {currentDifficultyMultiplier:F2}x " +
                         $"(Progress: {currentProgressLevel}/{maxProgressLevel})");
            }
        }
    }

    // -----------------------------------------------   DIFFICULTY CALCULATION PART - END  ----------------------------------------------- //


    // -----------------------------------------------   SCALING GETTER PART - START  ----------------------------------------------- //

    // Get current base difficulty multiplier
    public float GetDifficultyMultiplier()
    {
        return currentDifficultyMultiplier;
    }

    // Get scaled enemy damage based on base damage
    public int GetScaledEnemyDamage(int baseDamage)
    {
        float progressRatio = (float)currentProgressLevel / maxProgressLevel;
        float damageMultiplier = enemyDamageScaling.Evaluate(progressRatio);
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    // Get scaled enemy health based on base health
    public int GetScaledEnemyHealth(int baseHealth)
    {
        float progressRatio = (float)currentProgressLevel / maxProgressLevel;
        float healthMultiplier = enemyHealthScaling.Evaluate(progressRatio);
        return Mathf.RoundToInt(baseHealth * healthMultiplier);
    }

    // Get scaled rare loot chance (for higher tier loot)
    public float GetScaledRareLootChance(float baseChance)
    {
        float progressRatio = (float)currentProgressLevel / maxProgressLevel;
        float lootMultiplier = rareLootChanceScaling.Evaluate(progressRatio);
        return Mathf.Clamp01(baseChance * lootMultiplier);
    }

    // Get scaled enemy spawn count
    public int GetScaledEnemySpawnCount(int baseMinCount, int baseMaxCount)
    {
        float progressRatio = (float)currentProgressLevel / maxProgressLevel;
        float spawnMultiplier = enemySpawnScaling.Evaluate(progressRatio);

        int scaledMin = Mathf.RoundToInt(baseMinCount * spawnMultiplier);
        int scaledMax = Mathf.RoundToInt(baseMaxCount * spawnMultiplier);

        return UnityEngine.Random.Range(scaledMin, scaledMax + 1);
    }

    // Get scaled loot chest spawn chance
    public float GetScaledLootChestChance(float baseChance)
    {
        float progressRatio = (float)currentProgressLevel / maxProgressLevel;
        // Slightly increase loot chest spawn chance with difficulty
        float chestMultiplier = Mathf.Lerp(1f, 1.3f, progressRatio);
        return Mathf.Clamp01(baseChance * chestMultiplier);
    }

    // -----------------------------------------------   SCALING GETTER PART - END  ----------------------------------------------- //


    // -----------------------------------------------   GETTER FOR CURRENT STATE PART - START  ----------------------------------------------- //

    public int GetCurrentProgressLevel()
    {
        return currentProgressLevel;
    }

    public int GetMaxProgressLevel()
    {
        return maxProgressLevel;
    }

    public float GetProgressRatio()
    {
        return (float)currentProgressLevel / maxProgressLevel;
    }

    // -----------------------------------------------   GETTER FOR CURRENT STATE PART - END  ----------------------------------------------- //


    // -----------------------------------------------   DEBUG PART - START  ----------------------------------------------- //

    [ContextMenu("Increase Progress")]
    private void DebugIncreaseProgress()
    {
        IncreaseProgress(1);
    }

    [ContextMenu("Reset Progress")]
    private void DebugResetProgress()
    {
        ResetProgress();
    }

    [ContextMenu("Set Max Progress")]
    private void DebugSetMaxProgress()
    {
        SetProgressLevel(maxProgressLevel);
    }

    private void OnValidate()
    {
        // Ensure curves have proper ranges in editor
        if (enemyDamageScaling.keys.Length == 0)
        {
            enemyDamageScaling = AnimationCurve.Linear(0f, 1f, 1f, 2f);
        }
        if (enemyHealthScaling.keys.Length == 0)
        {
            enemyHealthScaling = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);
        }
        if (rareLootChanceScaling.keys.Length == 0)
        {
            rareLootChanceScaling = AnimationCurve.Linear(0f, 1f, 1f, 2f);
        }
        if (enemySpawnScaling.keys.Length == 0)
        {
            enemySpawnScaling = AnimationCurve.Linear(0f, 1f, 1f, 1.3f);
        }

        // Update multiplier if values changed in editor during play
        if (Application.isPlaying)
        {
            UpdateDifficultyMultiplier();
        }
    }

    // -----------------------------------------------   DEBUG PART - END  ----------------------------------------------- //   
}
