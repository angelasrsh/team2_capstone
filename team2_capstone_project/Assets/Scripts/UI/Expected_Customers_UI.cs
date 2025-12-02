using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Expected_Customers_UI : MonoBehaviour
{
    public static Expected_Customers_UI Instance;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI customerText;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float countUpDuration = 1.5f;

    private Coroutine animationRoutine;
    private bool hasPlayedAnimationForToday = false;
    private int lastShownCount = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        Day_Plan_Manager.OnPlanUpdated += HandlePlanUpdated;

        Player_Progress.OnProgressLoaded += RefreshAfterProgressLoads;
    }

    private void OnDisable()
    {
        Player_Progress.OnProgressLoaded -= RefreshAfterProgressLoads;
    }

    private void RefreshAfterProgressLoads()
    {
        TryRefreshUI();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        // Wait 1 frame to ensure Player_Progress and Tutorial Manager finish initializing
        yield return null;

        TryRefreshUI();
    }

    private void HandlePlanUpdated(int expectedCount)
    {
        // Always reset animation state when a new plan is set (i.e., start of a new day)
        hasPlayedAnimationForToday = false;
        ShowExpectedCustomerCount(expectedCount, animate: true);
    }

    private void TryRefreshUI()
    {
        Debug.Log(Player_Progress.Instance.InGameplayTutorial ? "In Gameplay Tutorial" : "Not in Gameplay Tutorial");

        // If the player is in tutorial, always show 1 expected customer immediately
        if (Player_Progress.Instance != null && Player_Progress.Instance.InGameplayTutorial)
        {
            ShowExpectedCustomerCount(1, animate: false);
            return;
        }
        else if (Player_Progress.Instance == null)
        {
            Debug.LogWarning("Expected_Customers_UI: Player_Progress instance is null. Cannot determine tutorial state.");
            return;
        }

        // Otherwise use the normal Day_Plan_Manager value when present
        if (Day_Plan_Manager.instance != null)
        {
            int planned = Day_Plan_Manager.instance.customersPlannedForEvening;
            ShowExpectedCustomerCount(planned, animate: !hasPlayedAnimationForToday);
        }
    }

    public void ShowExpectedCustomerCount(int count, bool animate = true)
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        if (!animate || hasPlayedAnimationForToday)
        {
            canvasGroup.alpha = 1f;
            customerText.text = $"Expected Customers: {count}";
            lastShownCount = count;
            return;
        }

        animationRoutine = StartCoroutine(AnimateExpectedCustomerCount(count));
    }

    private IEnumerator AnimateExpectedCustomerCount(int target)
    {
        hasPlayedAnimationForToday = true;  // only play once per day

        // Fade in
        float fadeT = 0f;
        while (fadeT < fadeDuration)
        {
            fadeT += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeT / fadeDuration);
            yield return null;
        }

        // Count-up animation
        int current = 0;
        float elapsed = 0f;
        while (elapsed < countUpDuration)
        {
            elapsed += Time.deltaTime;
            current = Mathf.RoundToInt(Mathf.Lerp(0, target, elapsed / countUpDuration));
            customerText.text = $"Expected Customers: {current}";
            yield return null;
        }

        customerText.text = $"Expected Customers: {target}";
        lastShownCount = target;
    }

    public void ResetAnimationState() => hasPlayedAnimationForToday = false;
}
