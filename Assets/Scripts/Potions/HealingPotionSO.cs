using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Potion", menuName = "Potions/Healing Potion")]
public class HealingPotionSO : PotionSO
{
    public int healingAmount;

    public override void UsePotion()
    {
        Debug.Log("Used Healing Potion");
    }
}
