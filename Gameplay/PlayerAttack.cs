using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Event Channel Listeners")]
    [SerializeField] private VoidEC onAttackInput; // Listens for Left Click

    [Header("Event Channel Broadcasters")]
    [SerializeField] private InteractableEC onBreakAttempt;

    // --- NEW: Add this to fire the planting event ---
    [SerializeField] private Vector2EC onEmptyGridClick;
    // -----------------------------------------------

    [Header("Component References")]
    [SerializeField] private GameObject swingHitbox; // The child hitbox object
    [SerializeField] private float swingDuration = 0.2f;

    [Header("Attack Settings")]
    [Tooltip("The distance the hitbox will be from the player's center")]
    [SerializeField] private float hitboxOffset = 0.5f;
    [Tooltip("Maximum range to search for breakable objects")]
    [SerializeField] private float attackRange = 2f;

    private PlayerController playerController;
    private bool isSwinging = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (swingHitbox != null)
        {
            swingHitbox.SetActive(false); // Start with hitbox disabled
        }
    }

    private void OnEnable()
    {
        if (onAttackInput != null)
            onAttackInput.OnEventRaised += HandleAttack;
    }

    private void OnDisable()
    {
        if (onAttackInput != null)
            onAttackInput.OnEventRaised -= HandleAttack;
    }

    private void HandleAttack()
    {
        if (isSwinging) return;

        StartCoroutine(SwingCoroutine());
    }

    private IEnumerator SwingCoroutine()
    {
        isSwinging = true;

        // Get mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePos.z = 0;

        // 1. Try to find a Breakable Object first
        Interactable clickedBreakable = GetClickedBreakableObject(mousePos);

        if (clickedBreakable != null)
        {
            // --- CASE A: We hit a breakable object ---

            // Position hitbox towards the clicked object
            Vector2 directionToObject = (clickedBreakable.transform.position - transform.position).normalized;
            swingHitbox.transform.localPosition = directionToObject * hitboxOffset;

            // Attack the clicked object
            if (onBreakAttempt != null)
            {
                onBreakAttempt.RaiseEvent(clickedBreakable);
            }
        }
        else
        {
            // --- CASE B: We hit NOTHING (Empty Ground) ---
            // This is where we trigger planting!

            if (onEmptyGridClick != null)
            {
                // Pass the mouse position to the PlacementManager
                onEmptyGridClick.RaiseEvent(mousePos);
            }

            // Show visual swing animation anyway
            Vector2 direction = playerController.FacingDirection;
            swingHitbox.transform.localPosition = direction * hitboxOffset;
        }

        // Activate the hitbox for visual feedback
        swingHitbox.SetActive(true);

        // Wait for the swing duration
        yield return new WaitForSeconds(swingDuration);

        // Deactivate the hitbox
        swingHitbox.SetActive(false);
        isSwinging = false;
    }

    private Interactable GetClickedBreakableObject(Vector3 mouseWorldPos)
    {
        // Raycast at mouse position to find clicked object
        Collider2D clickedCollider = Physics2D.OverlapPoint(mouseWorldPos);

        if (clickedCollider != null &&
            clickedCollider.TryGetComponent(out Interactable interactable) &&
            interactable.type == InteractionType.Breakable)
        {
            // Check if object is within attack range
            float distance = Vector2.Distance(transform.position, clickedCollider.transform.position);
            if (distance <= attackRange)
            {
                return interactable;
            }
        }

        return null;
    }
}