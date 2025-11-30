using UnityEngine;

public class InteractionRouter : MonoBehaviour
{
    [Header("Event Channel Listeners")]
    [SerializeField] private InteractableEC onHitInteractable;

    [Header("Event Channel Broadcasters")]
    [SerializeField] private InteractableEC onBreakAttempt;
    [SerializeField] private InteractableEC onCraftAttempt;
    // [SerializeField] private InteractableEC onFurnaceOpenAttempt;

    private void OnEnable()
    {
        if (onHitInteractable != null)
            onHitInteractable.OnEventRaised += HandleInteraction;
    }

    private void OnDisable()
    {
        if (onHitInteractable != null)
            onHitInteractable.OnEventRaised -= HandleInteraction;
    }

    // This is the "switchboard" logic.
    private void HandleInteraction(Interactable interactable)
    {
        switch (interactable.type)
        {
            case InteractionType.Breakable:
                if (onBreakAttempt != null)
                    onBreakAttempt.RaiseEvent(interactable);
                break;

            case InteractionType.CraftingStation:
                if (onCraftAttempt != null)
                    onCraftAttempt.RaiseEvent(interactable);
                // Debug.Log("Trying to interact with a Crafting Station");
                break;

            // Add other cases here as you expand the game
            default:
                Debug.LogWarning($"No interaction route defined for type: {interactable.type}");
                break;
        }
    }
}