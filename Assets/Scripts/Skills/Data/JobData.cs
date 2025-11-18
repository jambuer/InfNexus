using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bir "Job" (Meslek) için tüm temel verileri,
/// XP eğrisini ve seviye atlama ödüllerini içeren ana ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "Yeni Meslek", menuName = "Skills/Job Data")]
public class JobData : ScriptableObject
{
    [Header("Temel Kimlik")]
    [Tooltip("Bu mesleğin bağlı olduğu Enum kimliği")]
    public Job jobID; // Enums/SkillEnums.cs'den
    public string displayName;
    [TextArea(3, 5)] public string description;

    [Header("Seviye & XP Ayarları")]
    [Tooltip("Bir sonraki seviye için gereken XP çarpanı. Örn: 1.15")]
    public float xpToNextLevelMultiplier = 1.15f;
    [Tooltip("Seviye 1'den 2'ye geçmek için gereken temel XP miktarı.")]
    public int baseXPForNextLevel = 100;
    public int startLevel = 1;
    public int maxLevel = 100;

    [Header("Seviye Atlama Ödülleri")]
    [Tooltip("Bu meslek belirli seviyelere ulaştığında verilecek ödüller.")]
    public List<SkillLevelRewardMap> levelUpRewards;

    /// <summary>
    /// Bu struct, belirli bir seviyeyi bir ödül asset'ine bağlar.
    /// </summary>
    [System.Serializable]
    public class SkillLevelRewardMap
    {
        [Tooltip("Bu ödülün verileceği seviye")]
        public int levelToAward;
        
        [Tooltip("O seviyeye ulaşıldığında verilecek 'SkillLevelUpRewardData' asset'i")]
        public SkillLevelUpRewardData rewardDataAsset;
    }

    // --- MANTIK FONKSİYONLARI ---

    /// <summary>
    /// Belirli bir seviye için bir sonraki seviyeye geçmek için
    /// gereken toplam XP'yi hesaplar.
    /// </summary>
    public virtual float CalculateXPForNextLevel(int currentLevel)
    {
        if (currentLevel >= maxLevel) return float.MaxValue;
        
        return Mathf.FloorToInt(baseXPForNextLevel * Mathf.Pow(xpToNextLevelMultiplier, (currentLevel - startLevel)));
    }

    /// <summary>
    /// Bu mesleğin sağladığı özel bonusları almak için (ileride kullanılacak).
    /// </summary>
    public virtual float GetBonus(int currentLevel, string bonusType)
    {
        // İleride burası doldurulacak.
        return 0f;
    }
}