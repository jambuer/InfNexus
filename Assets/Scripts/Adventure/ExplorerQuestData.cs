using UnityEngine;
using System.Collections.Generic;
using static ExplorerPerkData; // ExplorerTag enum'unu kullanabilmek için

// Sağ panel görevleri için özel ödül yapısı
[System.Serializable]
public class ExplorerReward
{
    [Tooltip("UI'da gösterilecek ödül açıklaması (örn: '+3 Physical Stat')")]
    public string rewardDescription; // "+3 Physical Stat"

    [Tooltip("Bu tamamlamada verilecek stat ödülleri")]
    public List<StatReward> statRewards; // Mevcut StatReward yapısını kullanıyoruz

    [Tooltip("Bu tamamlamada verilecek eşya ödülleri")]
    public List<ItemDrop> itemRewards; // Mevcut ItemDrop yapısını kullanıyoruz

    [Tooltip("Bu tamamlamada verilecek Perk (Ustalık) ödülleri")]
    public List<PerkReward> perkRewards; // Sol paneldeki PerkReward yapısını kullanıyoruz
}

[CreateAssetMenu(fileName = "NewExplorerQuest", menuName = "Adventure/Explorer Quest (Sağ Panel)")]
public class ExplorerQuestData : ScriptableObject
{
    [Tooltip("Unity Editor'de göreceğimiz etiket")]
    public ExplorerTag tag; // Sol panelde tanımladığımız enum'u kullanıyoruz

    [Tooltip("Görevin benzersiz kimliği (kayıt sistemi için)")]
    public string questID;

    [Tooltip("Görevin açıklaması (Desc)")]
    [TextArea(3, 5)]
    public string description;

    [Header("Gereksinimler")]
    [Tooltip("Görevi başlatmak için gereken şartlar (enerji, eşya vb.)")]
    public List<Requirement> requirements; // Mevcut Requirement yapısını kullanıyoruz

    [Tooltip("Görevin temel tamamlanma süresi (saniye)")]
    public float baseCompletionTime = 60f;

    [Tooltip("Görevin kaç kez tamamlanabileceği")]
    public int completionLimit = 3;

    [Header("Ödüller")]
    [Tooltip("Her bir tamamlama için sırayla verilecek ödüller listesi. (Liste boyutu Completion Limit ile eşleşmeli)")]
    public List<ExplorerReward> rewardsPerCompletion;

    [Header("İlerleme")]
    [Tooltip("Bu görev tamamlandığında hangi Explorer Quest'inin kilidini açar (ID'si)")]
    public string unlocksQuestID; // Bir sonraki görevin kilidini açmak için

    [HideInInspector] public bool isTimerBased; // Zamanlayıcı mı (ExplorerTime) yoksa normal görev mi?

    private void OnValidate()
    {
        // Tag'e göre görevin zaman bazlı olup olmadığını otomatik ayarla
        isTimerBased = (tag == ExplorerTag.ExplorerTime);

        // questID boş ise otomatik ata
        if (string.IsNullOrEmpty(questID))
        {
            questID = System.Guid.NewGuid().ToString();
        }
    }
    // Not: StatReward, ItemDrop ve Requirement sınıflarının başka dosyalarda tanımlı olduğunu varsayıyoruz.
}