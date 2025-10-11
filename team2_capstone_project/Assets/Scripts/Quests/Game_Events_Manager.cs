using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This singleton script attaches to the QuestManager to listen for changes in quest state
/// And broadcast when those changes occur.
/// 
/// This script must run first to initialize quests (set in script execution order)
/// </summary>
public class Game_Events_Manager : MonoBehaviour
{
    public static Game_Events_Manager Instance { get; private set; }
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Tell objects when a quest state changes (and which quest, by ID)

    /////////// PLAYER EVENTS ////////////
    #region Player Events

    public event Action onPlayerMove;
    /// <summary>
    /// Broadcast the onPlayerMove event
    /// </summary>
    public void PlayerMoved()
    {
        if (onPlayerMove != null) // Guessing null means no subscribers?
            onPlayerMove();
        //Debug.Log("[G_E_M] Player Moved");
    }


    public event Action<bool> onInventoryToggle;
    /// <summary>
    /// Broadcast the InventoryToggled event
    /// </summary>
    /// <param name="isOpen"> true if the inventory has just been opened; false otherwise </param>
    public void InventoryToggled(bool isOpen)
    {
        if (onInventoryToggle != null)
            onInventoryToggle(isOpen);
    }

    public event Action<bool> onJournalToggle;
    /// <summary>
    /// Broadcast the JournalToggled event
    /// </summary>
    /// <param name="isOpen"> true if the Journal has just been opened; false otherwise </param>
    public void JournalToggled(bool isOpen)
    {
        if (onJournalToggle != null)
            onJournalToggle(isOpen);
    }


    public event Action<Dish_Data> onDishDetailsClick;
    /// <summary>
    /// Broadcast the RecipeClick event and send the dish being sent
    /// </summary>
    public void DishDetailsClick(Dish_Data dishData)
    {
        if (onDishDetailsClick != null)
            onDishDetailsClick(dishData);
    }

    public event Action onHarvestRequirementsMet;
    /// <summary>
    /// Broadcast event for unblocking the exit to the world map
    /// </summary>
    public void HarvestRequirementsMet()
    {
        if (onHarvestRequirementsMet != null)
            onHarvestRequirementsMet();
    }




    public event Action onHarvest;
    /// <summary>
    /// Broadcast the onHarvest event
    /// </summary>
    public void Harvest()
    {
        if (onHarvest != null)
            onHarvest();
    }
    #endregion


    //////////// QUEST EVENTS /////////////
    #region Quest Events
    public event Action<string> onStartQuest;
    /// <summary>
    /// Broadcast the start quest event
    /// </summary>
    /// <param name="id"> Name of the quest </param>
    public void StartQuest(string id)
    {
        if (onStartQuest != null)
        {
            onStartQuest(id);
            Debug.Log($"[G_E_M] Starting quest {id}");
        }
            
        
    }

    public event Action<string> onAdvanceQuest;
    /// <summary>
    /// Broadcast the a quest changing to the next step
    /// </summary>
    /// <param name="id"> Name of the quest </param>
    public void AdvanceQuest(string id)
    {
        if (onAdvanceQuest != null)
            onAdvanceQuest(id);
    }

    public event Action<string> onFinishQuest;
    /// <summary>
    /// Broadcast the finish quest event
    /// </summary>
    /// <param name="id"> Name of the quest </param>
    public void FinishQuest(string id)
    {
        if (onFinishQuest != null)
            onFinishQuest(id);
    }

    public event Action<Quest> onQuestStateChange;
    /// <summary>
    /// Broadcast that a quest's state has changed so listening objects know
    /// </summary>
    /// <param name="quest"> The quest whose state changed </param>
    public void QuestStateChange(Quest quest)
    {
        if (onQuestStateChange != null)
            onQuestStateChange(quest);
        //Debug.Log($"[G_E_M] Changing state of quest {quest.Info.id} to {quest.state}");
    }


    public event Action<string, int> onQuestStepChange;
    public void QuestStepChange(string id, int stepIndex)
    {
        if (onQuestStepChange != null)
            onQuestStepChange(id, stepIndex);

    }


    public event Action<string, bool> onSetQuestPaused;
    public void SetQuestPaused(string id, bool isPaused)
    {
        if (onSetQuestPaused != null)
            onSetQuestPaused(id, isPaused);
    }
    #endregion

    //////////// SCENE TRANSITION EVENTS ///////////////
    #region Scene Transition Events

    public event Action<Room_Data.RoomID, Room_Data.RoomID> onRoomChange;
    public void RoomChange(Room_Data.RoomID currentRoom, Room_Data.RoomID exitingTo)
    {
        if (onRoomChange != null)
            onRoomChange(currentRoom, exitingTo);
    }


    #endregion

    /////////// CAFE INTERACTION EVENTS /////////
    #region Cafe Interaction Events
    public event Action onGetOrder;
    public void GetOrder()
    {
        if (onGetOrder != null)
            onGetOrder();
    }

    public event Action onServeCustomer;
    public void ServeCustomer()
    {
        if (onServeCustomer != null)
            onServeCustomer();
    }

    public event Action onBeginDialogBox;
    public void BeginDialogueBox()
    {
        if (onBeginDialogBox != null)
            onBeginDialogBox();
    }

    public event Action onEndDialogBox;
    public void EndDialogBox()
    {
        if (onEndDialogBox != null)
            onEndDialogBox();
    }
    #endregion


    /////////// INVENTORY EVENTS /////////
    #region Inventory events
    public event Action<Ingredient_Data> onResourceAdd;
    public void ResourceAdd(Ingredient_Data ingredient)
    {
        if (onResourceAdd != null)
            onResourceAdd(ingredient);
    }

    public event Action<Ingredient_Data> onResourceRemove;
    public void ResourceRemove(Ingredient_Data ingredient)
    {
        if (onResourceRemove != null)
            onResourceRemove(ingredient);
    }

    public event Action<Dish_Data> onDishAdd;
    public void DishAdd(Dish_Data dish)
    {
        if (onDishAdd != null)
            onDishAdd(dish);
    }

    public event Action<Dish_Data> onDishRemove;
    public void DishRemove(Dish_Data dish)
    {
        if (onDishRemove != null)
            onDishRemove(dish);
    }

    #endregion

    

}
