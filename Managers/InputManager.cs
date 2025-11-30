using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Player Controls Event Channels")]
    [SerializeField] private Vector2EC onMove;
    [SerializeField] private VoidEC onInteract;
    [SerializeField] private VoidEC onAttack;

    private InputSystem inputSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            // If not, set this as the instance
            Instance = this;
            // And mark it to not be destroyed when new scenes are loaded
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this one to prevent duplicates
            Destroy(gameObject);
            return;
        }


        inputSystem = new InputSystem();
    }

    void OnEnable()
    {
        inputSystem.Player.Movement.performed += ctx => onMove.RaiseEvent(ctx.ReadValue<Vector2>());
        inputSystem.Player.Movement.canceled += ctx => onMove.RaiseEvent(Vector2.zero);
        // inputSystem.Player.Sprint.performed += ctx => onSprint.RaiseEvent(true);
        // inputSystem.Player.Sprint.canceled += ctx => onSprint.RaiseEvent(false);
        // inputSystem.Player.Roll.performed += ctx => onRoll.RaiseEvent();
        // inputSystem.Player.DrawWeapon.performed += ctx => onWeaponChanging.RaiseEvent();
        // inputSystem.Player.Attack.performed += ctx => onAttack.RaiseEvent();
        // inputSystem.Player.CyclePrev.performed += ctx => onCyclePrev.RaiseEvent();
        // inputSystem.Player.CycleNext.performed += ctx => onCycleNext.RaiseEvent();
        // inputSystem.Player.UseShortcut.performed += ctx => onUseShortcut.RaiseEvent();
        // inputSystem.Player.LockOn.performed += ctx => onLockOn.RaiseEvent();
        // inputSystem.Player.UseSkill.performed += ctx => onUseSkill.RaiseEvent();
        // inputSystem.Player.Summon.performed += ctx => onSummonClone.RaiseEvent();

        if (onInteract != null)
        {
            inputSystem.Player.Interact.performed += ctx => onInteract.RaiseEvent();
        }

        if (onAttack != null)
        {
            inputSystem.Player.Attack.performed += ctx => onAttack.RaiseEvent();
        }


        inputSystem.Enable();
    }

    void OnDisable()
    {
        // onGameStateChanged.OnEventRaised -= HandleGameState;
        inputSystem.Disable();
    }
}
