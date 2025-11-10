using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// "Odun Topla", "Maden Kaz" gibi her bir toplayıcılık eylemini
/// tanımlayan ScriptableObject veri yapısı.
/// </summary>
[CreateAssetMenu(fileName = "GatheringNode_", menuName = "Game/Gathering Node")]
public class GatheringNodeData : ScriptableObject
{
    [Header("Temel Bilgiler")]
    [Tooltip("Bu eylemin benzersiz kimliği (ID)")]
    public string nodeID;

    [Tooltip("UI'da görünecek ad (örn: Meşe Odunu Topla)")]
    public string displayName;

    [Tooltip("Eylemin açıklaması")]
    [TextArea(3, 5)]
    public string description;

    [Tooltip("Eylem kartında/prefabında gösterilecek ikon")]
    public Sprite icon;

    [Header("Mekanik ve Seviye")]
    [Tooltip("Bu eylemin hangi Yaşam Becerisini (LifeSkill) geliştirdiği")]
    public LifeSkill associatedSkill; // SkillEnums.cs'den

    [Tooltip("Bu eylemin tamamlanması için gereken temel süre (saniye)")]
    public float baseTimeInSeconds;

    [Tooltip("Eylem tamamlandığında 'associatedSkill'e verilecek temel XP")]
    public double baseSkillXP;

    [Header("Gereksinimler ve Ödüller")]
    [Tooltip("Bu eylemi yapabilmek için gerekenler (örn: Seviye 5 WoodCutter, 10 Enerji)")]
    public List<Requirement> costToPerform; // RequirementType.cs'den

    [Tooltip("Bu eylemi yapabilmek için kilidini açma gereksinimleri (eğer kilitliyse)")]
    public List<Requirement> unlockRequirements; // RequirementType.cs'den

    [Tooltip("Tamamlandığında verilecek ödüller (Item, Gold vb.)")]
    public List<GameReward> rewards; // RewardData.cs'den
}