using UnityEngine;
using UnityEngine.Tilemaps; // Important for tile-based games!

/// <summary>
/// This script goes on your Player GameObject.
/// It handles detecting clicks on the game world and placing items.
/// </summary>
public class ItemPlacer : MonoBehaviour
{
    // Assign your main tilemap in the inspector. This is where you'll place blocks.
    public Tilemap terrainTilemap;

    // Assign your "objects" tilemap (or just a parent GameObject) for non-tile items like seeds.
    public GameObject objectPlacerParent;

    public float placeDistance = 2.0f; // How far the player can place items

    void Update()
    {
        // // Check for the "Place" button (usually right-click)
        // if (Input.GetMouseButtonDown(1)) // 0 is left-click, 1 is right-click
        // {
        //     PlaceItem();
        // }
    }

    void PlaceItem()
    {
        // // 1. Get the currently selected item
        // ItemData itemToPlace = HotbarManager.instance.GetSelectedItem(); // Changed to ItemData

        // if (itemToPlace == null || !itemToPlace.isPlaceable) // Added isPlaceable check
        // {
        //     // No item selected, do nothing
        //     return;
        // }

        // // 2. Get click position and convert to world/grid position
        // Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // // This line is for a Tilemap. It finds the cell coordinates (e.g., [2, 5]) from the click.
        // Vector3Int cellPos = terrainTilemap.WorldToCell(mouseWorldPos);

        // // Get the center of the cell, which is where we'll instantiate
        // Vector3 cellCenter = terrainTilemap.GetCellCenterWorld(cellPos);

        // // 3. Check if we can place here

        // // Check distance from player
        // float distance = Vector3.Distance(transform.position, cellCenter);
        // if (distance > placeDistance)
        // {
        //     Debug.Log("Too far to place!");
        //     return; // Too far away
        // }

        // // Check if the tile is already occupied
        // if (terrainTilemap.GetTile(cellPos) != null)
        // {
        //     Debug.Log("Can't place, tile is occupied.");
        //     return; // Tile is already full
        // }

        // // You might also want to check for other objects (e.g., using Physics2D.OverlapCircle)
        // // if (!IsCellEmpty(cellCenter)) { return; }


        // // 4. Place the item
        // // Assuming your ItemData script has these fields (see next file)
        // if (itemToPlace.itemType == ItemData.ItemType.Block && itemToPlace.tileToPlace != null)
        // {
        //     // It's a block, so we set it on the Tilemap
        //     terrainTilemap.SetTile(cellPos, itemToPlace.tileToPlace);
        // }
        // else if (itemToPlace.itemType == ItemData.ItemType.Seed && itemToPlace.prefabToPlace != null)
        // {
        //     // It's a seed (or other object), so we instantiate its prefab
        //     // Ensure the parent is set so it stays organized in the hierarchy
        //     Instantiate(itemToPlace.prefabToPlace, cellCenter, Quaternion.identity, objectPlacerParent.transform);
        // }
        // else
        // {
        //     Debug.Log("This item is not placeable.");
        //     return; // Item isn't a placeable type
        // }

        // // 5. Consume the item from inventory
        // // --- THIS NOW CALLS YOUR INVENTORY MANAGER ---
        // // You will need to replace "ConsumeItem" with your actual function
        // // for removing one item from a slot.
        // // For example:
        // // InventoryManager.Instance.RemoveItem(HotbarManager.instance.selectedSlotIndex, 1);

        // // I am guessing you have a function like this:
        // InventoryManager.Instance.ConsumeItem(HotbarManager.instance.selectedSlotIndex);

        // Debug.Log("Placed item: " + itemToPlace.itemName);
    }
}