using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Oyuncunun Mesleklerinin (Job) XP ve Seviyelerini yönetir.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// </summary>
public class JobsManager : MonoBehaviour, IGameDataSaveable<JobSaveData>
{
    public static JobsManager Instance { get; private set; }

    // Kaydedilecek ana veriler
    private Dictionary<Job, double> _jobXP = new Dictionary<Job, double>();
    private Dictionary<Job, int> _jobLevels = new Dictionary<Job, int>();

    // Event'ler
    public event Action<Job, int> OnJobLeveledUp; // Hangi meslek, yeni seviyesi
    public event Action<Job, double> OnJobXPGained; // Hangi meslek, kazanılan xp
    public event Action OnJobDataLoaded; // Yükleme tamamlandığında UI'ı güncellemek için

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitializeJobs();
    }

    /// <summary>
    /// Sözlükleri (Dictionary) enum'daki tüm değerler için varsayılan değerlerle başlatır.
    /// </summary>
    private void InitializeJobs()
    {
        _jobXP.Clear();
        _jobLevels.Clear();

        foreach (Job job in Enum.GetValues(typeof(Job)))
        {
            if (job == Job.None) continue;
            if (!_jobXP.ContainsKey(job))
            {
                _jobXP.Add(job, 0);
            }
            if (!_jobLevels.ContainsKey(job))
            {
                _jobLevels.Add(job, 1); // Seviyeler 1'den başlar
            }
        }
    }

    /// <summary>
    /// Belirli bir Mesleğe XP ekler ve seviye atlama kontrolü yapar.
    /// </summary>
    public void AddXP(Job job, double amount)
    {
        if (job == Job.None || amount <= 0) return;

        if (!_jobXP.ContainsKey(job))
        {
            Debug.LogWarning($"[JobsManager] '{job}' için XP eklenemedi. Sözlükte başlatılmamış.");
            return;
        }

        _jobXP[job] += amount;
        OnJobXPGained?.Invoke(job, amount);

        // Seviye atlama kontrolü
        double xpToNextLevel = GetXPForNextLevel(_jobLevels[job]);
        
        while (_jobXP[job] >= xpToNextLevel)
        {
            _jobXP[job] -= xpToNextLevel; // Kalan XP'yi sakla
            _jobLevels[job]++;
            
            GameConsole.Instance?.AddMessage($"<color=purple>Meslek Becerisi Arttı! {job} seviye {_jobLevels[job]} oldu!</color>");
            OnJobLeveledUp?.Invoke(job, _jobLevels[job]);
            
            // Bir sonraki seviye için gereken XP'yi hesapla
            xpToNextLevel = GetXPForNextLevel(_jobLevels[job]);
        }
    }

    // Public Get Fonksiyonları
    public int GetLevel(Job job) => _jobLevels.TryGetValue(job, out int level) ? level : 1;
    public double GetXP(Job job) => _jobXP.TryGetValue(job, out double xp) ? xp : 0;
    
    /// <summary>
    /// Progress bar için (0.0 - 1.0 arası) ilerleme durumu verir.
    /// </summary>
    public float GetXPProgress(Job job)
    {
        if (job == Job.None) return 0f;
        int level = GetLevel(job);
        double currentXP = GetXP(job);
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
        return (double)currentLevel * 100;
    }


    // ===================================================================
    // KAYIT SİSTEMİ (IGameDataSaveable Arayüzü)
    // ===================================================================

    /// <summary>
    /// GameDataManager'ın 'SaveGame' demesi üzerine çağrılır.
    /// Tıpkı PerkManager.GetSaveData() gibi çalışır.
    /// </summary>
    public JobSaveData GetSaveData()
    {
        Debug.Log("[JobsManager] Kayıt verisi oluşturuluyor.");
        return new JobSaveData
        {
            jobXP = new Dictionary<Job, double>(_jobXP),
            jobLevels = new Dictionary<Job, int>(_jobLevels)
        };
    }

    /// <summary>
    /// GameDataManager'ın 'LoadGame' demesi üzerine çağrılır.
    /// Tıpkı PerkManager.LoadFromData() gibi çalışır.
    /// </summary>
    public void LoadFromData(JobSaveData data)
    {
        if (data == null)
        {
            Debug.Log("[JobsManager] Kayıtlı Job verisi bulunamadı, varsayılan başlatılıyor.");
            InitializeJobs(); // Kayıt yoksa sıfırdan başlat
            OnJobDataLoaded?.Invoke(); // UI'ı güncellemek için
            return;
        }

        // Kayıtlı veriyi yükle
        _jobXP = data.jobXP ?? new Dictionary<Job, double>();
        _jobLevels = data.jobLevels ?? new Dictionary<Job, int>();
        
        // Kayıttan sonra oyuna yeni bir job eklenmişse (enum güncellenmişse)
        // bu yeni job'ları da sözlüğe ekle.
        bool needsInitialization = false;
        foreach (Job job in Enum.GetValues(typeof(Job)))
        {
             if (job == Job.None) continue;
             if (!_jobXP.ContainsKey(job))
             {
                 _jobXP.Add(job, 0);
                 needsInitialization = true;
             }
             if (!_jobLevels.ContainsKey(job))
             {
                 _jobLevels.Add(job, 1);
                 needsInitialization = true;
             }
        }
        if (needsInitialization)
            Debug.Log("[JobsManager] Yeni eklenen Job'lar varsayılan değerlerle başlatıldı.");

        Debug.Log("[JobsManager] Job verisi yüklendi.");
        OnJobDataLoaded?.Invoke(); // Tüm UI panellerine "yenilenin" sinyali gönder
    }
}