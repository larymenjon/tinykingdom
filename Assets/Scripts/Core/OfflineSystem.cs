using System;
using UnityEngine;

[DisallowMultipleComponent]
public class OfflineSystem : MonoBehaviour
{
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private long maxOfflineSeconds = 43200L;

    private bool offlineAppliedThisSession;

    public event Action<long, float> OfflineProductionApplied;

    private void Awake()
    {
        if (saveManager == null)
        {
            saveManager = SaveManager.Instance;
        }
    }

    private void OnEnable()
    {
        if (saveManager == null)
        {
            saveManager = SaveManager.Instance;
        }

        if (saveManager != null)
        {
            saveManager.SaveLoaded += HandleSaveLoaded;

            if (saveManager.HasLoadedSave)
            {
                CalculateOfflineProduction();
            }
        }
    }

    private void Start()
    {
        if (saveManager != null && saveManager.HasLoadedSave)
        {
            CalculateOfflineProduction();
        }
    }

    private void OnDisable()
    {
        if (saveManager != null)
        {
            saveManager.SaveLoaded -= HandleSaveLoaded;
        }
    }

    public void CalculateOfflineProduction()
    {
        if (offlineAppliedThisSession || saveManager == null || !saveManager.HasLoadedSave)
        {
            return;
        }

        GameSaveData save = saveManager.CurrentSave;
        long savedTicks = save.lastPlayedAtUtcTicks > 0 ? save.lastPlayedAtUtcTicks : save.lastSavedAtUtcTicks;
        if (savedTicks <= 0)
        {
            return;
        }

        DateTime savedUtc = new DateTime(savedTicks, DateTimeKind.Utc);
        TimeSpan offlineSpan = DateTime.UtcNow - savedUtc;
        long secondsOffline = (long)Math.Floor(offlineSpan.TotalSeconds);

        if (secondsOffline <= 0)
        {
            return;
        }

        if (secondsOffline > maxOfflineSeconds)
        {
            secondsOffline = maxOfflineSeconds;
        }

        float offlineMultiplier = resourceManager != null ? resourceManager.GetOfflineMultiplier() : 1f;

        if (resourceManager != null)
        {
            resourceManager.ApplyOfflineProduction(secondsOffline, offlineMultiplier);
        }

        save.offlineProgressSeconds = secondsOffline;
        save.lastPlayedAtUtcTicks = DateTime.UtcNow.Ticks;
        saveManager.SaveGame();

        offlineAppliedThisSession = true;
        OfflineProductionApplied?.Invoke(secondsOffline, offlineMultiplier);
    }

    private void HandleSaveLoaded(GameSaveData saveData)
    {
        CalculateOfflineProduction();
    }
}
