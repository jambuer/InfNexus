using UnityEngine;

// Bu enum, PerkManager'da kullandığımız PerkEffectType ile AYNI olmalı.
// Tek bir yerde tanımlayıp her yerden kullanmak en iyisidir.
// Örneğin, bunu ayrı bir "PerkEnums.cs" dosyasına taşıyabiliriz.
// Şimdilik buraya kopyalıyorum, daha sonra taşıyabiliriz.
public enum PerkEffectType
{
    None, AddStat, AddAllStats, AddStatPoints, AddGoldBonus, AddXPBonus,
    ModifyResourceMaxHealth, ModifyResourceMaxEnergy, ModifyResourceMaxMana,
    ModifyResourceHealthRecovery, ModifyResourceEnergyRecovery, ModifyResourceManaRecovery,
    AddCriticalChance, AddCriticalDamage, AddDropRate, ReduceEnemyHealth,
    ReduceEnemyDamage, ReduceEnemyArmor, AddHitRate, AddProduction,
    AddCooldownReduction, AddResourceCostReduction, AddPrestigePoints,
    AddPrestigeBonus, UnlockFeature, GrantItem, GetExplorerTimeReduction,

    
    AddSkillEfficiency, // (Bunu bir önceki adımda eklemiştik)

    // [YENİ EKLENECEK KISIM]
    // Life Skills
    AddWoodCutterEfficiency,
    AddForagingEfficiency,
    AddMiningEfficiency,
    AddHuntingEfficiency,
    AddFishingEfficiency,
    AddScavengerEfficiency,

    // Jobs
    AddAlchemistEfficiency,
    AddTradingEfficiency,
    AddChefEfficiency,
    AddFarmingEfficiency,
    AddBlacksmithEfficiency,
    AddCarpenterEfficiency,
    AddTailorEfficiency,
    AddTannerEfficiency,
    AddTamerEfficiency,
    AddJewelerEfficiency,
    AddEngineerEfficiency
    // [YENİ EKLENECEK KISIM SONU]
}

// Efektin değerinin yüzde mi yoksa düz sayı mı olduğunu belirtmek için yeni enum
public enum PerkValueType
{
    Flat,       // Düz sayı (örn: +5 Stat, +10 Can)
    Percentage, // Yüzde (örn: +%10 Altın Bonusu, -%5 Cooldown) - Değer 0.10 gibi girilecek
    Multiplier  // Çarpan (örn: x1.5 Kritik Hasar) - Değer 1.5 gibi girilecek (Nadiren kullanılır)
}

public enum PerkName
{
    None,

    //   All Stats
    First_1, //+1allstat
    First_2, //+5allstat


    //Unspent Stat Points
    Unspent_1, //+5unspent stat points
    WoodCutter_1, //+5unspent   stat points


    // LUCK
    Lucky_1, //+5luck
    Lucky_2,  //+?Luck


    // Gold Bonus
    Undecided_1, //+5% gold bonus
    Undecided_2, //+10% gold bonus


    // Explorer Time Reduction
    Explorer_1, // -3m Explorer Time 
    Explorer_2, // -5m Explorer Time 


    //Main:
    Nexus,
    Raid,





    // Player Attack Efektler için

    BreakingDefensePerk,
    BoldHitPerk,
    BreakingHitPerk,
    CrushingHitPerk,
    DestroyedHitPerk,
    ExecutionPerk,
    MutlakDelmePerk,
    ChaosHitPerk,
    DefenseHitPerk,
    ErmeHitPerk,
    ExtraLuckyHitPerk,
    LuckyHitPerk,
    ManaCutPerk,
    PerfectHitPerk,
    PanicHitPerk,
    ResonancePerk,
    ShardHitPerk,
    SlashHitPerk,
    ScreamHitPerk,
    TenacityPerk,
    ReflexHitPerk






}/// <summary>
 /// Bir Perk'in temel tanımını (ne olduğu, ne yaptığı) içeren ScriptableObject.
 /// ExplorerPerkData'dan bağımsızdır ve yeniden kullanılabilir.
 /// </summary>
[CreateAssetMenu(fileName = "NewPerk_", menuName = "Adventure/Perk Definition")]
public class PerkDefinition : ScriptableObject
{
    [Tooltip("Perk'in benzersiz kimliği (ID) ve PerkManager'da kullanılacak adı.")]
    public string perkID; // Örn: "Core_AddAllStats_Tier1", "Explorer_TimeReduction"

    [Tooltip("Inspector ve kodda güvenli kullanım için Enum değeri.")]
    public PerkName perkNameValue;

    [Tooltip("Perk'in UI'da görünecek adı.")]
    public string displayName = "Görülen Perkismi";

    [Tooltip("Perk'in UI'da görünecek detaylı açıklaması.")]
    [TextArea(2, 5)]
    public string description = "Perk açıklaması...";

    [Header("Perk Etkisi")]
    [Tooltip("Bu perk'in SAHİP OLUNDUĞUNDA uygulayacağı etki türü.")]
    public PerkEffectType effectType = PerkEffectType.None;

    [Tooltip("Etkinin değeri yüzde mi, düz sayı mı?")]
    public PerkValueType valueType = PerkValueType.Flat;

    [Tooltip("Etkinin sayısal değeri (Yüzde ise 0.10=%10, Çarpan ise 1.5=x1.5).")]
    public float effectValue = 0;

    [Tooltip("Etki için ek bilgi (örn: 'AddStat' için stat adı, 'GrantItem' için eşya ID'si, 'UnlockFeature' için özellik adı).")]
    public string effectParameter = ""; // Örn: "Luck", "Iron Sword", "Raid"

    // İleride eklenebilecekler:
    // public Sprite icon; // Perk ikonu
    // public int maxStacks = 1; // Perk kaç kere alınabilir? (Sınırsız için -1 veya 0?)
    // public bool isHidden = false; // Gizli perk mi?
}