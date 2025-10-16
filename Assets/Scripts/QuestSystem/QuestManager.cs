using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

/// <summary>
/// Oyundaki tüm görevlerin durumunu ve ilerlemesini yöneten merkezi yönetici.
/// Görevleri başlatır, tamamlar, ödülleri dağıtır ve MasteryManager ile iletişim kurar.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private Dictionary<string, Coroutine> _activeQuests = new Dictionary<string, Coroutine>();
    private Dictionary<string, int> _questCompletionCounts = new Dictionary<string, int>();

    public event Action<QuestData, int> OnQuestProgress;

    // --- YENİ EKLENEN EVENT ---
    /// <summary>
    /// Bir görevin ilerlemesi güncellendiğinde tetiklenir.
    /// Parametreler: questID (string), progress (float, 0.0-1.0 arası)
    /// </summary>
    public event Action<string, float> OnQuestProgressUpdate;
    // --- BİTTİ ---

    // ====================================================================================================
    // SINGLETON VE BAŞLANGIÇ
    // ====================================================================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadQuestProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ====================================================================================================
    // GÖREV BAŞLATMA VE İŞLEME
    // ====================================================================================================

    public void StartQuest(QuestData quest)
    {
        if (quest == null) return;
        if (_activeQuests.ContainsKey(quest.questID)) return;
        int currentCompletions = GetCompletionCount(quest.questID);
        if (quest.completionLimit > 0 && currentCompletions >= quest.completionLimit) return;

        ComputedStats stats = StatCalculator.Instance.currentStats;
        float masteryCostBonus = 0f, masteryTimeBonus = 0f;
        if (MasteryManager.Instance != null && !string.IsNullOrEmpty(quest.masteryID))
        {
            masteryCostBonus = MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionCostPercent);
            masteryTimeBonus = MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionTimePercent);
        }

        double totalCostReductionPercent = stats.ResourceCostReduction + masteryCostBonus;
        double finalEnergyCost = quest.requirements.requiredEnergy * (1 - totalCostReductionPercent);
        if (finalEnergyCost < 0) finalEnergyCost = 0;

        if (ResourceManager.Instance.currentEnergy < finalEnergyCost)
        {
            Debug.Log($"Yetersiz Enerji! Gereken: {finalEnergyCost:F1}, Mevcut: {ResourceManager.Instance.currentEnergy:F0}");
            return;
        }

        ResourceManager.Instance.ModifyEnergy(-(float)finalEnergyCost);

        float baseDuration = quest.baseCompletionTime;
        float totalPercentCooldown = (float)stats.PercentCooldownReduction + masteryTimeBonus;
        float flatCooldown = (float)stats.FlatCooldownReduction;
        float durationAfterFlat = baseDuration - flatCooldown;
        if (durationAfterFlat < 0) durationAfterFlat = 0;
        float finalCompletionTime = durationAfterFlat / (1 + totalPercentCooldown);
        if (finalCompletionTime < 0.2f) finalCompletionTime = 0.2f;

        Coroutine questCoroutine = StartCoroutine(ProcessQuestCoroutine(quest, finalCompletionTime));
        _activeQuests.Add(quest.questID, questCoroutine);

        Debug.Log($"Görev '{quest.questName}' başlatıldı. Süre: {finalCompletionTime:F1}s, Maliyet: {finalEnergyCost:F1} Enerji.");
    }
    
    /// <summary>
    /// Belirtilen görevin aktif olup olmadığını kontrol eder.
    /// </summary>
    public bool IsQuestActive(string questID)
    {
        return _activeQuests.ContainsKey(questID);
    }

    /// <summary>
    /// Aktif bir görevi iptal eder ve progress bar'ını sıfırlar.
    /// </summary>
    public void CancelQuest(string questID)
    {
        if (_activeQuests.TryGetValue(questID, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            _activeQuests.Remove(questID);
            OnQuestProgressUpdate?.Invoke(questID, 0f); // UI'a progress bar'ı sıfırlamasını söyle
            Debug.Log($"Görev iptal edildi: {questID}");
            // Not: İptal edilen görevin maliyeti iade edilmiyor. Bu bir tasarım kararıdır.
        }
    }

        // --- STAT & MASTERY BONUSLARI ENTEGRASYONU (GÜNCELLENDİ) ---

        // StatCalculator'dan genel bonusları al
        ComputedStats stats = StatCalculator.Instance.currentStats;
        
        // Ustalık Sistemi'nden göreve özel bonusları al
        float masteryCostBonus = 0f;
        float masteryTimeBonus = 0f;
        if (MasteryManager.Instance != null && !string.IsNullOrEmpty(quest.masteryID))
        {
            masteryCostBonus = MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionCostPercent);
            masteryTimeBonus = MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionTimePercent);
        }
        
        // 1. Nihai Maliyet Hesabı ve Kontrolü
        double totalCostReductionPercent = stats.ResourceCostReduction + masteryCostBonus;
        double finalEnergyCost = quest.requirements.requiredEnergy * (1 - totalCostReductionPercent);
        if (finalEnergyCost < 0) finalEnergyCost = 0; // Maliyet eksiye düşemez

        if (ResourceManager.Instance.currentEnergy < finalEnergyCost)
        {
            Debug.Log($"Yetersiz Enerji! Gereken: {finalEnergyCost:F1}, Mevcut: {ResourceManager.Instance.currentEnergy:F0}");
            return;
        }
        // TODO: Diğer gereksinimleri de (stat, level vb.) burada kontrol et.

        // 2. Kaynakları Tüket
        ResourceManager.Instance.ModifyEnergy(-(float)finalEnergyCost);

        // 3. Nihai Süre Hesabı ve Görevi Başlatma
        float baseDuration = quest.baseCompletionTime;
        float totalPercentCooldown = (float)stats.PercentCooldownReduction + masteryTimeBonus;
        float flatCooldown = (float)stats.FlatCooldownReduction;

        // Önce düz saniye düşüşünü uygula
        float durationAfterFlat = baseDuration - flatCooldown;
        if (durationAfterFlat < 0) durationAfterFlat = 0; // Süre eksiye düşemez
        
        // Sonra kalan süreye yüzdesel indirimi uygula (Formül: Yeni Süre = Süre / (1 + Bonus))
        float finalCompletionTime = durationAfterFlat / (1 + totalPercentCooldown);
        
        if (finalCompletionTime < 0.2f) finalCompletionTime = 0.2f; // Minimum süre sınırı

        Coroutine questCoroutine = StartCoroutine(ProcessQuestCoroutine(quest, finalCompletionTime));
        _activeQuests.Add(quest.questID, questCoroutine);

        Debug.Log($"Görev '{quest.questName}' başlatıldı. Süre: {finalCompletionTime:F1}s, Maliyet: {finalEnergyCost:F1} Enerji.");
    }

    /// <summary>
    /// Görevin zamanlayıcısını yöneten ve anlık ilerleme bildiren Coroutine. (TAMAMEN YENİLENDİ)
    /// </summary>
    private IEnumerator ProcessQuestCoroutine(QuestData quest, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration); // 0 ile 1 arasında bir değer
            OnQuestProgressUpdate?.Invoke(quest.questID, progress); // İlerlemeyi anons et
            yield return null; // Bir sonraki frame'e kadar bekle
        }

        OnQuestProgressUpdate?.Invoke(quest.questID, 1f); // Tamamlandığından emin ol
        CompleteQuest(quest);
    }

    // ====================================================================================================
    // GÖREV TAMAMLAMA VE ÖDÜLLER
    // ====================================================================================================

    private void CompleteQuest(QuestData quest)
    {
        _activeQuests.Remove(quest.questID);

        if (!_questCompletionCounts.ContainsKey(quest.questID))
        {
            _questCompletionCounts[quest.questID] = 0;
        }
        _questCompletionCounts[quest.questID]++;
        int newCompletionCount = _questCompletionCounts[quest.questID];

        DistributeRewards(quest);

        if (MasteryManager.Instance != null && !string.IsNullOrEmpty(quest.masteryID))
        {
            MasteryManager.Instance.ProgressMastery(quest.masteryID);
        }

        OnQuestProgress?.Invoke(quest, newCompletionCount);
        SaveQuestProgress();

        Debug.Log($"Görev '{quest.questName}' tamamlandı! Toplam tamamlama: {newCompletionCount}.");
    }

    /// <summary>
    /// Görev ödüllerini hesaplar ve ilgili yöneticilere gönderir. (GÜNCELLENDİ)
    /// </summary>
    private void DistributeRewards(QuestData quest)
    {
        // Bonusları al
        ComputedStats stats = StatCalculator.Instance.currentStats;
        float masteryYieldBonus = 0;
        if (MasteryManager.Instance != null && !string.IsNullOrEmpty(quest.masteryID))
        {
            masteryYieldBonus = MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.IncreaseYieldFlat);
        }

        // Tecrübe Ödülü
        if (quest.experienceReward > 0) 
        {
            double finalExp = quest.experienceReward * (1 + stats.ExpBonus);
            LevelManager.Instance.AddXP(finalExp);
        }

        // Altın Ödülü
        if (quest.goldRewardTiers != null && quest.goldRewardTiers.Count > 0)
        {
            double baseGold = GetWeightedReward(quest.goldRewardTiers);
            double finalGold = (baseGold + masteryYieldBonus) * (1 + stats.GoldBonus); // Önce düz bonus, sonra yüzde
            if(finalGold > 0) CurrencyManager.Instance.AddGold(finalGold);
        }
        
        // Nexus Coin Ödülü
        if (quest.nexusCoinRewardTiers != null && quest.nexusCoinRewardTiers.Count > 0)
        {
            double baseNexus = GetWeightedReward(quest.nexusCoinRewardTiers);
            double finalNexus = (baseNexus + masteryYieldBonus) * (1 + stats.NexusCoinBonus);
            if(finalNexus > 0) CurrencyManager.Instance.AddNexusCoin(finalNexus);
        }
        
        // Eşya Ödülleri
        if (quest.itemRewards != null)
        {
            foreach (var itemDrop in quest.itemRewards)
            {
                // TODO: Item drop mantığını buraya ekle (stats.DropRate'i kullanarak)
            }
        }
        
        // Stat Ödülleri
        if (quest.statRewards != null)
        {
            foreach (var statReward in quest.statRewards)
            {
                StatManager.Instance.AddStat(statReward.statToReward.ToString(), statReward.amount);
            }
        }
    }

    private double GetWeightedReward(List<RewardTier> tiers)
    {
        float totalWeight = tiers.Sum(tier => tier.probabilityWeight);
        if (totalWeight <= 0) return 0;
        
        float randomPoint = UnityEngine.Random.Range(0, totalWeight);

        foreach (var tier in tiers)
        {
            if (randomPoint < tier.probabilityWeight)
            {
                return tier.GetRandomAmount();
            }
            randomPoint -= tier.probabilityWeight;
        }
        return tiers.Last().GetRandomAmount();
    }
    
    // ====================================================================================================
    // YARDIMCI VE KAYIT METOTLARI
    // ====================================================================================================

    public int GetCompletionCount(string questID)
    {
        return _questCompletionCounts.ContainsKey(questID) ? _questCompletionCounts[questID] : 0;
    }
    
    [Serializable]
    private class QuestSaveData
    {
        public List<string> questIDs;
        public List<int> completionCounts;
        public QuestSaveData(Dictionary<string, int> counts)
        {
            questIDs = counts.Keys.ToList();
            completionCounts = counts.Values.ToList();
        }
    }

    public void SaveQuestProgress()
    {
        QuestSaveData saveData = new QuestSaveData(_questCompletionCounts);
        string jsonData = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("QuestProgress", jsonData);
    }

    public void LoadQuestProgress()
    {
        if (PlayerPrefs.HasKey("QuestProgress"))
        {
            string jsonData = PlayerPrefs.GetString("QuestProgress");
            QuestSaveData saveData = JsonUtility.FromJson<QuestSaveData>(jsonData);
            _questCompletionCounts.Clear();
            for (int i = 0; i < saveData.questIDs.Count; i++)
            {
                _questCompletionCounts[saveData.questIDs[i]] = saveData.completionCounts[i];
            }
        }
    }
}