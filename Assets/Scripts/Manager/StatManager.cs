using UnityEngine;
using System;
using TMPro;

/// <summary>
/// Oyuncunun temel (base) stat'larını yönetir, bonusları ekler/çıkarır.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// </summary>
public class StatManager : MonoBehaviour, IGameDataSaveable<StatSaveData>
{
    public static StatManager Instance;

    [Header("Base Stats (Kaydedilen Değerler)")]
    public double physical = 0;
    public double mental = 0;
    public double perception = 0;
    public double spiritual = 0;
    public double luck = 0;
    public double social = 0;

    [Header("Bonus Stats (Geçici - Kaydedilmez)")]
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

    /// <summary>
    /// Gelen stat ismine göre ilgili statı kalıcı olarak artırır.
    /// QuestManager gibi sistemlerden gelen genel stat ödüllerini dağıtmak için kullanılır.
    /// </summary>
    public void AddStat(string statName, double amount)
    {
        switch (statName)
        {
            case "Physical":
                AddPhysical(amount, true);
                break;
            case "Mental":
                AddMental(amount, true);
                break;
            case "Perception":
                AddPerception(amount, true);
                break;
            case "Spiritual":
                AddSpiritual(amount, true);
                break;
            case "Luck":
                AddLuck(amount, true);
                break;
            case "Social":
                AddSocial(amount, true);
                break;
            default:
                Debug.LogWarning($"AddStat: Bilinmeyen stat adı: {statName}");
                break;
        }
    }

    /// <summary>
    /// Seviye atlandığında harcanan stat puanlarını kalıcı (base) stat'lara ekler.
    /// </summary>
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

    /// <summary>
    /// Tüm kalıcı (base) stat'lara belirtilen miktarı ekler.
    /// </summary>
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

    // ====================================================================================================
    // STAT ALMA (GET) METOTLARI
    // ====================================================================================================

    // Toplam stat değerlerini al (base + bonus)
    public double GetTotalPhysical() => physical + physicalBonus;
    public double GetTotalMental() => mental + mentalBonus;
    public double GetTotalPerception() => perception + perceptionBonus;
    public double GetTotalSpiritual() => spiritual + spiritualBonus;
    public double GetTotalLuck() => luck + luckBonus;
    public double GetTotalSocial() => social + socialBonus;

    /// <summary>
    /// Gelen stat ismine göre toplam (base + bonus) değeri döndürür.
    /// </summary>
    public float GetTotalStat(string statName)
    {
        // Gelen statName'e göre ilgili toplam değeri döndür
        switch (statName.ToLower()) // Küçük/büyük harf duyarsız yapalım
        {
            case "physical": return (float)GetTotalPhysical();
            case "mental": return (float)GetTotalMental();
            case "spiritual": return (float)GetTotalSpiritual();
            case "perception": return (float)GetTotalPerception();
            case "luck": return (float)GetTotalLuck();
            case "social": return (float)GetTotalSocial();
            default:
                Debug.LogWarning($"Bilinmeyen stat adı: {statName}");
                return 0; // Bilinmiyorsa 0 döndür
        }
    }

    // ====================================================================================================
    // BİREYSEL STAT DEĞİŞTİRME (ADD/REMOVE)
    // ====================================================================================================

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

    // ====================================================================================================
    // TOPLU İŞLEMLER VE BONUSLAR
    // ====================================================================================================

    /// <summary>
    /// Tüm geçici bonusları sıfırlar (örn: ekipman çıkarınca).
    /// </summary>
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

    /// <summary>
    /// Belirli bir stat'a, base stat üzerinden yüzdesel bonus ekler (örn: buff için).
    /// </summary>
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

    // ====================================================================================================
    // UI İŞLEMLERİ (Arayüz Bağlantıları)
    // ====================================================================================================

    [Header("UI Referansları (Opsiyonel)")]
    public TextMeshProUGUI physicalText;
    public TextMeshProUGUI mentalText;
    public TextMeshProUGUI perceptionText;
    public TextMeshProUGUI spiritualText;
    public TextMeshProUGUI luckText;
    public TextMeshProUGUI socialText;

    /// <summary>
    /// UI'daki tüm stat metinlerini günceller.
    /// </summary>
    public void UpdateStatUI()
    {
        if (physicalText != null) physicalText.text = physical.ToString();
        if (mentalText != null) mentalText.text = mental.ToString();
        if (perceptionText != null) perceptionText.text = perception.ToString();
        if (spiritualText != null) spiritualText.text = spiritual.ToString();
        if (luckText != null) luckText.text = luck.ToString();
        if (socialText != null) socialText.text = social.ToString();
    }

    // Bu 'Modify' metotları muhtemelen UI butonları içindir.
    // 'Addstatpoint' ile aynı işi yapıyorlar, ancak 'int' alıyorlar.
    // Fonksiyonellik 'Addstatpoint' ile çakışsa da, eski kodda olduğu için korundu.
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
    
    // ====================================================================================================
    // KAYIT SİSTEMİ (GameDataManager UYUMLU)
    // ====================================================================================================

    /// <summary>
    /// GameDataManager'a kaydedilecek verileri toplar ve döndürür.
    /// Bu, GameSaveData.cs içindeki 'StatSaveData' sınıfı ile eşleşmelidir.
    /// SADECE 'base' stat'lar kaydedilir, 'bonus' stat'lar kaydedilmez.
    /// </summary>
    public StatSaveData GetSaveData()
    {
        return new StatSaveData
        {
            physical = this.physical,
            mental = this.mental,
            perception = this.perception,
            spiritual = this.spiritual,
            luck = this.luck,
            social = this.social
        };
    }

    /// <summary>
    /// GameDataManager'dan gelen verileri bu yöneticiye yükler.
    /// </summary>
    public void LoadFromData(StatSaveData data)
    {
        if (data == null) 
        {
            Debug.LogWarning("StatManager LoadFromData: Yüklenecek veri bulunamadı (data == null).");
            return;
        }

        this.physical = data.physical;
        this.mental = data.mental;
        this.perception = data.perception;
        this.spiritual = data.spiritual;
        this.luck = data.luck;
        this.social = data.social;

        // Bonuslar geçici olduğu için yükleme sonrası sıfırlanır.
        ClearAllBonuses(); 

        // Tüm stat'ların yüklendiğini ve UI'ın güncellenmesi gerektiğini bildirir.
        OnStatChanged?.Invoke("All", 0); 
        UpdateStatUI(); // UI metinlerini de doğrudan güncelle
        Debug.Log("StatManager verisi yüklendi.");
    }
}