using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

/// <summary>
/// Görevleri yöneten, başlatan, tamamlayan ve ilerlemesini takip eden merkezi sistem.
/// Kayıt ve Yükleme işlemleri için GameDataManager ile "pasif" modda çalışır.
/// (Bu kod, GameDataManager ile uyumlu, tam ve eksiksiz sürümdür)
/// </summary>
public class QuestManager : MonoBehaviour, IGameDataSaveable<QuestSaveData>
{
    public static QuestManager Instance { get; private set; }

    // Aktif görevler (Coroutine'ler). Bunlar kaydedilmez.
    private Dictionary<string, Coroutine> _activeQuests = new Dictionary<string, Coroutine>();
    
    // Kaydedilecek olan veri: Hangi görev kaç kez tamamlandı.
    private Dictionary<string, int> _questCompletionCounts = new Dictionary<string, int>();

    // --- EVENTS ---
    public event Action<QuestData, int> OnQuestProgress;
    
    /// <summary>
    /// Bir görevin ilerlemesi güncellendiğinde tetiklenir.
    /// Parametreler: questID (string), progress (float, 0.0-1.0 arası)
    /// </summary>
    public event Action<string, float> OnQuestProgressUpdate;
    
    // Sadece bir kez tanımlanmalı
    public static event Action<string> OnQuestCompleted; 

    // ====================================================================================================
    // SINGLETON VE BAŞLANGIÇ
    // ====================================================================================================

