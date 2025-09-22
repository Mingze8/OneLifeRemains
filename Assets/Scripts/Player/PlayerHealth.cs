using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public int currentHealth;
    public int maxHealth;

    private TMP_Text healthText;

    private void Start()
    {
        healthText = GameObject.Find("AmountText").GetComponent<TMP_Text>();
        UpdateHealthUI();
    }
    public void changeHealth(int amount)
    {
        currentHealth += amount;
        
        if(currentHealth <= 0)
        {
            currentHealth = 0;
            gameObject.SetActive(false);
        }
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        healthText.text = currentHealth + " / " + maxHealth;
    }
}
