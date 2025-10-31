/// <Gereksinim>
/// Geçici buff/debuff etkilerinin türünü tanımlar.
/// </summary>
public enum StatusEffectType
{
    None,
    // StatManager'ı etkileyenler (Bonus olarak)
    AddStatBonus,       // effectParameter'a "Physical", "Mental" vb. yazarız
    AddAllStatsBonus,   // Tüm statlara bonus verir
    
    // ResourceManager'ı etkileyenler (Maksimum değerleri)
    ModifyMaxHealth,
    ModifyMaxEnergy,
    ModifyMaxMana,

    // StatCalculator'ı etkileyenler (İkincil statlar)
    // Not: StatCalculator'ın bu bonusları alıp işlemesi gerekir.
    // Şimdilik StatManager'daki regen statlarını kullanalım (varsa).
    ModifyHealthRecovery,
    ModifyEnergyRecovery,
    ModifyManaRecovery,

    // Diğer (Dövüş vb.)
    AddCriticalChance,
    AddCriticalDamage,
    AddDropRateBonus
}