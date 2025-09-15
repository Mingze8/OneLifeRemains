using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float initialSpeed = 5;
    private float speed = 5;
    public int facingDirection = 1;
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
        HandleMovement();
        HandleWeaponDirection();
        handleCombat();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal, vertical);
        movement.Normalize();
        anim.SetFloat("Speed", movement.magnitude);
        rb.velocity = movement * speed;
    }

    void HandleWeaponDirection()
    {
        // Get mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate direction from player to mouse
        Vector2 direction = (mousePos - transform.position).normalized;

        // Rotate weapon to face the mouse direction
        weaponRenderer.transform.up = direction;

        flipCharacter(mousePos);
    }

    void flipCharacter(Vector3 mousePos)
    {
        // Check if the mouse is to the left or right of the player
        if (mousePos.x < transform.position.x && facingDirection == 1)  // Mouse is left
        {
            Flip();
        }
        else if (mousePos.x > transform.position.x && facingDirection == -1)  // Mouse is right
        {
            Flip();
        }
    }

    void Flip()
    {
        facingDirection *= -1; // Update the facing direction
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void handleCombat()
    {
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
                speed = 3;
            }
            else if (currentWeaponSprite == bowWeapon)
            {
                anim.SetTrigger("AttackRanged");
                speed = 3;
            }
        }
    }

    void SwitchWeapon(Sprite newWeapon)
    {
        // Set the new sprite to the weapon renderer
        currentWeaponSprite = newWeapon;
        weaponRenderer.sprite = currentWeaponSprite;
    }

    public void ResetSpeed()
    {
        speed = initialSpeed;
    }
}