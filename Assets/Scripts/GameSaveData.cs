using System.Collections.Generic;
using System;

public enum CurrencyType 
{
    Gold,
    NexusCoin,
    PremiumCoin,
    People
}

// Önceki namespace'ler kaldırıldı. Tüm veri yapıları artık global alanda.

[System.Serializable]
public class GameSaveData
{
    public DateTime saveTime;
    public StatSaveData statData;
    public LevelSaveData levelData;
    public CurrencySaveData currencyData;
    public ResourceSaveData resourceData;
    public QuestSaveData questData;
    public MasterySaveData masteryData;
    public PerkSaveData perkData;
    public ExplorerSaveData explorerData;
    public ChapterSaveData chapterData;
    public TimerSaveData timerData;
}

[System.Serializable]
public class StatSaveData
{
    public double physical, mental, perception, spiritual, luck, social;
}

[System.Serializable]
public class LevelSaveData
{
    public int currentLevel;
    public double currentXP;
    public double xpToNextLevel;
    public int unspentStatPoints;
}

[System.Serializable]
public class CurrencySaveData
{
    public double gold, nexusCoin, premiumCoin, people;
}

[System.Serializable]
public class ResourceSaveData
{
    public float currentHealth, currentEnergy, currentMana;
}

[System.Serializable]
public class QuestSaveData
{
    public Dictionary<string, int> questCompletionCounts;
}

[System.Serializable]
public class MasterySaveData
{
    public Dictionary<string, int> completionCounts;
}

[System.Serializable]
public class PerkSaveData
{
    public Dictionary<string, int> perkCounts;
}

[System.Serializable]
public class ExplorerSaveData
{
    public int currentLeftPerkIndex;
    public HashSet<string> unlockedQuestIDs;
    public Dictionary<string, int> questCompletionCounts;
}

[System.Serializable]
public class ChapterSaveData
{
    public List<int> unlockedChapterIndices;
}