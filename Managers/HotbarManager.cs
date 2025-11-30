using UnityEngine;
using System.Collections.Generic;

public class HotbarManager : MonoBehaviour
{
    public static HotbarManager instance;

    [Header("Event Broadcasters")]
    [SerializeField] private ItemSelectedEventChannel onItemSelected; // <-- NEW

    public List<InventorySlot> hotbarSlots = new List<InventorySlot>();
    public int selectedSlotIndex = -1;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        DeselectAllSlots();
        if (hotbarSlots.Count > 0)
        {
            SelectSlot(0);
        }
    }

    public void SelectSlot(int slotIndex)
    {
        if (selectedSlotIndex == slotIndex) return;

        if (selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count)
        {
            hotbarSlots[selectedSlotIndex].Deselect();
        }

        if (slotIndex >= 0 && slotIndex < hotbarSlots.Count)
        {
            hotbarSlots[slotIndex].Select();
            selectedSlotIndex = slotIndex;

            // --- NOTIFY PLACEMENT MANAGER ---
            ItemData selectedItem = GetSelectedItem();
            if (onItemSelected != null)
            {
                onItemSelected.RaiseEvent(selectedItem);
            }
        }
    }

    public void DeselectAllSlots()
    {
        foreach (var slot in hotbarSlots)
        {
            slot.Deselect();
        }
        selectedSlotIndex = -1;

        // Notify that nothing is selected
        if (onItemSelected != null)
            onItemSelected.RaiseEvent(null);
    }

    public ItemData GetSelectedItem()
    {
        if (selectedSlotIndex < 0) return null;
        if (InventoryManager.Instance == null) return null;

        List<InventorySlotData> inventory = InventoryManager.Instance.GetInventoryContents();
        if (selectedSlotIndex < inventory.Count)
        {
            return inventory[selectedSlotIndex].item;
        }

        return null;
    }
}