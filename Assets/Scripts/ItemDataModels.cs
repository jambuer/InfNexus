using UnityEngine;
using System;

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
    public string requirementType; // "Level", "Stat", "Quest"
    public string requirementName; // Gerekli stat'ın adı veya görevin ID'si
    public int requiredValue;
}