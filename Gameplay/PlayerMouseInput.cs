using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMouseInput : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask groundLayer;
    private Camera mainCamera;

    [Header("Event Channel Listeners")]
    [SerializeField] private VoidEC onAttackInput;

    [Header("Event Channel Broadcasters")]
    [SerializeField] private InteractableEC onHitInteractable;
    [SerializeField] private Vector2EC onEmptyGridClick;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (onAttackInput != null)
        {
            onAttackInput.OnEventRaised += HandleClick;
        }
        else
        {
            // DEBUG: This will tell us if the very first link is broken.
            Debug.LogError("PlayerMouseInput: 'onAttackInput' event channel is NOT assigned!");
        }
    }

    private void OnDisable()
    {
        if (onAttackInput != null)
            onAttackInput.OnEventRaised -= HandleClick;
    }

    private void HandleClick()
    {
        // DEBUG 1: This is the first log you should see.
        Debug.Log("HandleClick received!");

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // DEBUG 2: Check our click position
        Debug.Log($"Click world position: {worldPos}");

        // 1. Check for an Interactable (Trees, etc.)
        Collider2D interactableHit = Physics2D.OverlapPoint(worldPos, interactableLayer);
        if (interactableHit != null)
        {
            // DEBUG 3
            Debug.Log($"SUCCESS: Hit an INTERACTABLE object: {interactableHit.name}");
            if (interactableHit.TryGetComponent(out Interactable interactable))
            {
                if (onHitInteractable != null)
                {
                    onHitInteractable.RaiseEvent(interactable);
                    return;
                }
            }
        }

        // 2. If no interactable, check for Ground
        Collider2D groundHit = Physics2D.OverlapPoint(worldPos, groundLayer);
        if (groundHit != null)
        {
            // DEBUG 4
            Debug.Log($"SUCCESS: Hit a GROUND object: {groundHit.name}");
            if (onEmptyGridClick != null)
            {
                onEmptyGridClick.RaiseEvent(worldPos);
                return;
            }
        }

        // 3. If we hit nothing at all
        // DEBUG 5: This means both OverlapPoint checks failed.
        Debug.LogWarning("FAIL: Click hit nothing. Firing 'onEmptyGridClick' anyway.");
        if (onEmptyGridClick != null)
        {
            onEmptyGridClick.RaiseEvent(worldPos);
        }
    }
}