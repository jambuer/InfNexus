using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // [YENİ] Linq eklendi

/// <summary>
/// Oyuncunun Yaşam Becerilerinin (LifeSkill) XP ve Seviyelerini yönetir.
/// Veri mantığını (XP formülü, ödüller) ScriptableObject'lardan yükler.
/// </summary>
public class LifeSkillManager : MonoBehaviour, IGameDataSaveable<LifeSkillSaveData>
{
    public static LifeSkillManager Instance { get; private set; }

    // Kaydedilecek ana veriler
    private Dictionary<LifeSkill, double> _skillXP = new Dictionary<LifeSkill, double>();
    private Dictionary<LifeSkill, int> _skillLevels = new Dictionary<LifeSkill, int>();

    // [YENİ] Becerilerin tüm verilerini (formüller, ödüller) tutan harita
    private Dictionary<LifeSkill, LifeSkillData> _skillDataMap = new Dictionary<LifeSkill, LifeSkillData>();

    // Event'ler
    public event Action<LifeSkill, int> OnSkillLeveledUp; // Hangi skill, yeni seviyesi
    public event Action<LifeSkill, double> OnSkillXPGained; // Hangi skill, kazanılan xp
    public event Action OnSkillDataLoaded; // Yükleme tamamlandığında UI'ı güncellemek için

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // [DEĞİŞTİ] Sıralama: Önce SO verilerini yükle, sonra varsayılan değerleri başlat
        LoadSkillDataAssets();
        InitializeSkills();
    }

    /// <summary>
    /// [YENİ] Resources klasöründen tüm LifeSkillData asset'lerini yükler ve haritaya ekler.
    /// </summary>
    private void LoadSkillDataAssets()
    {
        _skillDataMap.Clear();
        // ÖNEMLİ: Tüm LifeSkillData asset'leri "Resources/SkillData/LifeSkills" klasöründe olmalı
        var allSkillData = Resources.LoadAll<LifeSkillData>("SkillData/LifeSkills");

        foreach (var data in allSkillData)
        {
            if (!_skillDataMap.ContainsKey(data.skillID))
            {
                _skillDataMap.Add(data.skillID, data);
            }
            else
            {
                Debug.LogWarning($"[LifeSkillManager] '{data.skillID}' için mükerrer veri bulundu: {data.name}");
            }
        }
        Debug.Log($"[LifeSkillManager] {allSkillData.Length} adet Yaşam Becerisi verisi yüklendi.");
    }

    /// <summary>
    /// Sözlükleri (Dictionary) enum'daki tüm değerler için varsayılan değerlerle başlatır.
    /// [DEĞİŞTİ] Artık başlangıç seviyesini SO verisinden okur.
    /// </summary>
    private void InitializeSkills()
    {
        _skillXP.Clear();
        _skillLevels.Clear();

        foreach (LifeSkill skill in Enum.GetValues(typeof(LifeSkill)))
        {
            if (skill == LifeSkill.None) continue;

            // SO verisinden başlangıç seviyesini al
            int startLevel = 1;
            if (_skillDataMap.TryGetValue(skill, out LifeSkillData data))
            {
                startLevel = data.startLevel;
            }

            if (!_skillXP.ContainsKey(skill))
            {
                _skillXP.Add(skill, 0);
            }
            if (!_skillLevels.ContainsKey(skill))
            {
                _skillLevels.Add(skill, startLevel);
            }
        }
    }


    /// <summary>
    /// Belirli bir Life Skill'e XP ekler ve seviye atlama kontrolü yapar.
    /// [DEĞİŞTİ] Artık XP formülünü ve ödül mantığını SO'dan alır.
    /// </summary>
    public void AddXP(LifeSkill skill, double amount)
    {
        if (skill == LifeSkill.None || amount <= 0) return;

        if (!_skillXP.ContainsKey(skill) || !_skillDataMap.ContainsKey(skill))
        {
            Debug.LogWarning($"[LifeSkillManager] '{skill}' için XP eklenemedi. Sözlükte veya Veri Haritasında (SO) başlatılmamış.");
            return;
        }

        int currentLevel = GetLevel(skill);
        LifeSkillData skillData = _skillDataMap[skill];
        
        // [YENİ] Maksimum seviye kontrolü
        if (currentLevel >= skillData.maxLevel)
        {
            _skillXP[skill] = 0; // Maks seviyede XP'yi sıfırla (veya son seviye için gereken XP'de tut)
            return; 
        }

        _skillXP[skill] += amount;
        OnSkillXPGained?.Invoke(skill, amount);

        // Seviye atlama kontrolü
        double xpToNextLevel = GetXPForNextLevel(skill); // [DEĞİŞTİ] Yeni fonksiyonu kullan
        
        while (_skillXP[skill] >= xpToNextLevel && _skillLevels[skill] < skillData.maxLevel)
        {
            _skillXP[skill] -= xpToNextLevel; // Kalan XP'yi sakla
            _skillLevels[skill]++;
            
            int newLevel = _skillLevels[skill];
            
            GameConsole.Instance?.AddMessage($"<color=green>Yaşam Becerisi Arttı! {skillData.displayName} seviye {newLevel} oldu!</color>");
            OnSkillLeveledUp?.Invoke(skill, newLevel);
            
            // [YENİ] Seviye atlama ödüllerini kontrol et ve dağıt
            CheckForLevelUpRewards(skill, newLevel);
            
            // Bir sonraki seviye için gereken XP'yi hesapla
            xpToNextLevel = GetXPForNextLevel(skill);
        }
        
        // [YENİ] Maks seviyeye ulaşıldıysa ve hala XP fazlası varsa, XP'yi kilitle
        if (_skillLevels[skill] >= skillData.maxLevel)
        {
            _skillXP[skill] = 0;
        }
    }

    /// <summary>
    /// [YENİ] Belirli bir beceri ve seviye için tanımlanmış ödülleri dağıtır.
    /// </summary>
    private void CheckForLevelUpRewards(LifeSkill skill, int newLevel)
    {
        if (!_skillDataMap.TryGetValue(skill, out LifeSkillData data)) return;

        // O seviyeye özel tanımlanmış bir ödül var mı diye SO listesini kontrol et
        var rewardMap = data.levelUpRewards.FirstOrDefault(r => r.levelToAward == newLevel);
        
        if (rewardMap != null && rewardMap.rewardDataAsset != null)
        {
            var rewardAsset = rewardMap.rewardDataAsset;
            
            // GameConsole'a ve Ödül Dağıtıcısına haber ver
            string desc = string.IsNullOrEmpty(rewardAsset.descriptionForUI) ? "" : $"\n{rewardAsset.descriptionForUI}";
            GameConsole.Instance?.AddMessage($"<color=yellow>Beceri Ödülü: {rewardAsset.displayName}</color>{desc}");
            GameRewardDistributor.Instance?.DistributeRewards(rewardAsset.rewards);
        }
    }


    // Public Get Fonksiyonları
    public int GetLevel(LifeSkill skill) => _skillLevels.TryGetValue(skill, out int level) ? level : 1;
    public double GetXP(LifeSkill skill) => _skillXP.TryGetValue(skill, out double xp) ? xp : 0;

    /// <summary>
    /// [DÜZELTİLDİ] String ismine göre beceri seviyesini döndürür. (Mevcut kodunuzda bu zaten doğruydu, korundu)
    /// </summary>
    public int GetSkillLevel(string skillName)
    {
        if (System.Enum.TryParse<LifeSkill>(skillName, true, out LifeSkill skill))
        {
            return GetLevel(skill);
        }
        Debug.LogWarning($"[LifeSkillManager] '{skillName}' adında bir LifeSkill bulunamadı.");
        return 0;
    }
    
    /// <summary>
    /// Progress bar için (0.0 - 1.0 arası) ilerleme durumu verir.
    /// [DEĞİŞTİ] Formülü SO'dan alır
    /// </summary>
    public float GetXPProgress(LifeSkill skill)
    {
        if (skill == LifeSkill.None) return 0f;
        
        double currentXP = GetXP(skill);
        double neededXP = GetXPForNextLevel(skill); // [DEĞİŞTİ]

        if (neededXP <= 0 || neededXP == float.MaxValue) return 1f; // Maks seviyede bar dolsun
        return (float)(currentXP / neededXP);
    }

    /// <summary>
    /// Seviye atlamak için gereken XP formülü.
    /// [DEĞİŞTİ] Artık formülü SO verisinden okur.
    /// </summary>
    public double GetXPForNextLevel(LifeSkill skill)
    {
        if (!_skillDataMap.TryGetValue(skill, out LifeSkillData data))
        {
            Debug.LogError($"[LifeSkillManager] '{skill}' için SkillData (SO) bulunamadı! XP hesaplanamıyor.");
            return double.MaxValue; // Hata durumunda seviye atlamayı engelle
        }

        int currentLevel = GetLevel(skill);
        return data.CalculateXPForNextLevel(currentLevel);
    }

    // [YENİ] Dışarıdan beceri verisine (isim, açıklama vb.) erişim için
    public LifeSkillData GetSkillData(LifeSkill skill)
    {
        _skillDataMap.TryGetValue(skill, out LifeSkillData data);
        return data; // Bulunamazsa null döner
    }


    // ===================================================================
    // KAYIT SİSTEMİ (IGameDataSaveable Arayüzü)
    // ===================================================================
    // [DEĞİŞTİ] Kayıt ve Yükleme mantığı, SO'ların yüklenmesini hesaba katacak şekilde
    // elden geçirildi.

    public LifeSkillSaveData GetSaveData()
    {
        Debug.Log("[LifeSkillManager] Kayıt verisi oluşturuluyor.");
        return new LifeSkillSaveData
        {
            skillXP = new Dictionary<LifeSkill, double>(_skillXP),
            skillLevels = new Dictionary<LifeSkill, int>(_skillLevels)
        };
    }

    public void LoadFromData(LifeSkillSaveData data)
    {
        // ÖNEMLİ: SO verileri (LoadSkillDataAssets) ve varsayılanlar (InitializeSkills)
        // bu fonksiyon çağrılmadan *önce* Awake()'de zaten yüklendi.

        if (data == null)
        {
            Debug.Log("[LifeSkillManager] Kayıtlı LifeSkill verisi bulunamadı, varsayılan (SO) değerlerle devam ediliyor.");
            // InitializeSkills() zaten Awake'de çağrıldığı için tekrar çağırmaya gerek yok.
            OnSkillDataLoaded?.Invoke();
            return;
        }

        // Kayıtlı veriyi yükle (SO'dan gelen varsayılanların üzerine yaz)
        _skillXP = data.skillXP ?? _skillXP;
        _skillLevels = data.skillLevels ?? _skillLevels;

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
                // [DEĞİŞTİ] Yeni eklenen skill'in başlangıç seviyesini SO'dan al
                int startLevel = 1;
                if (_skillDataMap.TryGetValue(skill, out LifeSkillData skillData))
                {
                    startLevel = skillData.startLevel;
                }
                _skillLevels.Add(skill, startLevel);
                needsInitialization = true;
            }
        }
        if (needsInitialization)
            Debug.Log("[LifeSkillManager] Kayıt yüklenirken yeni eklenen LifeSkill'ler varsayılan değerlerle başlatıldı.");

        Debug.Log("[LifeSkillManager] LifeSkill verisi yüklendi.");
        OnSkillDataLoaded?.Invoke(); // Tüm UI panellerine "yenilenin" sinyali gönder
    }
    
    /// <summary>
/// [YENİ] Bir becerinin ScriptableObject'undan bonus değerini okur.
/// (Adım 12'de eklendi)
/// </summary>
public float GetBonusFromSkill(LifeSkill skill, string bonusType)
{
    if (_skillDataMap.TryGetValue(skill, out LifeSkillData data))
    {
        int currentLevel = GetLevel(skill);
        return data.GetBonus(currentLevel, bonusType);
    }
    return 0f;
}

    /// <summary>
    /// [YENİ] Bir becerinin ScriptableObject'undan bonus açıklamasını okur.
    /// (Adım 12'de eklendi)
    /// </summary>
    public string GetBonusDescription(LifeSkill skill)
    {
        if (_skillDataMap.TryGetValue(skill, out LifeSkillData data))
        {
            int currentLevel = GetLevel(skill);
            return data.GetBonusDescription(currentLevel);
        }
        return "Beceri verisi bulunamadı.";
    }

}