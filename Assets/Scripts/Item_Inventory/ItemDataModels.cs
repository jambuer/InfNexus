using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

// Item'ların sahip olabileceği etiketler. Burayı istediğin gibi genişletebilirsin.
public enum ItemTag
{
    Toplanabilir,
    GörevEşyası,
    Silah,
    Zırh,
    Tüketilebilir,
    Materyal
}

// Stat bonuslarını tanımlamak için (Örn: +10 Güç)
[Serializable]
public struct StatBonus
{
    public string statName; // Etkilenecek stat'ın adı (Örn: "Güç", "Dayanıklılık")
    public int value;
}

// Item'ı kullanmak için gerekenleri tanımlamak için (Örn: Seviye 5, "Demircilik" görevi tamamlandı)
[Serializable]
public struct Requirement
{
    // public string requirementType; // ESKİ SATIRI SİLİN VEYA YORUMA ALIN
    [Tooltip("Gereksinimin türü (Seviye, Eşya, Stat vb.)")]
    public RequirementType reqType; // YENİ ALAN (enum türünde)

    [Tooltip("Gereksinimle ilgili ek bilgi (Eşya adı, Stat adı, Görev ID'si vb.)")]
    public string requirementName; // Bu alan aynı kalıyor

    [Tooltip("Gereksinim UI'da nasıl görünecek (Boşsa, requirementName kullanılır)")]
    public string displayName;

    // DİKKAT: Orijinal kodunuzda int idi. Float mı olmalıydı?
    // Şimdilik int bırakıyorum, eğer float gerekiyorsa float yapın.
    [Tooltip("Gereken değer (Seviye, Eşya miktarı, Stat değeri vb.)")]
    public int requiredValue;
}



