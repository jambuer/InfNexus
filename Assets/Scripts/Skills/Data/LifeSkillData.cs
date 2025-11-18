using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bir "Life Skill" (Yaşam Becerisi) için tüm temel verileri,
/// XP eğrisini ve seviye atlama ödüllerini içeren ana ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "Yeni Yaşam Becerisi", menuName = "Skills/Life Skill Data")]
public class LifeSkillData : ScriptableObject
{
    [Header("Temel Kimlik")]
    [Tooltip("Bu becerinin bağlı olduğu Enum kimliği")]
    public LifeSkill skillID; // Enums/SkillEnums.cs'den
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
    [Tooltip("Bu beceri belirli seviyelere ulaştığında verilecek ödüller.")]
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
    /// gereken toplam XP'yi hesaplar (basit üssel formül).
    /// </summary>
    public virtual float CalculateXPForNextLevel(int currentLevel)
    {
        if (currentLevel >= maxLevel) return float.MaxValue;
        
        // Formül: TemelXP * (Çarpan ^ (MevcutSeviye - 1))
        // Seviye 1 -> 100 * (1.15^0) = 100
        // Seviye 2 -> 100 * (1.15^1) = 115
        // Seviye 3 -> 100 * (1.15^2) = 132
        return Mathf.FloorToInt(baseXPForNextLevel * Mathf.Pow(xpToNextLevelMultiplier, (currentLevel - startLevel)));
    }

    /// <summary>
    /// Bu becerinin sağladığı özel bonusları almak için (ileride kullanılacak).
    /// </summary>
    public virtual float GetBonus(int currentLevel, string bonusType)
    {
        // İleride burası doldurulacak.
        // Örn: if (bonusType == "GatheringSpeed") return currentLevel * 0.01f;
        return 0f;
    }

    /// <summary>
    /// [YENİ] Bu becerinin mevcut seviyede sağladığı bonusların
    /// UI'da gösterilecek açıklamasını döndürür.
    /// (Adım 12'de eklendi)
    /// </summary>
    public virtual string GetBonusDescription(int currentLevel)
    {
        // Varsayılan olarak, bu beceri seviye başına bonus vermiyorsa
        return "Seviye atlamak bu becerinin pasif bonuslarını artırmaz.";
    }
    
}