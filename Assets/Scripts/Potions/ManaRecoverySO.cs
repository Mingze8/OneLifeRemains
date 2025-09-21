using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Potion", menuName = "Potions/Mana Recovery Potion")]
public class ManaRecoverySO : PotionSO
{
    public int manaRecoveryAmount;

    public override void UsePotion()
    {
        Debug.Log("used mana potion");
    }
}
