using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System; // ContextMenu için eklendi

/// <summary>
/// Evrensel Ustalık Sistemi'nin ana yöneticisi.
/// İlerlemeyi takip eder, bonusları hesaplar ve GameDataManager ile uyumlu çalışır.
/// (Güncel ve Eski versiyonlar birleştirilip, tüm debug log'lar eklendi)
/// </summary>
public class MasteryManager : MonoBehaviour, IGameDataSaveable<MasterySaveData>
{
    public static MasteryManager Instance { get; private set; }

    [Header("Referanslar")]
    [Tooltip("Projenizdeki tüm ustalığı içeren MasteryDatabase ScriptableObject'ı.")]
    public MasteryDatabase masteryDatabase;

    [Header("Debug/Geliştirici Araçları")]
    [Tooltip("Ustalık ilerlemesinin konsola yazdırılıp yazdırılmayacağını belirler.")]
    public bool debugLogMastery = false; // ESKİ KODDAN EKLENDİ

    // Kaydedilen veri: Key: masteryID (string), Value: tamamlama sayısı (int)
    private Dictionary<string, int> _completionCounts = new Dictionary<string, int>();

    // Hesaplanan veri (kaydedilmez): Key: masteryID, Value: Dictionary<RewardType, ToplamBonus>
    private Dictionary<string, Dictionary<MasteryRewardType, float>> _unlockedTierBonuses = new Dictionary<string, Dictionary<MasteryRewardType, float>>();

