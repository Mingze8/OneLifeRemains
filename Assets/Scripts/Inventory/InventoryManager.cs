using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] itemSlots;
    public int coin;
    public TMP_Text coinText;
    public GameObject lootPrefab;
    public Transform player;

    private void Start()
    {
        foreach (var slot in itemSlots)
        {
            slot.UpdateUI();
        }

        // Subscribe to the item purchase event
        Loot.OnItemPurchased += PurchaseItem;
    }
    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;        
    }

    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
        Loot.OnItemPurchased -= PurchaseItem;
    }

    public void AddItem(LootSO lootSO, int quantity)
    {
        LootType type = lootSO.lootType;

        if(type == LootType.Weapon)
        {
            return;
        }

        if(type == LootType.Coin)
        {
            coin += quantity;
            coinText.text = coin.ToString();
            return;
        }
                
        // Stacking item
        foreach (var slot in itemSlots)
        {
            if (slot.lootSO == lootSO && slot.quantity < lootSO.stackSize)
            {
                int availableSpace = lootSO.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity -= amountToAdd;

                slot.UpdateUI();

                if (quantity <= 0)                
                    return;                
            }
        }

        foreach (var slot in itemSlots)
        {
            if (slot.lootSO == null)
            {
                int amountToAdd = Mathf.Min(lootSO.stackSize, quantity);
                slot.lootSO = lootSO;
                slot.quantity = quantity;
                slot.UpdateUI();
                return;
            }                
        }   
        
        if (quantity > 0)
        {
            DropLoot(lootSO, quantity);
        }
    }

    public void DropItem(InventorySlot slot)
    {
        DropLoot(slot.lootSO, 1);
        slot.quantity--;
        if(slot.quantity <= 0)
        {
            slot.lootSO = null;
        }
        slot.UpdateUI();
    }

    private void DropLoot(LootSO lootSO, int quantity)
    {
        Loot loot = Instantiate(lootPrefab, player.position, Quaternion.identity).GetComponent<Loot>();
        loot.Initialize(lootSO, quantity);
    }

    private void PurchaseItem(LootSO lootSO, int quantity)
    {
        // Check if lootSO is a valid item for the purchase
        if (lootSO != null && lootSO.weapon != null)
        {
            // Add the item to the inventory
            AddItem(lootSO, quantity);
            Debug.Log($"Purchased {lootSO.name} for {lootSO.weapon.GetPrice()} coins.");
        }
    }

    public void DeductCoins(int amount)
    {
        if (coin >= amount)
        {
            coin -= amount;
            coinText.text = coin.ToString();
        }
        else
        {
            Debug.Log("Not enough coins to deduct.");
        }
    }

    void Update()
    {

        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
        }

        // Check for key presses (1, 2, 3, 4) to use items
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseItem(0); // Use item at slot 0
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseItem(1); // Use item at slot 1
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            UseItem(2); // Use item at slot 2
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            UseItem(3); // Use item at slot 3
        }
    }

    void UseItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < itemSlots.Length)
        {
            itemSlots[slotIndex].UseItem(); // Use the item at the specified slot
        }
    }
}
