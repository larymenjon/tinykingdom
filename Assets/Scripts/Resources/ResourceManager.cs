using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ResourceManager : MonoBehaviour
{
    private const float TICK_INTERVAL_SECONDS = 1f;

    [Serializable]
    public struct ResourceProductionRule
    {
        public string resourceId;
        public float amountPerSecond;
        public bool enabled;
    }

    [Header("Production")]
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private List<ResourceProductionRule> productionRules = new List<ResourceProductionRule>();
    [SerializeField] private float offlineProductionMultiplier = 1f;

    private readonly Dictionary<string, float> productionRemainders = new Dictionary<string, float>();
    private Coroutine tickCoroutine;
    private bool isInitialized;

    public event Action<string, int, int> ResourceChanged;
    public event Action<float> TickProcessed;

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
                HandleSaveLoaded(saveManager.CurrentSave);
            }
        }
    }

    private void Start()
    {
        TryStartTickLoop();
    }

    private void OnDisable()
    {
        if (saveManager != null)
        {
            saveManager.SaveLoaded -= HandleSaveLoaded;
        }

        if (tickCoroutine != null)
        {
            StopCoroutine(tickCoroutine);
            tickCoroutine = null;
        }
    }

    public float GetOfflineMultiplier()
    {
        return Mathf.Max(0f, offlineProductionMultiplier);
    }

    public void SetOfflineMultiplier(float multiplier)
    {
        offlineProductionMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ProcessTick()
    {
        ProcessProduction(TICK_INTERVAL_SECONDS, 1f);
        TickProcessed?.Invoke(TICK_INTERVAL_SECONDS);
    }

    public void ApplyOfflineProduction(long secondsOffline, float multiplier)
    {
        if (secondsOffline <= 0)
        {
            return;
        }

        ProcessProduction(secondsOffline, multiplier);
        TickProcessed?.Invoke(secondsOffline);
    }

    public void AddResource(string resourceId, int amount)
    {
        if (!IsSaveReady() || string.IsNullOrWhiteSpace(resourceId) || amount == 0)
        {
            return;
        }

        GameSaveData save = saveManager.CurrentSave;
        int previousAmount = save.GetResourceAmount(resourceId);
        int nextAmount = previousAmount + amount;

        save.SetResourceAmount(resourceId, nextAmount);
        saveManager.MarkSaveChanged();
        ResourceChanged?.Invoke(resourceId, previousAmount, nextAmount);
    }

    public bool TryConsumeResource(string resourceId, int amount)
    {
        if (!IsSaveReady() || string.IsNullOrWhiteSpace(resourceId) || amount <= 0)
        {
            return false;
        }

        GameSaveData save = saveManager.CurrentSave;
        int previousAmount = save.GetResourceAmount(resourceId);
        if (previousAmount < amount)
        {
            return false;
        }

        int nextAmount = previousAmount - amount;
        save.SetResourceAmount(resourceId, nextAmount);
        saveManager.MarkSaveChanged();
        ResourceChanged?.Invoke(resourceId, previousAmount, nextAmount);
        return true;
    }

    public int GetResourceAmount(string resourceId)
    {
        if (!IsSaveReady() || string.IsNullOrWhiteSpace(resourceId))
        {
            return 0;
        }

        return saveManager.CurrentSave.GetResourceAmount(resourceId);
    }

    public IReadOnlyList<ResourceProductionRule> GetProductionRules()
    {
        return productionRules;
    }

    private void HandleSaveLoaded(GameSaveData saveData)
    {
        if (!isInitialized)
        {
            isInitialized = true;
            TryStartTickLoop();
        }
    }

    private void TryStartTickLoop()
    {
        if (tickCoroutine != null || saveManager == null || !saveManager.HasLoadedSave)
        {
            return;
        }

        tickCoroutine = StartCoroutine(TickLoop());
    }

    private IEnumerator TickLoop()
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(TICK_INTERVAL_SECONDS);

        while (true)
        {
            yield return wait;
            ProcessTick();
        }
    }

    private void ProcessProduction(float deltaSeconds, float multiplier)
    {
        if (!IsSaveReady() || deltaSeconds <= 0f || multiplier <= 0f)
        {
            return;
        }

        GameSaveData save = saveManager.CurrentSave;
        bool changed = false;

        for (int i = 0; i < productionRules.Count; i++)
        {
            ResourceProductionRule rule = productionRules[i];
            if (!rule.enabled || string.IsNullOrWhiteSpace(rule.resourceId) || rule.amountPerSecond <= 0f)
            {
                continue;
            }

            float productionAmount = rule.amountPerSecond * deltaSeconds * multiplier;
            if (ApplyProduction(save, rule.resourceId, productionAmount))
            {
                changed = true;
            }
        }

        if (changed)
        {
            saveManager.MarkSaveChanged();
        }
    }

    private bool ApplyProduction(GameSaveData save, string resourceId, float productionAmount)
    {
        float remainder = GetRemainder(resourceId);
        float totalAmount = productionAmount + remainder;
        int wholeAmount = Mathf.FloorToInt(totalAmount);

        if (wholeAmount <= 0)
        {
            SetRemainder(resourceId, totalAmount);
            return false;
        }

        int previousAmount = save.GetResourceAmount(resourceId);
        int nextAmount = previousAmount + wholeAmount;
        save.SetResourceAmount(resourceId, nextAmount);
        SetRemainder(resourceId, totalAmount - wholeAmount);
        ResourceChanged?.Invoke(resourceId, previousAmount, nextAmount);
        return true;
    }

    private float GetRemainder(string resourceId)
    {
        if (productionRemainders.TryGetValue(resourceId, out float remainder))
        {
            return remainder;
        }

        return 0f;
    }

    private void SetRemainder(string resourceId, float remainder)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return;
        }

        productionRemainders[resourceId] = Mathf.Max(0f, remainder);
    }

    private bool IsSaveReady()
    {
        return saveManager != null && saveManager.HasLoadedSave;
    }
}
