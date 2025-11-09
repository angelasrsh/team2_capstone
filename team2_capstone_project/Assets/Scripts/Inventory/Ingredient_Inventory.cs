using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Derived class of Inventory specifically for ingredients.
/// Uses the singleton pattern. Call functions using Ingredient_Inventory.Instance.<...>
/// 
/// Also contains ingredient specific methods, including add/remove resources for the IngredientType enum
/// and converting an Ingredient_Data to the enum.
/// This requires an updated list of all ingredients that need to be converted! This is annoying
/// </summary>
public class Ingredient_Inventory : Inventory
{
    public static Ingredient_Inventory Instance { get; private set; }
    [HideInInspector] public List<Ingredient_Data> AllIngredients;
    private Ingredient_Data water;
    public Dictionary<String, Ingredient_Data> IngredientDict = new Dictionary<string, Ingredient_Data>(); // Make private once we know it works

    new private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        base.Awake();
    }

    private void Start()
    {
        AllIngredients = Game_Manager.Instance.ingredientDatabase.allIngredients;
        IngredientDict.Clear();
        // Put the list of ingredients into the dictionary to be accessed by their name string
        foreach (Ingredient_Data idata in AllIngredients)
        {
            IngredientDict.Add(idata.Name, idata);
            if (idata.Name == "Water")
                water = idata;
        }
    }

    #region Add Resources

    /// <summary>
    /// Overload AddResources to allow for using the IngredientType enum
    /// </summary>
    /// <param name="type"> IngredientType enum for ingredients </param>
    /// <param name="count"> Amount to add </param>
    /// <returns> Amount actually added </returns>
    public int AddResources(IngredientType type, int count)
    {
        // Error-checking
        if (count < 0)
            Debug.LogError("[Invtry] Cannot add negative amount");

        // Track the amount of resources we still need to add
        int amtLeftToAdd = count;

        // Check if there is a slot with the same type and add if not full
        foreach (Item_Stack istack in InventoryStacks)
        {
            // Add as much as we can to existing stacks
            if (istack != null && istack.resource == IngrEnumToData(type) && istack.amount < istack.stackLimit)
            {
                int amtToAdd = Math.Min(istack.stackLimit - istack.amount, amtLeftToAdd);
                istack.amount += amtToAdd;
                amtLeftToAdd -= amtToAdd;
            }
        }
        // We were not able to add all items to existing slots, so check if we can start a new stack
        // These are two separate loops because we don't assume slots will be filled in order
        for (int i = 0; i < InventorySizeLimit; i++)
        {
            if ((InventoryStacks[i] == null || InventoryStacks[i].resource == null) && amtLeftToAdd > 0)
            {
                InventoryStacks[i] = new Item_Stack();
                InventoryStacks[i].stackLimit = ItemStackLimit;

                int amtToAdd = Math.Min(InventoryStacks[i].stackLimit, amtLeftToAdd);
                InventoryStacks[i].amount = amtToAdd;
                InventoryStacks[i].resource = IngrEnumToData(type);
                amtLeftToAdd -= amtToAdd;
            }
        }

        // Broadcast to listening events
        Game_Events_Manager.Instance.ResourceAdd(IngrEnumToData(type));

        int amtAdded = count - amtLeftToAdd;

        updateInventory();
        Debug.Log($"[Invtory] Added {amtAdded} {IngrEnumToData(type).Name}");
        return amtAdded; // Return how many items were actually added
    }
    #endregion

    #region Remove Resources

    /// <summary>
    /// Overload base inventory RemoveResources function to allow removing ingredients using type enum
    /// </summary>
    /// <param name="type"> type to remove </param>
    /// <param name="count"> amount to remove </param>
    /// <returns> Amount of type actually removed </returns>
    public int RemoveResources(IngredientType type, int count)
    {
        // Error-checking
        if (count < 0)
            Debug.LogError("[Invtry] Cannot remove negative amount"); // not tested

        // Track the amount of resources we still need to add
        int amtLeftToRemove = count;

        // Check if there is a slot with the same type and remove if possible
        foreach (Item_Stack istack in Enumerable.Reverse<Item_Stack>(InventoryStacks)) // Not sure if this works
        {
            // Check if a slot exists with the same type
            if (istack != null && istack.resource == IngrEnumToData(type))
            {
                // Remove as much as we can from this stack
                int amtToRemove = Math.Min(istack.amount, amtLeftToRemove);
                istack.amount -= amtToRemove;
                amtLeftToRemove -= amtToRemove;
            }
        }

        // Broadcast to listening events
        Game_Events_Manager.Instance.ResourceRemove(IngrEnumToData(type));

        updateInventory(); // Remove empty elements

        int amtRemoved = count - amtLeftToRemove;
        Debug.Log($"[Invtory] Removed {amtRemoved} {IngrEnumToData(type).Name}");
        // Return however much was added
        return amtRemoved;
    }

    #endregion

    public bool CanMakeDish(Dish_Data dish)
    {
        if (dish == null || dish.ingredientQuantities == null)
        {
            Debug.LogWarning("[Ingr_Inventory] Dish or ingredients list was null.");
            return false;
        }

        Debug.Log($"[Ingr_Inventory] Checking if we can make {dish.Name}");

        foreach (var req in dish.ingredientQuantities)
        {
            int have = GetItemCount(req.ingredient);
            int need = req.amountRequired;

            Debug.Log($"Requires {need}x {req.ingredient.Name}, player has {have}");

            // Extra debug for countsAs relationships
            foreach (var ingr in Ingredient_Inventory.Instance.AllIngredients)
            {
                if (Ingredient_Inventory.Instance.CountsAsOrIs(ingr, req.ingredient))
                    Debug.Log($"  [CountsAs] {ingr.Name} counts as {req.ingredient.Name}");
            }

            if (have < need)
            {
                Debug.LogWarning($"Not enough {req.ingredient.Name}. Needed {need}, but have {have}.");
                return false;
            }
        }

        Debug.Log($"All ingredients met for {dish.Name}");
        return true;
    }


    #region Ingredient enum/string conversion
    /// <summary>
    /// Convert enum IngredientType to a name string.
    /// String here must match Item_Data Name!
    /// </summary>
    /// <param name="t"> type to convert to a string </param>
    /// <returns></returns>
    public String IngrEnumToName(IngredientType t)
    {
        switch (t)
        {
            case IngredientType.Bone:
                return "Bone";
            case IngredientType.Bone_Broth:
                return "Bone Broth";
            case IngredientType.Bread:
                return "Bread";
            case IngredientType.Cheese:
                return "Cheese";
            case IngredientType.Cooked_Patty:
                return "Cooked Patty";
            case IngredientType.Cut_Fermented_Eye:
                return "Cut Fermented Eye";
            case IngredientType.Cut_Fogshroom:
                return "Cut Fogshroom";
            case IngredientType.Cut_Mandrake:
                return "Cut Mandrake";
            case IngredientType.French_Fries:
                return "French Fries";
            case IngredientType.Honey:
                return "Honey";
            case IngredientType.Milk:
                return "Milk";
            case IngredientType.Oil:
                return "Eleonora Oil";
            case IngredientType.Uncut_Slime:
                return "Slime Gelatin";
            case IngredientType.Uncooked_Patty:
                return "Patty";
            case IngredientType.Uncut_Fermented_Eye:
                return "Fermented Eye";
            case IngredientType.Uncut_Fogshroom:
                return "Fogshroom";
            case IngredientType.Uncut_Mandrake:
                return "Mandrake";
            case IngredientType.Water:
                return "Water";
            case IngredientType.Burnt_Blob:
                return "Burnt Blob";
            case IngredientType.Cut_Slime:
                return "Cut Slime";
            case IngredientType.Cut_Ficklegourd:
                return "Cut Ficklegourd";
            case IngredientType.Uncut_Ficklegourd:
                return "Ficklegourd";
            case IngredientType.Cooked_Cut_Ficklegourd:
                return "Cooked Ficklegourd";
            case IngredientType.PreWashed_Rice:
                return "Rice";
            case IngredientType.Steamed_Rice:
                return "Steamed Rice";
            default:
                return "";
        }
    }

    /// <summary>
    /// For use by Inventory_Overlap
    /// Converts Ingredient_Data to IngredientType
    /// </summary>
    /// <param name="d"> Ingredient_Data to convert to an enum </param>
    /// <returns> The corresponding IngredientType enum </returns>
    public IngredientType IngrDataToEnum(Ingredient_Data d)
    {
        if (d == null)
            return IngredientType.Null;

        switch (d.Name)
        {
            case "Bone":
                return IngredientType.Bone;
            case "Bone Broth":
                return IngredientType.Bone_Broth;
            case "Bread":
                return IngredientType.Bread;
            case "Cheese":
                return IngredientType.Cheese;
            case "Cooked Patty":
                return IngredientType.Cooked_Patty;
            case "Cut Fermented Eye":
                return IngredientType.Cut_Fermented_Eye;
            case "Cut Fogshroom":
                return IngredientType.Cut_Fogshroom;
            case "Cut Mandrake":
                return IngredientType.Cut_Mandrake;
            case "French Fries":
                return IngredientType.French_Fries;
            case "Honey":
                return IngredientType.Honey;
            case "Milk":
                return IngredientType.Milk;
            case "Eleonora Oil":
                return IngredientType.Oil;
            case "Slime Gelatin":
                return IngredientType.Uncut_Slime;
            case "Patty":
                return IngredientType.Uncooked_Patty;
            case "Fermented Eye":
                return IngredientType.Uncut_Fermented_Eye;
            case "Fogshroom":
                return IngredientType.Uncut_Fogshroom;
            case "Mandrake":
                return IngredientType.Uncut_Mandrake;
            case "Water":
                return IngredientType.Water;
            case "Burnt Blob":
                return IngredientType.Burnt_Blob;
            case "Cut Slime":
                return IngredientType.Cut_Slime;
            case "Cut Ficklegourd":
                return IngredientType.Cut_Ficklegourd;
            case "Ficklegourd":
                return IngredientType.Uncut_Ficklegourd;
            case "Cooked Ficklegourd":
                return IngredientType.Cooked_Cut_Ficklegourd;
            case "Rice":
                return IngredientType.PreWashed_Rice;
            case "Steamed Rice":
                return IngredientType.Steamed_Rice;
            default:
                return IngredientType.Null;
        }

    }

    /// <summary>
    /// Get the corresponding Ingredient_Data scriptable object for a given IngredientType enum.
    /// 
    /// Requires that the Ingredient_Inventory AllIngredientsList is up to date!
    /// 
    /// Must be better ways to do this but for now, this is the best I've got.
    /// </summary>
    /// <param name="t"> Ingredient enum </param>
    /// <returns> Reference to a corresponding Ingredient_Data scriptable object </returns>
    public Ingredient_Data IngrEnumToData(IngredientType t)
    {
        String name = IngrEnumToName(t);
        if (IngredientDict.ContainsKey(name))
            return IngredientDict[IngrEnumToName(t)];
        else
            Debug.LogWarning($"[Ingr_Intry] {IngrEnumToName(t)} not found in ingredient dictionary");
        return null;
    }
    #endregion

    /// <summary>
    /// get the total count of a specific ingredient in the inventory by name
    /// used for checking if we have enough ingredients to make a dish in Journal_Menu
    /// </summary>
    /// <param name="ingredientName"></param>
    /// <returns></returns>
    // public int GetItemCount(string ingredientName)
    // {
    //   int total = 0;
    //   foreach (Item_Stack stack in InventoryStacks)
    //   {
    //     if (stack != null && stack.resource != null && stack.resource.Name == ingredientName)
    //     {
    //       total += stack.amount;
    //     }
    //   }
    //   return total;
    // }

    public int GetItemCount(IngredientType type)
    {
        Ingredient_Data data = IngrEnumToData(type);
        return GetItemCount(data);
    }

    /// Return the number of ingredients of that type in the inventory
    public int GetItemCount(Ingredient_Data ingredient)
    {
        if (ingredient == null) 
            return 0;

        int total = 0;
        foreach (Item_Stack stack in InventoryStacks)
        {
            if (stack == null || stack.resource == null)
                continue;

            Ingredient_Data invIngredient = stack.resource as Ingredient_Data;
            if (invIngredient == null)
                continue;

            // Direct match
            if (invIngredient == ingredient || invIngredient.Name == ingredient.Name)
            {
                total += stack.amount;
                Debug.Log($"[Ingredient_Inventory] Counting {invIngredient.Name} as {ingredient.Name} (+{stack.amount})");
                continue;
            }

            // Check "countsAs" from the inventory item side
            if (invIngredient.countsAs != null &&
                invIngredient.countsAs.Any(c => c != null && c.Name == ingredient.Name))
            {
                total += stack.amount;
                Debug.Log($"[Ingredient_Inventory] Counting {invIngredient.Name} as {ingredient.Name} (+{stack.amount})");
                continue;
            }

            // Check "countsAs" from the ingredient side (less common, but safe)
            if (ingredient.countsAs != null &&
                ingredient.countsAs.Any(c => c != null && c.Name == invIngredient.Name))
            {
                total += stack.amount;
                Debug.Log($"[Ingredient_Inventory] Counting {invIngredient.Name} as {ingredient.Name} (+{stack.amount})");
                continue;
            }
        }

        return total;
    }

    
    /// <summary>
    /// Recursively checks if ingredient A is the same as or "counts as" ingredient B.
    /// Uses a visited set to prevent infinite loops from circular references.
    /// </summary>
    private bool CountsAsOrIs(Ingredient_Data a, Ingredient_Data b, HashSet<Ingredient_Data> visited = null)
    {
        if (a == null || b == null)
            return false;

        if (a == b)
            return true;

        // Initialize the visited set on the first call
        if (visited == null)
            visited = new HashSet<Ingredient_Data>();

        // If we've already checked this ingredient, stop to avoid infinite recursion
        if (visited.Contains(a))
            return false;

        visited.Add(a);

        if (a.countsAs == null)
            return false;

        foreach (var counted in a.countsAs)
        {
            if (counted == null) continue;

            if (counted == b || CountsAsOrIs(counted, b, visited))
                return true;
        }

        return false;
    }


    /// <returns>Water's ingredient data</returns>
    public Ingredient_Data getWaterData()
    {
        return water;
    }


    #region Save / Load
    public IngredientInventoryData GetSaveData()
    {
        IngredientInventoryData data = new IngredientInventoryData();

        foreach (var stack in InventoryStacks)
        {
            if (stack == null || stack.resource == null)
                continue;

            var ingr = stack.resource as Ingredient_Data;
            if (ingr == null)
                continue;

            data.stacks.Add(new SerializableItemStack
            {
                ingredientName = ingr.Name,
                amount = stack.amount
            });
        }

        return data;
    }

    public void LoadFromSaveData(IngredientInventoryData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[Ingredient_Inventory] No ingredient inventory data found — initializing empty.");
            InitializeInventoryStacks();
            return;
        }

        InitializeInventoryStacks();

        int index = 0;
        foreach (var s in data.stacks)
        {
            if (IngredientDict.TryGetValue(s.ingredientName, out var ingredient))
            {
                var newStack = new Item_Stack
                {
                    resource = ingredient,
                    amount = s.amount
                };
                InventoryStacks[index] = newStack;
                index++;
                if (index >= InventorySizeLimit) break;
            }
            else
            {
                Debug.LogWarning($"[Ingredient_Inventory] Could not find ingredient '{s.ingredientName}' in dictionary during load.");
            }
        }
        updateInventory();

        Debug.Log($"[Ingredient_Inventory] Loaded {data.stacks.Count} ingredient stacks from save.");
    }
    #endregion
}

#region Ingredient_Inventory_Save_Data
[System.Serializable]
public class IngredientInventoryData
{
    public List<SerializableItemStack> stacks = new List<SerializableItemStack>();
    public int TotalIngCount = 0;
}

[System.Serializable]
public class SerializableItemStack
{
    public string ingredientName;
    public int amount;
}
#endregion







