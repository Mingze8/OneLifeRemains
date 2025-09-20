using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    public float MaxMana = 100f;
    public float CurrentMana { get; private set; }
    public float ManaRegenRate = 1f;  // How fast mana regenerates

    void Start()
    {
        CurrentMana = MaxMana;  // Initialize mana to max value at the start
    }

    void Update()
    {
        // Regenerate mana over time
        if (CurrentMana < MaxMana)
        {
            CurrentMana += ManaRegenRate * Time.deltaTime;
            if (CurrentMana > MaxMana) CurrentMana = MaxMana;
        }
    }

    public void UseMana(float amount)
    {
        // Spend mana if there is enough
        if (CurrentMana >= amount)
        {
            CurrentMana -= amount;
        }
        else
        {
            Debug.Log("Not enough mana.");
        }
    }
}