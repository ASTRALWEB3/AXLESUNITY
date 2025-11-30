using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    // --- NEW: This ID is set by InventoryUI ---
    [HideInInspector] public int slotID;

    public GameObject highlightBorder;

    void Start()
    {
        // Ensure the highlight is off at the start
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(false);
        }
        // Note: You might want to add a check here, e.g., if this slot is not part of the hotbar, disable this feature.
        // For example: if (slotID >= 10) { this.enabled = false; } // if your hotbar is slots 0-9
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        InventoryItem droppedItem = droppedObject.GetComponent<InventoryItem>();
        if (droppedItem == null) return;

        // Get the source and destination slot IDs
        int sourceSlotID = droppedItem.originalSlotID;
        int destinationSlotID = this.slotID;

        // Don't do anything if dropping onto the same slot
        if (sourceSlotID == destinationSlotID) return;

        InventoryItem itemInThisSlot = null;
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<InventoryItem>(out itemInThisSlot))
            {
                break; // Found an item!
            }
        }
        if (itemInThisSlot == null)
        {
            // --- CASE 1: Dropping into an EMPTY slot ---
            // The slot is empty, so we just move the item.
            droppedItem.parentAfterDrag = transform;
            InventoryManager.Instance.MoveItemToEmpty(sourceSlotID, destinationSlotID);
        }
        else
        {
            // --- CASE 2: Dropping onto a FULL slot (Swap) ---

            // 1. We already found 'itemInThisSlot'

            // 2. Tell the item *in this slot* to move to the *original* slot
            itemInThisSlot.transform.SetParent(droppedItem.parentAfterDrag);
            itemInThisSlot.transform.localPosition = Vector3.zero; // Center it

            // 3. Tell the *dropped item* to move to *this* slot
            droppedItem.parentAfterDrag = transform;

            // 4. Tell the manager to swap the data
            InventoryManager.Instance.SwapItems(sourceSlotID, destinationSlotID);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Check for right-click
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (HotbarManager.instance != null)
            {
                // Use slotID, which your script already has
                HotbarManager.instance.SelectSlot(slotID);
            }
        }
    }

    public void Select()
    {
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(true);
        }
    }

    public void Deselect()
    {
        if (highlightBorder != null)
        {
            highlightBorder.SetActive(false);
        }
    }
}

