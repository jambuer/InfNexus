using UnityEngine;
using System;

public class StatCalculator : MonoBehaviour
{
    public static StatCalculator Instance;
    public ComputedStats currentStats { get; private set; }
    public event Action OnStatsRecalculated;
    private bool isRecalculating = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentStats = new ComputedStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (StatManager.Instance != null)
        {
            StatManager.Instance.OnStatChanged += (statName, newValue) => RecalculateAllStats();
        }

        if (PerkManager.Instance != null)
        {
            PerkManager.Instance.OnPerkUpdated += RecalculateAllStats;
        }
        else
        {
            Debug.LogWarning("[StatCalculator] PerkManager bulunamadı, perk güncellemeleri dinlenemiyor!");
        }
        
        RecalculateAllStats();
    }
    
    public void RecalculateAllStats()
    {
        if (isRecalculating) return;
        isRecalculating = true;

        if (StatManager.Instance == null)
        {
            isRecalculating = false;
            return;
        }

        ComputedStats newStats = new ComputedStats();
        StatManager sm = StatManager.Instance;

        double physical = sm.GetTotalPhysical();
        double mental = sm.GetTotalMental();
        double perception = sm.GetTotalPerception();
        double spiritual = sm.GetTotalSpiritual();
        double luck = sm.GetTotalLuck();
        double social = sm.GetTotalSocial();

        // --- GEÇİCİ DEĞİŞKENLER ---
        double flatAttack = 0, percentAttack = 0;
        double flatDefense = 0, percentDefense = 0;
        double flatMaxHealth = 0, percentMaxHealth = 0;
        double flatMaxEnergy = 0, percentMaxEnergy = 0;
        double flatMaxMana = 0, percentMaxMana = 0;
        double flatHealthRecovery = 5.0, percentHealthRecovery = 0;
        double flatEnergyRecovery = 5.0, percentEnergyRecovery = 0;
        double flatManaRecovery = 5.0, percentManaRecovery = 0;
        double flatProduction = 1.0, percentProduction = 0;
        double flatDropRate = 0, percentDropRate = 0;
        double flatHitRate = 10.0, percentHitRate = 0;
        double flatPrestigePoints = 0, percentPrestigePoints = 0;
        
        double flatCooldownReduction = 0; // Saniye cinsinden
        double percentCooldownReduction = 0; // Yüzde cinsinden

        // KRİTİK DÜZELTME: All Stats bonusunu direkt StatManager'a eklemek yerine burada biriktiriyoruz.
        double allStatsBonusFromThresholds = 0;

        // --- STAT ETKİLERİNİN HESAPLANMASI ---

        // Physical
        flatAttack += physical * 2;
        flatMaxHealth += physical * 3;
        flatMaxEnergy += physical * 2;
        flatDefense += physical * 1;
        flatHealthRecovery += physical * 0.1;
        flatEnergyRecovery += physical * 0.1;
        percentAttack += Math.Floor(physical / 100) * 0.01;
        percentMaxHealth += Math.Floor(physical / 100) * 0.01;
        percentMaxEnergy += Math.Floor(physical / 100) * 0.0075;
        percentDefense += Math.Floor(physical / 100) * 0.005;
        percentEnergyRecovery += Math.Floor(physical / 100) * 0.01;
        percentHealthRecovery += Math.Floor(physical / 100) * 0.01;
        newStats.GoldBonus += Math.Floor(physical / 1000) * 0.01 * sm.physicalBonus;
        flatAttack += Math.Floor(physical / 1000) * 100;
        newStats.ExpBonus += Math.Floor(physical / 1000) * 0.03;
        allStatsBonusFromThresholds += Math.Floor(physical / 10000) * 100;
        flatPrestigePoints += Math.Floor(physical / 10000) * 10;
        // Premium coin ekleme buradan kaldırıldı, CurrencyManager dinleyecek.

        // Mental
        flatAttack += mental * 1;
        flatMaxMana += mental * 3;
        flatMaxEnergy += mental * 1;
        flatManaRecovery += mental * 0.5;
        flatEnergyRecovery += mental * 0.1;
        percentAttack += Math.Floor(mental / 100) * 0.01;
        percentMaxMana += Math.Floor(mental / 100) * 0.01;
        percentMaxEnergy += Math.Floor(mental / 100) * 0.0040;
        percentDefense += Math.Floor(mental / 100) * 0.005;
        percentEnergyRecovery += Math.Floor(mental / 100) * 0.005;
        percentManaRecovery += Math.Floor(mental / 100) * 0.01;
        newStats.SkillExpBonus += Math.Floor(mental / 100) * 0.02;
        newStats.GoldBonus += Math.Floor(mental / 1000) * 0.01 * sm.mentalBonus;
        flatAttack += Math.Floor(mental / 1000) * 100;
        newStats.ExpBonus += Math.Floor(mental / 1000) * 0.03;
        newStats.ResourceCostReduction += Math.Floor(mental / 1000) * 0.01;
        allStatsBonusFromThresholds += Math.Floor(mental / 10000) * 100;
        flatPrestigePoints += Math.Floor(mental / 10000) * 10;
        // Premium coin ekleme buradan kaldırıldı

        // Perception
        newStats.CritRate += perception * 0.0002;
        newStats.CritDamage += perception * 0.0001;
        flatMaxEnergy += perception * 1;
        flatCooldownReduction += perception * 0.2; // Düz saniye düşüşü
        flatProduction += perception * 0.1;
        flatHitRate += perception * 1;
        newStats.ExpBonus += Math.Floor(perception / 100) * 0.01;
        percentCooldownReduction += Math.Floor(perception / 100) * 0.01; // Yüzde bonus
        percentProduction += Math.Floor(perception / 100) * 0.01;
        percentDropRate += Math.Floor(perception / 100) * 0.003;
        percentHitRate += Math.Floor(perception / 100) * 0.005;
        newStats.GoldBonus += Math.Floor(perception / 1000) * 0.01 * sm.perceptionBonus;
        newStats.ExpBonus += Math.Floor(perception / 1000) * 0.03;
        newStats.CritRate += Math.Floor(perception / 1000) * 0.03;
        newStats.CritDamage += Math.Floor(perception / 1000) * 0.05;
        allStatsBonusFromThresholds += Math.Floor(perception / 10000) * 100;
        flatPrestigePoints += Math.Floor(perception / 10000) * 10;
        // Premium coin ekleme buradan kaldırıldı

        // Spiritual
        flatHealthRecovery += spiritual * 0.1;
        flatEnergyRecovery += spiritual * 0.1;
        flatManaRecovery += spiritual * 0.1;
        flatPrestigePoints += spiritual * 0.01;
        allStatsBonusFromThresholds += spiritual * 0.02; // Bu zaten All Stats bonusu olduğu için ayrı toplanacak
        percentHealthRecovery += Math.Floor(spiritual / 100) * 0.005;
        percentEnergyRecovery += Math.Floor(spiritual / 100) * 0.005;
        percentManaRecovery += Math.Floor(spiritual / 100) * 0.005;
        percentPrestigePoints += Math.Floor(spiritual / 100) * 0.01;
        newStats.GoldBonus += Math.Floor(spiritual / 1000) * 0.01 * sm.spiritualBonus;
        newStats.ExpBonus += Math.Floor(spiritual / 1000) * 0.03;
        allStatsBonusFromThresholds += Math.Floor(spiritual / 10000) * 100;
        flatPrestigePoints += Math.Floor(spiritual / 10000) * 10;
        percentPrestigePoints += Math.Floor(spiritual / 10000) * 0.10;
        // Premium coin ekleme buradan kaldırıldı
        
        // Luck
        flatDropRate += luck * 1;
        newStats.CritRate += luck * 0.0003;
        flatHitRate += luck * 0.2;
        percentDropRate += Math.Floor(luck / 100) * 0.01;
        newStats.ExpBonus += Math.Floor(luck / 100) * 0.01;
        newStats.GoldBonus += Math.Floor(luck / 100) * 0.05;
        newStats.NexusCoinBonus += Math.Floor(luck / 100) * 0.01;
        newStats.GoldBonus += Math.Floor(luck / 1000) * 0.01 * sm.luckBonus;
        newStats.ExpBonus += Math.Floor(luck / 1000) * 0.03;
        percentHitRate += Math.Floor(luck / 1000) * 0.03;
        newStats.LuckyInvestmentChance += Math.Floor(luck / 1000) * 0.0075;
        allStatsBonusFromThresholds += Math.Floor(luck / 10000) * 100;
        flatPrestigePoints += Math.Floor(luck / 10000) * 10;
        // Premium coin ekleme buradan kaldırıldı

        // Social
        // KRİTİK DÜZELTME: Bu satırlar buradan kaldırıldı. CurrencyManager kendisi dinleyecek.
        // if(CurrencyManager.Instance != null) CurrencyManager.Instance.AddPeople(social * 1);
        // if(CurrencyManager.Instance != null && CurrencyManager.Instance.people > 0) CurrencyManager.Instance.AddPeople(CurrencyManager.Instance.people * (Math.Floor(social / 100) * 0.05));
        
        flatCooldownReduction += social * 0.05; // Düz saniye düşüşü
        flatProduction += social * 0.05;
        percentProduction += Math.Floor(social / 100) * 0.10;
        percentCooldownReduction += Math.Floor(social / 100) * 0.10; // Yüzde bonus
        
        newStats.NexusCoinBonus += Math.Floor(social / 100) * 0.01;
        newStats.GoldBonus += Math.Floor(social / 1000) * 0.01 * sm.socialBonus;
        // if(CurrencyManager.Instance != null) CurrencyManager.Instance.AddPeople(Math.Floor(social / 1000) * 100); // Kaldırıldı
        newStats.ExpBonus += Math.Floor(social / 1000) * 0.03;
        allStatsBonusFromThresholds += Math.Floor(social / 10000) * 100;
        flatPrestigePoints += Math.Floor(social / 10000) * 10;
        // Premium coin ekleme buradan kaldırıldı

        // =================================================================
        // YENİ EKLENECEK BÖLÜM: PERK BONUSLARI
        // =================================================================
        if (PerkManager.Instance != null)
        {
            // --- Mevcut Kod (Altın ve XP) ---
            float perkGoldBonus = PerkManager.Instance.GetBonusFromPerks(PerkEffectType.AddGoldBonus);
            newStats.GoldBonus += (double)(perkGoldBonus / 100.0f);

            float perkXPBonus = PerkManager.Instance.GetBonusFromPerks(PerkEffectType.AddXPBonus);
            newStats.ExpBonus += (double)(perkXPBonus / 100.0f);

            // --- YENİ EKLENECEK KODLAR (ÖRNEK) ---

            // Kritik Şans ekle (Perk'teki 'effectValue' 10 ise %10 varsayıyoruz)
            float perkCritChance = PerkManager.Instance.GetBonusFromPerks(PerkEffectType.AddCriticalChance);
            newStats.CritRate += (double)(perkCritChance / 100.0f); // %10'u 0.1'e çevir

            // Kritik Hasar ekle
            float perkCritDamage = PerkManager.Instance.GetBonusFromPerks(PerkEffectType.AddCriticalDamage);
            newStats.CritDamage += (double)(perkCritDamage / 100.0f); // %15'i 0.15'e çevir

            // Cooldown Azaltma ekle (Yüzdesel)
            float perkCDR = PerkManager.Instance.GetBonusFromPerks(PerkEffectType.AddCooldownReduction);
            newStats.PercentCooldownReduction += (double)(perkCDR / 100.0f);

            // Kaynak Maliyeti Azaltma ekle
            float perkCostReduction = PerkManager.Instance.GetBonusFromPerks(PerkEffectType.AddResourceCostReduction);
            newStats.ResourceCostReduction += (double)(perkCostReduction / 100.0f);



            // Diğer tüm pasif perk statlarını bu şekilde eklemeye devam edin...
            // Örn: newStats.TotalDefense += (double)PerkManager.Instance.GetBonusFromPerks(PerkEffectType.ReduceEnemyDamage);
        }
        else
        {
            Debug.LogWarning("[StatCalculator] PerkManager bulunamadı, perk bonusları hesaplanamadı.");
        }
        // =======

        // --- FİNAL HESAPLAMALAR ---
        newStats.TotalAttack = (flatAttack + allStatsBonusFromThresholds) * (1 + percentAttack);
        newStats.TotalDefense = (flatDefense + allStatsBonusFromThresholds) * (1 + percentDefense);
        newStats.Production = flatProduction * (1 + percentProduction);
        
        // CooldownReduction için yeni birleştirme formülü
        // newStats.CooldownReduction = yüzdesel + düz miktar (yüzdeye çevrilmiş)
        // newStats.CooldownReduction = percentCooldownReduction + (flatCooldownReduction / 100.0); // Düz değeri yüzdeye çevir
        newStats.FlatCooldownReduction = flatCooldownReduction;
        newStats.PercentCooldownReduction = percentCooldownReduction;

        newStats.MaxHealth = (100 + flatMaxHealth + allStatsBonusFromThresholds) * (1 + percentMaxHealth);
        newStats.MaxEnergy = (100 + flatMaxEnergy + allStatsBonusFromThresholds) * (1 + percentMaxEnergy);
        newStats.MaxMana = (100 + flatMaxMana + allStatsBonusFromThresholds) * (1 + percentMaxMana);

        newStats.HealthRecovery = (flatHealthRecovery + allStatsBonusFromThresholds) * (1 + percentHealthRecovery);
        newStats.EnergyRecovery = (flatEnergyRecovery + allStatsBonusFromThresholds) * (1 + percentEnergyRecovery);
        newStats.ManaRecovery = (flatManaRecovery + allStatsBonusFromThresholds) * (1 + percentManaRecovery);

        newStats.HitRate = (flatHitRate + allStatsBonusFromThresholds) * (1 + percentHitRate);
        newStats.DropRate = (flatDropRate + allStatsBonusFromThresholds) * (1 + percentDropRate);
        newStats.PrestigePoints = (flatPrestigePoints + allStatsBonusFromThresholds) * (1 + percentPrestigePoints);

        newStats.CritDamage += 1.0;

        // --- KRİTİK ŞANS/HASAR AYARLAMASI (%75 Limiti) ---
        if (newStats.CritRate > 0.75)
        {
            double excessCritRate = newStats.CritRate - 0.75;
            newStats.CritDamage += excessCritRate; // Fazlalığı CritDamage'e ekle
            newStats.CritRate = 0.75; // CritRate'i %75'e sabitle
            Debug.Log($"CritRate %75'i aştı. Fazlalık ({excessCritRate * 100:F1}%) CritDamage'e eklendi. Yeni CritDamage: +{((newStats.CritDamage -1)* 100):F1}%");
        }
        

        currentStats = newStats;
        OnStatsRecalculated?.Invoke();
        Debug.Log("Tüm statlar yeniden hesaplandı! Total Attack: " + currentStats.TotalAttack.ToString("F2"));
        
        isRecalculating = false;
    }
}