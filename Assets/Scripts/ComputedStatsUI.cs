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
    }
}