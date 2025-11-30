using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // Add this if you want to show quantity

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image image;
    [SerializeField] private TextMeshProUGUI quantityText; // Optional: Add a TextMeshPro object

    [HideInInspector] public Transform parentAfterDrag;

    // --- NEW DATA ---
    [HideInInspector] public ItemData itemData;
    [HideInInspector] public int quantity;
    [HideInInspector] public int originalSlotID; // So we know where we came from

    /// <summary>
    /// Called by InventoryUI to set this item's visual.
    /// </summary>
    public void InitializeItem(ItemData data, int quant)
    {
        itemData = data;
        quantity = quant;

        if (image != null && itemData != null)
        {
            image.sprite = itemData.itemIcon;
        }
        
        if (quantityText != null)
        {
            // Show quantity only if it's more than 1
            quantityText.text = quantity > 1 ? quantity.ToString() : ""; 
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (image != null)
            image.raycastTarget = false;

        parentAfterDrag = transform.parent;
        
        // --- NEW: Store where we came from ---
        originalSlotID = GetComponentInParent<InventorySlot>().slotID;

        // Parent to canvas to render on top
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            transform.SetParent(canvas.transform);
        else
            transform.SetParent(transform.root);

        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (image != null)
            image.raycastTarget = true;
        transform.SetParent(parentAfterDrag); // This snaps it to the correct slot
        transform.localPosition = Vector3.zero;
    }
}