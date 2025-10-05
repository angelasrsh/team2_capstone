using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Saves/PlayerProgress")]
public class Player_Progress : ScriptableObject
{
    public static Player_Progress Instance;
    // HashSet ensures no duplicates and fast lookups
    [SerializeField] private HashSet<Dish_Data.Dishes> unlockedDishes = new HashSet<Dish_Data.Dishes>();
    [SerializeField] private HashSet<CustomerData.NPCs> unlockedNPCs = new HashSet<CustomerData.NPCs>();
    [SerializeField] private HashSet<Ingredient_Data> unlockedIngredients = new HashSet<Ingredient_Data>();
    public event System.Action OnDishUnlocked; // Event to notify when a dish is unlocked
    public event System.Action OnNPCUnlocked; // Event to notify when an npc is unlocked
    public event System.Action OnIngredientUnlocked; // Event to notify when an ingredient is unlocked

    private void OnEnable()
    {
        Instance = this;
    }

    #region Dishes
    /// <summary>
    /// Unlock a dish by enum
    /// </summary>
    public void UnlockDish(Dish_Data.Dishes dish)
    {
        // HashSet.Add returns true if item was actually added
        if (unlockedDishes.Add(dish))
        {
            // Fire event only if a new dish was added
            OnDishUnlocked?.Invoke();
        }
    }

    /// <summary>
    /// Check if a dish is unlocked
    /// </summary>
    public bool IsDishUnlocked(Dish_Data.Dishes dish) => unlockedDishes.Contains(dish);

    /// <summary>
    /// Get all unlocked dishes as a list
    /// </summary>
    public List<Dish_Data.Dishes> GetUnlockedDishes() => new List<Dish_Data.Dishes>(unlockedDishes);
    #endregion

    #region Ingredients
    /// <summary>
    /// Unlock an ingredient by Ingredient_Data
    /// </summary>
    public void UnlockIngredient(Ingredient_Data ingr)
    {
        if (unlockedIngredients.Add(ingr))
        {
            // Fire event only if a new ingredient was added
            OnIngredientUnlocked?.Invoke();
        }
    }

    /// <summary>
    /// Check if an ingredient is unlocked
    /// </summary>
    public bool IsIngredientUnlocked(Ingredient_Data ingr) => unlockedIngredients.Contains(ingr);

    /// <summary>
    /// Get all unlocked ingredients as a list
    /// </summary>
    public List<Ingredient_Data> GetUnlockedIngredients() => new List<Ingredient_Data>(unlockedIngredients);
    #endregion

    #region NPCs
    /// <summary>
    /// Unlock an NPC by enum
    /// </summary>
    public void UnlockNPC(CustomerData.NPCs npc)
    {
        if (unlockedNPCs.Add(npc))
        {
            // Fire event only if a new npc was added
            OnNPCUnlocked?.Invoke();
        }
    }

    /// <summary>
    /// Check if an NPC is unlocked
    /// </summary>
    public bool IsNPCUnlocked(CustomerData.NPCs npc) => unlockedNPCs.Contains(npc);

    /// <summary>
    /// Get all unlocked NPCs as a list
    /// </summary>
    public List<CustomerData.NPCs> GetUnlockedNPCs() => new List<CustomerData.NPCs>(unlockedNPCs);
    #endregion
}