using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Make_Bone_Broth_Quest_Step : Dialogue_Quest_Step
{
    private Dialogue_Manager dialogueManager;
    private InputAction skipAction;

    protected override void OnEnable()
    {
        Game_Events_Manager.Instance.onResourceAdd += ResourceAdd;
        TryBindInput();
    }

    protected override void OnDisable()
    {
        Game_Events_Manager.Instance.onResourceAdd -= ResourceAdd;
        UnbindInput();
    }

    private void Start()
    {
        dialogueManager = FindObjectOfType<Dialogue_Manager>();
        DelayedDialogue(0, 0, false);
    }

    private void TryBindInput()
    {
        UnbindInput();

        PlayerInput playerInput = null;

        if (Game_Manager.Instance != null)
            playerInput = Game_Manager.Instance.GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            var pic = FindObjectOfType<Player_Input_Controller>();
            if (pic != null)
                playerInput = pic.GetComponent<PlayerInput>();
        }

        if (playerInput == null)
        {
            Debug.LogWarning("[Make_Bone_Broth_Quest_Step] No PlayerInput found for quest skip binding.");
            return;
        }

        var actions = playerInput.actions;
        if (actions == null)
        {
            Debug.LogWarning("[Make_Bone_Broth_Quest_Step] PlayerInput has no InputActionAsset.");
            return;
        }

        // Try to find the same SkipDialog action used by Dialog_UI_Manager
        skipAction = actions.FindAction("SkipDialog", false);
        if (skipAction == null)
        {
            Debug.LogWarning("[Make_Bone_Broth_Quest_Step] SkipDialog action not found in PlayerInput actions.");
            return;
        }

        skipAction.performed += OnSkipPressed;
        skipAction.Enable();

        Debug.Log("[Make_Bone_Broth_Quest_Step] Successfully bound SkipDialog input.");
    }

    private void UnbindInput()
    {
        if (skipAction != null)
        {
            skipAction.performed -= OnSkipPressed;
            skipAction.Disable();
            skipAction = null;
        }
    }

    private void OnSkipPressed(InputAction.CallbackContext ctx)
    {
        if (dialogueManager != null)
        {
            dialogueManager.EndDialog();
            Debug.Log("[Make_Bone_Broth_Quest_Step] Dialogue manually closed via SkipDialog input.");
        }
    }

    private void ResourceAdd(Ingredient_Data ingredient)
    {
        if (ingredient.Name == Ingredient_Inventory.Instance.IngrEnumToName(IngredientType.Bone_Broth))
        {
            FinishQuestStep(); // Complete and clean up quest
        }
    }
}
