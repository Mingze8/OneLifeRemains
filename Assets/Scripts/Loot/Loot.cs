using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Loot : MonoBehaviour
{
    public LootSO lootSO;
    public SpriteRenderer sr;
    public Animator anim;   

    public bool canBePickedUp = true;
    public int quantity;
    public static event Action<LootSO, int> OnItemLooted;
    public static event System.Action<LootSO> OnRareItemLooted;

    public static event Action<LootSO, int> OnItemPurchased;

    public KeyCode purchaseKey = KeyCode.Z;
    private Image interactionPromptImage;

    private bool playerInRange = false;
    private GameObject playerObject;
    private InventoryManager inventoryManager;
    private RoomManager roomManager;
    private TMP_Text statsText;

    private bool IsShopItem
    {
        get
        {            
            if (roomManager != null && roomManager.IsPlayerInShopRoom())
            {
                return true;
            }

            return false;
        }
    }

    private void Start()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
        roomManager = FindObjectOfType<RoomManager>();
        statsText = FindObjectOfType<TMP_Text>();

        interactionPromptImage = GameObject.Find("InteractionPrompt").GetComponent<Image>();
        if (interactionPromptImage != null)
        {
            interactionPromptImage.enabled = false; // Ensure it's initially hidden
        }
        else
        {
            Debug.LogError("InteractionPromptUI not found in the scene!");
        }
    }

    private void UpdateWeaponStats(WeaponSO weapon)
    {
        // Format the stats string for the weapon
        string stats = $"Name: {weapon.weaponName}\n" +
                       $"Attack Power: {weapon.baseAttackPower}\n" +
                       $"Attack Speed: {weapon.attackSpeed}\n" +
                       $"Crit Chance: {weapon.critChance * 100}%\n" +
                       $"Crit Damage: {weapon.critDamage * 100}%";

        // Update the UI text to display these stats
        statsText.text = stats;
    }

    private void UpdatePotionStats(PotionSO potion)
    {
        string stats = $"Name: {potion.potionName}";
        
        statsText.text = stats;
    }

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

    private void Update()
    {
        if (IsShopItem && playerInRange && Input.GetKeyDown(purchaseKey))
        {
            AttemptPurchase();
        }
    }

    private void AttemptPurchase()
    {
        int price = GetItemPrice();
        Debug.Log($"Attempting to purchase {lootSO.lootName}");
        Debug.Log($"Player coins: {inventoryManager.coin}, Item price: {price}");

        // Check for player coins
        if (inventoryManager.coin >= price)
        {
            inventoryManager.DeductCoins(price);
            Debug.Log($"Purchase successful! Paid {price} coins");

            AddItemToInventory(playerObject.GetComponent<Collider2D>());
            
            OnItemPurchased?.Invoke(lootSO, quantity);

            anim.Play("LootPickup");
            Destroy(gameObject, .5f);
        }
        else
        {
            Debug.Log($"Not enough coins! Need: {price}, Have: {inventoryManager.coin}");
        }
    }

    private void HandleRegularLoot(Collider2D collision)
    {
        anim.Play("LootPickup");

        AddItemToInventory(collision);

        // Check if it's a rare item
        if (lootSO.weapon != null && IsRareItem())
        {
            OnRareItemLooted?.Invoke(lootSO);
        }

        OnItemLooted?.Invoke(lootSO, quantity);
        Destroy(gameObject, .5f);
    }

    private void AddItemToInventory(Collider2D collision)
    {
        if (lootSO.weapon != null)
        {
            var weaponInventory = collision.GetComponent<WeaponInventory>();
            if (weaponInventory != null)
            {
                weaponInventory.AddWeapon(lootSO.weapon);
            }
        }
        if (lootSO.potion != null)
        {
            OnItemLooted?.Invoke(lootSO, 1);
        }        
    }

    private bool IsRareItem()
    {
        return lootSO.weapon.weaponRarity == Rarity.Rare ||
               lootSO.weapon.weaponRarity == Rarity.Epic ||
               lootSO.weapon.weaponRarity == Rarity.Legendary;
    }

    private int GetItemPrice()
    {
        if (lootSO.weapon != null)
        {
            return lootSO.weapon.GetPrice();
        }        
        else if (lootSO.potion != null)
        {
            return lootSO.potion.GetPrice();
        }

        return 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerObject = collision.gameObject;
            playerInRange = true;

            if (IsShopItem)
            {                
                int price = GetItemPrice();
                Debug.Log($"Press {purchaseKey} to buy {lootSO.lootName} for {price} coins");
                
                if (interactionPromptImage != null)
                {
                    interactionPromptImage.enabled = true; // Show the image (make it visible)
                }
            }
            else if (canBePickedUp)
            {                
                HandleRegularLoot(collision);
            }

            if (lootSO.lootType == LootType.Weapon && lootSO.weapon != null)
            {
                UpdateWeaponStats(lootSO.weapon);  // Update stats for weapon
            }
            else if (lootSO.lootType == LootType.Potion && lootSO.potion != null)
            {
                UpdatePotionStats(lootSO.potion);  // Update stats for potion
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            playerObject = null;

            if (IsShopItem)
            {
                if (interactionPromptImage != null)
                {
                    interactionPromptImage.enabled = false; // Hide the image (make it invisible)
                }
                Debug.Log("Left shop area");
            }
            else
            {
                // 只有非商店物品才能在退出后被拾取
                canBePickedUp = true;
            }

            statsText.text = string.Empty;
        }
    }
}
