using UnityEngine;
using System.Text;

/// <summary>
/// 'WoodCutter' (Odunculuk) becerisine özel bonus mantığını tanımlar.
/// LifeSkillData'dan miras alır ve GetBonus / GetBonusDescription fonksiyonlarını ezer.
/// (Adım 12'de eklendi)
/// </summary>
[CreateAssetMenu(fileName = "WoodCutterData", menuName = "Skills/Life Skill Data/WoodCutter Skill")]
public class WoodCutterData : LifeSkillData
{
    // Bu beceriye özel bonus katsayılarını Inspector'dan ayarlayabilmemiz için
    [Header("WoodCutter Bonusları")]
    [Tooltip("Seviye başına üretim (toplama) hızı bonusu (Örn: 0.01 = %1)")]
    [SerializeField] private float productionBonusPerLevel = 0.01f; // %1

    
    /// <summary>
    /// [DEĞİŞTİ] Adım 15 Öncesi: Artık SADECE 'Production' bonusu verir.
    /// 'DropRate' bonusu kaldırıldı (Efficiency sistemine geçti).
    /// </summary>
    public override float GetBonus(int currentLevel, string bonusType)
    {
        int effectiveLevel = currentLevel - this.startLevel;
        if (effectiveLevel <= 0) return 0f;

        switch (bonusType)
        {
            case "Production": 
                return effectiveLevel * productionBonusPerLevel;
            
            // case "DropRate": // [KALDIRILDI]
            //    return effectiveLevel * dropRateBonusPerLevel;
            
            default:
                return 0f;
        }
    }

    /// <summary>
    /// [DEĞİŞTİ] Adım 15 Öncesi: 'DropRate' açıklaması kaldırıldı.
    /// </summary>
    public override string GetBonusDescription(int currentLevel)
    {
        int effectiveLevel = currentLevel - this.startLevel;
        if (effectiveLevel <= 0)
        {
            return "Bonusları açmak için seviye atla.";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>Mevcut Seviye Bonusları:</b>");

        float prodBonus = GetBonus(currentLevel, "Production") * 100f;
        sb.AppendLine($"<color=green>- +{prodBonus:F1}% Toplama Hızı</color>");

        // float dropBonus = GetBonus(currentLevel, "DropRate") * 100f; // [KALDIRILDI]
        // sb.AppendLine($"<color=green>- +{dropBonus:F1}% Nadir Eşya Şansı</color>"); // [KALDIRILDI]
        
        return sb.ToString();
    }
}