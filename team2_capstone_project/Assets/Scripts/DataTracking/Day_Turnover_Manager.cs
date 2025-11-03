using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;

public class Day_Turnover_Manager : MonoBehaviour
{
  public static Day_Turnover_Manager Instance;

  public enum WeekDay { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
  public WeekDay CurrentDay { get; private set; } = WeekDay.Monday;
  public enum TimeOfDay { Morning, Evening }
  public TimeOfDay currentTimeOfDay = TimeOfDay.Morning;

  // Dict for tracking dishes + customers served each day
  private Dictionary<Dish_Data, int> dishesServed = new Dictionary<Dish_Data, int>();
  private Dictionary<string, int> customersServed = new Dictionary<string, int>();
  private int totalCurrencyEarned = 0;

  public static event System.Action<Day_Summary_Data> OnDayEnded;
  public static event System.Action OnDayStarted;


  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);
    }
    else Destroy(gameObject);
  }

  public void SetTimeOfDay(TimeOfDay newTime)
  {
    currentTimeOfDay = newTime;
    Debug.Log("Time of day set to: " + newTime);
  }

  public void RecordDishServed(Dish_Data dish, int currencyGained, string customerName)
  {
    if (dish != null)
    {
      if (!dishesServed.ContainsKey(dish)) dishesServed[dish] = 0;
      dishesServed[dish]++;
    }

    if (!string.IsNullOrEmpty(customerName))
    {
      if (!customersServed.ContainsKey(customerName)) customersServed[customerName] = 0;
      customersServed[customerName]++;
    }

    totalCurrencyEarned += currencyGained;
    Game_Manager.Instance.playerProgress.AddMoney(currencyGained);
  }

  public void EndDay()
  {
    var nextDay = (WeekDay)(((int)CurrentDay + 1) % 7);

    var summary = new Day_Summary_Data
    {
      currentDay = CurrentDay,
      nextDay = nextDay,
      dishesServed = new Dictionary<Dish_Data, int>(dishesServed),
      customersServed = new Dictionary<string, int>(customersServed),
      totalCurrencyEarned = totalCurrencyEarned
    };

    OnDayEnded?.Invoke(summary);

    // reset tracking
    dishesServed.Clear();
    customersServed.Clear();
    totalCurrencyEarned = 0;
    CurrentDay = nextDay;
    currentTimeOfDay = TimeOfDay.Morning;
    Player_Progress.Instance.ResetDailyRecipeFlags();

    // Reset weather globally for new day
    if (Weather_Manager.Instance != null)
      Weather_Manager.Instance.ResetWeatherForNewDay();
    else
      Debug.LogWarning("[Day_Turnover_Manager] No WeatherManager found — weather not reset.");

    if (Choose_Menu_Items.instance == null)
      Debug.LogWarning("[Day_Turnover_Manager] No Choose_Menu_Items found when ending day — will refresh next load.");
    else
    {
      Choose_Menu_Items.instance.GenerateDailyPool();
      Debug.Log($"Daily menu refreshed for {CurrentDay}.");
      Save_Manager.instance?.AutoSave();
    }

    // fire after reset
    OnDayStarted?.Invoke();

    // Show menu UI
    Choose_Menu_UI ui = FindObjectOfType<Choose_Menu_UI>();
    if (ui != null)
    {
      ui.gameObject.SetActive(true);
    }
  }

  public void SetCurrentDay(WeekDay newDay) => CurrentDay = newDay;
}

