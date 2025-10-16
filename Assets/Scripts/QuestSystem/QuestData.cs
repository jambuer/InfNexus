using UnityEngine;
using System.Collections.Generic; // Listeleri kullanmak için
using System; // Serializable sınıflar için

/// <summary>
/// Tek bir görevin tüm veri tanımlarını içeren ScriptableObject.
/// Unity Editor'dan yeni görevler oluşturmak için kullanılır.
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest Data")]
public class QuestData : ScriptableObject
{
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
    public QuestRequirements requirements;

    [Header("Ödüller")]
    /// <summary>Görevin tamamlanmasıyla kazanılacak temel tecrübe puanı.</summary>
    public double experienceReward;
    /// <summary>Görevin tamamlanmasıyla kazanılacak altın ödülü dilimleri.</summary>
    public List<RewardTier> goldRewardTiers;
    /// <summary>Görevin tamamlanmasıyla kazanılacak Nexus Coin ödülü dilimleri.</summary>
    public List<RewardTier> nexusCoinRewardTiers;
    /// <summary>Görevin tamamlanmasıyla düşebilecek eşya ödülleri.</summary>
    public List<ItemDrop> itemRewards; // ItemData referansı burada kullanılacak
    /// <summary>Görevin tamamlanmasıyla kazanılacak kalıcı stat ödülleri.</summary>
    public List<StatReward> statRewards;

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