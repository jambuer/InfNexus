using UnityEngine;
using System;
using TMPro;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance;

    [Header("Base Stats")]
    public double physical = 0;
    public double mental = 0;
    public double perception = 0;
    public double spiritual = 0;
    public double luck = 0;
    public double social = 0;

    [Header("Bonus Stats (Equipment, Items, etc.)")]
    public double physicalBonus = 0;
    public double mentalBonus = 0;
    public double perceptionBonus = 0;
    public double spiritualBonus = 0;
    public double luckBonus = 0;
    public double socialBonus = 0;

    // Events - Stat değiştiğinde diğer sistemler dinleyebilir
    public event Action<string, double> OnStatChanged;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Scene değişse bile kalır
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Addstatpoint(string statName, int points)
    {
        switch (statName.ToLower())
        {
            case "physical":
                physical += points;
                OnStatChanged?.Invoke("Physical", GetTotalPhysical());
                break;
            case "mental":
                mental += points;
                OnStatChanged?.Invoke("Mental", GetTotalMental());
                break;
            case "perception":
                perception += points;
                OnStatChanged?.Invoke("Perception", GetTotalPerception());
                break;
            case "spiritual":
                spiritual += points;
                OnStatChanged?.Invoke("Spiritual", GetTotalSpiritual());
                break;
            case "luck":
                luck += points;
                OnStatChanged?.Invoke("Luck", GetTotalLuck());
                break;
            case "social":
                social += points;
                OnStatChanged?.Invoke("Social", GetTotalSocial());
                break;
            default:
                Debug.LogWarning($"Bilinmeyen stat adı: {statName}");
                break;
        }
    }
    
    public void AddAllStats(double amount)
 {
    physical += amount;
    mental += amount;
    perception += amount;
    spiritual += amount;
    luck += amount;
    social += amount;
    // Toplu bir güncelleme olduğunu bildirmek için "All" event'ini tetikle
    OnStatChanged?.Invoke("All", 0);
 }


    // Toplam stat değerlerini al (base + bonus)
    public double GetTotalPhysical() => physical + physicalBonus;
    public double GetTotalMental() => mental + mentalBonus;
    public double GetTotalPerception() => perception + perceptionBonus;
    public double GetTotalSpiritual() => spiritual + spiritualBonus;
    public double GetTotalLuck() => luck + luckBonus;
    public double GetTotalSocial() => social + socialBonus;

    // === PHYSICAL STAT ===
    public void AddPhysical(double amount, bool isPermanent = true)
    {
        if (isPermanent)
            physical += amount;
        else
            physicalBonus += amount;
        
        OnStatChanged?.Invoke("Physical", GetTotalPhysical());
        Debug.Log($"Physical: {GetTotalPhysical()} (Base: {physical}, Bonus: {physicalBonus})");
    }

    public void RemovePhysicalBonus(double amount)
    {
        physicalBonus = Math.Max(0, physicalBonus - amount);
        OnStatChanged?.Invoke("Physical", GetTotalPhysical());
    }

    // === MENTAL STAT ===
    public void AddMental(double amount, bool isPermanent = true)
    {
        if (isPermanent)
            mental += amount;
        else
            mentalBonus += amount;
        
        OnStatChanged?.Invoke("Mental", GetTotalMental());
        Debug.Log($"Mental: {GetTotalMental()} (Base: {mental}, Bonus: {mentalBonus})");
    }

    public void RemoveMentalBonus(double amount)
    {
        mentalBonus = Math.Max(0, mentalBonus - amount);
        OnStatChanged?.Invoke("Mental", GetTotalMental());
    }

    // === PERCEPTION STAT ===
    public void AddPerception(double amount, bool isPermanent = true)
    {
        if (isPermanent)
            perception += amount;
        else
            perceptionBonus += amount;
        
        OnStatChanged?.Invoke("Perception", GetTotalPerception());
        Debug.Log($"Perception: {GetTotalPerception()} (Base: {perception}, Bonus: {perceptionBonus})");
    }

    public void RemovePerceptionBonus(double amount)
    {
        perceptionBonus = Math.Max(0, perceptionBonus - amount);
        OnStatChanged?.Invoke("Perception", GetTotalPerception());
    }

    // === SPIRITUAL STAT ===
    public void AddSpiritual(double amount, bool isPermanent = true)
    {
        if (isPermanent)
            spiritual += amount;
        else
            spiritualBonus += amount;
        
        OnStatChanged?.Invoke("Spiritual", GetTotalSpiritual());
        Debug.Log($"Spiritual: {GetTotalSpiritual()} (Base: {spiritual}, Bonus: {spiritualBonus})");
    }

    public void RemoveSpiritualBonus(double amount)
    {
        spiritualBonus = Math.Max(0, spiritualBonus - amount);
        OnStatChanged?.Invoke("Spiritual", GetTotalSpiritual());
    }

    // === LUCK STAT ===
    public void AddLuck(double amount, bool isPermanent = true)
    {
        if (isPermanent)
            luck += amount;
        else
            luckBonus += amount;
        
        OnStatChanged?.Invoke("Luck", GetTotalLuck());
        Debug.Log($"Luck: {GetTotalLuck()} (Base: {luck}, Bonus: {luckBonus})");
    }

    public void RemoveLuckBonus(double amount)
    {
        luckBonus = Math.Max(0, luckBonus - amount);
        OnStatChanged?.Invoke("Luck", GetTotalLuck());
    }

    // === SOCIAL STAT ===
    public void AddSocial(double amount, bool isPermanent = true)
    {
        if (isPermanent)
            social += amount;
        else
            socialBonus += amount;
        
        OnStatChanged?.Invoke("Social", GetTotalSocial());
        Debug.Log($"Social: {GetTotalSocial()} (Base: {social}, Bonus: {socialBonus})");
    }

    public void RemoveSocialBonus(double amount)
    {
        socialBonus = Math.Max(0, socialBonus - amount);
        OnStatChanged?.Invoke("Social", GetTotalSocial());
    }

    // === TOPLU İŞLEMLER ===
    // Tüm bonusları sıfırla (ekipman çıkarınca)
    public void ClearAllBonuses()
    {
        physicalBonus = 0;
        mentalBonus = 0;
        perceptionBonus = 0;
        spiritualBonus = 0;
        luckBonus = 0;
        socialBonus = 0;

        OnStatChanged?.Invoke("All", 0);
        Debug.Log("Tüm bonuslar sıfırlandı");
    }

    // Belirli bir yüzde bonus ekle (buff için)
    public void AddPercentageBonus(string statName, double percentage)
    {
        double bonus = 0;
        switch (statName.ToLower())
        {
            case "physical":
                bonus = physical * (percentage / 100.0);
                physicalBonus += bonus;
                OnStatChanged?.Invoke("Physical", GetTotalPhysical());
                break;
            case "mental":
                bonus = mental * (percentage / 100.0);
                mentalBonus += bonus;
                OnStatChanged?.Invoke("Mental", GetTotalMental());
                break;
            // Diğer statlar için de aynı şekilde...
        }
        Debug.Log($"{statName} için %{percentage} bonus eklendi: +{bonus}");
    }

    public TextMeshProUGUI physicalText;
    public TextMeshProUGUI mentalText;
    public TextMeshProUGUI perceptionText;
    public TextMeshProUGUI spiritualText;
    public TextMeshProUGUI luckText;
    public TextMeshProUGUI socialText;

    public void UpdateStatUI()
{
        physicalText.text = physical.ToString();
        mentalText.text = mental.ToString();
        perceptionText.text = perception.ToString();
        spiritualText.text = spiritual.ToString();
        luckText.text = luck.ToString();
        socialText.text = social.ToString();
    // Diğerlerini de güncelleyin
}

    // Örneğin ModifyStat fonksiyonunuzun sonunda UpdateStatUI() çağırın
    public void ModifyPhysical(int amount)
    {
        physical += amount;
        UpdateStatUI();
    }

    public void ModifyMental(int amount)
    {
        mental += amount;
        UpdateStatUI();
    }

    public void ModifyPerception(int amount)
    {
        perception += amount;
        UpdateStatUI();
    }

    public void ModifySpiritual(int amount)
    {
        spiritual += amount;
        UpdateStatUI();
    }

    public void ModifyLuck(int amount)
    {
        luck += amount;
        UpdateStatUI();
    }

    public void ModifySocial(int amount)
    {
        social += amount;
        UpdateStatUI();
    }




    // Save/Load için stat verilerini al
    public StatData GetStatData()
    {
        return new StatData
        {
            physical = this.physical,
            mental = this.mental,
            perception = this.perception,
            spiritual = this.spiritual,
            luck = this.luck,
            social = this.social
        };
    }

    // Save/Load için stat verilerini yükle
    public void LoadStatData(StatData data)
    {
        this.physical = data.physical;
        this.mental = data.mental;
        this.perception = data.perception;
        this.spiritual = data.spiritual;
        this.luck = data.luck;
        this.social = data.social;

        OnStatChanged?.Invoke("All", 0);
    }
}

// Save/Load için veri yapısı
[System.Serializable]
public class StatData
{ 
    public double physical;
    public double mental;
    public double perception;
    public double spiritual;
    public double luck;
    public double social;
 }