    // ====================================================================================================
    // SINGLETON VE BAŞLANGIÇ
    // ====================================================================================================

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        } 
        else 
        { 
            Destroy(gameObject); 
        }
    }

    void Start()
    {
        // LoadFromData çağrıldıktan sonra (veya yeni oyunda)
        // mevcut veriye göre bonusları hesapla.
        RecalculateAllMasteryBonuses();

        if (QuestManager.Instance != null)
        {
            // "QuestManager bir görev ilerlemesi (veya tamamlanması) duyurursa,
            // benim 'HandleQuestCompleted' metodumu çalıştır."
            QuestManager.Instance.OnQuestProgress += HandleQuestCompleted;
        }
    
    }

    void OnDestroy()
    {
        // YENİ EKLENEN KISIM:
        if (QuestManager.Instance != null)
        {
            // "Ben yok oluyorsam, artık QuestManager'ı dinlemeyi bırak."
            QuestManager.Instance.OnQuestProgress -= HandleQuestCompleted;
        }
    }

    /// <summary>
    /// QuestManager'dan gelen 'OnQuestProgress' event'ini (duyurusunu) yakalar.
    /// Bu, bir görev tamamlandığında tetiklenir.
    /// </summary>
    private void HandleQuestCompleted(QuestData completedQuest, int newCompletionCount)
    {
        // Gelen görev verisi (completedQuest) null değilse
        // ve bu görevin ilişkili bir 'masteryID'si varsa...
        if (completedQuest != null && !string.IsNullOrEmpty(completedQuest.masteryID))
        {
            // ...o zaman KENDİ 'ProgressMastery' metodumuzu çağırarak ustalığı ilerlet.
            ProgressMastery(completedQuest.masteryID, 1);
        }
    }



    // ====================================================================================================
    // İLERLEME VE HESAPLAMA
    // ====================================================================================================

    /// <summary>
    /// Belirtilen ustalık yolunda ilerleme kaydeder (örn: Quest tamamlanınca).
    /// </summary>
    public void ProgressMastery(string masteryID, int amount = 1)
    {
        if (string.IsNullOrEmpty(masteryID)) 
        {
            if (debugLogMastery) Debug.LogWarning("ProgressMastery: Boş bir masteryID ile ilerleme kaydedilemez."); // ESKİ KODDAN EKLENDİ
            return;
        }

        MasteryData masteryData = masteryDatabase.GetMasteryData(masteryID);
        if (masteryData == null) 
        {
            if (debugLogMastery) Debug.LogWarning($"ProgressMastery: {masteryID} ID'li ustalık yolu bulunamadı."); // ESKİ KODDAN EKLENDİ
            return;
        }

        int oldCompletionCount = GetCompletionCount(masteryID); // Mevcut sayıyı al
        _completionCounts[masteryID] = oldCompletionCount + amount;
        int newCompletionCount = _completionCounts[masteryID];

        if (debugLogMastery) Debug.Log($"Mastery '{masteryData.displayName}' ilerlemesi: {oldCompletionCount} -> {newCompletionCount} completions."); // ESKİ KODDAN EKLENDİ

        // Bonusları, yeni seviye atlama log'unu da gösterecek şekilde yeniden hesapla
        RecalculateMasteryBonuses(masteryID, newCompletionCount, oldCompletionCount);
        
        // KAYIT İŞLEMİ ARTIK BURADA YAPILMIYOR
        // SaveMasteryData(); // GameDataManager halledecek
    }

    /// <summary>
    /// Kayıttan yükleme sonrası tüm ustalık bonuslarını yeniden hesaplar.
    /// </summary>
    public void RecalculateAllMasteryBonuses()
    {
        _unlockedTierBonuses.Clear();
        foreach (var entry in _completionCounts)
        {
            // -1 göndererek, bunun bir "yeni seviye atlama" log'u değil,
            // toplu bir yeniden hesaplama olduğunu belirtiyoruz.
            RecalculateMasteryBonuses(entry.Key, entry.Value, -1);
        }
        if (debugLogMastery) Debug.Log("Tüm ustalık bonusları yeniden hesaplandı."); // ESKİ KODDAN EKLENDİ
    }

    /// <summary>
    /// Belirli bir ustalık yolunun bonuslarını yeniden hesaplar.
    /// </summary>
    /// <param name="masteryID">Hesaplanacak ustalık ID'si.</param>
    /// <param name="completions">Mevcut tamamlama sayısı.</param>
    /// <param name="oldCompletions">Önceki tamamlama sayısı (-1 ise loglama yapılmaz).</param>
    private void RecalculateMasteryBonuses(string masteryID, int completions, int oldCompletions = -1)
    {
        MasteryData masteryData = masteryDatabase.GetMasteryData(masteryID);
        if (masteryData == null) 
        {
            if (debugLogMastery) Debug.LogWarning($"RecalculateMasteryBonuses: {masteryID} ID'li ustalık yolu bulunamadı."); // ESKİ KODDAN EKLENDİ
            return;
        }

        // Bonusları sıfırla ve yeniden hesapla
        if (!_unlockedTierBonuses.ContainsKey(masteryID)) 
            _unlockedTierBonuses[masteryID] = new Dictionary<MasteryRewardType, float>();
        else 
            _unlockedTierBonuses[masteryID].Clear();

        foreach (MasteryTier tier in masteryData.masteryTiers.OrderBy(t => t.completionRequirement))
        {
            if (completions >= tier.completionRequirement)
            {
                // Bonusu ekle
                if (!_unlockedTierBonuses[masteryID].ContainsKey(tier.rewardType)) 
                    _unlockedTierBonuses[masteryID][tier.rewardType] = 0;
                
                _unlockedTierBonuses[masteryID][tier.rewardType] += tier.rewardValue;

                // ESKİ KODDAN EKLENDİ: Yeni açılan seviyeleri logla
                if (debugLogMastery && oldCompletions != -1 && oldCompletions < tier.completionRequirement)
                {
                    Debug.Log($"Mastery '{masteryData.displayName}' seviye atladı! Tier: {tier.completionRequirement} - Ödül: {tier.rewardType} ({tier.rewardValue})");
                }
            }
        }
    }

    // ====================================================================================================
    // BİLGİ ALMA (GET) METOTLARI
    // ====================================================================================================

    /// <summary>
    /// Belirtilen ustalık yolu ve ödül türü için toplam birikmiş bonusu döndürür.
    /// </summary>
    public float GetTotalBonusFor(string masteryID, MasteryRewardType rewardType)
    {
        if (string.IsNullOrEmpty(masteryID)) return 0f;
        
        if (_unlockedTierBonuses.TryGetValue(masteryID, out var bonuses) && bonuses.TryGetValue(rewardType, out float value)) 
        { 
            return value; 
        }
        return 0f;
    }

    /// <summary>
    /// Bir ustalık yolunun mevcut tamamlama sayısını döndürür.
    /// </summary>
    public int GetCompletionCount(string masteryID) => _completionCounts.TryGetValue(masteryID, out int count) ? count : 0;

    // ====================================================================================================
    // KAYIT SİSTEMİ (GameDataManager UYUMLU)
    // ====================================================================================================

    /// <summary>
    /// GameDataManager'a kaydedilecek verileri toplar ve döndürür.
    /// </summary>
    public MasterySaveData GetSaveData()
    {
        if (debugLogMastery) Debug.Log("Mastery ilerlemesi kaydedildi."); // ESKİ KODDAN EKLENDİ
        // GameSaveData.cs ile uyumlu
        return new MasterySaveData { completionCounts = new Dictionary<string, int>(_completionCounts) };
    }

    /// <summary>
    /// GameDataManager'dan gelen verileri bu yöneticiye yükler.
    /// </summary>
    public void LoadFromData(MasterySaveData data)
    {
        _completionCounts = data?.completionCounts ?? new Dictionary<string, int>();
        
        if (data != null)
        {
            if (debugLogMastery) Debug.Log("Mastery ilerlemesi yüklendi."); // ESKİ KODDAN EKLENDİ
        }
        else
        {
            if (debugLogMastery) Debug.Log("Kaydedilmiş Mastery ilerlemesi bulunamadı."); // ESKİ KODDAN EKLENDİ
        }

        // Yüklenen verilere göre tüm bonusları yeniden hesapla
        RecalculateAllMasteryBonuses();
    }
    
    /// <summary>
    /// ESKİ KODDAN EKLENDİ: Geliştirme amacıyla tüm ustalık ilerlemesini sıfırlar.
    /// (Sadece hafızadakini sıfırlar, kalıcı silme için 'Restart Game' gerekir)
    /// </summary>
    [ContextMenu("Reset All Mastery Progress (Debug Only)")]
    public void ResetAllMasteryProgress()
    {
        _completionCounts.Clear();
        _unlockedTierBonuses.Clear();
        // PlayerPrefs satırları kaldırıldı, çünkü artık GameDataManager yönetiyor.
        if (debugLogMastery) Debug.Log("Tüm ustalık ilerlemesi (hafızada) sıfırlandı.");
    }
}