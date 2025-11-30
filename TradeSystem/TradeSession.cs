using UnityEngine;
using Fusion;
using System.Collections;

public class TradeSession : NetworkBehaviour
{
    // --- WHO IS TRADING? ---
    [Networked] public PlayerRef PlayerA { get; set; }
    [Networked] public PlayerRef PlayerB { get; set; }

    // --- STATE MACHINE ---
    [Networked] public NetworkBool IsLockedA { get; set; }
    [Networked] public NetworkBool IsLockedB { get; set; }
    [Networked] public NetworkBool IsConfirmedA { get; set; }
    [Networked] public NetworkBool IsConfirmedB { get; set; }

    // --- THE ITEMS (Fixed Capacity of 4 slots per player) ---
    [Networked, Capacity(4)] public NetworkArray<NetworkedTradeItem> ItemsA { get; }
    [Networked, Capacity(4)] public NetworkArray<NetworkedTradeItem> ItemsB { get; }

    public override void Spawned()
    {
        // When this spawns, if I am one of the traders, Open my UI
        if (Runner.LocalPlayer == PlayerA || Runner.LocalPlayer == PlayerB)
        {
            if (TradeUI.Instance != null)
                TradeUI.Instance.OpenTradeWindow(this);
        }
    }

    // ========================================================================
    // 1. ADDING ITEMS
    // ========================================================================

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_AddItem(PlayerRef sender, string itemId, int quantity, bool isNft)
    {
        // Security: Can't add items if locked
        if (IsLockedA || IsLockedB) return;

        NetworkedTradeItem newItem = new NetworkedTradeItem
        {
            ItemID = itemId,
            Quantity = quantity,
            IsNft = isNft
        };

        // Find the first empty slot (Quantity 0 = Empty)
        if (sender == PlayerA) AddToFirstEmptySlot(ItemsA, newItem);
        else if (sender == PlayerB) AddToFirstEmptySlot(ItemsB, newItem);
    }

    private void AddToFirstEmptySlot(NetworkArray<NetworkedTradeItem> array, NetworkedTradeItem item)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i].Quantity == 0)
            {
                array.Set(i, item);
                return;
            }
        }
    }

    // ========================================================================
    // 2. LOCKING & CONFIRMING
    // ========================================================================

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_SetLockStatus(PlayerRef sender, bool isLocked)
    {
        if (sender == PlayerA) IsLockedA = isLocked;
        else if (sender == PlayerB) IsLockedB = isLocked;

        // Security: If anyone UNLOCKS, reset confirmations
        if (!isLocked)
        {
            IsConfirmedA = false;
            IsConfirmedB = false;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ConfirmTrade(PlayerRef sender)
    {
        // Can only confirm if both parties are locked
        if (!IsLockedA || !IsLockedB) return;

        if (sender == PlayerA) IsConfirmedA = true;
        else if (sender == PlayerB) IsConfirmedB = true;

        if (IsConfirmedA && IsConfirmedB)
        {
            Debug.Log("Both Confirmed! Executing Swap...");
            StartCoroutine(ExecuteMockSwap());
        }
    }

    // ========================================================================
    // 3. EXECUTION (MOCK WEB3)
    // ========================================================================

    private IEnumerator ExecuteMockSwap()
    {
        // This runs on the SERVER only.
        Debug.Log("Server: Contacting 'Blockchain' (Mock)...");

        // Simulate network delay
        yield return new WaitForSeconds(2.0f);

        Debug.Log("Server: Transfer Successful!");

        // Log the final exchange
        LogTrade(PlayerA, ItemsA, PlayerB);
        LogTrade(PlayerB, ItemsB, PlayerA);

        // Tell clients to close UI
        RPC_CloseTrade();

        // Destroy this session object
        Runner.Despawn(Object);
    }

    private void LogTrade(PlayerRef giver, NetworkArray<NetworkedTradeItem> items, PlayerRef receiver)
    {
        foreach (var item in items)
        {
            if (item.Quantity > 0)
                Debug.Log($"[MOCK CHAIN] Transferred {item.Quantity}x {item.ItemID} from Player {giver.PlayerId} to {receiver.PlayerId}");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_CloseTrade()
    {
        if (TradeUI.Instance != null)
            TradeUI.Instance.CloseTradeWindow();
    }
}