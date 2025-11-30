using UnityEngine;

// This is a static helper class. It does not go on a GameObject.
// Its only job is to process loot tables and fire spawn events.
public static class LootProcessor
{
    public static void ProcessLoot(BreakableObjectData data, Vector3 spawnPosition, SpawnItemEventChannel onSpawnItemInWorld)
    {
        if (data == null || onSpawnItemInWorld == null) return;

        // 1. Process Guaranteed Drops
        foreach (var drop in data.guaranteedDrops)
        {
            if (drop.item == null) continue;

            // Calculate random amount
            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
            if (amount > 0)
            {
                // Fire event to spawn item
                onSpawnItemInWorld.RaiseEvent(drop.item, amount, spawnPosition);
            }
        }

        // 2. Process Random Drops
        foreach (var drop in data.randomDrops)
        {
            if (drop.item == null) continue;

            // Roll for chance
            if (Random.value <= drop.dropChance)
            {
                // Calculate random amount
                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                if (amount > 0)
                {
                    // Fire event to spawn item
                    onSpawnItemInWorld.RaiseEvent(drop.item, amount, spawnPosition);
                }
            }
        }
    }
}