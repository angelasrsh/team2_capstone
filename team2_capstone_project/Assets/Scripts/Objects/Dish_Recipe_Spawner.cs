using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Grimoire; 

public class Dish_Recipe_Spawner : MonoBehaviour
{
    [Header("Recipe Spawn Settings")]
    [SerializeField] private GameObject recipeCollectiblePrefab;
    [SerializeField] private Dish_Data[] possibleRecipes;

    private GameObject currentRecipeObj;

    private void OnEnable()
    {
        Debug.Log("[Dish_Recipe_Spawner] Enabled and subscribed.");
        Day_Turnover_Manager.OnDayStarted += TrySpawnDailyRecipe;
    }

    private void OnDisable() => Day_Turnover_Manager.OnDayStarted -= TrySpawnDailyRecipe;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => Player_Progress.Instance != null && Recipe_Spawn_Manager.Instance != null);
        TrySpawnDailyRecipe();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("[Dish_Recipe_Spawner] Manual spawn attempt.");
            TrySpawnDailyRecipe();
        }
    }
#endif

    private void TrySpawnDailyRecipe()
    {
        if (Player_Progress.Instance.HasCollectedRecipeToday())
        {
            Debug.Log("[Dish_Recipe_Spawner] Player already collected today's recipe. Skipping spawn.");
            return;
        }

        if (possibleRecipes == null || possibleRecipes.Length == 0)
        {
            Debug.LogWarning("[Dish_Recipe_Spawner] possibleRecipes array is EMPTY!");
            return;
        }

        int todayIndex = (int)Day_Turnover_Manager.Instance.CurrentDay;
        var progress = Player_Progress.Instance;
        Debug.Log($"[Dish_Recipe_Spawner] Got Player_Progress, todayIndex = {todayIndex}");

        if (progress.GetLastRecipeSpawnedDay() == todayIndex)
        {
            var saved = progress.GetActiveDailyRecipe();
            if (saved != null)
            {
                SpawnRecipe((Dish_Data.Dishes)saved);
                return;
            }
            else
            {
                Debug.LogWarning("[Dish_Recipe_Spawner] Last spawn day matches but recipe data was null, forcing new spawn.");
            }
        }

        Dish_Data newDish = possibleRecipes[Random.Range(0, possibleRecipes.Length)];
        Debug.Log($"[Dish_Recipe_Spawner] Picked new daily recipe {newDish.dishType}");

        progress.SetDailyRecipe(newDish.dishType);
        progress.MarkDailyRecipeSpawned(todayIndex);

        SpawnRecipe(newDish.dishType);
    }


    private void SpawnRecipe(Dish_Data.Dishes dishType)
    {
        if (currentRecipeObj != null)
            Destroy(currentRecipeObj);

        Dish_Data data = System.Array.Find(possibleRecipes, d => d.dishType == dishType);
        if (data == null)
        {
            Debug.LogError($"[Dish_Recipe_Spawner] Dish_Data for {dishType} not found!");
            return;
        }

        // Get a random free spawn point
        Transform spawnPoint = Recipe_Spawn_Manager.Instance.GetRandomAvailablePoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("[Dish_Recipe_Spawner] No free spawn point available.");
            return;
        }

        currentRecipeObj = Instantiate(recipeCollectiblePrefab, spawnPoint.position, Quaternion.identity);
        var recipeComp = currentRecipeObj.GetComponent<Recipe_Collectible>();

        if (recipeComp == null) return;
        recipeComp.Initialize(data);

        Debug.Log($"[Dish_Recipe_Spawner] Spawned recipe {data.dishType} at {spawnPoint.position}");
    }
}
