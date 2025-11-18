using UnityEngine;
using System;

/// <summary>
/// "Ana Cephe" (Facade) Yöneticisi.
/// Tüm Yaşam Becerisi (LifeSkill) ve Meslek (Job) sistemleriyle dış dünyanın
/// (UI, Validator, Rewarder) konuştuğu tek noktadır.
/// XP bonuslarını (ComputedStats, Perk vb.) hesaplar ve formülleri uygular.
/// </summary>
public class LifeJobsSkillsManager : MonoBehaviour
{
    public static LifeJobsSkillsManager Instance { get; private set; }

    // Gerekli diğer yöneticilere referanslar
    private LifeSkillManager _skillManager;
    private JobsManager _jobManager;
    private GameConsole _console;

    // ComputedStats'a erişim yolu
    private StatCalculator _statCalculator;
    private ComputedStats _computedStats; // Hesaplanmış statların referansı

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Singleton'ları Start() içinde güvenle al
        try
        {
            _skillManager = LifeSkillManager.Instance;
            _jobManager = JobsManager.Instance;
            _console = GameConsole.Instance;

            _statCalculator = StatCalculator.Instance;

            if (_skillManager == null) Debug.LogError("[LJSM] LifeSkillManager bulunamadı!");
            if (_jobManager == null) Debug.LogError("[LJSM] JobsManager bulunamadı!");
            if (_statCalculator == null)
            {
                Debug.LogError("[LJSM] StatCalculator bulunamadı!");
            }
            else
            {
                // [DÜZELTME]
                // Hatalı: _statCalculator.computedStats
                // Doğru: _statCalculator.currentStats (StatCalculator.cs dosyanıza göre)
                _computedStats = _statCalculator.currentStats; //

                if (_computedStats == null)
                {
                    Debug.LogError("[LJSM] StatCalculator üzerinden ComputedStats (currentStats) bulunamadı!");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LJSM] Başlangıçta önemli bir yönetici bulunamadı: {ex.Message}");
        }
    }

    // ===================================================================
    // XP EKLEME (ANA GÖREVİ - BONUS HESAPLAMA)
    // =Girdiğiniz kodlara göre (ComputedStats.cs) bu fonksiyonlar artık hatasız çalışmalı.
    // ===================================================================

    public void AddSkillXP(LifeSkill skill, double baseAmount)
    {
        if (skill == LifeSkill.None || baseAmount <= 0) return;

        double bonusPercent = GetSkillXPBonus(); // % cinsinden bonus
        double finalAmount = baseAmount * (1.0 + bonusPercent);

        if (bonusPercent > 0 && _console != null)
        {
            _console.AddMessage($"<color=#99FF99>{skill} {baseAmount} XP kazandı (Bonus: +{finalAmount - baseAmount:F0})</color>");
        }
        _skillManager?.AddXP(skill, finalAmount);
    }

    public void AddJobXP(Job job, double baseAmount)
    {
        if (job == Job.None || baseAmount <= 0) return;

        double bonusPercent = GetSkillXPBonus();
        double finalAmount = baseAmount * (1.0 + bonusPercent);

        if (bonusPercent > 0 && _console != null)
        {
            _console.AddMessage($"<color=#CC99FF>{job} {baseAmount} XP kazandı (Bonus: +{finalAmount - baseAmount:F0})</color>");
        }
        _jobManager?.AddXP(job, finalAmount);
    }

    // ===================================================================
    // BONUS SORGULAMA (CEPHE GÖREVİ)
    // Bu fonksiyonlar ComputedStats.cs'teki public değişkenlere
    // doğrudan erişir ve artık hatasız çalışmalıdır.
    // ===================================================================

    public double GetSkillXPBonus()
    {
        if (_computedStats == null) return 0.0;
        return _computedStats.SkillExpBonus;
    }

    public float GetProductionBonus()
    {
        if (_computedStats == null) return 0f;
        return (float)_computedStats.Production;
    }

    public float GetDropRateBonus()
    {
        if (_computedStats == null) return 0f;
        return (float)_computedStats.DropRate;
    }

    public float GetSkillCooldownReduction()
    {
        if (_computedStats == null) return 0f;
        return (float)_computedStats.PercentCooldownReduction;
    }

    public float GetResourceCostReduction()
    {
        if (_computedStats == null) return 0f;
        return (float)_computedStats.ResourceCostReduction;
    }


    // ===================================================================
    // VERİ GETİRME (GETTERS - CEPHE GÖREVİ)
    // ===================================================================

    // --- Life Skills ---
    public int GetSkillLevel(LifeSkill skill) => _skillManager?.GetLevel(skill) ?? 1;
    public double GetSkillXP(LifeSkill skill) => _skillManager?.GetXP(skill) ?? 0;
    public float GetSkillXPProgress(LifeSkill skill) => _skillManager?.GetXPProgress(skill) ?? 0f;
    public LifeSkillData GetSkillData(LifeSkill skill) => _skillManager?.GetSkillData(skill);
    public double GetXPForNextSkillLevel(LifeSkill skill) => _skillManager?.GetXPForNextLevel(skill) ?? double.MaxValue;

    // --- Jobs ---
    public int GetJobLevel(Job job) => _jobManager?.GetLevel(job) ?? 1;
    public double GetJobXP(Job job) => _jobManager?.GetXP(job) ?? 0; // Hata burada düzeltildi (skill -> job)
    public float GetJobXPProgress(Job job) => _jobManager?.GetXPProgress(job) ?? 0f;
    public JobData GetJobData(Job job) => _jobManager?.GetJobData(job);
    public double GetXPForNextJobLevel(Job job) => _jobManager?.GetXPForNextLevel(job) ?? double.MaxValue;

    // ===================================================================
// [YENİ] ADIM 12 - BECERİ BONUSU SORGULAMA (CEPHE GÖREVİ)
// ===================================================================

/// <summary>
/// Belirli bir beceri seviyesinden gelen bonusu alır (örn: "Production")
/// </summary>
public float GetBonusFromSkill(LifeSkill skill, string bonusType)
{
    return _skillManager?.GetBonusFromSkill(skill, bonusType) ?? 0f;
}

    /// <summary>
    /// Sağ Panel (SkillDetailPanelUI) için beceri bonuslarının açıklamasını alır.
    /// </summary>
    public string GetSkillBonusDescription(LifeSkill skill)
    {
        return _skillManager?.GetBonusDescription(skill) ?? "";
    }

    // ===================================================================
    // [YENİ] ADIM 13 - BİRLEŞTİRİLMİŞ BONUS SORGULAMA
    // ===================================================================

    /// <summary>
    /// Hem Stat'lardan (Genel) hem de Beceri Seviyesinden (Özel)
    /// gelen toplam Üretim Hızı bonusunu döndürür.
    /// </summary>
    public float GetTotalProductionBonus(LifeSkill skill)
    {
        float statBonus = GetProductionBonus(); // StatCalculator'dan gelen genel bonus
        float skillBonus = GetBonusFromSkill(skill, "Production"); // LifeSkillData'dan gelen özel bonus
        return statBonus + skillBonus;
    }

    /// <summary>
    /// Hem Stat'lardan (Genel) hem de Beceri Seviyesinden (Özel)
    /// gelen toplam DropRate (düz değer) miktarını döndürür.
    /// </summary>
    public float GetTotalDropRate(LifeSkill skill)
    {
        float statBonus = GetDropRateBonus(); // StatCalculator'dan gelen genel bonus
        float skillBonus = GetBonusFromSkill(skill, "DropRate"); // LifeSkillData'dan gelen özel bonus
        return statBonus + skillBonus;
    }

    // ===================================================================
    // [YENİ] ADIM 15 ÖNCESİ - EFFICIENCY MANTIK KÖPRÜSÜ
    // ===================================================================

    /// <summary>
    /// Verilen 'LifeSkill' türünü, ilgili 'SkillEfficiencyType' türüne dönüştürür.
    /// </summary>
    private SkillEfficiencyType GetEfficiencyTypeForSkill(LifeSkill skill)
    {
        switch (skill)
        {
            case LifeSkill.WoodCutter: return SkillEfficiencyType.WoodCutterEfficiency;
            case LifeSkill.Foraging: return SkillEfficiencyType.ForagingEfficiency;
            case LifeSkill.Miner: return SkillEfficiencyType.MiningEfficiency;
            case LifeSkill.Hunting: return SkillEfficiencyType.HuntingEfficiency;
            case LifeSkill.Fishing: return SkillEfficiencyType.FishingEfficiency;
            case LifeSkill.Scavenger: return SkillEfficiencyType.ScavengerEfficiency;
            default: return SkillEfficiencyType.None;
        }
    }

    /// <summary>
    /// Bir 'LifeSkill' (örn: WoodCutter) alıp, o becerinin ComputedStats'taki
    /// güncel 'Efficiency' değerini (örn: 150) döndürür.
    /// </summary>
    public double GetEfficiency(LifeSkill skill)
    {
        if (_computedStats == null) return 0;

        SkillEfficiencyType type = GetEfficiencyTypeForSkill(skill);
        if (type == SkillEfficiencyType.None) return 0;

        // ComputedStats'taki ilgili alanı okumak için
        // (Bu kısım gelecekte bir Dictionary'e taşınabilir, şimdilik switch-case en temizi)
        switch (type)
        {
            case SkillEfficiencyType.WoodCutterEfficiency: return _computedStats.WoodCutterEfficiency;
            case SkillEfficiencyType.ForagingEfficiency: return _computedStats.ForagingEfficiency;
            case SkillEfficiencyType.MiningEfficiency: return _computedStats.MiningEfficiency;
            case SkillEfficiencyType.HuntingEfficiency: return _computedStats.HuntingEfficiency;
            case SkillEfficiencyType.FishingEfficiency: return _computedStats.FishingEfficiency;
            case SkillEfficiencyType.ScavengerEfficiency: return _computedStats.ScavengerEfficiency;
            // (Job'lar eklendikçe burası genişleyecek)
            default: return 0;
        }
    }

}

