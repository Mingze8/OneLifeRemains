using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Potion", menuName = "Potions/DamagePotion")]
public class DamagePotionSO : PotionSO
{
    public float damageIncreasePercentage;
    public float duration;

    public override void UsePotion()
    {
        Debug.Log("Used Damage Potion");
    }
}
