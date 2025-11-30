using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapManager : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap highlightTilemap; // <-- ASSIGN NEW TILEMAP HERE

    [Header("Assets")]
    [SerializeField] private TileBase highlightTileAsset; // <-- ASSIGN 1x1 WHITE SQUARE TILE


    public bool IsValidGround(Vector3Int cellPos, List<TileBase> validTypes)
    {
        TileBase tile = groundTilemap.GetTile(cellPos);
        if (tile == null) return false;

        // If validTypes is null/empty, we assume ANY ground is valid
        if (validTypes == null || validTypes.Count == 0) return true;

        return validTypes.Contains(tile);
    }

    /// <summary>
    /// Checks if a cell is occupied by an obstacle (collision check).
    /// </summary>
    public bool IsOccupied(Vector3 worldPos, LayerMask obstacleLayer)
    {
        // Use a small radius check instead of point to be more forgiving
        Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.4f, obstacleLayer);
        return hit != null;
    }

    public void HighlightTiles(List<Vector3Int> cells)
    {
        // 1. Clear old highlights
        highlightTilemap.ClearAllTiles();

        // 2. Draw new ones
        foreach (var cell in cells)
        {
            highlightTilemap.SetTile(cell, highlightTileAsset);
        }
    }

    public void ClearHighlights()
    {
        highlightTilemap.ClearAllTiles();
    }
}