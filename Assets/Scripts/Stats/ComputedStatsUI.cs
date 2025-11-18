using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ComputedStatsUI : MonoBehaviour
{
    [Header("Stat Metin Referansları")]
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI critRateText;
    public TextMeshProUGUI critDamageText;
    public TextMeshProUGUI cooldownReductionText;
    public TextMeshProUGUI dropRateText;
    public TextMeshProUGUI goldBonusText;
    public TextMeshProUGUI expBonusText;
    
    // YENİ EKLENEN REFERANSLAR
    [Header("Ek Stat Metin Referansları")]
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI maxEnergyText;
    public TextMeshProUGUI hitRateText;
    public TextMeshProUGUI skillExpBonusText;
    public TextMeshProUGUI productionText;
    public TextMeshProUGUI resourceCostReductionText;
    public TextMeshProUGUI prestigePointsText;
    public TextMeshProUGUI maxManaText;

    [Header("Detaylı Cooldown Metin Referansları")]
    public TextMeshProUGUI flatCooldownText; // Örneğin "-5s"
    public TextMeshProUGUI percentCooldownText; // Örneğin "+%10"

    [Header("Beceri Verimliliği Referansları")]
    public TextMeshProUGUI skillEfficiencyText; // Global Yüzdesel Bonus

    [Header("Life Skill Verimlilikleri")]
    public TextMeshProUGUI woodCutterEfficiencyText;
    public TextMeshProUGUI foragingEfficiencyText;
    public TextMeshProUGUI miningEfficiencyText;
    public TextMeshProUGUI huntingEfficiencyText;
    public TextMeshProUGUI fishingEfficiencyText;
    public TextMeshProUGUI scavengerEfficiencyText;

    [Header("Job Verimlilikleri")]
    public TextMeshProUGUI alchemistEfficiencyText;
    public TextMeshProUGUI tradingEfficiencyText;
    public TextMeshProUGUI chefEfficiencyText;
    public TextMeshProUGUI farmingEfficiencyText;
    public TextMeshProUGUI blacksmithEfficiencyText;
    public TextMeshProUGUI carpenterEfficiencyText;
    public TextMeshProUGUI tailorEfficiencyText;
    public TextMeshProUGUI tannerEfficiencyText;
    public TextMeshProUGUI tamerEfficiencyText;
    public TextMeshProUGUI jewelerEfficiencyText;
    public TextMeshProUGUI engineerEfficiencyText;

    void Start()
    {
        // StatCalculator'dan gelen anonsları dinlemeye başla
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated += UpdateAllComputedStats;
        }
        // Başlangıçta UI'ı bir kez doldur
        UpdateAllComputedStats();
    }

    void OnDestroy()
    {
        // Obje yok olduğunda anons dinlemeyi bırak
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated -= UpdateAllComputedStats;
        }
    }

    // StatCalculator anons yaptığında bu fonksiyon çalışacak
    private void UpdateAllComputedStats()
    {
        if (StatCalculator.Instance == null) return;

        ComputedStats stats = StatCalculator.Instance.currentStats;

        // --- GÜNCELLENMİŞ KISIM ---

        // Mevcut statları güncelle
        attackText.text = stats.TotalAttack.ToString("F0");
        defenseText.text = stats.TotalDefense.ToString("F0");
        critRateText.text = (stats.CritRate * 100).ToString("F1") + "%"; 
        critDamageText.text = "+" + (stats.CritDamage * 100).ToString("F1") + "%";
        flatCooldownText.text = stats.FlatCooldownReduction.ToString("F1") + "s"; // Örn: -5.0s
        cooldownReductionText.text = $"-{stats.FlatCooldownReduction:F1}s & +{(stats.PercentCooldownReduction * 100):F1}%";

        percentCooldownText.text = (stats.PercentCooldownReduction * 100).ToString("F1") + "%"; // Örn: +10.0%
        dropRateText.text = stats.DropRate.ToString("F0");
        goldBonusText.text = "+" + (stats.GoldBonus * 100).ToString("F1") + "%";
        expBonusText.text = "+" + (stats.ExpBonus * 100).ToString("F1") + "%";

        // Yeni eklenen statları güncelle
        maxHealthText.text = stats.MaxHealth.ToString("F0") + " (" + stats.HealthRecovery.ToString("F1") + "/s)";
        maxEnergyText.text = stats.MaxEnergy.ToString("F0") + " (" + stats.EnergyRecovery.ToString("F1") + "/s)";
        hitRateText.text = stats.HitRate.ToString("F1");
        skillExpBonusText.text = "+" + (stats.SkillExpBonus * 100).ToString("F1") + "%";
        productionText.text = stats.Production.ToString("F1");
        resourceCostReductionText.text = (stats.ResourceCostReduction * 100).ToString("F1") + "%";
        prestigePointsText.text = stats.PrestigePoints.ToString("F2");
        maxManaText.text = stats.MaxMana.ToString("F0") + " (" + stats.ManaRecovery.ToString("F1") + "/s)";

        // Global Yüzdesel Bonusu Güncelle
        if (skillEfficiencyText != null)
            skillEfficiencyText.text = "+" + (stats.SkillEfficiency * 100).ToString("F1") + "%";

        // Life Skill Verimliliklerini Güncelle (Düz değer olarak)
        if (woodCutterEfficiencyText != null)
            woodCutterEfficiencyText.text = stats.WoodCutterEfficiency.ToString("F0");
        if (foragingEfficiencyText != null)
            foragingEfficiencyText.text = stats.ForagingEfficiency.ToString("F0");
        if (miningEfficiencyText != null)
            miningEfficiencyText.text = stats.MiningEfficiency.ToString("F0");
        if (huntingEfficiencyText != null)
            huntingEfficiencyText.text = stats.HuntingEfficiency.ToString("F0");
        if (fishingEfficiencyText != null)
            fishingEfficiencyText.text = stats.FishingEfficiency.ToString("F0");
        if (scavengerEfficiencyText != null)
            scavengerEfficiencyText.text = stats.ScavengerEfficiency.ToString("F0");

        // Job Verimliliklerini Güncelle (Düz değer olarak)
        if (alchemistEfficiencyText != null)
            alchemistEfficiencyText.text = stats.AlchemistEfficiency.ToString("F0");
        if (tradingEfficiencyText != null)
            tradingEfficiencyText.text = stats.TradingEfficiency.ToString("F0");
        if (chefEfficiencyText != null)
            chefEfficiencyText.text = stats.ChefEfficiency.ToString("F0");
        if (farmingEfficiencyText != null)
            farmingEfficiencyText.text = stats.FarmingEfficiency.ToString("F0");
        if (blacksmithEfficiencyText != null)
            blacksmithEfficiencyText.text = stats.BlacksmithEfficiency.ToString("F0");
        if (carpenterEfficiencyText != null)
            carpenterEfficiencyText.text = stats.CarpenterEfficiency.ToString("F0");
        if (tailorEfficiencyText != null)
            tailorEfficiencyText.text = stats.TailorEfficiency.ToString("F0");
        if (tannerEfficiencyText != null)
            tannerEfficiencyText.text = stats.TannerEfficiency.ToString("F0");
        if (tamerEfficiencyText != null)
            tamerEfficiencyText.text = stats.TamerEfficiency.ToString("F0");
        if (jewelerEfficiencyText != null)
            jewelerEfficiencyText.text = stats.JewelerEfficiency.ToString("F0");
        if (engineerEfficiencyText != null)
            engineerEfficiencyText.text = stats.EngineerEfficiency.ToString("F0");
    }
}