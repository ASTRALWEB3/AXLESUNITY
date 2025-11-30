using UnityEngine;

// Add new types here as you create them
public enum InteractionType
{
    Breakable,
    CraftingStation,
    Furnace,
    Sign
}

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Header("Configuration")]
    public InteractionType type;

    [Header("State")]
    [Tooltip("The unique ID for this specific object instance in the world.")]
    [ReadOnly] public string worldInstanceID;

    private void Awake()
    {
        // Generate a unique ID for this object instance
        if (string.IsNullOrEmpty(worldInstanceID))
        {
            worldInstanceID = System.Guid.NewGuid().ToString();
        }
    }
}

// (Optional) Helper attribute to make fields read-only in the inspector
public class ReadOnlyAttribute : PropertyAttribute { }