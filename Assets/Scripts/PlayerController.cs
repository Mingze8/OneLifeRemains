using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5;    
    public int facingDirection = -1;
    public Rigidbody2D rb;

    public Animator anim;

    [Header("Weapon")]
    public Sprite meleeWeapon;
    public Sprite bowWeapon;

    private SpriteRenderer weaponRenderer;
    private Sprite currentWeaponSprite;

    private void Start()
    {
        // Get the SpriteRenderer component of the R_Weapon object
        weaponRenderer = GameObject.Find("R_Weapon").GetComponent<SpriteRenderer>();

        // Set the default weapon sprite
        currentWeaponSprite = meleeWeapon;
        weaponRenderer.sprite = currentWeaponSprite;
    }
    
    void Update()
    {
        float horizontal = Input.GetButton("Horizontal") ? (Input.GetKey(KeyCode.A) ? -1 : (Input.GetKey(KeyCode.D) ? 1 : 0)) : 0;
        float vertical = Input.GetButton("Vertical") ? (Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? -1 : 0)) : 0;

        if (horizontal > 0 && facingDirection < 0 ||
            horizontal < 0 && facingDirection > 0)
        {
            Flip();
        }

        Vector2 movement = new Vector2(horizontal, vertical);
        movement.Normalize();

        anim.SetFloat("Speed", movement.magnitude);
        rb.velocity = movement * speed;

        // Weapon Switching
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(meleeWeapon);
            Debug.Log("Switch: 1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(bowWeapon);
            Debug.Log("Switch: 2");
        }

        // Combat Input
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (currentWeaponSprite == meleeWeapon)
            {
                anim.SetTrigger("AttackMelee");
            }
            else if (currentWeaponSprite == bowWeapon) 
            {
                anim.SetTrigger("AttackRanged");
            }
        }
    }   

    void SwitchWeapon(Sprite newWeapon)
    {
        // Set the new sprite to the weapon renderer
        currentWeaponSprite = newWeapon;
        weaponRenderer.sprite = currentWeaponSprite;
    }
    
    void Flip()
    {
        facingDirection *= -1;
        transform.localScale = new Vector3 (transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }
}