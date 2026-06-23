using System;
using System.Collections;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string LegacySavePrefsKey = "TinyKingdom.GameSave";

    [Header("Local Save")]
    [SerializeField] private bool loadSaveOnAwake = true;
    [SerializeField] private string localSaveFileName = "game_save.json";
    [SerializeField] private float autoSaveIntervalSeconds = 30f;
    [SerializeField] private bool saveOnApplicationPause = true;
    [SerializeField] private bool saveOnApplicationQuit = true;
    [SerializeField] private bool saveOnApplicationFocusLost = true;
    [SerializeField] private bool prettyPrintJson;

    [Header("Cloud Upload")]
    [SerializeField] private bool uploadToPlayFabOnSave = true;
    [SerializeField] private bool uploadToFirebaseOnSave = true;
    [SerializeField] private bool uploadOnlyWhenInternetAvailable = true;
    [SerializeField] private UploadToPlayFab playFabUploader;
    [SerializeField] private UploadToFirebase firebaseUploader;

    [Header("Lightweight PlayerPrefs")]
    [SerializeField] private string masterVolumePrefsKey = "TinyKingdom.Settings.MasterVolume";
    [SerializeField] private string musicVolumePrefsKey = "TinyKingdom.Settings.MusicVolume";
    [SerializeField] private string sfxVolumePrefsKey = "TinyKingdom.Settings.SfxVolume";
    [SerializeField] private string tutorialCompletedPrefix = "TinyKingdom.Tutorial.";

    private GameSaveData currentSave;
    private bool hasLoadedSave;
    private bool hasPendingCloudBackup;
    private bool isSaving;
    private Coroutine autoSaveCoroutine;

    public event Action<GameSaveData> SaveLoaded;
    public event Action<GameSaveData> SaveChanged;
    public event Action<GameSaveData> SaveStored;

    public GameSaveData CurrentSave
    {
        get
        {
            if (currentSave == null)
            {
                currentSave = GameSaveData.CreateDefault();
            }

            return currentSave;
        }
    }

    public bool HasLoadedSave
    {
        get { return hasLoadedSave; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadSaveOnAwake)
        {
            LoadGame();
        }

        autoSaveCoroutine = StartCoroutine(AutoSaveLoop());
    }

    public GameSaveData LoadGame()
    {
        string json = ReadLocalSaveJson();
        GameSaveData loadedSave;

        if (string.IsNullOrWhiteSpace(json))
        {
            loadedSave = GameSaveData.CreateDefault();
        }
        else
        {
            loadedSave = JsonUtility.FromJson<GameSaveData>(json);
            if (loadedSave == null)
            {
                loadedSave = GameSaveData.CreateDefault();
            }
        }

        loadedSave.Normalize();
        currentSave = loadedSave;
        hasLoadedSave = true;
        SaveLoaded?.Invoke(currentSave);
        return currentSave;
    }

    public void ReplaceCurrentSave(GameSaveData saveData)
    {
        if (saveData == null)
        {
            currentSave = GameSaveData.CreateDefault();
        }
        else
        {
            currentSave = saveData;
            currentSave.Normalize();
        }

        hasLoadedSave = true;
        SaveChanged?.Invoke(currentSave);
    }

    public void MarkSaveChanged()
    {
        CurrentSave.lastPlayedAtUtcTicks = DateTime.UtcNow.Ticks;
        hasPendingCloudBackup = true;
        SaveChanged?.Invoke(CurrentSave);
    }

    public void SaveGame()
    {
        SaveGameLocal();
        TryUploadCloudBackup();
    }

    public void SaveGameLocal()
    {
        if (isSaving)
        {
            return;
        }

        isSaving = true;

        try
        {
            GameSaveData save = CurrentSave;
            save.Normalize();
            save.lastSavedAtUtcTicks = DateTime.UtcNow.Ticks;
            save.lastPlayedAtUtcTicks = save.lastSavedAtUtcTicks;

            string json = JsonUtility.ToJson(save, prettyPrintJson);
            WriteLocalSaveJson(json);

            SaveStored?.Invoke(save);
            hasPendingCloudBackup = true;
        }
        finally
        {
            isSaving = false;
        }
    }

    public void DeleteLocalSave()
    {
        string path = GetLocalSavePath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        currentSave = GameSaveData.CreateDefault();
        hasLoadedSave = true;
        hasPendingCloudBackup = false;
        SaveChanged?.Invoke(currentSave);
    }

    public void UploadCurrentSaveToPlayFab()
    {
        if (playFabUploader == null)
        {
            Debug.LogWarning("SaveManager: PlayFab uploader is not assigned.");
            return;
        }

        StartCoroutine(playFabUploader.UploadGameSave(CurrentSave, HandlePlayFabUploadResult));
    }

    public void UploadCurrentSaveToFirebase()
    {
        if (firebaseUploader == null)
        {
            Debug.LogWarning("SaveManager: Firebase uploader is not assigned.");
            return;
        }

        StartCoroutine(firebaseUploader.UploadGameSave(CurrentSave, HandleFirebaseUploadResult));
    }

    public void SetMasterVolume(float volume)
    {
        PlayerPrefs.SetFloat(masterVolumePrefsKey, Clamp01(volume));
        PlayerPrefs.Save();
    }

    public float GetMasterVolume(float defaultValue = 1f)
    {
        return PlayerPrefs.GetFloat(masterVolumePrefsKey, defaultValue);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(musicVolumePrefsKey, Clamp01(volume));
        PlayerPrefs.Save();
    }

    public float GetMusicVolume(float defaultValue = 1f)
    {
        return PlayerPrefs.GetFloat(musicVolumePrefsKey, defaultValue);
    }

    public void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(sfxVolumePrefsKey, Clamp01(volume));
        PlayerPrefs.Save();
    }

    public float GetSfxVolume(float defaultValue = 1f)
    {
        return PlayerPrefs.GetFloat(sfxVolumePrefsKey, defaultValue);
    }

    public void SetTutorialCompleted(string tutorialId, bool completed = true)
    {
        if (string.IsNullOrWhiteSpace(tutorialId))
        {
            return;
        }

        PlayerPrefs.SetInt(GetTutorialPrefsKey(tutorialId), completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsTutorialCompleted(string tutorialId)
    {
        if (string.IsNullOrWhiteSpace(tutorialId))
        {
            return false;
        }

        return PlayerPrefs.GetInt(GetTutorialPrefsKey(tutorialId), 0) == 1;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && saveOnApplicationPause)
        {
            SaveGame();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && saveOnApplicationFocusLost)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        if (saveOnApplicationQuit)
        {
            SaveGame();
        }
    }

    private void OnDestroy()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }
    }

    private IEnumerator AutoSaveLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(1f, autoSaveIntervalSeconds));

        while (true)
        {
            yield return wait;

            if (isSaving)
            {
                continue;
            }

            if (hasLoadedSave)
            {
                SaveGame();
            }
        }
    }

    private string ReadLocalSaveJson()
    {
        string path = GetLocalSavePath();
        if (!File.Exists(path))
        {
            if (PlayerPrefs.HasKey(LegacySavePrefsKey))
            {
                string legacyJson = PlayerPrefs.GetString(LegacySavePrefsKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(legacyJson))
                {
                    WriteLocalSaveJson(legacyJson);
                    PlayerPrefs.DeleteKey(LegacySavePrefsKey);
                    PlayerPrefs.Save();
                    return legacyJson;
                }
            }

            return string.Empty;
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("SaveManager: failed to read local save file. " + exception.Message);
            return string.Empty;
        }
    }

    private void WriteLocalSaveJson(string json)
    {
        string path = GetLocalSavePath();

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, json);
        }
        catch (Exception exception)
        {
            Debug.LogError("SaveManager: failed to write local save file. " + exception.Message);
        }
    }

    private string GetLocalSavePath()
    {
        return Path.Combine(Application.persistentDataPath, localSaveFileName);
    }

    private string GetTutorialPrefsKey(string tutorialId)
    {
        return tutorialCompletedPrefix + tutorialId;
    }

    private void TryUploadCloudBackup()
    {
        if (!hasPendingCloudBackup)
        {
            return;
        }

        if (uploadOnlyWhenInternetAvailable && Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        hasPendingCloudBackup = false;

        if (uploadToPlayFabOnSave)
        {
            UploadCurrentSaveToPlayFab();
        }

        if (uploadToFirebaseOnSave)
        {
            UploadCurrentSaveToFirebase();
        }
    }

    private void HandlePlayFabUploadResult(bool success, string message)
    {
        if (!success)
        {
            Debug.LogWarning("PlayFab upload failed: " + message);
            hasPendingCloudBackup = true;
        }
    }

    private void HandleFirebaseUploadResult(bool success, string message)
    {
        if (!success)
        {
            Debug.LogWarning("Firebase upload failed: " + message);
            hasPendingCloudBackup = true;
        }
    }

    private static float Clamp01(float value)
    {
        return Mathf.Clamp01(value);
    }
}
