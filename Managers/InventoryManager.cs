using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlotData
{
    public ItemData item;
    public int quantity;

    public InventorySlotData(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public bool IsEmpty() { return item == null; }

    public void Clear()
    {
        item = null;
        quantity = 0;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }
}


public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Event Channel Listeners")]
    [SerializeField] private ItemEventChannel onItemGained;

    [Header("Event Channel Broadcasters")]
    [SerializeField] private VoidEC onInventoryUpdated;

    [Header("Inventory Config")]
    [SerializeField] private int inventorySize = 20;

    public List<InventorySlotData> inventorySlots = new List<InventorySlotData>();
    
    private void OnEnable()
    {
        if (onItemGained != null)
        {
            onItemGained.OnEventRaised += AddItem;
            Debug.Log("INVENTORY MANAGER: Subscribed to item gain events.");
        }
    }

    private void OnDisable()
    {
        if (onItemGained != null)
        {
            onItemGained.OnEventRaised -= AddItem;
            Debug.Log("INVENTORY MANAGER: Unsubscribed from item gain events.");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inventorySlots = new List<InventorySlotData>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
        {
            inventorySlots.Add(new InventorySlotData(null, 0));
        }
    }
    
    public void AddItem(ItemData item, int quantity)
    {
        if (item == null)
        {
            Debug.LogWarning("INVENTORY: Cannot add null item");
            return;
        }
        
        if (quantity <= 0)
        {
            Debug.LogWarning($"INVENTORY: Invalid quantity {quantity} for item {item.itemName}");
            return;
        }
        bool itemAdded = false;

        if (item.isStackable)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (!inventorySlots[i].IsEmpty() && 
                    string.Equals(inventorySlots[i].item.itemID, item.itemID, System.StringComparison.Ordinal))
                {
                    inventorySlots[i].AddQuantity(quantity);
                    itemAdded = true;
                    Debug.Log($"INVENTORY: Added {quantity}x {item.itemName} to existing stack in slot {i} (Total: {inventorySlots[i].quantity})");
                    break;
                }
            }
        }

        if (!itemAdded)
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].IsEmpty())
                {
                    inventorySlots[i] = new InventorySlotData(item, quantity);
                    itemAdded = true;
                    Debug.Log($"INVENTORY: Added {quantity}x {item.itemName} to new slot {i}");
                    break;
                }
            }
        }

        if (!itemAdded)
        {
            Debug.LogWarning($"INVENTORY: Failed to add {quantity}x {item.itemName}. Inventory full!");
        }

        if (onInventoryUpdated != null)
        {
            onInventoryUpdated.RaiseEvent();
        }
    }

    public void RemoveItem(ItemData item, int quantity)
    {
        if (item == null || quantity <= 0) return;

        int quantityRemainingToRemove = quantity;

        // Iterate through slots to find the item
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            // Skip empty slots
            if (inventorySlots[i].IsEmpty()) continue;

            // Check if this slot contains the item we want to remove
            // We check reference OR ID to be safe
            if (inventorySlots[i].item == item || string.Equals(inventorySlots[i].item.itemID, item.itemID, System.StringComparison.Ordinal))
            {
                int amountInSlot = inventorySlots[i].quantity;

                if (amountInSlot > quantityRemainingToRemove)
                {
                    // This slot has more than enough; just reduce and finish
                    inventorySlots[i].quantity -= quantityRemainingToRemove;
                    quantityRemainingToRemove = 0;
                }
                else
                {
                    // We need all of this slot (or exactly this amount)
                    quantityRemainingToRemove -= amountInSlot;
                    inventorySlots[i].Clear();
                }

                // If we have removed enough, stop looking
                if (quantityRemainingToRemove <= 0)
                {
                    break;
                }
            }
        }

        Debug.Log($"INVENTORY: Removed {quantity - quantityRemainingToRemove} of {item.itemName}");

        // --- BROADCAST UPDATE ---
        // This ensures the UI refreshes immediately
        if (onInventoryUpdated != null)
        {
            onInventoryUpdated.RaiseEvent();
        }
    }

    public void SwapItems(int slotA_Index, int slotB_Index)
    {
        if (!IsValidSlot(slotA_Index) || !IsValidSlot(slotB_Index) || slotA_Index == slotB_Index)
            return;

        InventorySlotData temp = inventorySlots[slotA_Index];
        inventorySlots[slotA_Index] = inventorySlots[slotB_Index];
        inventorySlots[slotB_Index] = temp;

        Debug.Log($"INVENTORY: Swapped items in slot {slotA_Index} and {slotB_Index}");
    }

    public void MoveItemToEmpty(int sourceIndex, int destinationIndex)
    {
        if (!IsValidSlot(sourceIndex) || !IsValidSlot(destinationIndex))
        {
            Debug.LogError($"INVENTORY: Invalid slot indices - source: {sourceIndex}, destination: {destinationIndex}");
            return;
        }
        
        if (inventorySlots[sourceIndex].IsEmpty())
        {
            Debug.LogWarning($"INVENTORY: Cannot move from empty slot {sourceIndex}");
            return;
        }
        
        if (!inventorySlots[destinationIndex].IsEmpty())
        {
            Debug.LogWarning($"INVENTORY: Destination slot {destinationIndex} is not empty");
            return;
        }
            
        inventorySlots[destinationIndex] = inventorySlots[sourceIndex];
        inventorySlots[sourceIndex] = new InventorySlotData(null, 0); 
        
        Debug.Log($"INVENTORY: Moved item from {sourceIndex} to {destinationIndex}");
    }

    private bool IsValidSlot(int index)
    {
        return index >= 0 && index < inventorySlots.Count;
    }

    public List<InventorySlotData> GetInventoryContents()
    {
        return inventorySlots;
    }
}