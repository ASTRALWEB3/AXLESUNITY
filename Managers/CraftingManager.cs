using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    [Header("Event Channel Listeners")]
    [SerializeField] private InteractableEC onCraftAttempt;

    [Header("Game Data")]
    [SerializeField] private List<RecipeData> allRecipes;

    private void OnEnable()
    {
        if (onCraftAttempt != null)
            onCraftAttempt.OnEventRaised += HandleCraftingAttempt;
    }

    private void OnDisable()
    {
        if (onCraftAttempt != null)
            onCraftAttempt.OnEventRaised -= HandleCraftingAttempt;
    }

    private void HandleCraftingAttempt(Interactable craftingStation)
    {
        // This is the trigger!
        Debug.Log($"Player interacted with {craftingStation.name}! Opening craft menu...");

        // --- LATER, YOU WILL ADD LOGIC HERE ---
        // 1. Open the Crafting UI panel.
        // 2. Show the list of 'allRecipes' in the UI.
        // 3. When the player clicks a recipe, check if they have the items.
        // 4. If they do, call a 'CraftItem(RecipeData recipe)' function.
    }

    public void CraftItem(RecipeData recipe)
    {
        // 1. Check if player has items (e.g., inventoryManager.HasItems(recipe.ingredients))
        // 2. If yes, remove items (e.g., inventoryManager.RemoveItems(recipe.ingredients))
        // 3. If yes, add result (e.g., inventoryManager.AddItem(recipe.resultItem, recipe.resultAmount))
        Debug.Log($"Crafted {recipe.resultItem.name}!");
    }
}
