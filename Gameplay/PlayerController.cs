using UnityEngine;
using Fusion;
using Unity.Cinemachine;

// We inherit from NetworkBehaviour instead of MonoBehaviour
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 5f;

    // Component references
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // --- NETWORKED VARIABLES ---
    [Networked] public Vector2 FacingDirection { get; set; }
    [Networked] public NetworkBool IsMoving { get; set; }

    public override void Spawned()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set default facing direction (Only the owner/server needs to set this initially)
        if (Object.HasStateAuthority)
        {
            FacingDirection = Vector2.down;
        }

        // --- LOCAL PLAYER INITIALIZATION ---
        // This block only runs for the player object that YOU control.
        if (Object.HasInputAuthority)
        {
            // 1. Camera Setup
            var virtualCam = FindFirstObjectByType<CinemachineCamera>();
            if (virtualCam != null)
            {
                Debug.Log("Found Camera! Following Local Player.");
                virtualCam.Follow = this.transform;
            }
            else
            {
                Debug.LogWarning("No CinemachineVirtualCamera found in scene!");
            }

            // 2. Placement Manager Registration (NEW)
            // This tells the placement system "Draw the grid around ME"
            if (PlacementManager.Instance != null)
            {
                PlacementManager.Instance.SetLocalPlayer(this.transform);
                Debug.Log("Local Player Registered with PlacementManager.");
            }
            else
            {
                Debug.LogWarning("PlacementManager not found in scene!");
            }

            var input_chat = FindFirstObjectByType<UI_InputChat>(FindObjectsInactive.Include);
            if (input_chat != null)
            {
                input_chat.playerTransform = this.transform;
                Debug.Log("Assigned playerTransform to UI_InputChat.");
            }
            else
            {
                Debug.LogWarning("UI_InputChat not found in scene!");
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector2 inputVector = data.direction.normalized;

            rb.linearVelocity = inputVector * moveSpeed;

            if (inputVector.magnitude > 0.1f)
            {
                IsMoving = true;
                if (Mathf.Abs(inputVector.x) > Mathf.Abs(inputVector.y))
                {
                    FacingDirection = new Vector2(Mathf.Sign(inputVector.x), 0);
                }
                else
                {
                    FacingDirection = new Vector2(0, Mathf.Sign(inputVector.y));
                }
            }
            else
            {
                IsMoving = false;
            }
        }
    }

    public override void Render()
    {
        animator.SetBool("isMoving", IsMoving);
        animator.SetFloat("moveX", FacingDirection.x);
        animator.SetFloat("moveY", FacingDirection.y);

        if (FacingDirection.x < -0.1f)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}