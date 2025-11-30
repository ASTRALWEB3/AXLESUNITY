using UnityEngine;
using System.Collections.Generic;

public class BreakableObjectManager : MonoBehaviour
{
    [Header("Event Channel Listeners")]
    [SerializeField] private InteractableEC onBreakAttempt;

    [Header("Event Channel Broadcasters")]
    [SerializeField] private ObjectDamagedEC onObjectDamaged;
    [SerializeField] private ObjectBrokenEC onObjectBroken;
    [SerializeField] private SpawnItemEventChannel onSpawnItemInWorld;

    private Dictionary<string, int> objectHealthDatabase = new Dictionary<string, int>();

    private void OnEnable()
    {
        if (onBreakAttempt != null)
            onBreakAttempt.OnEventRaised += HandleBreakAttempt;
    }

    private void OnDisable()
    {
        if (onBreakAttempt != null)
            onBreakAttempt.OnEventRaised -= HandleBreakAttempt;
    }

    private void HandleBreakAttempt(Interactable interactable)
    {
        string id = interactable.worldInstanceID;

        BreakableObjectView view = interactable.GetComponent<BreakableObjectView>();
        if (view == null || view.objectData == null) return;

        BreakableObjectData data = view.objectData;

        if (!objectHealthDatabase.ContainsKey(id))
        {
            objectHealthDatabase[id] = data.maxHealth;
        }

        int damage = 1;
        int newHealth = objectHealthDatabase[id] - damage;
        objectHealthDatabase[id] = newHealth;

        if (newHealth > 0)
        {
            if (onObjectDamaged != null)
                onObjectDamaged.RaiseEvent(id, newHealth);
        }
        else
        {
            // Object is broken!

            // --- NEW LOOT LOGIC ---
            if (data.IsGrowable())
            {
                // Check the VIEW to see if it finished growing
                if (view.IsGrown)
                {
                    Debug.Log("Processing GROWN loot (Wood)");
                    // Use the GROWN lists
                    ProcessLoot(data.grownGuaranteedDrops, data.grownRandomDrops, interactable.transform.position);
                }
                else
                {
                    Debug.Log("Processing SEEDLING loot (Seed)");
                    // Use the NORMAL lists
                    ProcessLoot(data.guaranteedDrops, data.randomDrops, interactable.transform.position);
                }
            }
            else
            {
                // Not a plant (Rock/Tree), use normal lists
                ProcessLoot(data.guaranteedDrops, data.randomDrops, interactable.transform.position);
            }
            // ---------------------

            if (onObjectBroken != null)
                onObjectBroken.RaiseEvent(id);

            objectHealthDatabase.Remove(id);
        }
    }

    // Helper to process specific lists
    private void ProcessLoot(List<GuaranteedItemDrop> guaranteed, List<RandomItemDrop> random, Vector3 spawnPosition)
    {
        if (onSpawnItemInWorld == null) return;

        if (guaranteed != null)
        {
            foreach (var drop in guaranteed)
            {
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                if (amount > 0) onSpawnItemInWorld.RaiseEvent(drop.item, amount, spawnPosition);
            }
        }

        if (random != null)
        {
            foreach (var drop in random)
            {
                if (Random.value <= drop.dropChance)
                {
                    int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                    if (amount > 0) onSpawnItemInWorld.RaiseEvent(drop.item, amount, spawnPosition);
                }
            }
        }
    }
}