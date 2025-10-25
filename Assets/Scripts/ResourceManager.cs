using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Oyuncunun Can, Enerji ve Mana gibi ana kaynaklarını yönetir.
/// Rejenerasyonu ve StatCalculator'dan gelen maksimum değer güncellemelerini işler.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// </summary>
public class ResourceManager : MonoBehaviour, IGameDataSaveable<ResourceSaveData>
{
    public static ResourceManager Instance;

    [Header("Health")]
    public Image healthFill;
    public TextMeshProUGUI healthText;
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("Energy")]
    public Image energyFill;
    public TextMeshProUGUI energyText;
    public float currentEnergy = 100f;
    public float maxEnergy = 100f;

    [Header("Mana")]
    public Image manaFill;
    public TextMeshProUGUI manaText;
    public float currentMana = 100f;
    public float maxMana = 100f;

    // Diğer sistemlerin kaynak değişikliklerini dinleyebilmesi için event
    public event Action OnValuesChanged;

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
        // Not: Bu değerler, hemen ardından GameDataManager'dan Load gelirse üzerine yazılacaktır.
        // Bu, "Yeni Oyun" senaryosu için varsayılan değerleri ayarlar.
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentMana = maxMana;

        UpdateAllBars();

        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated += UpdateMaxStats;
            Debug.Log("ResourceManager, StatCalculator'a abone oldu.");

            // Başlangıçta maksimum statları hemen çek
            UpdateMaxStats();
        }
        LevelManager.OnPlayerLeveledUp += HandlePlayerLevelUp;
    }

    void OnDestroy()
    {
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated -= UpdateMaxStats;
        }
        LevelManager.OnPlayerLeveledUp -= HandlePlayerLevelUp;
    }

    void Update()
    {
        if (StatCalculator.Instance == null) return;

        // Rejenerasyonları uygula
        float healthRegenRate = (float)StatCalculator.Instance.currentStats.HealthRecovery;
        float energyRegenRate = (float)StatCalculator.Instance.currentStats.EnergyRecovery;
        float manaRegenRate = (float)StatCalculator.Instance.currentStats.ManaRecovery;

        float oldHealth = currentHealth;
        float oldEnergy = currentEnergy;
        float oldMana = currentMana;

        currentHealth = Mathf.Clamp(currentHealth + healthRegenRate * Time.deltaTime, 0, maxHealth);
        currentEnergy = Mathf.Clamp(currentEnergy + energyRegenRate * Time.deltaTime, 0, maxEnergy);
        currentMana = Mathf.Clamp(currentMana + manaRegenRate * Time.deltaTime, 0, maxMana);

        // Sadece değerler gerçekten değiştiyse UI güncellemesi yap
        if (oldHealth != currentHealth || oldEnergy != currentEnergy || oldMana != currentMana)
        {
            UpdateAllBars();
            OnValuesChanged?.Invoke(); // Değerler değiştiyse haber ver
        }
    }

    /// <summary>
    /// LevelManager'dan gelen 'OnPlayerLeveledUp' event'ini (duyurusunu) yakalar
    /// ve kaynakları (Health, Energy, Mana) günceller.
    /// </summary>
    private void HandlePlayerLevelUp()
    {
        // LevelManager'dan seviye atlama bonuslarını al
        // (Burada hala LevelManager.Instance'a erişiyoruz, ama sadece
        // BİLGİ ALMAK için, EMİR VERMEK için değil. Bu daha kabul edilebilir.)
        if (LevelManager.Instance == null) return;

        float healthBonus = LevelManager.Instance.maxHealthPerLevel;
        float energyBonus = LevelManager.Instance.maxEnergyPerLevel;
        float manaBonus = LevelManager.Instance.maxManaPerLevel;

        // ResourceManager KENDİ işini KENDİSİ yapar
        this.maxHealth += healthBonus;
        this.maxEnergy += energyBonus;
        this.maxMana += manaBonus;

        // Kaynakları tam doldur
        this.currentHealth = this.maxHealth;
        this.currentEnergy = this.maxEnergy;
        this.currentMana = this.maxMana;

        // UI'ı güncelle
        UpdateAllBars();
        OnValuesChanged?.Invoke();

        Debug.Log($"[ResourceManager] Seviye atlandı! Yeni Max Health: {maxHealth}");
    }


    /// <summary>
    /// StatCalculator'dan gelen 'OnStatsRecalculated' event'i ile tetiklenir.
    /// Maksimum değerleri günceller ve mevcut değerleri bu maks'ın içinde tutar.
    /// </summary>
    private void UpdateMaxStats()
    {
        if (StatCalculator.Instance == null) return;

        maxHealth = (float)StatCalculator.Instance.currentStats.MaxHealth;
        maxEnergy = (float)StatCalculator.Instance.currentStats.MaxEnergy;
        maxMana = (float)StatCalculator.Instance.currentStats.MaxMana;

        // Mevcut değerlerin yeni maksimum değerleri aşmadığından emin ol
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        currentMana = Mathf.Min(currentMana, maxMana);

        UpdateAllBars();
        OnValuesChanged?.Invoke(); // Max statlar değiştiğinde de haber ver
    }

    /// <summary>
    /// Tüm UI barlarını ve metinlerini günceller.
    /// </summary>
    public void UpdateAllBars()
    {
        UpdateBar(healthFill, healthText, currentHealth, maxHealth);
        UpdateBar(energyFill, energyText, currentEnergy, maxEnergy);
        UpdateBar(manaFill, manaText, currentMana, maxMana);
    }

    /// <summary>
    /// (Eski koddaki) Özel UI bar doldurma mantığı.
    /// </summary>
    void UpdateBar(Image fillImage, TextMeshProUGUI text, float current, float max)
    {
        if (fillImage == null || text == null) return;

        float percentage = (max > 0) ? Mathf.Clamp01(current / max) : 0;

        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        RectTransform parentRect = fillRect.parent.GetComponent<RectTransform>();
        if (parentRect == null) return;

        float parentWidth = parentRect.rect.width;
        float originalLeft = 7f;
        float originalRight = 8.1f;
        float rightOffset = -((parentWidth - originalLeft - originalRight) * (1f - percentage) + originalRight);

        fillRect.offsetMin = new Vector2(originalLeft, fillRect.offsetMin.y);
        fillRect.offsetMax = new Vector2(rightOffset, fillRect.offsetMax.y);

        text.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
    }

    // ====================================================================================================
    // KAYNAK DEĞİŞTİRME METOTLARI
    // ====================================================================================================

    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateAllBars();
        OnValuesChanged?.Invoke();
    }

    public void ModifyEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        UpdateAllBars();
        OnValuesChanged?.Invoke();
    }

    public void ModifyMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        UpdateAllBars();
        OnValuesChanged?.Invoke();
    }

    // Not: Bu 'ModifyMax...' metotları muhtemelen artık StatCalculator tarafından
    // otomatik olarak yönetildiği için harici olarak çağrılmamalıdır.
    // Ancak eski kodda oldukları için güvenlik açısından korundu.
    public void ModifyMaxHealth(float amount)
    {
        maxHealth += amount;
        if (maxHealth < 1) maxHealth = 1;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        OnValuesChanged?.Invoke();
    }

    public void ModifyMaxEnergy(float amount)
    {
        maxEnergy += amount;
        if (maxEnergy < 1) maxEnergy = 1; 
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        OnValuesChanged?.Invoke();
    }
    
    public void ModifyMaxMana(float amount)
    {
        maxMana += amount;
        if (maxMana < 1) maxMana = 1;
        if (currentMana > maxMana) currentMana = maxMana;
        OnValuesChanged?.Invoke();
    }
    
    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    // ====================================================================================================
    // KAYIT SİSTEMİ (GameDataManager UYUMLU)
    // ====================================================================================================

    /// <summary>
    /// GameDataManager'a kaydedilecek verileri toplar ve döndürür.
    /// Bu, GameSaveData.cs içindeki 'ResourceSaveData' sınıfı ile eşleşmelidir.
    /// SADECE 'current' değerler kaydedilir, 'max' değerler Stat'lardan hesaplanır.
    /// </summary>
    public ResourceSaveData GetSaveData()
    {
        Debug.Log("ResourceManager: Kayıt verisi oluşturuluyor.");
        return new ResourceSaveData
        {
            currentHealth = this.currentHealth,
            currentEnergy = this.currentEnergy,
            currentMana = this.currentMana
        };
    }

    /// <summary>
    /// GameDataManager'dan gelen verileri bu yöneticiye yükler.
    /// Bu metodun, StatManager'ın verileri yüklendikten SONRA çağrılması kritiktir.
    /// </summary>
    public void LoadFromData(ResourceSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("ResourceManager LoadFromData: Yüklenecek veri bulunamadı (data == null).");
            return;
        }

        // maxHealth, maxEnergy, maxMana'nın StatCalculator tarafından
        // bu fonksiyondan ÖNCE güncellendiğini varsayıyoruz.
        // GameDataManager'daki yükleme sırası bunu garanti etmelidir.
        
        this.currentHealth = Mathf.Clamp(data.currentHealth, 0, this.maxHealth);
        this.currentEnergy = Mathf.Clamp(data.currentEnergy, 0, this.maxEnergy);
        this.currentMana = Mathf.Clamp(data.currentMana, 0, this.maxMana);
        
        UpdateAllBars();
        OnValuesChanged?.Invoke();
        Debug.Log($"ResourceManager verisi yüklendi. Health: {currentHealth}/{maxHealth}");
    }
}