// Bu bir MonoBehaviour olmadığı için UnityEngine'e gerek yok.
// Sadece bir veri tutucudur.
public class ComputedStats
{
    // Temel Savaş Statları
    public double TotalAttack;
    public double TotalDefense;

    // Kritik Vuruş
    public double CritRate; // Yüzde olarak (örn: 0.75 = %75)
    public double CritDamage; // Yüzde olarak (örn: 1.30 = +%130)

    // Kaynaklar (Resources)
    public double MaxHealth;
    public double MaxEnergy;
    public double MaxMana;
    public double HealthRecovery;
    public double EnergyRecovery;
    public double ManaRecovery;

    // Diğer Mekanikler
    public double HitRate;
    public double DropRate;
    public double Production;
    public double PrestigePoints;

    public double FlatCooldownReduction = 0; // Saniye cinsinden düz düşüş (örn: -5 saniye)
    public double PercentCooldownReduction = 0; // Yüzdesel düşüş (örn: 0.15 = %15)
    
    // Yüzdesel Bonuslar
    public double GoldBonus;
    public double ExpBonus;
    public double SkillExpBonus;
    public double NexusCoinBonus;
    public double ResourceCostReduction; // Yüzde olarak
    public double LuckyInvestmentChance; // Yüzde olarak
}