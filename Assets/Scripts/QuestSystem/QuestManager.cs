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

    /// <summary>
    /// Bir görevin ilerlemesi güncellendiğinde tetiklenir.
    /// Parametreler: questID (string), progress (float, 0.0-1.0 arası)
    /// </summary>
    public event Action<string, float> OnQuestProgressUpdate;

    // ====================================================================================================
    // SINGLETON VE BAŞLANGIÇ
    // ====================================================================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değişse bile bu obje kalıcı olsun diye
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

    /// <summary>
    /// Belirtilen görevi, stat ve ustalık bonuslarını hesaplayarak başlatır.
    /// </summary>
    public void StartQuest(QuestData quest)
    {
        if (quest == null)
        {
            Debug.LogError("Başlatılmaya çalışılan quest verisi null!");
            return;
        }

        if (_activeQuests.ContainsKey(quest.questID))
        {
            Debug.LogWarning($"Görev '{quest.questName}' zaten aktif.");
            return;
        }

        int currentCompletions = GetCompletionCount(quest.questID);
        if (quest.completionLimit > 0 && currentCompletions >= quest.completionLimit)
        {
            Debug.Log($"Görev '{quest.questName}' tamamlanma limitine ulaştı.");
            return;
        }

        // --- STAT & MASTERY BONUSLARI ENTEGRASYONU ---

        // StatCalculator'dan ve MasteryManager'dan anlık bonusları al
        ComputedStats stats = StatCalculator.Instance.currentStats;
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

        if (LevelManager.Instance.currentLevel < quest.requirements.requiredLevel)
        {
            Debug.Log($"Yetersiz Seviye! Gereken: {quest.requirements.requiredLevel}, Mevcut: {LevelManager.Instance.currentLevel}");
            return;
        }

        if (ResourceManager.Instance.currentHealth < quest.requirements.requiredHealth)
        {
            Debug.Log($"Yetersiz Can! Gereken: {quest.requirements.requiredHealth}, Mevcut: {ResourceManager.Instance.currentHealth:F0}");
            return;
        }

        // Altın Kontrolü (Maliyet olarak)
        if (CurrencyManager.Instance.gold < quest.requirements.requiredGold)
        {
            Debug.Log($"Yetersiz Altın! Gereken: {quest.requirements.requiredGold}, Mevcut: {CurrencyManager.Instance.gold:F0}");
            return;
        }

        if (ResourceManager.Instance.currentMana < quest.requirements.requiredMana)
        {
            Debug.Log($"Yetersiz Mana! Gereken: {quest.requirements.requiredMana}, Mevcut: {ResourceManager.Instance.currentMana:F0}");
            return;
        }

        if (CurrencyManager.Instance.nexusCoin < quest.requirements.requiredNexusCoin)
        {
            Debug.Log($"Yetersiz Nexus Coin! Gereken: {quest.requirements.requiredNexusCoin}, Mevcut: {CurrencyManager.Instance.nexusCoin:F0}");
            return;
        }
        // TODO: Diğer gereksinimleri de (stat, level vb.) burada kontrol et.

        // 2. Kaynakları Tüket
        ResourceManager.Instance.ModifyEnergy(-(float)finalEnergyCost);

        if (quest.requirements.requiredHealth > 0)
        {
            ResourceManager.Instance.ModifyHealth(-(float)quest.requirements.requiredHealth);
        }
        if (quest.requirements.requiredGold > 0)
        {
            CurrencyManager.Instance.SpendGold(quest.requirements.requiredGold);
        }

        if (quest.requirements.requiredMana > 0)
        {
            ResourceManager.Instance.ModifyMana(-(float)quest.requirements.requiredMana);
        }

        if (quest.requirements.requiredNexusCoin > 0)
        {
            CurrencyManager.Instance.SpendNexusCoin(quest.requirements.requiredNexusCoin);
        }

        

        // 3. Nihai Süre Hesabı
        float baseDuration = quest.baseCompletionTime;
        // StatCalculator'dan gelen bonusları al (Yüzdesel ve Düz)
        float totalPercentCooldown = (float)stats.PercentCooldownReduction + masteryTimeBonus;
        float flatCooldown = (float)stats.FlatCooldownReduction;

        // Önce düz saniye düşüşünü uygula
        float durationAfterFlat = baseDuration - flatCooldown;
        if (durationAfterFlat < 0) durationAfterFlat = 0; // Süre eksiye düşemez

        // Sonra kalan süreye yüzdesel indirimi uygula (Formül: Yeni Süre = Süre / (1 + Bonus))
        float finalCompletionTime = durationAfterFlat / (1 + totalPercentCooldown);

        // Minimum süre sınırı
        if (finalCompletionTime < 0.2f) finalCompletionTime = 0.2f;

        // 4. Görevi Coroutine olarak başlat
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


    /// <summary>
    /// Görevin zamanlayıcısını yöneten ve anlık ilerleme bildiren Coroutine.
    /// </summary>
    private IEnumerator ProcessQuestCoroutine(QuestData quest, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            OnQuestProgressUpdate?.Invoke(quest.questID, progress);
            yield return null;
        }

        OnQuestProgressUpdate?.Invoke(quest.questID, 1f); // Tamamlandığından emin ol
        CompleteQuest(quest);
    }

    // ====================================================================================================
    // GÖREV TAMAMLAMA VE ÖDÜLLER
    // ====================================================================================================

    private void CompleteQuest(QuestData quest)
    {
        if (!_activeQuests.ContainsKey(quest.questID)) return; // Eğer görev zaten iptal edildiyse, tamamlama.

        _activeQuests.Remove(quest.questID);

        _questCompletionCounts[quest.questID] = GetCompletionCount(quest.questID) + 1;
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
    /// Görev ödüllerini Stat ve Ustalık bonuslarını hesaba katarak dağıtır.
    /// </summary>
    private void DistributeRewards(QuestData quest)
    {
        // StatCalculator ve MasteryManager'dan anlık bonusları al
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
            // Önce düz bonuslar (mastery vb.), sonra yüzdesel bonuslar uygulanır
            double finalGold = (baseGold + masteryYieldBonus) * (1 + stats.GoldBonus);
            if (finalGold > 0) CurrencyManager.Instance.AddGold(finalGold);
        }

        // Nexus Coin Ödülü
        if (quest.nexusCoinRewardTiers != null && quest.nexusCoinRewardTiers.Count > 0)
        {
            double baseNexus = GetWeightedReward(quest.nexusCoinRewardTiers);
            // Önce düz bonuslar, sonra yüzdesel bonuslar
            double finalNexus = (baseNexus + masteryYieldBonus) * (1 + stats.NexusCoinBonus);
            if (finalNexus > 0) CurrencyManager.Instance.AddNexusCoin(finalNexus);
        }

        // Eşya Ödülleri
        if (quest.itemRewards != null)
        {
            foreach (var itemDrop in quest.itemRewards)
            {
                // TODO: Item drop mantığını buraya ekle (stats.DropRate'i kullanarak)
                // Örnek: if (Random.value < itemDrop.dropChance * (1 + stats.DropRate)) { ... }
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

    /// <summary>
    /// Olasılık ağırlıklarına göre rastgele bir ödül miktarı seçer.
    /// </summary>
    private double GetWeightedReward(List<RewardTier> tiers)
    {
        if (tiers == null || tiers.Count == 0) return 0;

        float totalWeight = tiers.Sum(tier => tier.probabilityWeight);
        if (totalWeight <= 0) return tiers.LastOrDefault()?.GetRandomAmount() ?? 0;

        float randomPoint = UnityEngine.Random.Range(0, totalWeight);

        foreach (var tier in tiers)
        {
            if (randomPoint < tier.probabilityWeight)
            {
                return tier.GetRandomAmount();
            }
            randomPoint -= tier.probabilityWeight;
        }
        // Eğer bir hata olursa son tier'ı döndür
        return tiers.Last().GetRandomAmount();
    }

    // ====================================================================================================
    // YARDIMCI VE KAYIT METOTLARI
    // ====================================================================================================

    public int GetCompletionCount(string questID)
    {
        return _questCompletionCounts.TryGetValue(questID, out int count) ? count : 0;
    }

    [Serializable]
    private class QuestSaveData
    {
        public List<string> questIDs = new List<string>();
        public List<int> completionCounts = new List<int>();

        public QuestSaveData(Dictionary<string, int> counts)
        {
            if (counts != null)
            {
                questIDs = counts.Keys.ToList();
                completionCounts = counts.Values.ToList();
            }
        }
    }

    public void SaveQuestProgress()
    {
        QuestSaveData saveData = new QuestSaveData(_questCompletionCounts);
        string jsonData = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("QuestProgress", jsonData);
        PlayerPrefs.Save(); // Değişikliklerin diske yazıldığından emin ol
    }

    public void LoadQuestProgress()
    {
        if (PlayerPrefs.HasKey("QuestProgress"))
        {
            string jsonData = PlayerPrefs.GetString("QuestProgress");
            if (string.IsNullOrEmpty(jsonData)) return;

            QuestSaveData saveData = JsonUtility.FromJson<QuestSaveData>(jsonData);
            _questCompletionCounts.Clear();
            if (saveData != null && saveData.questIDs != null)
            {
                for (int i = 0; i < saveData.questIDs.Count; i++)
                {
                    _questCompletionCounts[saveData.questIDs[i]] = saveData.completionCounts[i];
                }
            }
        }
    }
}

