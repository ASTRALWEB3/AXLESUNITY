using UnityEngine;
using Fusion;
using UnityEngine.InputSystem; // 1. Add Input System

public class TradeInteraction : NetworkBehaviour
{
    [Header("Configuration")]
    [SerializeField] private NetworkPrefabRef tradeSessionPrefab;
    [SerializeField] private float interactionRange = 3.0f;
    [SerializeField] private LayerMask playerLayer;

    // --- INPUT (Local Player) ---
    void Update()
    {
        if (!Object.HasInputAuthority) return;

        // 2. Use New Input System for Right Click
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CheckForPlayerClick();
        }
    }

    void CheckForPlayerClick()
    {
        // 3. Get Mouse Position from New Input System
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPos);

        // Raycast
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, playerLayer);

        if (hit.collider != null)
        {
            NetworkObject targetObj = hit.collider.GetComponentInParent<NetworkObject>();
            if (targetObj != null && targetObj != Object) // Don't trade with self
            {
                float dist = Vector3.Distance(transform.position, targetObj.transform.position);
                if (dist <= interactionRange)
                {
                    Debug.Log($"Sending Trade Request to Player {targetObj.InputAuthority.PlayerId}...");
                    RPC_RelayInvite(targetObj.InputAuthority);
                }
            }
        }
    }

    // --- NETWORK LOGIC (Same as before) ---

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RelayInvite(PlayerRef targetPlayer)
    {
        RPC_ReceiveInvite(targetPlayer, Object.InputAuthority);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ReceiveInvite([RpcTarget] PlayerRef target, PlayerRef sender)
    {
        if (Runner.LocalPlayer == target)
        {
            if (TradeUI.Instance != null)
            {
                TradeUI.Instance.ShowInvitePopup(sender, () =>
                {
                    var myObject = Runner.GetPlayerObject(Runner.LocalPlayer);
                    if (myObject != null)
                    {
                        var myInteraction = myObject.GetComponent<TradeInteraction>();
                        if (myInteraction != null)
                        {
                            myInteraction.RPC_AcceptInvite(sender);
                        }
                    }
                    else
                    {
                        Debug.LogError("Could not find local player object to send accept RPC!");
                    }
                });
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_AcceptInvite(PlayerRef originalSender)
    {
        Debug.Log("Invite Accepted! Spawning Session...");

        NetworkObject sessionObj = Runner.Spawn(tradeSessionPrefab);
        TradeSession session = sessionObj.GetComponent<TradeSession>();

        session.PlayerA = originalSender;
        session.PlayerB = Object.InputAuthority;
    }
}