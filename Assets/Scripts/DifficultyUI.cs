using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to show current difficulty multiplier")]
    public TextMeshProUGUI difficultyText;

    [Tooltip("Text component to show current progress level")]
    public TextMeshProUGUI progressText;

    [Tooltip("Slider to show progress bar")]
    public Slider progressSlider;

    [Tooltip("Image component to change difficulty indicator color")]
    public Image difficultyIndicator;

    [Header("Display Settings")]
    [Tooltip("Show difficulty as percentage (150%) or multiplier (1.5x)")]
    public bool showAsPercentage = false;

    [Tooltip("Number of decimal places to show")]
    [Range(0, 3)]
    public int decimalPlaces = 1;

    [Header("Color Coding")]
    public Color easyColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color hardColor = Color.cyan;
    public Color extremeColor = Color.red;

    [Header("Animation")]
    public bool animateChanges = true;
    public float animationSpeed = 2f;

    private float targetSliderValue = 0f;
    private Color targetIndicatorColor = Color.white;
    
    private void Start()
    {
        // Subscribe to difficulty manager events
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.OnDifficultyChanged += OnDifficultyChanged;
            DifficultyManager.OnProgressLevelChanged += OnProgressLevelChanged;

            // Initialize UI with current values
            UpdateDifficultyDisplay(DifficultyManager.Instance.GetDifficultyMultiplier());
            UpdateProgressDisplay(DifficultyManager.Instance.GetCurrentProgressLevel());
        }
        else
        {
            Debug.LogWarning("DifficultyManager not found! UI will not update.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.OnDifficultyChanged -= OnDifficultyChanged;
            DifficultyManager.OnProgressLevelChanged -= OnProgressLevelChanged;
        }
    }

    private void Update()
    {
        // Handle smooth animations
        if (animateChanges && progressSlider != null)
        {
            progressSlider.value = Mathf.MoveTowards(progressSlider.value, targetSliderValue, animationSpeed * Time.deltaTime);
        }

        if (animateChanges && difficultyIndicator != null)
        {
            difficultyIndicator.color = Color.Lerp(difficultyIndicator.color, targetIndicatorColor, animationSpeed * Time.deltaTime);
        }
    }

    private void OnDifficultyChanged(float newMultiplier)
    {
        UpdateDifficultyDisplay(newMultiplier);
    }

    private void OnProgressLevelChanged(int newProgressLevel)
    {
        UpdateProgressDisplay(newProgressLevel);
    }

    private void UpdateDifficultyDisplay(float multiplier)
    {
        // Update difficulty text
        if (difficultyText != null)
        {
            string format = "F" + decimalPlaces;

            if (showAsPercentage)
            {
                float percentage = multiplier * 100f;
                difficultyText.text = $"Difficulty: {percentage.ToString(format)}%";
            }
            else
            {
                difficultyText.text = $"Difficulty: {multiplier.ToString(format)}x";
            }
        }

        // Update difficulty indicator color
        UpdateDifficultyColor(multiplier);
    }

    private void UpdateProgressDisplay(int progressLevel)
    {
        if (DifficultyManager.Instance == null) return;

        int maxProgress = DifficultyManager.Instance.GetMaxProgressLevel();
        float progressRatio = (float)progressLevel / maxProgress;

        // Update progress text
        if (progressText != null)
        {
            progressText.text = $"Progress: {progressLevel}/{maxProgress}";
        }

        // Update progress slider
        if (progressSlider != null)
        {
            progressSlider.maxValue = maxProgress;

            if (animateChanges)
            {
                targetSliderValue = progressLevel;
            }
            else
            {
                progressSlider.value = progressLevel;
            }
        }
    }

    private void UpdateDifficultyColor(float multiplier)
    {
        if (difficultyIndicator == null) return;

        Color newColor;

        // Determine color based on difficulty multiplier
        if (multiplier <= 1.2f)
        {
            newColor = easyColor;
        }
        else if (multiplier <= 1.5f)
        {
            newColor = mediumColor;
        }
        else if (multiplier <= 1.8f)
        {
            newColor = hardColor;
        }
        else
        {
            newColor = extremeColor;
        }

        if (animateChanges)
        {
            targetIndicatorColor = newColor;
        }
        else
        {
            difficultyIndicator.color = newColor;
        }
    }

    public void RefreshDisplay()
    {
        if (DifficultyManager.Instance != null)
        {
            UpdateDifficultyDisplay(DifficultyManager.Instance.GetDifficultyMultiplier());
            UpdateProgressDisplay(DifficultyManager.Instance.GetCurrentProgressLevel());
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public string GetDetailedInfo()
    {
        if (DifficultyManager.Instance == null)
            return "Difficulty system not available";

        float multiplier = DifficultyManager.Instance.GetDifficultyMultiplier();
        int progress = DifficultyManager.Instance.GetCurrentProgressLevel();
        int maxProgress = DifficultyManager.Instance.GetMaxProgressLevel();

        return $"Difficulty Multiplier: {multiplier:F2}x\n" +
               $"Progress: {progress}/{maxProgress}\n" +
               $"Enemy Damage: +{((multiplier - 1) * 100):F0}%\n" +
               $"Enemy Health: Scaled\n" +
               $"Rare Loot Chance: +{((multiplier - 1) * 100):F0}%";
    }
}
