/// <summary>
/// LifeSkill ve Job'lar için tüm "Efficiency" (Verimlilik)
/// stat'larını listeleyen enum. (Adım 15'ten önce eklendi)
/// </summary>
public enum SkillEfficiencyType
{
    None,

    // Life Skills
    WoodCutterEfficiency,
    ForagingEfficiency,
    MiningEfficiency,
    HuntingEfficiency,
    FishingEfficiency,
    ScavengerEfficiency,

    // Jobs
    AlchemistEfficiency,
    TradingEfficiency,
    ChefEfficiency,
    FarmingEfficiency,
    BlacksmithEfficiency,
    CarpenterEfficiency,
    TailorEfficiency,
    TannerEfficiency,
    TamerEfficiency,
    JewelerEfficiency,
    EngineerEfficiency,

    // Yüzdesel Bonus (Global)
    SkillEfficiency
}