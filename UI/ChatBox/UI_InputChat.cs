using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UI_InputChat : MonoBehaviour
{
    private static UI_InputChat instance;

    // --- NEW HELPER PROPERTY ---
    // Allows PlayerTalk to check if the window is open
    public static bool IsVisible
    {
        get
        {
            return instance != null && instance.gameObject.activeSelf;
        }
    }

    private Button okBtn;
    private Button cancelBtn;
    private TextMeshProUGUI titleText;
    private TMP_InputField inputField;
    [SerializeField] private Chat chatBubblePrefab;
    public Transform playerTransform;

    private void Awake()
    {
        Debug.Log("[UI_InputChat] Awake called");

        // Set up singleton FIRST
        if (instance != null && instance != this)
        {
            Debug.Log("[UI_InputChat] Duplicate instance found, destroying");
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        // Find components while GameObject is active
        okBtn = transform.Find("okBtn")?.GetComponent<Button>();
        cancelBtn = transform.Find("cancleBtn")?.GetComponent<Button>();
        titleText = transform.Find("titleText")?.GetComponent<TextMeshProUGUI>();
        inputField = transform.Find("inputField")?.GetComponent<TMP_InputField>();

        // // Debug component finding
        // Debug.Log($"[UI_InputChat] okBtn: {(okBtn != null ? "Found" : "NULL")}");
        // Debug.Log($"[UI_InputChat] cancelBtn: {(cancelBtn != null ? "Found" : "NULL")}");
        // Debug.Log($"[UI_InputChat] titleText: {(titleText != null ? "Found" : "NULL")}");
        // Debug.Log($"[UI_InputChat] inputField: {(inputField != null ? "Found" : "NULL")}");

        // THEN hide the window
        Hide();
    }

    private void Update()
    {
        // Only check input if the window is active
        if (!gameObject.activeSelf) return;

        // Use new Input System
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            Debug.Log("[UI_InputChat] Enter key pressed, invoking OK button");
            okBtn.onClick.Invoke();
        }
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("[UI_InputChat] Escape key pressed, invoking Cancel button");
            cancelBtn.onClick.Invoke();
        }
    }

    private void Show(string title, string inputString, string validCharacters, int characterLimit, Action onCancel, Action<string> onOk)
    {
        transform.SetAsLastSibling();
        gameObject.SetActive(true);

        titleText.text = title;
        inputField.text = inputString;
        inputField.characterLimit = characterLimit;

        if (!string.IsNullOrEmpty(validCharacters))
        {
            inputField.onValidateInput = (string text, int charIndex, char addedChar) =>
            {
                return (validCharacters.IndexOf(addedChar) >= 0) ? addedChar : '\0';
            };
        }
        else
        {
            inputField.onValidateInput = null;
        }

        okBtn.onClick.RemoveAllListeners();
        cancelBtn.onClick.RemoveAllListeners();

        okBtn.onClick.AddListener(() =>
        {
            string text = inputField.text;
            Hide();
            if (onOk != null) onOk(text);
        });

        cancelBtn.onClick.AddListener(() =>
        {
            Hide();
            if (onCancel != null) onCancel();
        });

        // Focus the input field
        inputField.Select();
        inputField.ActivateInputField();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public static void Show_Static(string title, string inputString, string validCharacters, int characterLimit, Action onCancle, Action<string> onOk)
    {
        if (instance == null)
        {
            Debug.LogError("[UI_InputChat] UI_InputChat instance not found!");
            return;
        }
        instance.Show(title, inputString, validCharacters, characterLimit, onCancle, onOk);
    }
}