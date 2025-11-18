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
    // --- YENİ VE REFACTOR EDİLMİŞ StartQuest FONKSİYONU ---
    // (Eskisinin tamamını silip bunu yapıştırın)

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

        // ========================================================================
        // GEREKSİNİM KONTROLÜ (REFACTOR EDİLDİ)
        // ========================================================================
        
        // [YENİ] Artık 20 satırlık 'if' bloğu yerine,
        // merkezi GameValidator'a "şartlar uygun mu?" diye soruyoruz.
        if (!GameValidator.Instance.AreRequirementsMet(quest.requirements))
        {
            // GameValidator zaten konsola detaylı hata log'u basıyor olabilir,
            // veya biz buradan genel bir mesaj verebiliriz.
            Debug.Log($"Görev '{quest.questName}' başlatılamadı. Gereksinimler karşılanmıyor.");
            return;
        }

        // ========================================================================
        // MALİYETLERİ DÜŞ (REFACTOR EDİLDİ)
        // ========================================================================

        // [YENİ] Artık 5+ satırlık 'ModifyEnergy', 'SpendGold' vb. kodları yerine,
        // merkezi GameCostConsumer'a "maliyetleri harca" diyoruz.
        // GameCostConsumer akıllıca davranıp sadece harcanabilir
        // şeyleri (Gold, Item, Energy) harcayacak, Level veya Stat gibi
        // şartları es geçecektir.
        //
        // [YENİ] Enerji indirimi için görev bağlamını oluştur ve gönder
        CostContext questContext = new CostContext { masteryID = quest.masteryID };
        GameCostConsumer.Instance.ConsumeRequirements(quest.requirements, questContext);

        // --- ESKİ 'finalEnergyCost' HESAPLAMASI VE DİĞER KONTROLLER SİLİNDİ ---

        // ========================================================================

        // --- Süre Hesabı (Bu kısım aynı kalabilir, ancak 'requirements'a değil, 'quest'e bağlı) ---
        ComputedStats stats = StatCalculator.Instance.currentStats;
        float masteryTimeBonus = MasteryManager.Instance?.GetTotalBonusFor(quest.masteryID, MasteryRewardType.ReduceActionTimePercent) ?? 0f;
        
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
        Debug.Log($"Görev '{quest.questName}' başlatıldı. Süre: {finalCompletionTime:F1}s");
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

        // [YENİ] Tüm ödül listesini (XP, Altın, Eşya, Stat hepsi bir arada)
        // doğrudan GameRewardDistributor'a gönder.
        // O bizim için hepsini tek seferde dağıtacak.
        if (quest.rewards != null && quest.rewards.Count > 0)
        {
            GameRewardDistributor.Instance.DistributeRewards(quest.rewards);
        }
     
        
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