using UnityEngine;

public class WorldItemManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private WorldItem worldItemPrefab; // Drag your prefab here

    [Header("Event Channel Listeners")]
    [SerializeField] private SpawnItemEventChannel onSpawnItemInWorld;

    [Header("Event Channel Broadcasters")]
    [SerializeField] private ItemEventChannel onItemGained; // Pass this to the prefab

    private void OnEnable()
    {
        if (onSpawnItemInWorld != null)
        {
            onSpawnItemInWorld.OnEventRaised += SpawnItem;
        }
    }

    private void OnDisable()
    {
        if (onSpawnItemInWorld != null)
        {
            onSpawnItemInWorld.OnEventRaised -= SpawnItem;
        }
    }

    private void SpawnItem(ItemData item, int quantity, Vector3 position)
    {
        if (worldItemPrefab == null) return;

        // Create the new item in the world
        WorldItem newItem = Instantiate(worldItemPrefab, position, Quaternion.identity);

        // Initialize it with the correct data and event
        newItem.Initialize(item, quantity, onItemGained);

        Debug.Log($"DROP: Spawned {quantity}x {item.name} at position {position}");

        // You could add logic here to make it "pop" out, etc.
    }
}