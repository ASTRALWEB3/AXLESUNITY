using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion; // Needed for TextMeshPro elements

public class PlayerNametag : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text nameText;

    // --- NETWORKED VARIABLE ---
    // [Networked] synchronizes this variable to all clients.
    // OnChanged ensures that when it changes, the UI updates for everyone.
    [Networked, OnChangedRender(nameof(OnNameChanged))]
    public NetworkString<_32> NickName { get; set; }

    public override void Spawned()
    {
        // 1. If this object belongs to ME (the local player on this computer)
        if (Object.HasInputAuthority)
        {
            // Get the name I saved in the Menu
            string myName = PlayerPrefs.GetString("PlayerName", "Guest");

            // Tell the Server to update my networked name
            RPC_SetNickName(myName);
        }
        else
        {
            // If this is a remote player, just update the label to match whatever their Networked Name already is.
            // This handles late-joiners or if the name was already set before we spawned.
            UpdateNameLabel();
        }
    }

    // --- RPC: Client -> Server ---
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string name, RpcInfo info = default)
    {
        // This code runs on the SERVER.
        // We update the Networked Variable here.
        // Fusion automatically syncs "NickName" to all other clients.
        NickName = name;

        // Force update on server too (Host mode)
        UpdateNameLabel();
    }

    // --- CALLBACK: Runs on ALL Clients when "NickName" changes ---
    public void OnNameChanged()
    {
        UpdateNameLabel();
    }

    private void UpdateNameLabel()
    {
        if (nameText != null)
        {
            // If name is empty, don't show anything yet
            if (!string.IsNullOrEmpty(NickName.ToString()))
            {
                nameText.text = NickName.ToString();
            }
        }
    }
}