using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // Dictionary işlemleri için

/// <summary>
/// Evrensel Ustalık Sistemi'nin ana yöneticisi.
/// Oyuncunun tüm ustalık yollarındaki ilerlemesini takip eder,
/// seviye atlamaları yönetir ve kazandığı bonusları hesaplar.
/// Singleton pattern kullanılarak her yerden erişilebilir.
/// </summary>
public class MasteryManager : MonoBehaviour
{
    public static MasteryManager Instance { get; private set; }

    [Header("Referanslar")]
    [Tooltip("Projenizdeki tüm ustalığı içeren MasteryDatabase ScriptableObject'ı.")]
    public MasteryDatabase masteryDatabase; // Inspector'dan atanacak

    [Header("Debug/Geliştirici Araçları")]
    [Tooltip("Ustalık ilerlemesinin konsola yazdırılıp yazdırılmayacağını belirler.")]
    public bool debugLogMastery = false;

    // Oyuncunun her bir ustalık yolundaki tamamlanma sayıları
    // Key: masteryID (string), Value: o ustalık için toplam tamamlama sayısı (int)
    private Dictionary<string, int> _completionCounts = new Dictionary<string, int>();

    // Oyuncunun her bir ustalık yolu için açtığı seviye bonusları
    // Key: masteryID (string)
    // Value: Dictionary<MasteryRewardType, float> -> O ustalık için her bonus türünün toplam değeri
    private Dictionary<string, Dictionary<MasteryRewardType, float>> _unlockedTierBonuses = new Dictionary<string, Dictionary<MasteryRewardType, float>>();

