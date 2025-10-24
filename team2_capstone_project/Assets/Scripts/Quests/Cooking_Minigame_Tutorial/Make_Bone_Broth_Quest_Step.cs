using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Make_Bone_Broth_Quest_Step : Dialogue_Quest_Step
{
    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceAdd += ResourceAdd;
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceAdd -= ResourceAdd;
    }

    void Start()
    {
        DelayedDialogue(0, 0, false);
    }

    private void ResourceAdd(Ingredient_Data ingredient)
    {
        if (ingredient.Name == Ingredient_Inventory.Instance.IngrEnumToName(IngredientType.Bone_Broth))
            FinishQuestStep(); // Finish and destroy this object
            
    }
}
