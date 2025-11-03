using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Grimoire;

public class Leave_Resource_Area_Canvas_Script : MonoBehaviour
{
    public TextMeshProUGUI questionTextRef;
    public string questionText;
    public string warningQuestionText;

    public void SetText()
    {
        GameObject panel = transform.Find("Panel")?.gameObject;
        GameObject textGameObj = panel?.transform.Find("QuestionText")?.gameObject;
        questionTextRef = textGameObj?.GetComponent<TextMeshProUGUI>();

        if (questionTextRef == null)
        {
            Debug.LogWarning("[Leave_Resource_Area_Canvas_Script] Error: questionTextRef is null. Is there a Panel/QuestionText child present?");
            return;
        }

        // Only warn if in the foraging area AND not enough resources
        if (SceneManager.GetActiveScene().name == "Foraging_Area_Whitebox" && !HaveEnoughResources())
            questionTextRef.text = warningQuestionText;
        else
            questionTextRef.text = questionText;
    }

    /// <summary>
    /// Checks if the player can make at least one dish from today's selected menu.
    /// </summary>
    private bool HaveEnoughResources()
    {
        // Ensure ingredient system exists
        if (Ingredient_Inventory.Instance == null)
        {
            Debug.LogWarning("[Leave_Resource_Area_Canvas_Script] Ingredient_Inventory instance not found!");
            return false;
        }

        // Get selected daily dishes from Choose_Menu_Items
        var selectedDishEnums = Choose_Menu_Items.instance?.GetSelectedDishes();
        if (selectedDishEnums == null || selectedDishEnums.Count == 0)
        {
            Debug.LogWarning("[Leave_Resource_Area_Canvas_Script] No dishes selected for today.");
            return false;
        }

        // Check if any selected dish can be made
        foreach (var dishEnum in selectedDishEnums)
        {
            Dish_Data dish = Game_Manager.Instance.dishDatabase.GetDish(dishEnum);
            if (dish == null) continue;

            bool canMake = Ingredient_Inventory.Instance.CanMakeDish(dish);
            Debug.Log($"Checking if can make {dish.name}: {canMake}");

            if (canMake)
            {
                Debug.Log($"Enough resources to make {dish.name}! (Player can leave safely.)");
                return true;
            }
        }

        Debug.Log("Not enough resources to make ANY selected dishes!");
        return false;
    }
}
