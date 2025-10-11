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
    public event System.Action OnDishUnlocked; // Event to notify when a dish is unlocked

  private void OnEnable()
  {
    Instance = this;
    // Initialize with default unlocks if needed
    UnlockDish(Dish_Data.Dishes.Blinding_Stew);
    // UnlockDish(Dish_Data.Dishes.Mc_Dragons_Burger);
    UnlockNPC(CustomerData.NPCs.Elf);
    UnlockNPC(CustomerData.NPCs.Phrog);
    UnlockIngredient(IngredientType.Slime_Gelatin);
    UnlockIngredient(IngredientType.Water);
    UnlockIngredient(IngredientType.Bone_Broth);
    UnlockIngredient(IngredientType.Bone);
    UnlockIngredient(IngredientType.Uncut_Fogshroom);
    UnlockIngredient(IngredientType.Uncut_Fermented_Eye);
    UnlockIngredient(IngredientType.Cut_Fogshroom);
    UnlockIngredient(IngredientType.Cut_Fermented_Eye);
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

    #region NPCs
    /// <summary>
    /// Unlock an NPC by enum
    /// </summary>
    public void UnlockNPC(CustomerData.NPCs npc)
    {
        unlockedNPCs.Add(npc);
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