using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Leave_Resource_Area_Canvas_Script : MonoBehaviour
{

    public TextMeshProUGUI questionTextRef;
    public string questionText;
    public string warningQuestionText;


    // Start is called before the first frame update
    public void SetText()
    {
        GameObject panel = transform.Find("Panel").gameObject;
        GameObject textGameObj = panel.transform.Find("QuestionText").gameObject;
        questionTextRef = textGameObj.GetComponent<TextMeshProUGUI>();
        if (questionTextRef == null)
            Helpers.printLabeled(this, "Error: questionTextRef is null. Is there a Panel/QuestionText child present?");

        if (!haveEnoughResources() && SceneManager.GetActiveScene().name == "Foraging_Area_Whitebox")
            questionTextRef.text = warningQuestionText;
        else
            questionTextRef.text = questionText;
    }
    

       /// <summary>
    /// Check if we have enough resources. Hard-coded to stew
    /// TODO: Check for all selected daily recipes
    /// </summary>
    private bool haveEnoughResources()
    {
        int numShrooms = Ingredient_Inventory.Instance.GetItemCount(IngredientType.Uncut_Fogshroom);
        int numEyes = Ingredient_Inventory.Instance.GetItemCount(IngredientType.Uncut_Fermented_Eye);
        int numBones = Ingredient_Inventory.Instance.GetItemCount(IngredientType.Bone);

        if (numShrooms >= Day_Plan_Manager.instance.customersPlannedForEvening
            && numEyes >= Day_Plan_Manager.instance.customersPlannedForEvening
            && numBones >= Day_Plan_Manager.instance.customersPlannedForEvening)
            return true;
        else
            return false;
    }
}
