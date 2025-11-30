using UnityEngine;
using System; // Required for DateTime
using System.Collections;

[RequireComponent(typeof(Interactable), typeof(SpriteRenderer))]
public class BreakableObjectView : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public BreakableObjectData objectData;

    [Header("State")]
    [Tooltip("Check this for trees already placed in the scene so they don't start as seeds.")]
    [SerializeField] private bool startsGrown = false;

    [Header("Event Channel Listeners")]
    [SerializeField] private ObjectDamagedEC onObjectDamaged;
    [SerializeField] private ObjectBrokenEC onObjectBroken;

    [Header("Shake Feedback")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.05f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    // --- GROWTH STATE (REAL TIME) ---
    // We use Ticks (long) because they are easy to save to a database/file later.
    private long timePlantedTicks;
    public bool IsGrown { get; private set; }

    private Interactable interactable;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        interactable.type = InteractionType.Breakable;
        spriteRenderer.sortingLayerName = "GroundDetails";
        originalPosition = transform.position;

        if (objectData == null) return;

        // --- INITIALIZATION ---
        if (objectData.IsGrowable())
        {
            if (startsGrown)
            {
                IsGrown = true;
                // For wild trees, we don't care when they were planted.
            }
            else
            {
                IsGrown = false;
                // If timePlantedTicks is 0, it implies this is a brand new plant.
                // (If we were loading from a save file, we would have set this already).
                if (timePlantedTicks == 0)
                {
                    timePlantedTicks = DateTime.UtcNow.Ticks;
                }
                UpdateGrowthSprite(0);
            }
        }
        else
        {
            IsGrown = true;
        }
    }

    private void OnEnable()
    {
        if (onObjectDamaged != null) onObjectDamaged.OnEventRaised += HandleDamage;
        if (onObjectBroken != null) onObjectBroken.OnEventRaised += HandleBroken;
    }

    private void OnDisable()
    {
        if (onObjectDamaged != null) onObjectDamaged.OnEventRaised -= HandleDamage;
        if (onObjectBroken != null) onObjectBroken.OnEventRaised -= HandleBroken;
    }

    // --- REAL-TIME GROWTH LOOP ---
    private void Update()
    {
        if (objectData == null || !objectData.IsGrowable() || IsGrown) return;

        // 1. Get the current real-world time
        DateTime timeNow = DateTime.UtcNow;
        DateTime timePlanted = new DateTime(timePlantedTicks);

        // 2. Calculate difference in seconds
        // (TotalSeconds returns a double, so we cast to float)
        double secondsElapsed = (timeNow - timePlanted).TotalSeconds;
        float growthPercent = (float)secondsElapsed / objectData.growthTimeInSeconds;

        // 3. Check Growth
        if (growthPercent >= 1.0f)
        {
            IsGrown = true;
            UpdateGrowthSprite(objectData.growthSprites.Count - 1);
        }
        else
        {
            // Update stage
            int stageIndex = Mathf.FloorToInt(growthPercent * objectData.growthSprites.Count);
            UpdateGrowthSprite(stageIndex);
        }
    }

    // --- SAVE SYSTEM HOOK ---
    // Call this function when you implement your Save/Load system later.
    public void LoadState(long savedTicks)
    {
        timePlantedTicks = savedTicks;
        // Force an immediate check so the sprite updates instantly on load
        IsGrown = false;
        Update();
    }
    // -----------------------

    private void UpdateGrowthSprite(int index)
    {
        if (objectData.growthSprites == null || index >= objectData.growthSprites.Count) return;
        Sprite newSprite = objectData.growthSprites[index];
        if (spriteRenderer.sprite != newSprite) spriteRenderer.sprite = newSprite;
    }

    private void HandleDamage(string instanceID, int newHealth)
    {
        if (instanceID != interactable.worldInstanceID) return;
        if (shakeCoroutine != null) { StopCoroutine(shakeCoroutine); transform.position = originalPosition; }
        shakeCoroutine = StartCoroutine(Shake());
    }

    private void HandleBroken(string instanceID)
    {
        if (instanceID != interactable.worldInstanceID) return;
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        Destroy(gameObject);
    }

    private IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
        shakeCoroutine = null;
    }
}