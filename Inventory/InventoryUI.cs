using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Event Channel Listener")]
    [SerializeField] private VoidEC onInventoryUpdated;

    [Header("UI Components")]
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Containers")]
    [Tooltip("The parent object for Hotbar slots (Slots 0-9)")]
    [SerializeField] private Transform hotbarContainer;

    [Tooltip("The parent object for main Inventory slots (Slots 10+)")]
    [SerializeField] private Transform inventoryContainer;

    // A list of all the UI slot components (Hotbar + Inventory combined)
    private List<InventorySlot> uiSlots = new List<InventorySlot>();

    private void Awake()
    {
        // 1. Clear the list to start fresh
        uiSlots.Clear();

        // 2. Find Hotbar slots FIRST (Indices 0 to X)
        if (hotbarContainer != null)
        {
            InventorySlot[] hotbarSlots = hotbarContainer.GetComponentsInChildren<InventorySlot>(true);
            foreach (var slot in hotbarSlots)
            {
                slot.slotID = uiSlots.Count; // Assign ID (e.g., 0, 1, 2...)
                uiSlots.Add(slot);
            }

            // --- CONNECT TO HOTBAR MANAGER ---
            // Send these specific slots to the HotbarManager so it knows what to highlight
            if (HotbarManager.instance != null)
            {
                HotbarManager.instance.hotbarSlots = new List<InventorySlot>(hotbarSlots);
            }
        }

        // 3. Find Main Inventory slots NEXT (Indices X+1 to End)
        if (inventoryContainer != null)
        {
            InventorySlot[] mainSlots = inventoryContainer.GetComponentsInChildren<InventorySlot>(true);
            foreach (var slot in mainSlots)
            {
                slot.slotID = uiSlots.Count; // Continue IDs (e.g., 10, 11, 12...)
                uiSlots.Add(slot);
            }
        }
    }

    private void OnEnable()
    {
        if (onInventoryUpdated != null)
            onInventoryUpdated.OnEventRaised += RefreshInventoryUI;

        if (InventoryManager.Instance != null)
            RefreshInventoryUI();
    }

    private void OnDisable()
    {
        if (onInventoryUpdated != null)
            onInventoryUpdated.OnEventRaised -= RefreshInventoryUI;
    }

    private void RefreshInventoryUI()
    {
        if (InventoryManager.Instance == null) return;

        // 1. Clear all existing items from ALL slots
        foreach (InventorySlot slot in uiSlots)
        {
            foreach (Transform child in slot.transform)
            {
                // Clean up only InventoryItems, leave borders/backgrounds
                if (child.GetComponent<InventoryItem>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        // 2. Get the data
        List<InventorySlotData> currentInventory = InventoryManager.Instance.GetInventoryContents();

        // 3. Redraw items
        for (int i = 0; i < uiSlots.Count; i++)
        {
            // Safety check: Do we have data for this slot?
            if (i < currentInventory.Count && !currentInventory[i].IsEmpty())
            {
                InventorySlot targetUiSlot = uiSlots[i];

                // Create item
                GameObject itemGO = Instantiate(inventoryItemPrefab, targetUiSlot.transform);
                InventoryItem itemComponent = itemGO.GetComponent<InventoryItem>();

                // Initialize
                itemComponent.InitializeItem(currentInventory[i].item, currentInventory[i].quantity);
            }
        }
    }
}