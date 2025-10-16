using UnityEngine;
using TMPro;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance; // Singleton - her yerden erişim için

    [Header("Currency Values")]
    public double gold = 0;
    public double nexusCoin = 0;
    public double premiumCoin = 0;
    public double people = 0;

    [Header("UI References")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI nexusText;
    public TextMeshProUGUI premiumText;
    public TextMeshProUGUI peopleText;


    private double lastCalculatedPrestige = 0;
    private double lastCalculatedPremiumFromThresholds = 0;
    private double lastCalculatedPeopleFromSocial = 0;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateAllCurrencyUI();

        // YENİ: StatCalculator'dan gelen anonsları dinlemeye başla
        if (StatCalculator.Instance != null)
        {

            StatCalculator.Instance.OnStatsRecalculated += GrantThresholdRewards;
            StatCalculator.Instance.OnStatsRecalculated += UpdatePeopleFromSocial;
        }

    }

    void OnDestroy()
    {
        // YENİ: Abonelikten çık
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated -= GrantThresholdRewards;
            StatCalculator.Instance.OnStatsRecalculated -= UpdatePeopleFromSocial; // BU SATIRI EKLE

        }
    }


        private void UpdatePeopleFromSocial() // BU FONKSİYONU GÜNCELLEYİN
    {
        if (StatManager.Instance == null) return;

        double totalSocial = StatManager.Instance.GetTotalSocial();
        
        // Social stattan gelen People hesaplaması:
        // Her 1 Social için 1 People
        // Her 1000 Social için 100 People
        double calculatedPeopleFromSocial = totalSocial * 1; 
        calculatedPeopleFromSocial += Math.Floor(totalSocial / 1000) * 100;
        
        // Yüzdesel People bonusu: Her 100 Social için %5 People artışı (şimdiki toplam social'dan)
        // Bu bonusun kendisi katlanma yapmamalı, sadece social'ın mevcut değerine göre hesaplanmalı
        calculatedPeopleFromSocial += totalSocial * (Math.Floor(totalSocial / 100) * 0.05);


        // Sadece aradaki fark kadar People ekle
        if (calculatedPeopleFromSocial > lastCalculatedPeopleFromSocial)
        {
            double difference = calculatedPeopleFromSocial - lastCalculatedPeopleFromSocial;
            AddPeople(difference);
            Debug.Log($"Social stattan {difference} People kazanıldı!");
        }
        // Eğer stat düşerse (ki olmamalı) veya sıfırlanırsa da People miktarını güncelleyebiliriz:
        // else if (calculatedPeopleFromSocial < lastCalculatedPeopleFromSocial)
        // {
        //     double difference = lastCalculatedPeopleFromSocial - calculatedPeopleFromSocial;
        //     SpendPeople(difference); // Veya benzeri bir azaltma mantığı
        // }
        
        lastCalculatedPeopleFromSocial = calculatedPeopleFromSocial;
        UpdateAllCurrencyUI(); // UI'ı güncelle
    }
    
        // YENİ FONKSİYON: Stat eşiklerinden gelen ödülleri akıllıca verir
    private void GrantThresholdRewards()
    {
        if (StatCalculator.Instance == null) return;

        // StatCalculator'dan en son hesaplanan değerleri al
        ComputedStats stats = StatCalculator.Instance.currentStats;
        
        // 1. Premium Coin Ödülleri
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
        lastCalculatedPremiumFromThresholds = totalPremiumFromStats; // Son değeri güncelle

        // 2. Prestige Points Ödülleri
        if (stats.PrestigePoints > lastCalculatedPrestige)
        {
            // Bu kısım şimdilik boş, çünkü prestij mekaniği henüz yok.
            // Ama mantık aynı: aradaki fark kadar prestij puanı ekle.
        }
        lastCalculatedPrestige = stats.PrestigePoints; // Son değeri güncelle
        UpdateAllCurrencyUI(); // UI'ı güncelle
    }



    // Test için - Inspector'dan çağırılabilir
    void Update()
    {/*
        //* Test tuşları
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
    */}

    // Para ekle/çıkar fonksiyonları
    public bool AddGold(double amount)
    {
        gold += amount;
        UpdateCurrencyUI(goldText, gold);
        return true;
    }


    public bool SpendGold(double amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateCurrencyUI(goldText, gold);
            return true;
        }
        return false; // Yeterli altın yok
    }

    public bool AddNexusCoin(double amount)
    {
        nexusCoin += amount;
        UpdateCurrencyUI(nexusText, nexusCoin);
        return true;
    }

    public bool SpendNexusCoin(double amount)
    {
        if (nexusCoin >= amount)
        {
            nexusCoin -= amount;
            UpdateCurrencyUI(nexusText, nexusCoin);
            return true;
        }
        return false;
    }

    public bool AddPremiumCoin(double amount)
    {
        premiumCoin += amount;
        UpdateCurrencyUI(premiumText, premiumCoin);
        return true;
    }

    public bool SpendPremiumCoin(double amount)
    {
        if (premiumCoin >= amount)
        {
            premiumCoin -= amount;
            UpdateCurrencyUI(premiumText, premiumCoin);
            return true;
        }
        return false;
    }

    public bool AddPeople(double amount)
    {
        people += amount;
        UpdateCurrencyUI(peopleText, people);
        return true;
    }

    public bool SpendPeople(double amount)
    {
        if (people >= amount)
        {
            people -= amount;
            UpdateCurrencyUI(peopleText, people);
            return true;
        }
        return false;
    }

    // Tüm UI'ları güncelle

    
    public void UpdateAllCurrencyUI()
    {
        UpdateCurrencyUI(goldText, gold);
        UpdateCurrencyUI(nexusText, nexusCoin);
        UpdateCurrencyUI(premiumText, premiumCoin);
        UpdateCurrencyUI(peopleText, people);
    }

    // Tek bir para biriminin UI'ını güncelle
    void UpdateCurrencyUI(TextMeshProUGUI text, double value)
    {
        if (text != null)
        {
            text.text = FormatNumber(value);
        }
    }

    // Sayıları formatla (K, M, B, T veya bilimsel)
    string FormatNumber(double value)
    {
        if (value < 1000)
        {
            return value.ToString("F0"); // 0-999: tam sayı
        }
        else if (value < 1000000) // 1K - 999K
        {
            return (value / 1000).ToString("F1") + "K";
        }
        else if (value < 1000000000) // 1M - 999M
        {
            return (value / 1000000).ToString("F1") + "M";
        }
        else if (value < 1000000000000) // 1B - 999B
        {
            return (value / 1000000000).ToString("F1") + "B";
        }
        else if (value < 1000000000000000) // 1T - 999T
        {
            return (value / 1000000000000).ToString("F1") + "T";
        }
        else // Bilimsel notasyon
        {
            return value.ToString("E2");
        }
    }

    // Yeterli para var mı kontrol
    public bool CanAffordGold(double amount) => gold >= amount;
    public bool CanAffordNexus(double amount) => nexusCoin >= amount;
    public bool CanAffordPremium(double amount) => premiumCoin >= amount;
    public bool CanAffordPeople(double amount) => people >= amount;
}