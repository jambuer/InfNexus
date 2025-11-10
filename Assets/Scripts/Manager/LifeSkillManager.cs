using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Oyuncunun Yaşam Becerilerinin (LifeSkill) XP ve Seviyelerini yönetir.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// </summary>
public class LifeSkillManager : MonoBehaviour, IGameDataSaveable<LifeSkillSaveData>
{
    public static LifeSkillManager Instance { get; private set; }

    // Kaydedilecek ana veriler
    private Dictionary<LifeSkill, double> _skillXP = new Dictionary<LifeSkill, double>();
    private Dictionary<LifeSkill, int> _skillLevels = new Dictionary<LifeSkill, int>();

    // Event'ler
    public event Action<LifeSkill, int> OnSkillLeveledUp; // Hangi skill, yeni seviyesi
    public event Action<LifeSkill, double> OnSkillXPGained; // Hangi skill, kazanılan xp
    public event Action OnSkillDataLoaded; // Yükleme tamamlandığında UI'ı güncellemek için

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // Veri yüklenene kadar sözlüklerin boş olmamasını garantile
        InitializeSkills();
    }

    /// <summary>
    /// Sözlükleri (Dictionary) enum'daki tüm değerler için varsayılan değerlerle başlatır.
    /// </summary>
    private void InitializeSkills()
    {
        _skillXP.Clear();
        _skillLevels.Clear();

        foreach (LifeSkill skill in Enum.GetValues(typeof(LifeSkill)))
        {
            if (skill == LifeSkill.None) continue;
            if (!_skillXP.ContainsKey(skill))
            {
                _skillXP.Add(skill, 0);
            }
            if (!_skillLevels.ContainsKey(skill))
            {
                _skillLevels.Add(skill, 1); // Seviyeler 1'den başlar
            }
        }
    }

    /// <summary>
    /// Belirli bir Life Skill'e XP ekler ve seviye atlama kontrolü yapar.
    /// </summary>
    public void AddXP(LifeSkill skill, double amount)
    {
        if (skill == LifeSkill.None || amount <= 0) return;

        if (!_skillXP.ContainsKey(skill))
        {
            Debug.LogWarning($"[LifeSkillManager] '{skill}' için XP eklenemedi. Sözlükte başlatılmamış.");
            return;
        }

        _skillXP[skill] += amount;
        OnSkillXPGained?.Invoke(skill, amount);

        // Seviye atlama kontrolü (Şimdilik basit bir formül varsayalım)
        double xpToNextLevel = GetXPForNextLevel(_skillLevels[skill]);
        
        while (_skillXP[skill] >= xpToNextLevel)
        {
            _skillXP[skill] -= xpToNextLevel; // Kalan XP'yi sakla
            _skillLevels[skill]++;
            
            GameConsole.Instance?.AddMessage($"<color=green>Yaşam Becerisi Arttı! {skill} seviye {_skillLevels[skill]} oldu!</color>");
            OnSkillLeveledUp?.Invoke(skill, _skillLevels[skill]);
            
            // Bir sonraki seviye için gereken XP'yi hesapla
            xpToNextLevel = GetXPForNextLevel(_skillLevels[skill]);
        }
    }

    // Public Get Fonksiyonları
    public int GetLevel(LifeSkill skill) => _skillLevels.TryGetValue(skill, out int level) ? level : 1;
    public double GetXP(LifeSkill skill) => _skillXP.TryGetValue(skill, out double xp) ? xp : 0;
    
    /// <summary>
    /// Progress bar için (0.0 - 1.0 arası) ilerleme durumu verir.
    /// </summary>
    public float GetXPProgress(LifeSkill skill)
    {
        if (skill == LifeSkill.None) return 0f;
        int level = GetLevel(skill);
        double currentXP = GetXP(skill);
        double neededXP = GetXPForNextLevel(level);

        if (neededXP <= 0) return 0f;
        return (float)(currentXP / neededXP);
    }

    /// <summary>
    /// Seviye atlamak için gereken XP formülü.
    /// </summary>
    public double GetXPForNextLevel(int currentLevel)
    {
        // Örnek: Seviye 1 -> 100 XP, Seviye 2 -> 200 XP
        // Formülü istediğiniz gibi karmaşıklaştırabilirsiniz.
        return (double)currentLevel * 100;
    }

    // ===================================================================
    // KAYIT SİSTEMİ (IGameDataSaveable Arayüzü)
    // ===================================================================

    /// <summary>
    /// GameDataManager'ın 'SaveGame' demesi üzerine çağrılır.
    /// Tıpkı StatManager.GetSaveData() gibi çalışır.
    /// </summary>
    public LifeSkillSaveData GetSaveData()
    {
        Debug.Log("[LifeSkillManager] Kayıt verisi oluşturuluyor.");
        return new LifeSkillSaveData
        {
            skillXP = new Dictionary<LifeSkill, double>(_skillXP),
            skillLevels = new Dictionary<LifeSkill, int>(_skillLevels)
        };
    }

    /// <summary>
    /// GameDataManager'ın 'LoadGame' demesi üzerine çağrılır.
    /// Tıpkı StatManager.LoadFromData() gibi çalışır.
    /// </summary>
    public void LoadFromData(LifeSkillSaveData data)
    {
        if (data == null)
        {
            Debug.Log("[LifeSkillManager] Kayıtlı LifeSkill verisi bulunamadı, varsayılan başlatılıyor.");
            InitializeSkills(); // Kayıt yoksa sıfırdan başlat
            OnSkillDataLoaded?.Invoke(); // UI'ı güncellemek için
            return;
        }

        // Kayıtlı veriyi yükle
        _skillXP = data.skillXP ?? new Dictionary<LifeSkill, double>();
        _skillLevels = data.skillLevels ?? new Dictionary<LifeSkill, int>();
        
        // Kayıttan sonra oyuna yeni bir skill eklenmişse (enum güncellenmişse)
        // bu yeni skill'leri de sözlüğe ekle.
        bool needsInitialization = false;
        foreach (LifeSkill skill in Enum.GetValues(typeof(LifeSkill)))
        {
             if (skill == LifeSkill.None) continue;
             if (!_skillXP.ContainsKey(skill))
             {
                 _skillXP.Add(skill, 0);
                 needsInitialization = true;
             }
             if (!_skillLevels.ContainsKey(skill))
             {
                 _skillLevels.Add(skill, 1);
                 needsInitialization = true;
             }
        }
        if (needsInitialization)
            Debug.Log("[LifeSkillManager] Yeni eklenen LifeSkill'ler varsayılan değerlerle başlatıldı.");

        Debug.Log("[LifeSkillManager] LifeSkill verisi yüklendi.");
        OnSkillDataLoaded?.Invoke(); // Tüm UI panellerine "yenilenin" sinyali gönder
    }
}