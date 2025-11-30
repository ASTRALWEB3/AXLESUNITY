using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TilemapManager tilemapManager;
    [SerializeField] private Tilemap groundTilemap;

    // --- MODIFIED: No longer assigned in Inspector ---
    private Transform playerTransform;
    // ------------------------------------------------

    [Header("Config")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Event Listeners")]
    [SerializeField] private ItemSelectedEventChannel onItemSelected;
    // [SerializeField] private Vector2EC onEmptyGridClick;

    [Header("Event Broadcasters")]
    [SerializeField] private ItemEventChannel onItemGained;

    // State
    private ItemData currentItem;
    private List<Vector3Int> validCells = new List<Vector3Int>();
    private Vector3Int lastPlayerCell;

    private void Awake()
    {
        // Singleton Setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (onItemSelected != null) onItemSelected.OnEventRaised += HandleItemSelected;
        // if (onEmptyGridClick != null) onEmptyGridClick.OnEventRaised += HandleClick;
    }

    private void OnDisable()
    {
        if (onItemSelected != null) onItemSelected.OnEventRaised -= HandleItemSelected;
        // if (onEmptyGridClick != null) onEmptyGridClick.OnEventRaised -= HandleClick;
    }

    // --- NEW: Public method for the Local Player to register themselves ---
    public void SetLocalPlayer(Transform player)
    {
        playerTransform = player;
        Debug.Log("PlacementManager: Local Player Registered");
    }
    // ---------------------------------------------------------------------

    private void Update()
    {
        // Safety Check: If player hasn't spawned/registered yet, do nothing
        if (playerTransform == null) return;

        if (currentItem != null && currentItem.isPlaceable)
        {
            Vector3Int playerCell = groundTilemap.WorldToCell(playerTransform.position);

            if (playerCell != lastPlayerCell)
            {
                lastPlayerCell = playerCell;
                CalculateValidGrid();
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleDirectClick();
            }
        }
    }

    private void HandleDirectClick()
    {
        // A. Stop if clicking on UI (Buttons, Inventory, etc)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Mouse.current == null) return;
        // B. Get Mouse Position in World
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3Int clickedCell = groundTilemap.WorldToCell(mousePos);

        if (validCells.Contains(clickedCell))
        {
            PlaceItem(clickedCell);
        }
    }

    private void PlaceItem(Vector3Int cell)
    {
        Vector3 spawnPos = groundTilemap.GetCellCenterWorld(cell);

        if (currentItem.itemPrefab != null)
        {
            Instantiate(currentItem.itemPrefab, spawnPos, Quaternion.identity);

            // Consume Item
            if (onItemGained != null)
                onItemGained.RaiseEvent(currentItem, -1);

            // Force refresh grid immediately so we can't place twice in same spot
            CalculateValidGrid();
        }
    }

    private void HandleItemSelected(ItemData item)
    {
        currentItem = item;

        // Safety Check
        if (playerTransform == null) return;

        if (currentItem != null && currentItem.isPlaceable)
        {
            CalculateValidGrid();
        }
        else
        {
            tilemapManager.ClearHighlights();
            validCells.Clear();
        }
    }

    private void CalculateValidGrid()
    {
        // Safety Check
        if (playerTransform == null) return;

        validCells.Clear();
        Vector3Int playerCell = groundTilemap.WorldToCell(playerTransform.position);
        int range = currentItem.placementRange;

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int cellToCheck = new Vector3Int(playerCell.x + x, playerCell.y + y, 0);

                if (tilemapManager.IsValidGround(cellToCheck, currentItem.validGroundTypes))
                {
                    Vector3 worldPos = groundTilemap.GetCellCenterWorld(cellToCheck);
                    if (!tilemapManager.IsOccupied(worldPos, obstacleLayer))
                    {
                        validCells.Add(cellToCheck);
                    }
                }
            }
        }

        tilemapManager.HighlightTiles(validCells);
    }

    private void HandleClick(Vector2 clickPos)
    {
        if (playerTransform == null) return;
        if (currentItem == null || !currentItem.isPlaceable) return;

        Vector3Int clickedCell = groundTilemap.WorldToCell(clickPos);
        if (!validCells.Contains(clickedCell)) return;

        Vector3 spawnPos = groundTilemap.GetCellCenterWorld(clickedCell);

        if (currentItem.itemPrefab != null)
        {
            Instantiate(currentItem.itemPrefab, spawnPos, Quaternion.identity);

            // Remove item using the generic ItemEventChannel with -1 quantity
            if (onItemGained != null)
                onItemGained.RaiseEvent(currentItem, -1);

            CalculateValidGrid();
        }
    }
}