using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float initialSpeed = 5;
    private float speed = 5;
    public int facingDirection = 1;
    public Rigidbody2D rb;

    public Animator anim;
    private bool isStunned;

    private void Start()
    {
        
    }

    void Update()
    {
        if (!isStunned)
        {
            HandleMovement();
            handleFlipCharacter();
            handleCombat();
        }        
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

    void handleFlipCharacter()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

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

        //// Combat Input
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    if (currentWeapon.CompareTag("Melee"))
        //    {
        //        anim.SetTrigger("AttackMelee");
        //        speed = 3;
        //    }
        //    else if (currentWeapon.CompareTag("Ranged"))
        //    {
        //        anim.SetTrigger("AttackRanged");
        //        speed = 3;
        //    }
        //}
    }

    //void SwitchWeapon(GameObject newWeapon)
    //{
    //    // Set the new sprite to the weapon renderer
    //    Destroy(currentWeapon);
    //    currentWeapon = Instantiate(newWeapon, weaponParent);
    //    currentWeapon.SetActive(true);
    //}

    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public void Stunned(float stunTime)
    {
        isStunned = true;        
        StartCoroutine(stunCounter(stunTime));

        IEnumerator stunCounter(float stunTime)
        {
            anim.SetBool("isStunned", true);
            yield return new WaitForSeconds(stunTime);
            anim.SetBool("isStunned", false);
            isStunned = false;            
        }
    }
}