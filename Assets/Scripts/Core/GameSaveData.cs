using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public const int CurrentVersion = 1;

    public int saveVersion = CurrentVersion;
    public string playerId = string.Empty;
    public long createdAtUtcTicks;
    public long lastSavedAtUtcTicks;
    public long lastPlayedAtUtcTicks;
    public float totalPlayTimeSeconds;
    public int currentStageIndex;
    public long offlineProgressSeconds;
    public List<NamedIntValue> resources = new List<NamedIntValue>();
    public List<NamedIntValue> buildingLevels = new List<NamedIntValue>();
    public List<string> unlockedResearchIds = new List<string>();
    public List<string> completedTutorialSteps = new List<string>();

    public static GameSaveData CreateDefault()
    {
        GameSaveData data = new GameSaveData();
        data.createdAtUtcTicks = DateTime.UtcNow.Ticks;
        data.lastPlayedAtUtcTicks = data.createdAtUtcTicks;
        data.lastSavedAtUtcTicks = 0;
        data.saveVersion = CurrentVersion;
        data.EnsureLists();
        return data;
    }

    public void EnsureLists()
    {
        if (resources == null)
        {
            resources = new List<NamedIntValue>();
        }

        if (buildingLevels == null)
        {
            buildingLevels = new List<NamedIntValue>();
        }

        if (unlockedResearchIds == null)
        {
            unlockedResearchIds = new List<string>();
        }

        if (completedTutorialSteps == null)
        {
            completedTutorialSteps = new List<string>();
        }
    }

    public void Normalize()
    {
        EnsureLists();

        if (saveVersion <= 0)
        {
            saveVersion = CurrentVersion;
        }

        if (createdAtUtcTicks <= 0)
        {
            createdAtUtcTicks = DateTime.UtcNow.Ticks;
        }
    }

    public DateTime GetCreatedAtUtc()
    {
        return new DateTime(createdAtUtcTicks, DateTimeKind.Utc);
    }

    public DateTime GetLastSavedAtUtc()
    {
        if (lastSavedAtUtcTicks <= 0)
        {
            return DateTime.MinValue;
        }

        return new DateTime(lastSavedAtUtcTicks, DateTimeKind.Utc);
    }

    public int GetResourceAmount(string resourceId)
    {
        return GetNamedIntValue(resources, resourceId);
    }

    public void SetResourceAmount(string resourceId, int amount)
    {
        SetNamedIntValue(resources, resourceId, amount);
    }

    public int GetBuildingLevel(string buildingId)
    {
        return GetNamedIntValue(buildingLevels, buildingId);
    }

    public void SetBuildingLevel(string buildingId, int level)
    {
        SetNamedIntValue(buildingLevels, buildingId, level);
    }

    public void UnlockResearch(string researchId)
    {
        if (string.IsNullOrWhiteSpace(researchId))
        {
            return;
        }

        if (!unlockedResearchIds.Contains(researchId))
        {
            unlockedResearchIds.Add(researchId);
        }
    }

    public bool IsResearchUnlocked(string researchId)
    {
        if (string.IsNullOrWhiteSpace(researchId))
        {
            return false;
        }

        return unlockedResearchIds.Contains(researchId);
    }

    public void CompleteTutorialStep(string stepId)
    {
        if (string.IsNullOrWhiteSpace(stepId))
        {
            return;
        }

        if (!completedTutorialSteps.Contains(stepId))
        {
            completedTutorialSteps.Add(stepId);
        }
    }

    private static int GetNamedIntValue(List<NamedIntValue> entries, string id)
    {
        if (entries == null || string.IsNullOrWhiteSpace(id))
        {
            return 0;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].id == id)
            {
                return entries[i].value;
            }
        }

        return 0;
    }

    private static void SetNamedIntValue(List<NamedIntValue> entries, string id, int value)
    {
        if (entries == null || string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].id == id)
            {
                entries[i] = new NamedIntValue(id, value);
                return;
            }
        }

        entries.Add(new NamedIntValue(id, value));
    }
}

[Serializable]
public struct NamedIntValue
{
    public string id;
    public int value;

    public NamedIntValue(string id, int value)
    {
        this.id = id;
        this.value = value;
    }
}
