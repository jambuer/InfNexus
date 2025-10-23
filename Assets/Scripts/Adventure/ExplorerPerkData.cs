using UnityEngine;
using System.Collections.Generic;

// Sol paneldeki her bir öğenin hangi aşamada olduğunu tutan enum
public enum PerkState
{
    Locked,     // Henüz görünmüyor (bir önceki alınmadı)
    Unlockable, // Görünüyor, "UNLOCK" butonu aktif (Sadece Req gösterilir)
    Payable,    // Kilidi açıldı, "PAY" butonu aktif (Req + Price gösterilir)
    Purchased   // Satın alındı, listeden kaybolacak
}

// Inspector'dan seçeceğimiz etiketler
public enum ExplorerTag
{
    Stat,
    XPGold,
    Explorer,
    ExplorerTime,
    ExplorerQuest,
    ExplorerBoss,
    Unlock,
    Bad,
    Nexus,
    Premium
}

// Verilecek Mastery Ödülü
[System.Serializable]
public class PerkReward
{
    [Tooltip("PerkManager'da tutulacak benzersiz mastery adı (örn: 'First', 'Explorer')")]
    public string masteryName;
    [Tooltip("Bu alım oyuncuya kaç 'stack' kazandıracak?")]
    public int amount = 1;
    [Tooltip("UI'da gösterilecek ödül açıklaması (örn: '+1 AllStats')")]
    public string description;
}

[CreateAssetMenu(fileName = "NewExplorerPerk", menuName = "Adventure/Explorer Perk (Sol Panel)")]
public class ExplorerPerkData : ScriptableObject
{
    [Tooltip("Unity Editor'de göreceğimiz etiket (filtreleme için değil)")]
    public ExplorerTag tag;

    [Tooltip("Kilit açılmadan önce gösterilecek açıklama (Desc)")]
    [TextArea(3, 5)]
    public string description;

    [Header("Aşama 2: Kilit Açma")]
    [Tooltip("'UNLOCK' butonu için gereken şartlar")]
    public List<Requirement> unlockRequirements;

    [Header("Aşama 3: Satın Alma")]
    [Tooltip("'PAY' butonu için gereken maliyetler")]
    public List<Requirement> purchasePrice;

    [Header("Aşama 4: Ödül")]
    [Tooltip("Satın alındığında verilecek kalıcı ustalık (Perk) ödülü")]
    public PerkReward reward;
}