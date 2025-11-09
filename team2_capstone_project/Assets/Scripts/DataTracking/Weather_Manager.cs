using System.Collections;
using System.Collections.Generic;
using Grimoire;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Weather_Manager : MonoBehaviour
{
    public static Weather_Manager Instance;

    [Header("Global Weather Settings")]
    [Range(0f, 1f)] public float rainChance = 0.3f;
    public bool isRaining = false;

    [Header("Shared Visuals")]
    public Material clearSkybox;
    public Material cloudySkybox;
    public GameObject rainSystemPrefab;

    [Header("Audio")]
    public float fadeDuration = 2f;

    private GameObject activeRainSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Room_Data room = Room_Manager.GetRoomFromActiveScene();

        // special case: stop music in foraging area to let weather logic take over
        if (room != null && room.roomID == Room_Data.RoomID.Foraging_Area_Whitebox)
        {
            Debug.Log("[WeatherManager] Foraging area loaded — stopping default music before weather logic.");

            if (Music_Persistence.instance?.musicSource != null && Music_Persistence.instance.musicSource.isPlaying)
                Music_Persistence.instance.musicSource.Stop();
        }

        // apply as usual
        ApplyWeatherForCurrentScene();
    }


    private void Start()
    {
        var data = Save_Manager.GetGameData();
        isRaining = data != null ? data.isRaining : Random.value < rainChance;
        ApplyWeatherForCurrentScene();
        Debug.Log($"[Weather_Manager] Start() initialized. RainChance={rainChance}, Rolled={isRaining}");
    }

    public void ApplyWeatherForCurrentScene()
    {
        Room_Data room = Room_Manager.GetRoomFromActiveScene();
        if (room == null)
        {
            Debug.Log("[WeatherManager] No Room_Data for this scene (likely title or non-game scene) — skipping weather/audio logic.");
            return;
        }

        // skip main menu
        if (room.roomID == Room_Data.RoomID.Main_Menu)
        {
            Debug.Log("[WeatherManager] Skipping weather/audio for Main Menu.");
            return;
        }

        Debug.Log($"[WeatherManager] Applying weather for scene: {room.name}, isRaining={isRaining}");

        // --- Skybox and visuals ---
        RenderSettings.skybox = isRaining ? cloudySkybox : clearSkybox;
        HandleRainVisuals(room);

        // --- Music ---
        if (isRaining)
        {
            if (room.rainMusic != null)
            {
                Debug.Log("[WeatherManager] Playing rain-specific music for this room.");
                Music_Persistence.instance.CheckMusic(room.rainMusic, room.rainMusicVolume);
            }
            else if (room.music != null)
                Music_Persistence.instance.CheckMusic(room.music, room.musicVolume);
            else
                Music_Persistence.instance.StopMusic();
        }
        else
        {
            if (room.music != null)
                Music_Persistence.instance.CheckMusic(room.music, room.musicVolume);
            else
                Music_Persistence.instance.StopMusic();
        }

        // --- Ambient (handles rain vs normal ambient) ---
        AudioClip targetClip;
        float targetVolume;

        if (isRaining)
        {
            if (room.rainAmbient != null)
            {
                targetClip = room.rainAmbient;
                targetVolume = room.rainAmbientVolume;
            }
            else
            {
                // fallback if rain ambient not set
                targetClip = room.ambientSound;
                targetVolume = room.ambientVolume;
            }
        }
        else
        {
            targetClip = room.ambientSound;
            targetVolume = room.ambientVolume;
        }

        if (Music_Persistence.instance != null)
        {
            if (targetClip != null)
                Music_Persistence.instance.CheckAmbient(targetClip, targetVolume);
            else
                Music_Persistence.instance.StopAmbient();
        }
    }

    private void HandleRainVisuals(Room_Data room)
    {
        if (room.isOutdoorScene && isRaining)
        {
            if (activeRainSystem == null && rainSystemPrefab != null)
            {
                activeRainSystem = Instantiate(rainSystemPrefab);
                Debug.Log("[WeatherManager] Rain system activated.");
            }
        }
        else
        {
            if (activeRainSystem != null)
            {
                Destroy(activeRainSystem);
                activeRainSystem = null;
                Debug.Log("[WeatherManager] Rain system deactivated.");
            }
        }
    }

    public void ResetWeatherForNewDay()
    {
        bool newRain = Random.value < rainChance;

        // If rain stops at day change, fade out
        if (!newRain && isRaining)
            StartCoroutine(FadeOutRainAmbient());

        isRaining = newRain;
        SaveWeatherState();

        // Apply new weather immediately to current room
        ApplyWeatherForCurrentScene();
    }

    private IEnumerator FadeOutRainAmbient()
    {
        if (Audio_Manager.instance == null) yield break;

        var ambientSource = typeof(Audio_Manager)
            .GetField("ambientSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(Audio_Manager.instance) as AudioSource;

        if (ambientSource == null || !ambientSource.isPlaying) yield break;

        float startVol = ambientSource.volume;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        ambientSource.Stop();
    }

    private void SaveWeatherState()
    {
        var data = Save_Manager.GetGameData() ?? new GameData();
        data.isRaining = isRaining;
        Save_Manager.SetGameData(data);
        Save_Manager.instance.SaveGameData();
    }
}
