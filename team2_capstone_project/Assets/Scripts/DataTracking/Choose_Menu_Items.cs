using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Choose_Menu_Items : MonoBehaviour
{
  public static Choose_Menu_Items instance;
  private List<Dish_Data.Dishes> dailyPool = new List<Dish_Data.Dishes>();
  private List<Dish_Data.Dishes> dishesSelected = new List<Dish_Data.Dishes>();

  public static event System.Action<List<Dish_Data.Dishes>> OnDailyMenuSelected;
  public static event System.Action OnMenuSelectedNoParams;

  // Dish pool and selection constraints
  [SerializeField] private int minPoolSize = 3;
  [SerializeField] private int maxPoolSize = 5;
  [SerializeField] private int minSelect = 1;
  [SerializeField] private int maxSelect = 2;

  // Customer constraints for the day
  [SerializeField] private int minCustomersForDay = 2;
  [SerializeField] private int maxCustomersForDay = 5;

  private void Awake()
  {
    if (instance == null)
    {
      instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else
    {
      Destroy(gameObject);
    }
  }

  private void Start()
  {
    if (dailyPool.Count == 0)
    {
      GenerateDailyPool();
    }
  }

  /// <summary>
  /// Builds the pool of dishes the player can choose from for the day.
  /// </summary>
  public void GenerateDailyPool()
  {
    dailyPool.Clear();
    dishesSelected.Clear();

    var learned = Player_Progress.Instance.GetUnlockedDishes();
    if (learned == null || learned.Count == 0)
    {
      Debug.LogWarning("No unlocked recipes available!");
      return;
    }

    int poolSize = Random.Range(minPoolSize, maxPoolSize + 1);
    poolSize = Mathf.Min(poolSize, learned.Count);

    // Shuffle learned recipes
    List<Dish_Data.Dishes> shuffled = new List<Dish_Data.Dishes>(learned);
    for (int i = 0; i < shuffled.Count; i++)
    {
      int rand = Random.Range(i, shuffled.Count);
      (shuffled[i], shuffled[rand]) = (shuffled[rand], shuffled[i]);
    }

    dailyPool.AddRange(shuffled.GetRange(0, poolSize));
    Debug.Log($"Daily pool generated with {dailyPool.Count} options: {string.Join(", ", dailyPool)}");
  }

  public List<Dish_Data.Dishes> GetDailyPool() => new List<Dish_Data.Dishes>(dailyPool);

  public void AddDish(Dish_Data.Dishes dishType)
  {
    if (!dailyPool.Contains(dishType))
    {
      Debug.LogWarning($"{dishType} is not available in today's pool!");
      return;
    }

    if (dishesSelected.Contains(dishType))
    {
      Debug.Log($"{dishType} is already selected.");
      return;
    }

    if (dishesSelected.Count >= maxSelect)
    {
      Debug.LogWarning($"Cannot select more than {maxSelect} dishes!");
      return;
    } 

    dishesSelected.Add(dishType);
    Debug.Log($"{dishType} added. Total dishes: {dishesSelected.Count}");
  }

  public void RemoveDish(Dish_Data.Dishes dishType)
  {
    if (dishesSelected.Contains(dishType))
    {
      dishesSelected.Remove(dishType);
      Debug.Log($"{dishType} removed. Total dishes: {dishesSelected.Count}");
    }
    else
    {
      Debug.Log($"{dishType} is not in the selected list.");
    }
  }

  public List<Dish_Data.Dishes> GetSelectedDishes() => new List<Dish_Data.Dishes>(dishesSelected);

  public bool HasSelectedDishes() => dishesSelected.Count >= minSelect;

  /// <summary>
  /// Finalizes the daily menu selection, sets the day plan, and triggers events.
  /// </summary>
  public void NotifyMenuConfirmed()
  {
    if (!HasSelectedDishes())
    {
      Debug.LogWarning($"You must select at least {minSelect} dishes!");
      return;
    }

    // 1. Decide how many customers today (with day-based scaling)
    int customersToday = CalculateCustomersForToday();

    // 2. Save the day's plan (selected dishes + expected customer count)
    Day_Plan_Manager.instance.SetPlan(dishesSelected, customersToday);

    // 3. Notify other systems
    OnDailyMenuSelected?.Invoke(new List<Dish_Data.Dishes>(dishesSelected));
    OnMenuSelectedNoParams?.Invoke();

    Debug.Log($"Daily menu confirmed with {dishesSelected.Count} dishes and {customersToday} expected customers.");
  }

  /// <summary>
  /// Determines the number of customers for the day based on constraints and modifiers.
  /// (E.g. Thursdays and Sundays have increased traffic.)
  /// </summary>
  private int CalculateCustomersForToday()
  {
    var turnover = Day_Turnover_Manager.Instance;
    int dayIndex = turnover != null ? (int)turnover.CurrentDay : 0;

    // Base random between min/max
    int baseCount = Random.Range(minCustomersForDay, maxCustomersForDay + 1);

    // Thursdays and Sundays get boosted traffic
    var currentDay = turnover.CurrentDay;
    if (currentDay == Day_Turnover_Manager.WeekDay.Thursday ||
        currentDay == Day_Turnover_Manager.WeekDay.Sunday)
    {
      baseCount += Random.Range(1, 3); // boost by 1–2
      Debug.Log($"Increased traffic! Today ({currentDay}) has boosted customer count.");
    }

    // Clamp to ensure within absolute min/max
    return Mathf.Clamp(baseCount, minCustomersForDay, maxCustomersForDay + 3);
  }
}
