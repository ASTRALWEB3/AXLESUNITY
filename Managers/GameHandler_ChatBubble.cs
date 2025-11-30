using UnityEngine;

public class GameHandler_ChatBubble : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Chat chatBubblePrefab; // Add this field

    private void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("PlayerTransform is not assigned!");
            return;
        }

        if (chatBubblePrefab == null)
        {
            Debug.LogError("ChatBubblePrefab is not assigned!");
            return;
        }

        Chat.Create(
            prefab: chatBubblePrefab,
            parent: playerTransform,
            localPosition: new Vector3(0.8f, 1f, 0f),
            text: "Hello, this is a chat bubble!"
        );
    }
}


