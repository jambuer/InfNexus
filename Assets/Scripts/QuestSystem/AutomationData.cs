using System;
using System.Collections.Generic;

/// <summary>
/// Bir görevin otomasyonuyla ilgili tüm verileri içeren sınıf.
/// </summary>
[Serializable]
public class AutomationData
{
    /// <summary>Bu görevin otomatik olarak tamamlanıp tamamlanamayacağını belirtir.</summary>
    public bool canBeAutomated = false;
    /// <summary>Otomasyonu aktif etmek için gereken koşullar.</summary>
    public QuestRequirements unlockRequirements; // QuestRequirements sınıfı tekrar kullanılıyor
    /// <summary>Otomasyonu açmak için görevin kaç kez tamamlanması gerektiği.</summary>
    public int totalCompletionsToUnlock = 0;
    /// <summary>Otomasyonun yükseltme seviyeleri ve her seviyenin sağladığı faydalar.</summary>
    public List<AutomationTier> automationTiers;
}