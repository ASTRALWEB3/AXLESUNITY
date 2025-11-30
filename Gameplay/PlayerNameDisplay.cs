using UnityEngine;
using Fusion;
using TMPro;
using Thirdweb;
using Unity.VisualScripting; // If you need to fetch wallet info directly

public class PlayerNameDisplay : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshPro nameText; // Drag your TMP object here

    // --- NETWORKED VARIABLE ---
    // "OnChanged" calls the static method whenever this variable updates.
    [Networked, OnChangedRender(nameof(OnNameChanged))]
    public NetworkString<_32> NickName { get; set; }

    public override void Spawned()
    {
        // 1. LOCAL PLAYER INITIALIZATION
        // Only the local player sets their own name initially
        if (Object.HasInputAuthority)
        {
            // Option A: Get from PlayerPrefs (Simple)
            string myName = PlayerPrefs.GetString("PlayerName", "Guest");

            // Option B: Get from Thirdweb Wallet (Advanced)
            // string myAddress = await ThirdwebManager.Instance.SDK.Wallet.GetAddress();
            // string shortAddress = myAddress.Substring(0, 6) + "...";
            // string myName = string.IsNullOrEmpty(savedName) ? shortAddress : savedName;

            // Send RPC to server to update the Networked Variable
            RPC_SetNickName(myName);
        }
        else
        {
            // Remote players just update the text from the existing value
            UpdateNameLabel();
        }
    }

    // --- RPC: Client sends name to Server ---
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string name, RpcInfo info = default)
    {
        // Server updates the Networked Variable
        // This change will automatically propagate to all clients via OnNameChanged
        NickName = name;
    }

    // --- CALLBACK: Updates the UI ---
    public void OnNameChanged()
    {
        UpdateNameLabel();
    }

    private void UpdateNameLabel()
    {
        if (nameText != null)
        {
            nameText.text = NickName.ToString();
        }
    }
}