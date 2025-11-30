using Fusion;
using Thirdweb.Api;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldNetworkSync : NetworkBehaviour
{
    [Header("References")]
    public Tilemap groundTilemap;
    // public ItemDatabase itemDatabase;
    public NetworkPrefabRef dropItemPrefab; // Changed to NetworkPrefabRef for Fusion

    public override void Spawned()
    {
        Debug.Log("WorldNetworkSync Spawned!");
    }

    // ========================================================================
    // 1. PLACING BLOCKS
    // ========================================================================

    // Called by Client (ItemPlacer.cs)
    public void RequestPlaceBlock(Vector3Int pos, string itemID)
    {
        // Fusion handles the "Target" logic via the attribute below
        RPC_ServerVerifyPlace(pos, itemID);
    }

    // CLIENT -> SERVER
    // RpcSources.InputAuthority: Only the player controlling this object can call this
    // RpcTargets.StateAuthority: This code runs on the Server
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ServerVerifyPlace(Vector3Int pos, string itemID, RpcInfo info = default)
    {
        // 1. Validation Logic (Server Side)
        // Check if info.Source (the player) has permission, inventory, etc.
        // Check if tile is empty using groundTilemap.HasTile(pos)

        // 2. If valid, update the database (Postgres)

        // 3. Tell everyone to update
        RPC_ClientUpdateBlock(pos, itemID);
    }

    // SERVER -> ALL CLIENTS
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ClientUpdateBlock(Vector3Int pos, string itemID)
    {
        // ItemData item = itemDatabase.GetItemByID(itemID);
        // if (item != null && item.tileToPlace != null)
        // {
        //     groundTilemap.SetTile(pos, item.tileToPlace);
        // }
    }

    // ========================================================================
    // 2. BREAKING BLOCKS
    // ========================================================================

    public void RequestBreakBlock(Vector3Int pos)
    {
        RPC_ServerVerifyBreak(pos);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ServerVerifyBreak(Vector3Int pos, RpcInfo info = default)
    {
        // 1. Validate
        if (!groundTilemap.HasTile(pos)) return;

        TileBase tile = groundTilemap.GetTile(pos);
        // ItemData data = itemDatabase.GetItemByTile(tile);

        // // 2. Tell everyone to remove it
        // RPC_ClientRemoveBlock(pos);

        // // 3. Drop Item
        // if (data != null)
        // {
        //     Vector3 worldPos = groundTilemap.GetCellCenterWorld(pos);
        //     // Fusion Spawn
        //     Runner.Spawn(dropItemPrefab, worldPos, Quaternion.identity);
        // }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ClientRemoveBlock(Vector3Int pos)
    {
        groundTilemap.SetTile(pos, null);
        // Play sound effects
    }
}
