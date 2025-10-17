using UnityEngine;
using System;
using JetBrains.Annotations; // Event'ler için bu satır gerekli

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [Header("Seviye Bilgileri")]
    public int currentLevel = 1;
    public double currentXP = 0;
    public double xpToNextLevel = 100;
    public int unspentStatPoints = 0;

    [Header("Seviye Atlama Ayarları")]
    public float xpMultiplier = 1.2f; // Bir sonraki seviye için ne kadar daha fazla XP gerekeceğini belirler
    public int statPointsPerLevel = 5;
    public float maxHealthPerLevel = 10f;
    public float maxEnergyPerLevel = 5f;
    public float maxManaPerLevel = 2f;

    // Seviye atlandığında veya XP değiştiğinde UI'ı bilgilendirmek için event'ler
    public event Action OnXPChanged;
    public event Action OnLevelUp;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
            DontDestroyOnLoad(gameObject); // Sahne değişse bile bu obje kalıcı olsun
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Bu fonksiyon, görevlerden veya başka kaynaklardan XP eklemek için çağrılacak
    public void AddXP(double amount)
    {
        currentXP += amount;
        OnXPChanged?.Invoke(); // XP'nin değiştiğini UI'a bildir

        // Yeterli XP toplandıysa seviye atla (birden fazla seviye atlanabilir)
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // Kalan XP'yi bir sonraki seviyeye aktar
        currentXP -= xpToNextLevel;

        currentLevel++;

        // Bir sonraki seviye için gereken XP'yi artır
        xpToNextLevel *= xpMultiplier;

        // Ödülleri ver
        unspentStatPoints += statPointsPerLevel;

        

        // ResourceManager'daki maksimum değerleri artır
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.maxHealth += maxHealthPerLevel;
            ResourceManager.Instance.maxEnergy += maxEnergyPerLevel;
            ResourceManager.Instance.maxMana += maxManaPerLevel;

            // Can, enerji ve manayı tamamen doldur
            ResourceManager.Instance.currentHealth = ResourceManager.Instance.maxHealth;
            ResourceManager.Instance.currentEnergy = ResourceManager.Instance.maxEnergy;
            ResourceManager.Instance.currentMana = ResourceManager.Instance.maxMana;

            // ResourceManager'daki barların güncellenmesini tetikle
            ResourceManager.Instance.UpdateAllBars();
        }

        Debug.Log($"SEVİYE ATLADIN! Yeni Seviye: {currentLevel}. Dağıtılmamış Puan: {unspentStatPoints}");
        OnLevelUp?.Invoke(); // Seviye atlandığını UI'a bildir
        OnXPChanged?.Invoke(); // Kalan XP'yi de UI'da güncelle
    }

    public bool SpendStatPoint(int amountToSpend)
    {
        // Yeterli puan var mı diye kontrol et
        if (unspentStatPoints >= amountToSpend)
        {
            unspentStatPoints -= amountToSpend;
            OnLevelUp?.Invoke(); 
            OnXPChanged?.Invoke(); 
            return true; // Puanlar başarıyla harcandı
        }
        
        return false; // Harcanacak yeterli puan yok
    }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }