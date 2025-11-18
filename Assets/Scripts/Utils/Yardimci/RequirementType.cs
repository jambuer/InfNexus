// RequirementType.cs

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;


public enum RequirementType
{
None, 
Level,
Quest,
ExplorerQuest,
Item,
Stat,
Gold,
NexusCoin,
People,
Health, 
Energy, 
Mana, 
MaxHealth,
MaxEnergy,
MaxMana,
Perk, 
ExplorerTime,

Chapter, // YENİ EKLENDİ

LifeSkillLevel, // YORUM KALDIRILDI
JobLevel // YORUM KALDIRILDI
}

[Serializable]
public struct GameRequirement // <-- ADI DEĞİŞTİ
{
    [Tooltip("Gereksinim türü (Seviye, Eşya, Altın, Beceri Seviyesi vb.)")]
    public RequirementType requirementType;

    [Tooltip("Gerekli olan şeyin adı/ID'si (Eşya Adı, Stat Adı, Beceri Adı vb.)")]
    public string stringParameter;

    [Tooltip("Gereken miktar (Seviye, Eşya Sayısı, Altın Miktarı vb.)")]
    public double amount;
}