    void Awake()
    {
        Debug.Log($"QuestManager Awake çalışıyor: {gameObject.name}"); // Eski koddan
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Yükleme işlemi artık GameDataManager tarafından yönetiliyor.
            // Bu yüzden 'LoadQuestProgress()' çağrısı burada YOK.
            Debug.Log($"QuestManager Instance atandı: {gameObject.name}"); // Eski koddan
        }
        else
        {
            Debug.LogWarning($"!!! Fazladan QuestManager yok ediliyor: {gameObject.name}. Mevcut Instance: {Instance.gameObject.name}", gameObject); // Eski koddan
            Destroy(gameObject);
        }
    }

    // ====================================================================================================
    // GÖREV BAŞLATMA VE YÖNETİMİ
    // ====================================================================================================

    /// <summary>
    /// Gerekli kontrolleri (maliyet, seviye, stat) yaptıktan sonra bir görevi başlatır.
    /// </summary>
    public void StartQuest(QuestData quest)
    {
        if (quest == null) { Debug.LogError("Başlatılmaya çalışılan quest verisi null!"); return; }
        if (_activeQuests.ContainsKey(quest.questID)) { Debug.LogWarning($"Görev '{quest.questName}' zaten aktif."); return; }
        
        int currentCompletions = GetCompletionCount(quest.questID);
        if (quest.completionLimit > 0 && currentCompletions >= quest.completionLimit) 
        { 
            Debug.Log($"Görev '{quest.questName}' tamamlanma limitine ulaştı."); 
            return; 
        }

        // --- Bonus ve Stat Hesaplamaları ---
        ComputedStats stats = StatCalculator.Instance.currentStats;
        float masteryCostBonus = MasteryManager.Instance?.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionCostPercent) ?? 0f;
        float masteryTimeBonus = MasteryManager.Instance?.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionTimePercent) ?? 0f;

        // --- Gereksinim Kontrolleri (Tümü) ---
        double finalEnergyCost = quest.requirements.requiredEnergy * (1 - (stats.ResourceCostReduction + masteryCostBonus));
        if (finalEnergyCost < 0) finalEnergyCost = 0;

        if (ResourceManager.Instance.currentEnergy < finalEnergyCost) { Debug.Log($"Yetersiz Enerji! Gereken: {finalEnergyCost:F1}, Mevcut: {ResourceManager.Instance.currentEnergy:F0}"); return; }
        if (LevelManager.Instance.currentLevel < quest.requirements.requiredLevel) { Debug.Log($"Yetersiz Seviye! Gereken: {quest.requirements.requiredLevel}, Mevcut: {LevelManager.Instance.currentLevel}"); return; }
        if (ResourceManager.Instance.currentHealth < quest.requirements.requiredHealth) { Debug.Log($"Yetersiz Can! Gereken: {quest.requirements.requiredHealth}, Mevcut: {ResourceManager.Instance.currentHealth:F0}"); return; }
        if (CurrencyManager.Instance.gold < quest.requirements.requiredGold) { Debug.Log($"Yetersiz Altın! Gereken: {quest.requirements.requiredGold}, Mevcut: {CurrencyManager.Instance.gold:F0}"); return; }
        if (ResourceManager.Instance.currentMana < quest.requirements.requiredMana) { Debug.Log($"Yetersiz Mana! Gereken: {quest.requirements.requiredMana}, Mevcut: {ResourceManager.Instance.currentMana:F0}"); return; }
        if (CurrencyManager.Instance.nexusCoin < quest.requirements.requiredNexusCoin) { Debug.Log($"Yetersiz Nexus Coin! Gereken: {quest.requirements.requiredNexusCoin}, Mevcut: {CurrencyManager.Instance.nexusCoin:F0}"); return; }
        if (StatManager.Instance.GetTotalPhysical() < quest.requirements.requiredPhysical) { Debug.Log($"Yetersiz Physical Stat! Gereken: {quest.requirements.requiredPhysical}, Mevcut: {StatManager.Instance.GetTotalPhysical():F0}"); return; }
        if (StatManager.Instance.GetTotalMental() < quest.requirements.requiredMental) { Debug.Log($"Yetersiz Mental Stat! Gereken: {quest.requirements.requiredMental}, Mevcut: {StatManager.Instance.GetTotalMental():F0}"); return; }
        if (StatManager.Instance.GetTotalSpiritual() < quest.requirements.requiredSpiritual) { Debug.Log($"Yetersiz Spiritual Stat! Gereken: {quest.requirements.requiredSpiritual}, Mevcut: {StatManager.Instance.GetTotalSpiritual():F0}"); return; }
        if (StatManager.Instance.GetTotalPerception() < quest.requirements.requiredPerception) { Debug.Log($"Yetersiz Perception Stat! Gereken: {quest.requirements.requiredPerception}, Mevcut: {StatManager.Instance.GetTotalPerception():F0}"); return; }
        if (StatManager.Instance.GetTotalLuck() < quest.requirements.requiredLuck) { Debug.Log($"Yetersiz Luck Stat! Gereken: {quest.requirements.requiredLuck}, Mevcut: {StatManager.Instance.GetTotalLuck():F0}"); return; }
        if (StatManager.Instance.GetTotalSocial() < quest.requirements.requiredSocial) { Debug.Log($"Yetersiz Social Stat! Gereken: {quest.requirements.requiredSocial}, Mevcut: {StatManager.Instance.GetTotalSocial():F0}"); return; }

        // --- Maliyetleri Düş ---
        ResourceManager.Instance.ModifyEnergy(-(float)finalEnergyCost);
        if (quest.requirements.requiredHealth > 0) { ResourceManager.Instance.ModifyHealth(-(float)quest.requirements.requiredHealth); }
        if (quest.requirements.requiredGold > 0) { CurrencyManager.Instance.SpendGold(quest.requirements.requiredGold); }
        if (quest.requirements.requiredMana > 0) { ResourceManager.Instance.ModifyMana(-(float)quest.requirements.requiredMana); }
        if (quest.requirements.requiredNexusCoin > 0) { CurrencyManager.Instance.SpendNexusCoin(quest.requirements.requiredNexusCoin); }

        // --- Süre Hesabı ---
        float baseDuration = quest.baseCompletionTime;
        float totalPercentCooldown = (float)stats.PercentCooldownReduction + masteryTimeBonus;
        float flatCooldown = (float)stats.FlatCooldownReduction;
        float durationAfterFlat = baseDuration - flatCooldown;
        if (durationAfterFlat < 0) durationAfterFlat = 0;
        float finalCompletionTime = durationAfterFlat / (1 + totalPercentCooldown);
        if (finalCompletionTime < 0.2f) finalCompletionTime = 0.2f;

        // --- Görevi Başlat ---
        Coroutine questCoroutine = StartCoroutine(ProcessQuestCoroutine(quest, finalCompletionTime));
        _activeQuests.Add(quest.questID, questCoroutine);
        Debug.Log($"Görev '{quest.questName}' başlatıldı. Süre: {finalCompletionTime:F1}s, Maliyet: {finalEnergyCost:F1} Enerji.");
    }

    /// <summary>
    /// Belirtilen ID'li görev şu anda aktif mi?
    /// </summary>
    public bool IsQuestActive(string questID) => _activeQuests.ContainsKey(questID);

    /// <summary>
    /// Aktif bir görevi iptal eder ve UI ilerlemesini sıfırlar.
    /// </summary>
    public void CancelQuest(string questID)
    {
        if (_activeQuests.TryGetValue(questID, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            _activeQuests.Remove(questID);
            OnQuestProgressUpdate?.Invoke(questID, 0f);
            Debug.Log($"Görev iptal edildi: {questID}");
        }
    }

    /// <summary>
    /// Görevin süresini işleyen ve ilerlemeyi güncelleyen Coroutine.
    /// </summary>
    private IEnumerator ProcessQuestCoroutine(QuestData quest, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            OnQuestProgressUpdate?.Invoke(quest.questID, Mathf.Clamp01(timer / duration));
            yield return null;
        }
        OnQuestProgressUpdate?.Invoke(quest.questID, 1f);
        CompleteQuest(quest);
    }

    // ====================================================================================================
    // GÖREV TAMAMLAMA VE ÖDÜLLER
    // ====================================================================================================

    /// <summary>
    /// Bir görevi tamamlar, kaydını tutar, ödülleri dağıtır ve event'leri tetikler.
    /// </summary>
    private void CompleteQuest(QuestData quest)
    {
        if (!_activeQuests.ContainsKey(quest.questID)) return; // İptal edildiyse tekrar tamamlanmasın

        _activeQuests.Remove(quest.questID);
        _questCompletionCounts[quest.questID] = GetCompletionCount(quest.questID) + 1;
        
        // Kapsamlı ödül dağıtımını çağır
        DistributeRewards(quest);
        
        
        
        OnQuestProgress?.Invoke(quest, _questCompletionCounts[quest.questID]);
        
        // Kayıt işlemi artık GameDataManager tarafından yönetiliyor.
        // 'SaveQuestProgress()' çağrısı burada YOK.
        
        OnQuestCompleted?.Invoke(quest.questID);
        Debug.Log($"Görev '{quest.questName}' tamamlandı! Toplam tamamlama: {_questCompletionCounts[quest.questID]}.");
    }

    /// <summary>
    /// Tamamlanan görevin ödüllerini stat/mastery bonuslarını hesaplayarak dağıtır.
    /// TÜM GAMECONSOLE ÇIKTILARINI İÇERİR.
    /// </summary>
    private void DistributeRewards(QuestData quest)
    {
        // StatCalculator ve MasteryManager'dan anlık bonusları al
        ComputedStats stats = StatCalculator.Instance.currentStats;
        float masteryYieldBonus = MasteryManager.Instance?.GetTotalBonusFor(quest.masteryID, MasteryRewardType.IncreaseYieldFlat) ?? 0f;

        // --- Tecrübe Ödülü ---
        if (quest.experienceReward > 0)
        {
            double finalExp = quest.experienceReward * (1 + stats.ExpBonus);
            LevelManager.Instance.AddXP(finalExp);

            if (GameConsole.Instance != null)
            {
                GameConsole.Instance.AddMessage($"<color=green>+{finalExp:F0} XP</color> kazanıldı ({quest.questName}).");
            }
        }

        // --- Altın Ödülü ---
        if (quest.goldRewardTiers != null && quest.goldRewardTiers.Count > 0)
        {
            double baseGold = GetWeightedReward(quest.goldRewardTiers);
            double finalGold = (baseGold + masteryYieldBonus) * (1 + stats.GoldBonus);
            
            if (finalGold > 0) // Güvenli kontrol
                CurrencyManager.Instance.AddGold(finalGold);

            if (GameConsole.Instance != null)
            {
                GameConsole.Instance.AddMessage($"<color=yellow>+{finalGold:F0} Altın</color> kazanıldı ({quest.questName}).");
            }
        }

        // --- Nexus Coin Ödülü ---
        if (quest.nexusCoinRewardTiers != null && quest.nexusCoinRewardTiers.Count > 0)
        {
            double baseNexus = GetWeightedReward(quest.nexusCoinRewardTiers);
            double finalNexus = (baseNexus + masteryYieldBonus) * (1 + stats.NexusCoinBonus);
            
            if (finalNexus > 0) // Güvenli kontrol
                CurrencyManager.Instance.AddNexusCoin(finalNexus);

            if (GameConsole.Instance != null)
            {
                GameConsole.Instance.AddMessage($"<color=purple>+{finalNexus:F0} Nexus Coin</color> kazanıldı ({quest.questName}).");
            }
        }

        // --- Eşya Ödülleri ---
        if (quest.itemRewards != null)
        {
            foreach (var itemDrop in quest.itemRewards)
            {
                // Güvenli null kontrolü
                if (itemDrop == null || itemDrop.itemToDrop == null) continue;

                float finalDropChance = itemDrop.dropChance * (1 + (float)stats.DropRate); 

                if (UnityEngine.Random.value <= finalDropChance)
                {
                    int amountToGive = itemDrop.amount; 
                    Inventory.Instance.AddItem(itemDrop.itemToDrop, amountToGive);

                    if (GameConsole.Instance != null)
                    {
                        GameConsole.Instance.AddMessage($"<color=orange>+{amountToGive} {itemDrop.itemToDrop.itemName}</color> elde edildi ({quest.questName}).");
                    }
                }
            }
        }

        // --- Stat Ödülleri ---
        if (quest.statRewards != null)
        {
            foreach (var statReward in quest.statRewards)
            {
                StatManager.Instance.AddStat(statReward.statToReward.ToString(), statReward.amount);

                if (GameConsole.Instance != null)
                {
                    GameConsole.Instance.AddMessage($"<color=cyan>+{statReward.amount} {statReward.statToReward}</color> kalıcı stat kazanıldı ({quest.questName}).");
                }
            }
        }
    }


    /// <summary>
    /// Verilen ödül tier listesine göre ağırlıklı bir rastgele ödül miktarı seçer.
    /// </summary>
    private double GetWeightedReward(List<RewardTier> tiers)
    {
        if (tiers == null || tiers.Count == 0) return 0;

        float totalWeight = tiers.Sum(t => t.probabilityWeight);
        if (totalWeight <= 0) return tiers.LastOrDefault()?.GetRandomAmount() ?? 0;
        
        float randomPoint = UnityEngine.Random.Range(0, totalWeight);
        foreach (var tier in tiers) 
        { 
            if (randomPoint < tier.probabilityWeight) return tier.GetRandomAmount(); 
            randomPoint -= tier.probabilityWeight; 
        }
        return tiers.Last().GetRandomAmount(); // Hata durumunda sonuncuyu ver
    }

    /// <summary>
    /// Bir görevin toplam tamamlanma sayısını döndürür.
    /// </summary>
    public int GetCompletionCount(string questID) => _questCompletionCounts.TryGetValue(questID, out int count) ? count : 0;

    // ====================================================================================================
    // KAYIT SİSTEMİ (GameDataManager UYUMLU)
    // ====================================================================================================

    /// <summary>
    /// GameDataManager'a kaydedilecek verileri toplar ve döndürür.
    /// </summary>
    public QuestSaveData GetSaveData()
    {
        // Bu yapı, GameSaveData.cs dosyanızdaki QuestSaveData ile uyumludur.
        Debug.Log("QuestManager: Kayıt verisi oluşturuluyor.");
        return new QuestSaveData
        {
            // Verinin bir kopyasını oluşturarak gönderiyoruz.
            questCompletionCounts = new Dictionary<string, int>(_questCompletionCounts)
        };
    }

    /// <summary>
    /// GameDataManager'dan gelen verileri bu yöneticiye yükler.
    /// </summary>
    public void LoadFromData(QuestSaveData data)
    {
        if (data != null && data.questCompletionCounts != null)
        {
            _questCompletionCounts = data.questCompletionCounts;
        }
        else
        {
            // Gelen veri null ise (örn: yeni oyun) temiz bir sözlükle başla.
            _questCompletionCounts = new Dictionary<string, int>();
        }

        // Oyunu yüklediğimizde, önceki oturumdan kalmış olabilecek
        // tüm aktif görevleri (Coroutine'leri) temizle.
        _activeQuests.Clear();
        
        Debug.Log("QuestManager verisi yüklendi. Tamamlanan görev sayısı: " + _questCompletionCounts.Count);
    }
}