using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public LootSO lootSO;
    public SpriteRenderer sr;
    public Animator anim;

    public bool canBePickedUp = true;
    public int quantity;
    public static event Action<LootSO, int> OnItemLooted;

    private void OnValidate()
    {
        if (lootSO == null)
            return;

        UpdateAppearance();
    }

    public void Initialize(LootSO lootSO, int quantity)
    {
        this.lootSO = lootSO;
        this.quantity = quantity;
        canBePickedUp = false;
        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        sr.sprite = lootSO.icon;
        this.name = lootSO.name;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canBePickedUp == true)
        {
            {
                anim.Play("LootPickup");
                OnItemLooted?.Invoke(lootSO, quantity);
                Destroy(gameObject, .5f);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canBePickedUp = true;
        }
    }
}
