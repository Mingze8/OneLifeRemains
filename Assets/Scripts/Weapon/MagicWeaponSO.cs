using UnityEngine;

[CreateAssetMenu(fileName = "NewMagicWeapon", menuName = "Weapons/MagicWeapon")]
public class MagicWeaponSO : WeaponSO
{
    [Header("Magic Weapon Stats")]
    public int manaCost;
    public float spellCooldown;    

    public GameObject magicProjectilePrefab;
    public float spellRange = 3f;

    public override void UseWeapon(GameObject player, Animator animator, Transform attackPoint, Vector2 direction)
    {
        if (player.GetComponent<PlayerMana>().CurrentMana >= manaCost)
        {
            // Trigger the magic spell animation
            animator.SetTrigger("AttackMagic");

            // Cast the magic projectile or effect
            CastMagicSpell(player, attackPoint, direction);

            // Spend mana for the spell
            player.GetComponent<PlayerMana>().UseMana(manaCost);
        }
        else
        {
            Debug.Log("Not enough mana");
        }
        
    }

    private void CastMagicSpell(GameObject player, Transform attackPoint, Vector2 direction)
    {
        // Instantiate the magic projectile at the attack point
        GameObject magicProjectile = Instantiate(magicProjectilePrefab, attackPoint.position, Quaternion.identity);

        // Get the Rigidbody2D component of the projectile
        Rigidbody2D rb = magicProjectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Set the projectile's velocity towards the direction
            rb.velocity = direction * 5f;  // Use the spell range to set the projectile speed
        }

        magicProjectile.GetComponent<MagicProjectile>().Initialize(direction, spellRange);

        Debug.Log("Magic spell casted!");
    }    
}