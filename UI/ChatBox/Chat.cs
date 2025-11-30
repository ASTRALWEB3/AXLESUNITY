using TMPro;
using UnityEngine;
using System;

public class Chat : MonoBehaviour
{
    private SpriteRenderer backgroundSpriteRenderer;
    private TextMeshPro textMeshPro;

    private void Awake()
    {
        backgroundSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private Chat Setup(string text)
    {
        // Set initial text to calculate size (temporarily)
        textMeshPro.SetText(text);
        textMeshPro.ForceMeshUpdate();

        // Calculate the required text size
        Vector2 unscaledTextSize = textMeshPro.GetPreferredValues(text);

        // 2. Get the text's local scale (tiny if Scale is 0.1)
        Vector2 textSize = textMeshPro.GetRenderedValues(false);

        // Set padding around the text
        Vector2 padding = new Vector2(0.7f, 0.5f);

        // Set background size
        backgroundSpriteRenderer.size = textSize/10 + padding;

        // Calculate center positions
        float backgroundWidthDivide = backgroundSpriteRenderer.size.x / 6f;
        float backgroundHeightDivide = backgroundSpriteRenderer.size.y / 2f;

        Vector2 offset = new Vector2(-0.9f,0.76f);

        // Set background position to center it behind the text
        backgroundSpriteRenderer.transform.localPosition = 
            new Vector3(
                backgroundWidthDivide + offset.x,
                backgroundHeightDivide + offset.y,
                0f
            );

        // Clear the text BEFORE starting the text writer effect
        textMeshPro.SetText("");

        // Add text writer effect
        TextWriter.AddWriter_Static(textMeshPro, text, 0.05f, true, true, () =>
        {
            Debug.Log("Chat typing complete!");
        });

        return this;
    }

    public static Chat Create(Chat prefab, Transform parent, Vector3 localPosition, string text)
    {
        if (prefab == null)
        {
            Debug.LogError("[Chat] Chat prefab is null! Please assign it in the Inspector.");
            return null;
        }

        Chat chat = Instantiate(prefab, parent);
        chat.transform.localPosition = localPosition;
        chat.Setup(text);

        Destroy(chat.gameObject, 5f); 

        return chat;
    }
}
