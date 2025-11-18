using UnityEngine;
using System.Collections.Generic;
using static ExplorerPerkData; // ExplorerTag enum'unu kullanabilmek için

// ========================================================================
// REFACTORING BURADA BAŞLIYOR
// ========================================================================

// Artık 'ExplorerReward' sınıfına ihtiyacımız yok.
// Onun yerine, merkezi GameReward listemizi tutacak basit bir wrapper kullanacağız.
// Unity'nin Inspector'da List<List<GameReward>> gösterememesi (serialize edememesi)
// nedeniyle bu wrapper sınıfa ihtiyacımız var.

[System.Serializable]
public class GameRewardList
{
    [Tooltip("Bu tamamlama seviyesi için verilecek ödüller (XP, Altın, Eşya, Stat vb.)")]
    public List<GameReward> rewards; // RewardData.cs'deki merkezi struct
}

// --- ESKİ YAPI SİLİNDİ ---
// [System.Serializable]
// public class ExplorerReward 
// { ... } // Bu sınıf (StatReward içeren) tamamen silindi.

// ========================================================================
// REFACTORING BURADA BİTİYOR
// ========================================================================


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
    public List<Requirement> requirements; // RequirementType.cs'deki merkezi struct

    [Tooltip("Görevin temel tamamlanma süresi (saniye)")]
    public float baseCompletionTime = 60f;

    [Tooltip("Görevin kaç kez tamamlanabileceği")]
    public int completionLimit = 3;

    [Header("Ödüller (Yeni Merkezi Sistem)")]
    [Tooltip("Her bir tamamlama için sırayla verilecek ödüller listesi. (Liste boyutu Completion Limit ile eşleşmeli)")]
    // [YENİ] Artık ExplorerReward yerine GameRewardList kullanıyor.
    public List<GameRewardList> rewardsPerCompletion;

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
}