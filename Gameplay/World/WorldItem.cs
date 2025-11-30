using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldItem : MonoBehaviour
{
    [Header("Event Channel Broadcasters")]
    [SerializeField] private ItemEventChannel onItemGained; // The event to add to inventory

    [Header("Item State (Set by Spawner)")]
    public ItemData itemData;
    public int quantity;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // This function will be called by the WorldItemManager after spawning
    public void Initialize(ItemData item, int qty, ItemEventChannel gainChannel)
    {
        this.itemData = item;
        this.quantity = qty;
        this.onItemGained = gainChannel;

        // Set the sprite from the ItemData
        if (itemData != null)
        {
            spriteRenderer.sprite = itemData.itemIcon;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that touched this is the Player
        if (other.CompareTag("Player"))
        {
            if (onItemGained != null && itemData != null)
            {
                Debug.Log($"PICKUP: Player collected {quantity}x {itemData.name} from world");
                
                // Fire the event for the InventoryManager to hear
                onItemGained.RaiseEvent(itemData, quantity);

                // Destroy this world item
                Destroy(gameObject);
            }
        }
    }
}