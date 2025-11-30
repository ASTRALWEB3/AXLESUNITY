using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public static class CMDebug
{
    private static GameObject textPopupPrefab;

    public static void TextPopupMouse(string text)
    {
        // Use new Input System
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        TextPopupMouse(text, mousePosition);
    }

    public static void TextPopupMouse(string text, Vector3 screenPosition)
    {
        if (textPopupPrefab == null)
        {
            CreateTextPopupPrefab();
        }

        GameObject textPopupObject = Object.Instantiate(textPopupPrefab);
        TextPopup textPopup = textPopupObject.GetComponent<TextPopup>();
        textPopup.Setup(text, screenPosition);
    }

    private static void CreateTextPopupPrefab()
    {
        // Create prefab programmatically
        textPopupPrefab = new GameObject("TextPopup");
        TextMeshProUGUI textMesh = textPopupPrefab.AddComponent<TextMeshProUGUI>();
        textMesh.fontSize = 24;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        
        textPopupPrefab.AddComponent<TextPopup>();
        textPopupPrefab.transform.SetParent(null);
        
        Object.DontDestroyOnLoad(textPopupPrefab);
    }
}