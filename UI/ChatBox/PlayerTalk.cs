using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTalk : NetworkBehaviour
{
    [SerializeField] private Chat chatBubblePrefab;
    [SerializeField] private Vector3 chatBubbleOffset = new Vector3(0.5f, 1, 0);

    private List<Chat> _activeBubbles = new List<Chat>();
    private const int MAX_BUBBLES = 2; // Max number of bubbles before destroying oldest
    private const float BUBBLE_STACK_HEIGHT = 1.5f; // How much to move older bubbles up

    public override void Spawned()
    {
        // Optional debug
        if (Object.HasInputAuthority)
        {
            Debug.Log("[PlayerTalk] Local Player Ready to Chat");
        }
    }

    private void Update()
    {
        if (!Object.HasInputAuthority) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            if (!UI_InputChat.IsVisible)
            {
                OpenChatInput();
            }
        }
    }

    private void OpenChatInput()
    {
        UI_Blocker.Show_Static();

        // Show input window
        UI_InputChat.Show_Static(
            title: "Talk",
            inputString: "",
            validCharacters: "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ,.!?'-",
            characterLimit: 50,
            onCancle: () =>
            {
                UI_Blocker.Hide_Static();
            },
            onOk: (string inputText) =>
            {
                UI_Blocker.Hide_Static();

                if (!string.IsNullOrEmpty(inputText))
                {
                    RPC_RequestChat(inputText);
                }
            }
        );
    }

    // --- NETWORK LOGIC ---

    // STEP 1: Client sends message to Server
    // RpcSources.InputAuthority = Only the owner of this player can call this
    // RpcTargets.StateAuthority = The code runs on the Server
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestChat(string message, RpcInfo info = default)
    {
        // (Optional) Server can filter bad words here
        // string cleanMessage = FilterProfanity(message);

        // Server tells ALL clients to show the bubble
        RPC_BroadcastChat(message);
    }

    // STEP 2: Server tells ALL clients to draw the bubble
    // RpcTargets.All = Runs on Server and ALL Clients
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastChat(string message)
    {
        if (chatBubblePrefab != null)
        {
            // 1. CLEANUP: Remove any bubbles that destroyed themselves (timeout)
            _activeBubbles.RemoveAll(item => item == null);

            // 2. LIMIT: If we hit the max, destroy the oldest one (Index 0)
            while (_activeBubbles.Count >= MAX_BUBBLES)
            {
                if (_activeBubbles[0] != null)
                {
                    Destroy(_activeBubbles[0].gameObject);
                }
                _activeBubbles.RemoveAt(0);
            }

            // 3. STACK: Move existing bubbles UP so they don't overlap the new one
            foreach (Chat oldChat in _activeBubbles)
            {
                if (oldChat != null)
                {
                    oldChat.transform.localPosition += new Vector3(0, BUBBLE_STACK_HEIGHT, 0);
                }
            }

            // 4. CREATE: Spawn the new bubble at the base position
            Chat chat = Chat.Create(chatBubblePrefab, transform, Vector3.zero, message);

            if (chat != null)
            {
                chat.transform.SetParent(transform);
                chat.transform.localPosition = chatBubbleOffset;

                // 5. TRACK: Add to the list
                _activeBubbles.Add(chat);
            }
        }
    }

}
