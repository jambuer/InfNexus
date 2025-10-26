using UnityEngine;
using System.Collections.Generic;


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



public enum PerkState { Locked, Unlockable, Payable, Purchased }

// Verilecek Mastery Ödülü
[System.Serializable]
public class PerkReward
{
    [Tooltip("Bu Explorer Perk'i tamamlandığında verilecek olan temel Perk Definition asset'i.")]
    public PerkDefinition perkToGrant; // Artık PerkDefinition'a referans tutuyor

    [Tooltip("Bu alım oyuncuya kaç 'stack' kazandıracak? (Genellikle 1)")]
    public int amount = 1;

    // EffectType, Value, Parameter alanları buradan KALDIRILDI.
    // Onlar artık perkToGrant referansı üzerinden okunacak.

    [Tooltip("UI'da gösterilecek ödül açıklaması (Opsiyonel, PerkDefinition'dan alınabilir).")]
    public string descriptionOverride = ""; // İsteğe bağlı açıklama
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