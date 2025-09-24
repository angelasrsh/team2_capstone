using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make_Bone_Broth_Quest_Step : Quest_Step
{
    void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceAdd += ResourceAdd;
    }

    // Unsubscribe to clean up
    void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceAdd -= ResourceAdd;
    }


    private void ResourceAdd(Ingredient_Data ingredient)
    {
        if (ingredient.Name == Ingredient_Inventory.Instance.IngrEnumToName(IngredientType.Bone_Broth))
            FinishQuestStep(); // Finish and destroy this object
            
    }
}
