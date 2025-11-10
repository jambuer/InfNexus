using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

// RequirementType.cs

/// <summary>
/// Adventure sisteminde kullanılabilecek tüm gereksinim türlerini tanımlar.
/// </summary>
public enum RequirementType
{
    None, // Varsayılan, geçersiz
    Level,
    Quest,          // Ana görev sistemindeki görev
    ExplorerQuest,  // Adventure sağ panel görevi
    Item,
    Stat,
    Gold,
    NexusCoin,
    People,
    Health,         // Mevcut can
    Energy,         // Mevcut enerji
    Mana,           // Mevcut mana
    MaxHealth,      // Maksimum can (hem gereksinim hem maliyet olabilir)
    MaxEnergy,      // Maksimum enerji (hem gereksinim hem maliyet olabilir)
    MaxMana,        // Maksimum mana (hem gereksinim hem maliyet olabilir)
    Perk,           // Belirli bir sol panel perk'ine sahip olma
    ExplorerTime,    // Özel zamanlayıcı gereksinimi (Qperk8'deki gibi)
    // === GATHERING SİSTEMİ İÇİN GEÇİCİ OLARAK DEVRE DIŞI ===
    // === KOD TOPARLAMA (REFACTOR) SONRASI AÇILACAK ===
    /*
    LifeSkillLevel, // Belirli bir Yaşam Becerisi seviyesine sahip olma
    JobLevel        // Belirli bir İş (Job) seviyesine sahip olma
    */
    // İhtiyaç duydukça buraya yeni türler ekleyebilirsiniz
}

// === GATHERING SİSTEMİ İÇİN GEÇİCİ OLARAK DEVRE DIŞI ===
    // === KOD TOPARLAMA (REFACTOR) SONRASI AÇILACAK ===
    /*
    LifeSkillLevel, // Belirli bir Yaşam Becerisi seviyesine sahip olma
    JobLevel        // Belirli bir İş (Job) seviyesine sahip olma
    */