//using System.Collections.Generic;
//using UnityEngine;

//public class FarmingManager : MonoBehaviour
//{
//    [Header("References")]
//    //[SerializeField] private GrowingPlant plantPrefab;
//    [SerializeField] private TilemapManager tilemapManager;

//    // TODO: This should come from InventoryManager
//    [SerializeField] private PlantData temp_seedToPlant; // TEMPORARY

//    [Header("Event Channel Listeners")]
//    [SerializeField] private Vector2EC onPlantAttempt;
//    [SerializeField] private InteractableEC onBreakPlantAttempt;

//    [Header("Event Channel Broadcasters")]
//    [SerializeField] private SpawnItemEventChannel onSpawnItemInWorld;

//    // Core Logic: List of all growing plants (for tracking)
//    //private List<GrowingPlant> growingPlants = new List<GrowingPlant>();

//    private void OnEnable()
//    {
//        if (onPlantAttempt != null)
//            onPlantAttempt.OnEventRaised += HandlePlantAttempt;
//        if (onBreakPlantAttempt != null)
//            onBreakPlantAttempt.OnEventRaised += HandleBreakAttempt;
//    }

//    private void OnDisable()
//    {
//        if (onPlantAttempt != null)
//            onPlantAttempt.OnEventRaised -= HandlePlantAttempt;
//        if (onBreakPlantAttempt != null)
//            onBreakPlantAttempt.OnEventRaised -= HandleBreakAttempt;
//    }

//    // --- THE UPDATE() LOOP IS NOW GONE ---
//    // (Each GrowingPlant prefab handles its own Update)

//    private void HandlePlantAttempt(Vector2 worldPos)
//    {
//        // TODO: Get selected seed from InventoryManager
//        PlantData seedToPlant = temp_seedToPlant;

//        if (seedToPlant == null)
//        {
//            Debug.Log("No seed equipped!");
//            return;
//        }

//        // 1. Spawn the plant prefab
//        GrowingPlant newPlant = Instantiate(plantPrefab, worldPos, Quaternion.identity);
//        newPlant.Initialize(seedToPlant);

//        // 2. Add to our tracking list
//        growingPlants.Add(newPlant);

//        // 3. Tell TilemapManager to remove the "TilledSoil" tile
//        tilemapManager.ClearFarmingTile(worldPos);

//        // TODO: Remove seed from inventory
//    }

//    private void HandleBreakAttempt(Interactable interactable)
//    {
//        GrowingPlant plant = interactable.GetComponent<GrowingPlant>();
//        if (plant == null) return;

//        BreakableObjectData lootToDrop;

//        // --- THIS IS THE LOGIC FOR BREAKING ---
//        // This logic was already correct, but it now works
//        // because the plant's `currentStage` is being
//        // properly updated by its own Update() loop.

//        if (plant.currentStage == GrowthStage.Grown)
//        {
//            Debug.Log("Breaking a GROWN plant.");
//            lootToDrop = plant.plantData.grownLoot;
//        }
//        else
//        {
//            Debug.Log("Breaking a SEEDLING.");
//            lootToDrop = plant.plantData.seedlingLoot;
//        }

//        // Use the static LootProcessor to handle spawning
//        LootProcessor.ProcessLoot(lootToDrop, plant.transform.position, onSpawnItemInWorld);

//        // Remove from our tracking list
//        growingPlants.Remove(plant);

//        // Destroy the plant
//        Destroy(plant.gameObject);
//    }
//}