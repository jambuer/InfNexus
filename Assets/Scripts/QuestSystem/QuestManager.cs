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

    // Aktif olarak ilerleyen görevleri takip etmek için.
    // Key: QuestData'nın benzersiz questID'si
    // Value: Görevin ilerlemesini içeren Coroutine
    private Dictionary<string, Coroutine> _activeQuests = new Dictionary<string, Coroutine>();

    // Oyuncunun her görevi kaç kez tamamladığını tutan veri.
    // Key: QuestData'nın benzersiz questID'si, Value: Tamamlama sayısı
    private Dictionary<string, int> _questCompletionCounts = new Dictionary<string, int>();

    /// <summary>
    /// Bir görev tamamlandığında veya ilerlediğinde tetiklenir. UI güncellemeleri için kullanılır.
    /// Parametreler: QuestData, yeni tamamlama sayısı
    /// </summary>
    public event Action<QuestData, int> OnQuestProgress;

    // ====================================================================================================
    // SINGLETON VE BAŞLANGIÇ
    // ====================================================================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadQuestProgress(); // Kaydedilmiş görev ilerlemesini yükle
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
    /// Belirtilen görevi başlatmaya çalışır. Gereksinimleri kontrol eder ve kaynakları tüketir.
    /// </summary>
    /// <param name="quest">Başlatılacak QuestData.</param>
    public void StartQuest(QuestData quest)
    {
        if (quest == null)
        {
            Debug.LogError("StartQuest: Başlatılacak görev verisi (QuestData) null.");
            return;
        }

        // 1. Görev zaten aktif mi kontrol et
        if (_activeQuests.ContainsKey(quest.questID))
        {
            Debug.Log($"Görev '{quest.questName}' zaten aktif.");
            return;
        }

        // 2. Tamamlama limitini kontrol et
        int currentCompletions = GetCompletionCount(quest.questID);
        if (quest.completionLimit > 0 && currentCompletions >= quest.completionLimit)
        {
            Debug.Log($"Görev '{quest.questName}' tamamlama limitine ulaştı.");
            return;
        }

        // 3. Ustalık Sisteminden bonusları sorgula
        float costModifier = 1.0f; // %10 indirim = 0.90
        float timeModifier = 1.0f; // %10 hızlanma = 0.90
        if (MasteryManager.Instance != null && !string.IsNullOrEmpty(quest.masteryID))
        {
            costModifier -= MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionCostPercent);
            timeModifier -= MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionTimePercent);
        }
        
        // 4. Nihai Gereksinimleri Hesapla ve Kontrol Et
        double finalEnergyCost = quest.requirements.requiredEnergy * costModifier;
        if (ResourceManager.Instance.currentEnergy < finalEnergyCost)
        {
            Debug.Log($"Yetersiz Enerji! Gereken: {finalEnergyCost:F0}, Mevcut: {ResourceManager.Instance.currentEnergy:F0}");
            return;
        }
        // TODO: Diğer gereksinimleri de (stat, level vb.) burada kontrol et.

        // 5. Kaynakları Tüket
        ResourceManager.Instance.ModifyEnergy(- (float)finalEnergyCost);

        // 6. Görevi başlat
        float finalCompletionTime = quest.baseCompletionTime * timeModifier;
        Coroutine questCoroutine = StartCoroutine(ProcessQuestCoroutine(quest, finalCompletionTime));
        _activeQuests.Add(quest.questID, questCoroutine);

        Debug.Log($"Görev '{quest.questName}' başlatıldı. Süre: {finalCompletionTime:F1}s, Maliyet: {finalEnergyCost:F0} Enerji.");
    }

    /// <summary>
    /// Görevin zamanlayıcısını yöneten Coroutine.
    /// </summary>
    private IEnumerator ProcessQuestCoroutine(QuestData quest, float duration)
    {
        // TODO: Bu kısım, UI'daki progress bar'ı güncellemek için event'ler yayabilir.
        yield return new WaitForSeconds(duration);

        // Görev tamamlandı
        CompleteQuest(quest);
    }

    // ====================================================================================================
    // GÖREV TAMAMLAMA VE ÖDÜLLER
    // ====================================================================================================

    /// <summary>
    /// Bir görevi tamamlar, ödülleri dağıtır ve ustalık ilerlemesini kaydeder.
    /// </summary>
    private void CompleteQuest(QuestData quest)
    {
        // Aktif görevler listesinden çıkar
        _activeQuests.Remove(quest.questID);

        // 1. Tamamlama sayısını artır
        if (!_questCompletionCounts.ContainsKey(quest.questID))
        {
            _questCompletionCounts[quest.questID] = 0;
        }
        _questCompletionCounts[quest.questID]++;
        int newCompletionCount = _questCompletionCounts[quest.questID];

        // 2. Ustalık Sisteminden bonusları sorgula
        float yieldBonus = 0;
        if (MasteryManager.Instance != null && !string.IsNullOrEmpty(quest.masteryID))
        {
            yieldBonus = MasteryManager.Instance.GetTotalBonusFor(quest.masteryID, MasteryRewardType.IncreaseYieldFlat);
        }

        // 3. Ödülleri Dağıt
        DistributeRewards(quest, yieldBonus);

        // 4. Ustalık Sistemini İlerlet
        if (MasteryManager.Instance != null && !string.IsNullOrEmpty(quest.masteryID))
        {
            MasteryManager.Instance.ProgressMastery(quest.masteryID);
        }

        // 5. UI ve diğer sistemleri bilgilendir
        OnQuestProgress?.Invoke(quest, newCompletionCount);
        SaveQuestProgress(); // Her tamamlamada ilerlemeyi kaydet

        Debug.Log($"Görev '{quest.questName}' tamamlandı! Toplam tamamlama: {newCompletionCount}.");
    }

    /// <summary>
    /// Görev ödüllerini hesaplar ve ilgili yöneticilere gönderir.
    /// </summary>
    private void DistributeRewards(QuestData quest, float yieldBonus)
    {
        // Altın Ödülü
        if (quest.goldRewardTiers != null && quest.goldRewardTiers.Count > 0)
        {
            double goldAmount = GetWeightedReward(quest.goldRewardTiers) + yieldBonus;
            CurrencyManager.Instance.AddGold(goldAmount);
        }
        // TODO: Nexus Coin, Eşya ve Stat ödüllerini de benzer şekilde dağıt.
    }

    /// <summary>
    /// Ağırlıklı olasılığa sahip ödül dilimlerinden birini seçer ve rastgele bir miktar döndürür.
    /// </summary>
    private double GetWeightedReward(List<RewardTier> tiers)
    {
        float totalWeight = tiers.Sum(tier => tier.probabilityWeight);
        float randomPoint = UnityEngine.Random.Range(0, totalWeight);

        foreach (var tier in tiers)
        {
            if (randomPoint < tier.probabilityWeight)
            {
                return tier.GetRandomAmount();
            }
            randomPoint -= tier.probabilityWeight;
        }
        return tiers.Last().GetRandomAmount(); // Hata durumunda sonuncuyu döndür
    }
    
    // ====================================================================================================
    // YARDIMCI VE KAYIT METOTLARI
    // ====================================================================================================

    /// <summary>
    /// Bir görevin toplam tamamlama sayısını döndürür.
    /// </summary>
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