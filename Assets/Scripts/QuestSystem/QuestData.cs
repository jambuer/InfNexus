using UnityEngine;
using System.Collections.Generic; // Listeleri kullanmak için
using System; // Serializable sınıflar için

/// <summary>
/// Tek bir görevin tüm veri tanımlarını içeren ScriptableObject.
/// Unity Editor'dan yeni görevler oluşturmak için kullanılır.
/// </summary>
/// 
public enum QuestArea {
    Global, // Genel görevler
    City,   // Şehir görevleri
    Adventure, // Macera bölgesi görevleri
    Dungeon // Zindan görevleri (örnek)
    // İhtiyaç oldukça burayı genişletebilirsin
}
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
    
    [Header("Bölge Bilgisi")]
    public QuestArea questArea = QuestArea.Global; // Varsayılan olarak Global olsun

    [Header("Temel Bilgiler")]
    /// <summary>Görevin benzersiz kimliği.</summary>
    public string questID; // Görevin benzersiz kimliği, otomatik atanabilir veya manuel verilebilir.
    /// <summary>Görevin adı.</summary>
    public string questName = "New Quest";
    /// <summary>Görevin açıklaması.</summary>
    [TextArea(3, 10)]
    public string description = "A detailed description of the quest objectives.";
    /// <summary>Görevin arayüzdeki ikonu.</summary>
    public Sprite questIcon;

    
    /// <summary>Görevin temel tamamlanma süresi (saniye cinsinden).</summary>
    public float baseCompletionTime = 10f; // Saniye cinsinden
    /// <summary>Görevin birincil kategorisi.</summary>
    public MainQuestTag mainTag;
    /// <summary>Görevin ikincil kategorisi.</summary>
    public SubQuestTag subTag;
    /// <summary>Görevin kaç kez tamamlanabileceği. 0 ise sınırsız anlamına gelir.</summary>
    public int completionLimit = 0;
    /// <summary>Bu görevin Ustalık Sistemi'ndeki benzersiz kimliği. Boş bırakılırsa ustalık kazandırmaz.</summary>
    public string masteryID = ""; // Boş ise ustalık kazandırmaz

    [Header("Gereksinimler")]
    /// <summary>Görevi başlatmak için gerekenleri içeren sınıf.</summary>
    public List<Requirement> requirements; // [YENİ] Merkezi gereksinim listesi

    // ========================================================================
    // REFACTORING BURADA BAŞLIYOR
    // ========================================================================

    [Header("Ödüller (Yeni Merkezi Sistem)")]
    /// <summary>
    /// [YENİ] Görev tamamlandığında verilecek tüm ödüllerin listesi.
    /// Bu liste, GameRewardDistributor tarafından işlenecektir.
    /// </summary>
    public List<GameReward> rewards; // RewardData.cs içindeki struct

    
    // --- ESKİ ÖDÜL SİSTEMİ KALDIRILDI ---
    // public double experienceReward; // KALDIRILDI (Artık 'rewards' listesinde)
    // public List<RewardTier> goldRewardTiers; // KALDIRILDI (Artık 'rewards' listesinde)
    // public List<RewardTier> nexusCoinRewardTiers; // KALDIRILDI (Artık 'rewards' listesinde)
    // public List<ItemDrop> itemRewards; // KALDIRILDI (Artık 'rewards' listesinde)
    // public List<StatReward> statRewards; // KALDIRILDI (Artık 'rewards' listesinde)
    //
    // ========================================================================
    // REFACTORING BURADA BİTİYOR
    // ========================================================================


    [Header("Otomasyon")]
    /// <summary>Görevin otomasyonuyla ilgili tüm verileri içeren sınıf.</summary>
    public AutomationData automationData;

    // Her yeni QuestData ScriptableObject'ı oluşturulduğunda benzersiz bir ID atar
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = Guid.NewGuid().ToString();
        }
    }
}