    // ====================================================================================================
    // SINGLETON VE BAŞLANGIÇ METOTLARI
    // ====================================================================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne geçişlerinde yok olmamasını sağlar
            LoadMasteryData(); // Kaydedilmiş verileri yükle
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Tüm ustalılık yollarını veri tabanından yükledikten sonra bonusları hesapla
        // Bu, oyun başladığında oyuncunun sahip olduğu tüm ustalık bonuslarının aktif olmasını sağlar.
        RecalculateAllMasteryBonuses();
    }

    // ====================================================================================================
    // ANA İLERLEME METOTLARI
    // ====================================================================================================

    /// <summary>
    /// Belirtilen ustalık yolunda ilerleme kaydeder.
    /// Genellikle bir görev tamamlandığında veya eylem yapıldığında çağrılır.
    /// </summary>
    /// <param name="masteryID">İlerlenecek ustalık yolunun benzersiz kimliği.</param>
    /// <param name="amount">İlerlenecek miktar (varsayılan: 1).</param>
    public void ProgressMastery(string masteryID, int amount = 1)
    {
        if (string.IsNullOrEmpty(masteryID))
        {
            if (debugLogMastery) Debug.LogWarning("ProgressMastery: Boş bir masteryID ile ilerleme kaydedilemez.");
            return;
        }

        // MasteryData'yı veritabanından bul
        MasteryData masteryData = masteryDatabase.GetMasteryData(masteryID);
        if (masteryData == null)
        {
            if (debugLogMastery) Debug.LogWarning($"ProgressMastery: {masteryID} ID'li ustalık yolu bulunamadı.");
            return;
        }

        // Mevcut ilerlemeyi al veya sıfırdan başla
        if (!_completionCounts.ContainsKey(masteryID))
        {
            _completionCounts[masteryID] = 0;
        }
        int oldCompletionCount = _completionCounts[masteryID];
        _completionCounts[masteryID] += amount;
        int newCompletionCount = _completionCounts[masteryID];

        if (debugLogMastery) Debug.Log($"Mastery '{masteryData.displayName}' ilerlemesi: {oldCompletionCount} -> {newCompletionCount} completions.");

        // Seviye atlamalarını kontrol et ve bonusları güncelle
        CheckForTierUnlocks(masteryData, oldCompletionCount, newCompletionCount);

        SaveMasteryData(); // İlerlemeyi kaydet
    }

    /// <summary>
    /// Bir ustalık yolunda yeni seviyelerin açılıp açılmadığını kontrol eder ve bonusları günceller.
    /// </summary>
    /// <param name="masteryData">Kontrol edilecek MasteryData objesi.</param>
    /// <param name="oldCompletions">Önceki tamamlama sayısı.</param>
    /// <param name="newCompletions">Yeni tamamlama sayısı.</param>
    private void CheckForTierUnlocks(MasteryData masteryData, int oldCompletions, int newCompletions)
    {
        // Bu ustalık için bonusları tutacak Dictionary'yi hazırla
        if (!_unlockedTierBonuses.ContainsKey(masteryData.masteryID))
        {
            _unlockedTierBonuses[masteryData.masteryID] = new Dictionary<MasteryRewardType, float>();
        }
        else
        {
            // Bonusları sıfırla ve yeniden hesapla (her seviye atladığında güncel kalmasını sağlar)
            _unlockedTierBonuses[masteryData.masteryID].Clear();
        }

        // Tüm seviyeleri kontrol et
        foreach (MasteryTier tier in masteryData.masteryTiers.OrderBy(t => t.completionRequirement)) // Gereksinime göre sırala
        {
            if (newCompletions >= tier.completionRequirement)
            {
                // Seviye açılmışsa bonusu ekle
                if (_unlockedTierBonuses[masteryData.masteryID].ContainsKey(tier.rewardType))
                {
                    _unlockedTierBonuses[masteryData.masteryID][tier.rewardType] += tier.rewardValue;
                }
                else
                {
                    _unlockedTierBonuses[masteryData.masteryID][tier.rewardType] = tier.rewardValue;
                }

                if (debugLogMastery && oldCompletions < tier.completionRequirement) // Sadece yeni açıldığında logla
                {
                    Debug.Log($"Mastery '{masteryData.displayName}' seviye atladı! Tier: {tier.completionRequirement} - Ödül: {tier.rewardType} ({tier.rewardValue})");
                }
            }
        }
    }

    /// <summary>
    /// Oyun başladığında veya yükleme sonrası tüm ustalık bonuslarını yeniden hesaplar.
    /// </summary>
    public void RecalculateAllMasteryBonuses()
    {
        _unlockedTierBonuses.Clear(); // Önceki tüm bonusları temizle

        foreach (var entry in _completionCounts)
        {
            string masteryID = entry.Key;
            int completions = entry.Value;

            MasteryData masteryData = masteryDatabase.GetMasteryData(masteryID);
            if (masteryData == null)
            {
                if (debugLogMastery) Debug.LogWarning($"RecalculateAllMasteryBonuses: {masteryID} ID'li ustalık yolu bulunamadı.");
                continue;
            }

            // Bu ustalık için bonusları tutacak Dictionary'yi hazırla
            if (!_unlockedTierBonuses.ContainsKey(masteryID))
            {
                _unlockedTierBonuses[masteryID] = new Dictionary<MasteryRewardType, float>();
            }

            foreach (MasteryTier tier in masteryData.masteryTiers.OrderBy(t => t.completionRequirement))
            {
                if (completions >= tier.completionRequirement)
                {
                    if (_unlockedTierBonuses[masteryID].ContainsKey(tier.rewardType))
                    {
                        _unlockedTierBonuses[masteryID][tier.rewardType] += tier.rewardValue;
                    }
                    else
                    {
                        _unlockedTierBonuses[masteryID][tier.rewardType] = tier.rewardValue;
                    }
                }
            }
        }
        if (debugLogMastery) Debug.Log("Tüm ustalık bonusları yeniden hesaplandı.");
    }


    // ====================================================================================================
    // BONUS SORGULAMA METOTLARI
    // ====================================================================================================

    /// <summary>
    /// Belirtilen ustalık yolunda, belirtilen ödül türü için kazanılan toplam bonusu döndürür.
    /// Diğer sistemler (örn: QuestManager) bu metodu kullanarak bonusları sorgular.
    /// </summary>
    /// <param name="masteryID">Bonusu sorgulanacak ustalık yolunun kimliği.</param>
    /// <param name="rewardType">Sorgulanacak ödül türü.</param>
    /// <returns>Kazanılan toplam bonus miktarı. Eğer bonus yoksa 0 döndürür.</returns>
    public float GetTotalBonusFor(string masteryID, MasteryRewardType rewardType)
    {
        if (string.IsNullOrEmpty(masteryID)) return 0f;

        if (_unlockedTierBonuses.ContainsKey(masteryID) && _unlockedTierBonuses[masteryID].ContainsKey(rewardType))
        {
            return _unlockedTierBonuses[masteryID][rewardType];
        }
        return 0f;
    }

    /// <summary>
    /// Bir ustalık yolu için toplam tamamlama sayısını döndürür.
    /// </summary>
    /// <param name="masteryID">Sorgulanacak ustalık yolunun kimliği.</param>
    /// <returns>Toplam tamamlama sayısı.</returns>
    public int GetCompletionCount(string masteryID)
    {
        if (_completionCounts.ContainsKey(masteryID))
        {
            return _completionCounts[masteryID];
        }
        return 0;
    }

    // ====================================================================================================
    // KAYDETME/YÜKLEME (SAVE/LOAD) ALTYAPISI
    // ====================================================================================================

    // Kaydedilebilir veriler için yardımcı sınıf
    [Serializable]
    private class MasterySaveData
    {
        public List<string> masteryIDs;
        public List<int> completionCounts;

        public MasterySaveData(Dictionary<string, int> counts)
        {
            masteryIDs = counts.Keys.ToList();
            completionCounts = counts.Values.ToList();
        }
    }

    /// <summary>
    /// Ustalık ilerlemesi verilerini kaydeder.
    /// </summary>
    public void SaveMasteryData()
    {
        MasterySaveData saveData = new MasterySaveData(_completionCounts);
        string jsonData = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("MasteryProgress", jsonData);
        PlayerPrefs.Save();
        if (debugLogMastery) Debug.Log("Mastery ilerlemesi kaydedildi.");
    }

    /// <summary>
    /// Ustalık ilerlemesi verilerini yükler.
    /// </summary>
    public void LoadMasteryData()
    {
        if (PlayerPrefs.HasKey("MasteryProgress"))
        {
            string jsonData = PlayerPrefs.GetString("MasteryProgress");
            MasterySaveData saveData = JsonUtility.FromJson<MasterySaveData>(jsonData);

            _completionCounts.Clear();
            for (int i = 0; i < saveData.masteryIDs.Count; i++)
            {
                _completionCounts[saveData.masteryIDs[i]] = saveData.completionCounts[i];
            }
            if (debugLogMastery) Debug.Log("Mastery ilerlemesi yüklendi.");
            RecalculateAllMasteryBonuses(); // Yüklendikten sonra bonusları yeniden hesapla
        }
        else
        {
            if (debugLogMastery) Debug.Log("Kaydedilmiş Mastery ilerlemesi bulunamadı.");
        }
    }

    /// <summary>
    /// Geliştirme amacıyla tüm ustalık ilerlemesini sıfırlar.
    /// </summary>
    [ContextMenu("Reset All Mastery Progress (Debug Only)")]
    public void ResetAllMasteryProgress()
    {
        _completionCounts.Clear();
        _unlockedTierBonuses.Clear();
        PlayerPrefs.DeleteKey("MasteryProgress");
        PlayerPrefs.Save();
        if (debugLogMastery) Debug.Log("Tüm ustalık ilerlemesi sıfırlandı.");
    }
}