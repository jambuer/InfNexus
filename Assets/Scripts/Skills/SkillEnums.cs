using UnityEngine;

/// <summary>
/// Oyuncunun yaşam becerilerini (toplayıcılık vb.) tanımlar.
/// </summary>
public enum LifeSkill
{
    None,
    WoodCutter, // Odunculuk
    Foraging,   // Toplayıcılık (Bitki)
    Miner,      // Madencilik
    Hunting,    // Avcılık
    Fishing,    // Balıkçılık
    Scavenger   // Yağmacılık
}

/// <summary>
/// Oyuncunun edinebileceği üretim ve diğer meslekleri tanımlar.
/// </summary>
public enum Job
{
    None,
    Alchemist,  // Simyacı
    Trader,     // Tüccar
    Chef,       // Aşçı
    Blacksmith, // Demirci
    Farmer,     // Çiftçi
    Carpenter,  // Marangoz
    Tailor,     // Terzi
    Jeweler,    // Kuyumcu
    Engineer,   // Mühendis
    Tanner,     // Derici
    Tamer       // Evcilleştirici
}