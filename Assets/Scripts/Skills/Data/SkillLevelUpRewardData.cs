using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// "Usta Oduncu" (Seviye 5) veya "Madenci Kaskı" (Seviye 10) gibi
/// spesifik bir beceri seviye atlama ödülünü tanımlayan ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "Yeni Beceri Seviye Ödülü", menuName = "Skills/Skill Level-Up Reward")]
public class SkillLevelUpRewardData : ScriptableObject
{
    [Header("UI & Açıklama")]
    [Tooltip("Örn: 'Usta Oduncu' veya 'Çiftçi Elideğeri'")]
    public string displayName;

    [Tooltip("Örn: 'Odunculuk hızın kalıcı olarak %5 arttı!'")]
    [TextArea(3, 5)]
    public string descriptionForUI;

    [Header("Dağıtılacak Gerçek Ödüller")]
    [Tooltip("GameRewardDistributor tarafından dağıtılacak ödüllerin listesi.")]
    public List<GameReward> rewards; // Utils/Yardimci/RewardData.cs içindeki struct
}