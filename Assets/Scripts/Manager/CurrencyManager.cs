using UnityEngine;
using TMPro;
using System;

/// <summary>
/// Oyuncunun tüm para birimlerini (Gold, Nexus, vb.) yönetir.
/// Stat eşiklerinden ve Social stattan gelen pasif kazançları hesaplar.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// </summary>
public class CurrencyManager : MonoBehaviour, IGameDataSaveable<CurrencySaveData>
{
    public event Action<CurrencyType, double> OnCurrencyChanged;
    public static CurrencyManager Instance;

    [Header("Currency Values (Kaydedilen)")]
    public double gold = 0;
    public double nexusCoin = 0;
    public double premiumCoin = 0;
    public double people = 0;

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI nexusText;
    public TextMeshProUGUI premiumText;
    public TextMeshProUGUI peopleText;

    // Stat'lardan gelen pasif ödülleri takip etmek için
    private double lastCalculatedPrestige = 0;
    private double lastCalculatedPremiumFromThresholds = 0;
    private double lastCalculatedPeopleFromSocial = 0;

    void Awake()
    {
        // Güncel kodun 'DontDestroyOnLoad' içeren hali (doğru olan)
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
        UpdateAllCurrencyUI();

        // StatCalculator'ı dinlemeye başla (Eski ve yeni kodda da vardı)
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated += GrantThresholdRewards;
            StatCalculator.Instance.OnStatsRecalculated += UpdatePeopleFromSocial;
            Debug.Log("CurrencyManager, StatCalculator'a abone oldu.");
        }
    }

    void OnDestroy()
    {
        // Abonelikten çık (Eski ve yeni kodda da vardı)
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated -= GrantThresholdRewards;
            StatCalculator.Instance.OnStatsRecalculated -= UpdatePeopleFromSocial;
        }
    }

    // ESKİ KODDAN EKLENDİ: Test amaçlı Update bloğu
    void Update()
    {
        /*
        // Test tuşları
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(1000);
            Debug.Log("Gold eklendi: " + gold);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            AddNexusCoin(500);
            Debug.Log("Nexus eklendi: " + nexusCoin);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddPremiumCoin(10);
            Debug.Log("Premium eklendi: " + premiumCoin);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            AddPeople(100);
            Debug.Log("People eklendi: " + people);
        }
        */
    }

    // ====================================================================================================
    // PASİF KAZANÇLAR (StatCalculator'dan tetiklenir)
    // ====================================================================================================

    /// <summary>
    /// Social stat değişimlerine göre 'People' kazancını hesaplar ve ekler.
    /// </summary>
    private void UpdatePeopleFromSocial()
    {
        if (StatManager.Instance == null) return;

        double totalSocial = StatManager.Instance.GetTotalSocial();

        // (Eski ve yeni koddaki aynı mantık)
        double calculatedPeopleFromSocial = totalSocial * 1;
        calculatedPeopleFromSocial += Math.Floor(totalSocial / 1000) * 100;
        calculatedPeopleFromSocial += totalSocial * (Math.Floor(totalSocial / 100) * 0.05);

        if (calculatedPeopleFromSocial > lastCalculatedPeopleFromSocial)
        {
            double difference = calculatedPeopleFromSocial - lastCalculatedPeopleFromSocial;
            AddPeople(difference);
            Debug.Log($"Social stattan {difference} People kazanıldı!");
        }

        lastCalculatedPeopleFromSocial = calculatedPeopleFromSocial;
        UpdateAllCurrencyUI();
    }

    /// <summary>
    /// Stat eşiklerine (örn: her 10k stat) göre 'Premium Coin' kazancını hesaplar ve ekler.
    /// </summary>
    private void GrantThresholdRewards()
    {
        if (StatCalculator.Instance == null) return;

        ComputedStats stats = StatCalculator.Instance.currentStats;

        double totalPremiumFromStats = 0;
        if (StatManager.Instance != null)
        {
            StatManager sm = StatManager.Instance;
            totalPremiumFromStats += Math.Floor(sm.GetTotalPhysical() / 10000);
            totalPremiumFromStats += Math.Floor(sm.GetTotalMental() / 10000);
            totalPremiumFromStats += Math.Floor(sm.GetTotalPerception() / 10000);
            totalPremiumFromStats += Math.Floor(sm.GetTotalSpiritual() / 10000);
            totalPremiumFromStats += Math.Floor(sm.GetTotalLuck() / 10000);
            totalPremiumFromStats += Math.Floor(sm.GetTotalSocial() / 10000);
        }

        if (totalPremiumFromStats > lastCalculatedPremiumFromThresholds)
        {
            double difference = totalPremiumFromStats - lastCalculatedPremiumFromThresholds;
            AddPremiumCoin(difference);
            Debug.Log($"Stat eşiklerinden {difference} Premium Coin kazanıldı!");
        }
        lastCalculatedPremiumFromThresholds = totalPremiumFromStats;

        if (stats.PrestigePoints > lastCalculatedPrestige)
        {
            // Bu kısım şimdilik boş, çünkü prestij mekaniği henüz yok.
        }
        lastCalculatedPrestige = stats.PrestigePoints;
        UpdateAllCurrencyUI();
    }

    // ====================================================================================================
    // PARA BİRİMİ İŞLEMLERİ (Add/Spend)
    // ====================================================================================================

    // --- GOLD ---
    public bool AddGold(double amount)
    {
        gold += amount;
        UpdateCurrencyUI(goldText, gold);
        OnCurrencyChanged?.Invoke(CurrencyType.Gold, gold); // Güncel koddaki doğru hali
        return true;
    }

    public bool SpendGold(double amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateCurrencyUI(goldText, gold);
            OnCurrencyChanged?.Invoke(CurrencyType.Gold, gold); // Güncel koddaki doğru hali
            return true;
        }
        return false;
    }

    // --- NEXUS COIN ---
    public bool AddNexusCoin(double amount)
    {
        nexusCoin += amount;
        UpdateCurrencyUI(nexusText, nexusCoin);
        OnCurrencyChanged?.Invoke(CurrencyType.NexusCoin, nexusCoin); // Güncel koddaki doğru hali
        return true;
    }

    public bool SpendNexusCoin(double amount)
    {
        if (nexusCoin >= amount)
        {
            nexusCoin -= amount;
            UpdateCurrencyUI(nexusText, nexusCoin);
            OnCurrencyChanged?.Invoke(CurrencyType.NexusCoin, nexusCoin); // Güncel koddaki doğru hali
            return true;
        }
        return false;
    }

    // --- PREMIUM COIN ---
    public bool AddPremiumCoin(double amount)
    {
        premiumCoin += amount;
        UpdateCurrencyUI(premiumText, premiumCoin);
        OnCurrencyChanged?.Invoke(CurrencyType.PremiumCoin, premiumCoin); // Güncel koddaki doğru hali
        return true;
    }

    public bool SpendPremiumCoin(double amount)
    {
        if (premiumCoin >= amount)
        {
            premiumCoin -= amount;
            UpdateCurrencyUI(premiumText, premiumCoin);
            OnCurrencyChanged?.Invoke(CurrencyType.PremiumCoin, premiumCoin); // Güncel koddaki doğru hali
            return true;
        }
        return false;
    }

    // --- PEOPLE ---
    public bool AddPeople(double amount)
    {
        people += amount;
        UpdateCurrencyUI(peopleText, people);
        OnCurrencyChanged?.Invoke(CurrencyType.People, people); // Güncel koddaki doğru hali
        return true;
    }

    public bool SpendPeople(double amount)
    {
        if (people >= amount)
        {
            people -= amount;
            UpdateCurrencyUI(peopleText, people);
            OnCurrencyChanged?.Invoke(CurrencyType.People, people); // Güncel koddaki doğru hali
            return true;
        }
        return false;
    }

    // ====================================================================================================
    // UI VE KONTROL METOTLARI
    // ====================================================================================================

    public void UpdateAllCurrencyUI()
    {
        UpdateCurrencyUI(goldText, gold);
        UpdateCurrencyUI(nexusText, nexusCoin);
        UpdateCurrencyUI(premiumText, premiumCoin);
        UpdateCurrencyUI(peopleText, people);
    }

    void UpdateCurrencyUI(TextMeshProUGUI text, double value)
    {
        if (text != null)
        {
            text.text = FormatNumber(value);
        }
    }

    string FormatNumber(double value)
    {
        // (Güncel koddaki daha temiz formatlama)
        if (value < 1000) return value.ToString("F0");
        if (value < 1000000) return (value / 1000).ToString("F1") + "K";
        if (value < 1000000000) return (value / 1000000).ToString("F1") + "M";
        if (value < 1000000000000) return (value / 1000000000).ToString("F1") + "B";
        if (value < 1000000000000000) return (value / 1000000000000).ToString("F1") + "T";
        return value.ToString("E2");
    }

    // Yeterlilik Kontrolleri
    public bool CanAffordGold(double amount) => gold >= amount;
    public bool CanAffordNexus(double amount) => nexusCoin >= amount;
    public bool CanAffordPremium(double amount) => premiumCoin >= amount;
    public bool CanAffordPeople(double amount) => people >= amount;

    // ====================================================================================================
    // KAYIT SİSTEMİ (GameDataManager UYUMLU)
    // ====================================================================================================

    /// <summary>
    /// GameDataManager'a kaydedilecek verileri toplar ve döndürür.
    /// Bu, GameSaveData.cs içindeki 'CurrencySaveData' sınıfı ile eşleşmelidir.
    /// </summary>
    public CurrencySaveData GetSaveData()
    {
        Debug.Log("CurrencyManager: Kayıt verisi oluşturuluyor.");
        return new CurrencySaveData
        {
            gold = this.gold,
            nexusCoin = this.nexusCoin,
            premiumCoin = this.premiumCoin,
            people = this.people
        };
    }

    /// <summary>
    /// GameDataManager'dan gelen verileri bu yöneticiye yükler.
    /// </summary>
    public void LoadFromData(CurrencySaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("CurrencyManager LoadFromData: Yüklenecek veri bulunamadı (data == null).");
            return;
        }

        this.gold = data.gold;
        this.nexusCoin = data.nexusCoin;
        this.premiumCoin = data.premiumCoin;
        this.people = data.people;
        
        UpdateAllCurrencyUI();
        Debug.Log($"CurrencyManager verisi yüklendi. Gold: {gold}");
    }
}