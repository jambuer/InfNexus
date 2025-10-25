using UnityEngine;
using System;
using JetBrains.Annotations; // Eski koddan geri eklendi (GetDebuggerDisplay için)

/// <summary>
/// Oyuncunun seviyesini, XP'sini ve stat puanlarını yönetir.
/// Seviye atlama mantığını ve ödüllerini işler.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// </summary>
public class LevelManager : MonoBehaviour, IGameDataSaveable<LevelSaveData>
{
    public static LevelManager Instance;
    [Header("Seviye Bilgileri")]
    public int currentLevel = 1;
    public double currentXP = 0;
    public double xpToNextLevel = 100;
    public int unspentStatPoints = 0;

    [Header("Seviye Atlama Ayarları")]
    public float xpMultiplier = 1.2f;
    public int statPointsPerLevel = 5;
    public float maxHealthPerLevel = 10f;
    public float maxEnergyPerLevel = 5f;
    public float maxManaPerLevel = 2f;

    // UI ve diğer sistemler için event'ler
    public event Action OnXPChanged;
    public event Action OnLevelUp;
    
    // Statik event (InteractableObject gibi diğer sistemlerin dinlemesi için)
    public static event Action OnPlayerLeveledUp;

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

    /// <summary>
    /// Oyuncuya XP ekler ve gerekirse seviye atlatır.
    /// </summary>
    public void AddXP(double amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        OnXPChanged?.Invoke();
        
        // Birden fazla seviye atlanabilmesi için 'while' döngüsü kullanılır
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    /// <summary>
    /// Dışarıdan (örn: ödül) harcanmamış stat puanı ekler.
    /// </summary>
    public void AddUnspentStatPoints(int amount)
    {
        unspentStatPoints += amount;
        OnLevelUp?.Invoke(); // UI'daki stat puanı göstergesini günceller
        OnXPChanged?.Invoke(); // Bazen aynı UI'da olabilir
    }

    /// <summary>
    /// Seviye atlama işlemini gerçekleştirir.
    /// </summary>
    private void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel *= xpMultiplier;
        unspentStatPoints += statPointsPerLevel;

        
        Debug.Log($"SEVİYE ATLADIN! Yeni Seviye: {currentLevel}. Dağıtılmamış Puan: {unspentStatPoints}");
        OnLevelUp?.Invoke();
        OnPlayerLeveledUp?.Invoke();
        OnXPChanged?.Invoke(); // Kalan XP'yi ve yeni hedefi UI'da güncelle
    }

    /// <summary>
    /// Belirtilen miktarda stat puanı harcar. Yeterli puan varsa true döner.
    /// </summary>
    public bool SpendStatPoint(int amountToSpend)
    {
        if (unspentStatPoints >= amountToSpend)
        {
            unspentStatPoints -= amountToSpend;
            OnLevelUp?.Invoke(); // UI'ı güncelle
            OnXPChanged?.Invoke(); // UI'ı güncelle
            return true;
        }
        return false;
    }

    // ====================================================================================================
    // KAYIT SİSTEMİ (GameDataManager UYUMLU)
    // ====================================================================================================

    /// <summary>
    /// GameDataManager'a kaydedilecek verileri toplar ve döndürür.
    /// Bu, GameSaveData.cs içindeki 'LevelSaveData' sınıfı ile eşleşmelidir.
    /// </summary>
    public LevelSaveData GetSaveData()
    {
        Debug.Log("LevelManager: Kayıt verisi oluşturuluyor.");
        return new LevelSaveData
        {
            currentLevel = this.currentLevel,
            currentXP = this.currentXP,
            xpToNextLevel = this.xpToNextLevel,
            unspentStatPoints = this.unspentStatPoints
        };
    }

    /// <summary>
    /// GameDataManager'dan gelen verileri bu yöneticiye yükler.
    /// </summary>
    public void LoadFromData(LevelSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("LevelManager LoadFromData: Yüklenecek veri bulunamadı (data == null).");
            return;
        }

        this.currentLevel = data.currentLevel;
        this.currentXP = data.currentXP;
        this.xpToNextLevel = data.xpToNextLevel;
        this.unspentStatPoints = data.unspentStatPoints;
        
        // Yükleme sonrası UI'ın güncellenmesi için event'leri tetikle
        OnLevelUp?.Invoke();
        OnXPChanged?.Invoke();
        Debug.Log($"LevelManager verisi yüklendi. Seviye: {currentLevel}");
    }

    // Eski koddan geri eklendi
    [UsedImplicitly]
    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}