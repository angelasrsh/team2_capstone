using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Click_Journal_Recipe_Quest_Step : Dialogue_Quest_Step
{
    private bool journalHasBeenOpened = false;
    private bool recipeClicked = false;
    private bool journalHasBeenClosed = false;
    private bool journalRecipeClickFirstDialogueComplete = false;
    private bool journalRecipeClickDialogueComplete = false;
    private bool journalRecipeClickDialogueStarted = false;

    [SerializeField] private Dish_Data DishToMake; // Which dish the player must click. Set to one-day blinding stew.
    override protected void OnEnable()
    {
        Game_Events_Manager.Instance.onJournalToggle += JournalToggled;
        Game_Events_Manager.Instance.onDishDetailsClick += RecipeClick;
        Game_Events_Manager.Instance.onDialogueComplete += onDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox += endQuest;

        base.OnEnable();
    }

    // Unsubscribe to clean up
    override protected void OnDisable()
    {
        Game_Events_Manager.Instance.onJournalToggle -= JournalToggled;
        Game_Events_Manager.Instance.onDishDetailsClick -= RecipeClick;
        Game_Events_Manager.Instance.onDialogueComplete -= onDialogComplete;
        // Game_Events_Manager.Instance.onEndDialogBox -= endQuest;

        base.OnDisable();
    }

    private void Start()
    {
        DelayedDialogue(0, 0, false);        
    }

    private void onDialogComplete(string dialogKey)
    {
        // So we don't start the next dialogue too soon
        if (dialogKey == "Journal.Click_Journal_Recipe")
            journalRecipeClickFirstDialogueComplete = true; 
        // Need to make sure player actually sees this instruction
        if (dialogKey == "Journal.Click_Journal_Recipe_Text")
            journalRecipeClickDialogueComplete = true;
        

            
        checkPlayDialog();
    }

    /// <summary>
    /// Track which dialogue to give
    /// </summary>
    private void checkPlayDialog()
    {
        if (journalHasBeenOpened && !recipeClicked) // Opened journal but didn't click recipe
            DelayedDialogue(0, 0, false, "Journal.Click_Journal_Recipe");
        else if (recipeClicked && !journalRecipeClickDialogueStarted && journalRecipeClickFirstDialogueComplete)
        { // Opened journal and clicked recipe but didn't get instruction yet
            DelayedDialogue(0, 0, false, "Journal.Click_Journal_Recipe_Text");
            journalRecipeClickDialogueStarted = true;
        }
        else if (recipeClicked && !journalHasBeenClosed && journalRecipeClickDialogueComplete) // Did steps and read dialogue but journal is still open
            if (SystemInfo.deviceType != DeviceType.Handheld && !simulateMobile)
                DelayedDialogue(10, 0, false, "Journal.Close_Journal_PC"); // Play in 10 secs (or not if quest is finished and destroyed)
            else
                DelayedDialogue(10, 0, false, "Journal.Close_Journal_Mobile"); // Play in 10 secs (or not if quest is finished and destroyed)

        if (journalRecipeClickDialogueComplete && journalHasBeenClosed && journalHasBeenOpened && recipeClicked)
                FinishQuestStep();

    }
    
   


    #region Events
    /// <summary>
    /// Step 1 - player must open journal
    /// </summary>
    /// <param name="isOpen"></param>
    private void JournalToggled(bool isOpen)
    {
        if (isOpen)
            journalHasBeenOpened = true; // Finish and destroy this object
        else if (journalHasBeenOpened && !isOpen)
            journalHasBeenClosed = true;

        checkPlayDialog();
    }


    /// <summary>
    /// Step 2- click on recipe
    /// Note that recipe has been clicked and finish quest step if the journal is done talking
    /// </summary>
    /// <param name="dishData"></param>
    private void RecipeClick(Dish_Data dishData)
    {
        if (dishData == DishToMake)
            recipeClicked = true;

        checkPlayDialog();
    }

    #endregion

}
