using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class TradeUI : MonoBehaviour
{
    public static TradeUI Instance;

    [Header("Main Window")]
    public GameObject tradeWindow;
    public Transform leftGrid;  // "My Items"
    public Transform rightGrid; // "Their Items"

    [Header("Status UI")]
    public Button lockButton;
    public Button confirmButton;
    public TextMeshProUGUI statusText;

    // --- NEW: Visual Assets ---
    [Header("Visual Assets")]
    public Sprite slotBackgroundSprite; // Drag your dark square here
    public Sprite checkmarkSprite;      // Drag your orange checkmark here
    public Color lockedColor = new Color(0.5f, 1f, 0.5f); // Green tint for lock
    public Image myLockIndicator;       // Image to tint when I lock
    public Image theirLockIndicator;    // Image to tint when they lock
    public Image myConfirmIcon;         // Image to show checkmark when I confirm
    public Image theirConfirmIcon;      // Image to show checkmark when they confirm

    [Header("Prefabs")]
    public GameObject tradeItemIconPrefab;

    [Header("Invite Popup")]
    public GameObject invitePanel;
    public TextMeshProUGUI inviteText;
    public Button acceptButton;
    public Button declineButton;

    private TradeSession currentSession;

    void Awake()
    {
        Instance = this;
        tradeWindow.SetActive(false);
        invitePanel.SetActive(false);
    }

    public void OpenTradeWindow(TradeSession session)
    {
        currentSession = session;
        tradeWindow.SetActive(true);
        invitePanel.SetActive(false);
    }

    public void CloseTradeWindow()
    {
        tradeWindow.SetActive(false);
        currentSession = null;
    }

    void Update()
    {
        if (currentSession == null || !currentSession.Object.IsValid) return;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        bool amPlayerA = currentSession.Runner.LocalPlayer == currentSession.PlayerA;

        var myItems = amPlayerA ? currentSession.ItemsA : currentSession.ItemsB;
        var theirItems = amPlayerA ? currentSession.ItemsB : currentSession.ItemsA;

        RenderGrid(leftGrid, myItems);
        RenderGrid(rightGrid, theirItems);

        // --- UPDATE STATUS VISUALS ---
        bool meLocked = amPlayerA ? currentSession.IsLockedA : currentSession.IsLockedB;
        bool themLocked = amPlayerA ? currentSession.IsLockedB : currentSession.IsLockedA;

        bool meConfirmed = amPlayerA ? currentSession.IsConfirmedA : currentSession.IsConfirmedB;
        bool themConfirmed = amPlayerA ? currentSession.IsConfirmedB : currentSession.IsConfirmedA;

        // 1. Lock Indicators (Change color of a panel/border)
        if (myLockIndicator) myLockIndicator.color = meLocked ? lockedColor : Color.white;
        if (theirLockIndicator) theirLockIndicator.color = themLocked ? lockedColor : Color.white;

        // 2. Confirm Icons (Show/Hide Checkmark)
        if (myConfirmIcon)
        {
            myConfirmIcon.sprite = checkmarkSprite;
            myConfirmIcon.gameObject.SetActive(meConfirmed);
        }
        if (theirConfirmIcon)
        {
            theirConfirmIcon.sprite = checkmarkSprite;
            theirConfirmIcon.gameObject.SetActive(themConfirmed);
        }

        // 3. Text Status
        statusText.text = $"YOU: {(meLocked ? "LOCKED" : "...")} | THEM: {(themLocked ? "LOCKED" : "...")}";

        // 4. Button Logic
        if (lockButton) lockButton.interactable = !meLocked;
        if (confirmButton) confirmButton.interactable = meLocked && themLocked && !meConfirmed;
    }

    void RenderGrid(Transform gridParent, NetworkArray<NetworkedTradeItem> items)
    {
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        foreach (var item in items)
        {
            if (item.Quantity > 0)
            {
                if (tradeItemIconPrefab != null)
                {
                    GameObject icon = Instantiate(tradeItemIconPrefab, gridParent);

                    // Set Background
                    Image bg = icon.GetComponent<Image>();
                    if (bg != null && slotBackgroundSprite != null) bg.sprite = slotBackgroundSprite;

                    // Set Text
                    var text = icon.GetComponentInChildren<TextMeshProUGUI>();
                    if (text != null) text.text = $"{item.Quantity}";
                }
            }
        }
    }

    public void OnLockClicked()
    {
        if (currentSession != null)
            currentSession.RPC_SetLockStatus(currentSession.Runner.LocalPlayer, true);
    }

    public void OnConfirmClicked()
    {
        if (currentSession != null)
            currentSession.RPC_ConfirmTrade(currentSession.Runner.LocalPlayer);
    }

    // --- DEBUG ---
    public void Debug_AddStone()
    {
        if (currentSession != null)
            currentSession.RPC_AddItem(currentSession.Runner.LocalPlayer, "Stone", 1, false);
    }

    // --- INVITE SYSTEM ---
    public void ShowInvitePopup(PlayerRef sender, System.Action onAccept)
    {
        invitePanel.SetActive(true);
        inviteText.text = $"Trade Request from Player {sender.PlayerId}";

        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(() =>
        {
            invitePanel.SetActive(false);
            onAccept?.Invoke();
        });

        declineButton.onClick.RemoveAllListeners();
        declineButton.onClick.AddListener(() =>
        {
            invitePanel.SetActive(false);
        });
    }
}