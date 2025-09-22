using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{    
    public LootSO lootSO;
    public int quantity;

    public Image itemImage;
    public TMP_Text quantityText;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) 
        {
            inventoryManager.DropItem(this);
        }
    }

    public void UpdateUI()
    {
        if (lootSO != null)
        {
            itemImage.sprite = lootSO.icon;
            itemImage.gameObject.SetActive(true);
            quantityText.text = quantity.ToString();
        }
        else
        {
            itemImage.gameObject.SetActive(false);
            quantityText.text = "";
        }
    }

    // Method to use the item
    public void UseItem()
    {
        if (lootSO != null)
        {
            Debug.Log("Using item: " + lootSO.lootName);
            // Call the Use method of the item
            lootSO.Use();
            quantity--;
            if (quantity <= 0)
            {
                lootSO = null; // Remove item if quantity is 0
                quantity = 0;
            }
            UpdateUI(); // Update the UI after using the item
        }
    }
}
