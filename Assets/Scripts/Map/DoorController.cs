using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorController : MonoBehaviour
{
    private Collider2D doorCollider;
    private Animator doorAnimator;
    private SpriteRenderer doorSpriteRenderer;

    private void Start()
    {
        doorCollider = GetComponent<Collider2D>();    
        doorAnimator = GetComponent<Animator>();
        doorSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            doorCollider.enabled = false;            

            if (doorSpriteRenderer.sprite.name.Contains("Dungeon_Tileset_36"))
            {
                doorAnimator.SetTrigger("OpenDoor");
            }
            else
            {
                doorAnimator.SetTrigger("OpenVerticalDoor");
            }
            
        }
    }
}
