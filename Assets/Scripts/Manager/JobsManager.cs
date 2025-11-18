using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // [YENİ] Linq eklendi

/// <summary>
/// Oyuncunun Mesleklerinin (Job) XP ve Seviyelerini yönetir.
/// Veri mantığını (XP formülü, ödüller) ScriptableObject'lardan yükler.
/// </summary>
public class JobsManager : MonoBehaviour, IGameDataSaveable<JobSaveData>
{
    public static JobsManager Instance { get; private set; }

    // Kaydedilecek ana veriler
    private Dictionary<Job, double> _jobXP = new Dictionary<Job, double>();
    private Dictionary<Job, int> _jobLevels = new Dictionary<Job, int>();

    // [YENİ] Mesleklerin tüm verilerini (formüller, ödüller) tutan harita
    private Dictionary<Job, JobData> _jobDataMap = new Dictionary<Job, JobData>();

    // Event'ler
    public event Action<Job, int> OnJobLeveledUp; // Hangi meslek, yeni seviyesi
    public event Action<Job, double> OnJobXPGained; // Hangi meslek, kazanılan xp
    public event Action OnJobDataLoaded; // Yükleme tamamlandığında UI'ı güncellemek için

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // [DEĞİŞTİ] Sıralama: Önce SO verilerini yükle, sonra varsayılan değerleri başlat
        LoadJobDataAssets();
        InitializeJobs();
    }

    /// <summary>
    /// [YENİ] Resources klasöründen tüm JobData asset'lerini yükler ve haritaya ekler.
    /// </summary>
    private void LoadJobDataAssets()
    {
        _jobDataMap.Clear();
        // ÖNEMLİ: Tüm JobData asset'leri "Resources/SkillData/Jobs" klasöründe olmalı
        var allJobData = Resources.LoadAll<JobData>("SkillData/Jobs");

        foreach (var data in allJobData)
        {
            if (!_jobDataMap.ContainsKey(data.jobID))
            {
                _jobDataMap.Add(data.jobID, data);
            }
            else
            {
                Debug.LogWarning($"[JobsManager] '{data.jobID}' için mükerrer veri bulundu: {data.name}");
            }
        }
        Debug.Log($"[JobsManager] {allJobData.Length} adet Meslek verisi yüklendi.");
    }


    /// <summary>
    /// Sözlükleri (Dictionary) enum'daki tüm değerler için varsayılan değerlerle başlatır.
    /// [DEĞİŞTİ] Artık başlangıç seviyesini SO verisinden okur.
    /// </summary>
    private void InitializeJobs()
    {
        _jobXP.Clear();
        _jobLevels.Clear();

        foreach (Job job in Enum.GetValues(typeof(Job)))
        {
            if (job == Job.None) continue;

            // SO verisinden başlangıç seviyesini al
            int startLevel = 1;
            if (_jobDataMap.TryGetValue(job, out JobData data))
            {
                startLevel = data.startLevel;
            }

            if (!_jobXP.ContainsKey(job))
            {
                _jobXP.Add(job, 0);
            }
            if (!_jobLevels.ContainsKey(job))
            {
                _jobLevels.Add(job, startLevel);
            }
        }
    }


    /// <summary>
    /// Belirli bir Mesleğe XP ekler ve seviye atlama kontrolü yapar.
    /// [DEĞİŞTİ] Artık XP formülünü ve ödül mantığını SO'dan alır.
    /// </summary>
    public void AddXP(Job job, double amount)
    {
        if (job == Job.None || amount <= 0) return;

        if (!_jobXP.ContainsKey(job) || !_jobDataMap.ContainsKey(job))
        {
            Debug.LogWarning($"[JobsManager] '{job}' için XP eklenemedi. Sözlükte veya Veri Haritasında (SO) başlatılmamış.");
            return;
        }

        int currentLevel = GetLevel(job);
        JobData jobData = _jobDataMap[job];

        // [YENİ] Maksimum seviye kontrolü
        if (currentLevel >= jobData.maxLevel)
        {
            _jobXP[job] = 0; 
            return;
        }

        _jobXP[job] += amount;
        OnJobXPGained?.Invoke(job, amount);

        // Seviye atlama kontrolü
        double xpToNextLevel = GetXPForNextLevel(job); // [DEĞİŞTİ] Yeni fonksiyonu kullan

        while (_jobXP[job] >= xpToNextLevel && _jobLevels[job] < jobData.maxLevel)
        {
            _jobXP[job] -= xpToNextLevel; // Kalan XP'yi sakla
            _jobLevels[job]++;

            int newLevel = _jobLevels[job];

            GameConsole.Instance?.AddMessage($"<color=purple>Meslek Becerisi Arttı! {jobData.displayName} seviye {newLevel} oldu!</color>");
            OnJobLeveledUp?.Invoke(job, newLevel);

            // [YENİ] Seviye atlama ödüllerini kontrol et ve dağıt
            CheckForLevelUpRewards(job, newLevel);

            // Bir sonraki seviye için gereken XP'yi hesapla
            xpToNextLevel = GetXPForNextLevel(job);
        }
        
        // [YENİ] Maks seviyeye ulaşıldıysa ve hala XP fazlası varsa, XP'yi kilitle
        if (_jobLevels[job] >= jobData.maxLevel)
        {
            _jobXP[job] = 0;
        }
    }

    /// <summary>
    /// [YENİ] Belirli bir meslek ve seviye için tanımlanmış ödülleri dağıtır.
    /// </summary>
    private void CheckForLevelUpRewards(Job job, int newLevel)
    {
        if (!_jobDataMap.TryGetValue(job, out JobData data)) return;

        // O seviyeye özel tanımlanmış bir ödül var mı diye SO listesini kontrol et
        var rewardMap = data.levelUpRewards.FirstOrDefault(r => r.levelToAward == newLevel);
        
        if (rewardMap != null && rewardMap.rewardDataAsset != null)
        {
            var rewardAsset = rewardMap.rewardDataAsset;
            
            // GameConsole'a ve Ödül Dağıtıcısına haber ver
            string desc = string.IsNullOrEmpty(rewardAsset.descriptionForUI) ? "" : $"\n{rewardAsset.descriptionForUI}";
            GameConsole.Instance?.AddMessage($"<color=yellow>Meslek Ödülü: {rewardAsset.displayName}</color>\n{rewardAsset.descriptionForUI}");
            GameRewardDistributor.Instance?.DistributeRewards(rewardAsset.rewards);
        }
    }

    // Public Get Fonksiyonları
    public int GetLevel(Job job) => _jobLevels.TryGetValue(job, out int level) ? level : 1;
    public double GetXP(Job job) => _jobXP.TryGetValue(job, out double xp) ? xp : 0;


    /// <summary>
    /// [DÜZELTİLDİ] String ismine göre meslek seviyesini döndürür. (Mevcut kodunuzda bu zaten doğruydu, korundu)
    /// </summary>
    public int GetJobLevel(string jobName)
    {
        if (System.Enum.TryParse<Job>(jobName, true, out Job job))
        {
            return GetLevel(job);
        }
        Debug.LogWarning($"[JobsManager] '{jobName}' adında bir Job bulunamadı.");
        return 0;
    }

    /// <summary>
    /// Progress bar için (0.0 - 1.0 arası) ilerleme durumu verir.
    /// [DEĞİŞTİ] Formülü SO'dan alır
    /// </summary>
    public float GetXPProgress(Job job)
    {
        if (job == Job.None) return 0f;
        
        double currentXP = GetXP(job);
        double neededXP = GetXPForNextLevel(job); // [DEĞİŞTİ]

        if (neededXP <= 0 || neededXP == float.MaxValue) return 1f; // Maks seviyede bar dolsun
        return (float)(currentXP / neededXP);
    }

    /// <summary>
    /// Seviye atlamak için gereken XP formülü.
    /// [DEĞİŞTİ] Artık formülü SO verisinden okur.
    /// </summary>
    public double GetXPForNextLevel(Job job)
    {
        if (!_jobDataMap.TryGetValue(job, out JobData data))
        {
            Debug.LogError($"[JobsManager] '{job}' için JobData (SO) bulunamadı! XP hesaplanamıyor.");
            return double.MaxValue; // Hata durumunda seviye atlamayı engelle
        }

        int currentLevel = GetLevel(job);
        return data.CalculateXPForNextLevel(currentLevel);
    }

    // [YENİ] Dışarıdan meslek verisine (isim, açıklama vb.) erişim için
    public JobData GetJobData(Job job)
    {
        _jobDataMap.TryGetValue(job, out JobData data);
        return data; // Bulunamazsa null döner
    }


    // ===================================================================
    // KAYIT SİSTEMİ (IGameDataSaveable Arayüzü)
    // ===================================================================
    // [DEĞİŞTİ] Kayıt ve Yükleme mantığı, SO'ların yüklenmesini hesaba katacak şekilde
    // elden geçirildi.

    public JobSaveData GetSaveData()
    {
        Debug.Log("[JobsManager] Kayıt verisi oluşturuluyor.");
        return new JobSaveData
        {
            jobXP = new Dictionary<Job, double>(_jobXP),
            jobLevels = new Dictionary<Job, int>(_jobLevels)
        };
    }

    public void LoadFromData(JobSaveData data)
    {
        // SO verileri (LoadJobDataAssets) ve varsayılanlar (InitializeJobs)
        // bu fonksiyon çağrılmadan *önce* Awake()'de zaten yüklendi.

        if (data == null)
        {
            Debug.Log("[JobsManager] Kayıtlı Job verisi bulunamadı, varsayılan (SO) değerlerle devam ediliyor.");
            OnJobDataLoaded?.Invoke();
            return;
        }

        // Kayıtlı veriyi yükle (SO'dan gelen varsayılanların üzerine yaz)
        _jobXP = data.jobXP ?? _jobXP;
        _jobLevels = data.jobLevels ?? _jobLevels;
        
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
                // [DEĞİŞTİ] Yeni eklenen job'un başlangıç seviyesini SO'dan al
                int startLevel = 1;
                if (_jobDataMap.TryGetValue(job, out JobData jobData))
                {
                    startLevel = jobData.startLevel;
                }
                 _jobLevels.Add(job, startLevel);
                 needsInitialization = true;
             }
        }
        if (needsInitialization)
            Debug.Log("[JobsManager] Kayıt yüklenirken yeni eklenen Job'lar varsayılan değerlerle başlatıldı.");

        Debug.Log("[JobsManager] Job verisi yüklendi.");
        OnJobDataLoaded?.Invoke(); // Tüm UI panellerine "yenilenin" sinyali gönder
    }
}