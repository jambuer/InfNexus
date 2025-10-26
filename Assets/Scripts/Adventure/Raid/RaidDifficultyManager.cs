using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// DifficultyManager'dan kopyalandı ve Raid içeriği için uyarlandı
public class RaidDifficultyManager : MonoBehaviour
{
    // Raid içeriği için ayrı bir Singleton yapısı
    public static RaidDifficultyManager Instance { get; private set; }

    // Zorluk seviyelerini tanımlayan enum (DifficultyManager ile aynı)
    public enum Difficulty { Easy, Normal, Hard, VeryHard, Nightmare }

    // NOT: Bu script'in UI butonları (varsa) Raid panelindeki ilgili butonlar olmalı.
    // Şimdilik genel bir liste bırakıyoruz, daha sonra düşman bazlı dropdown'lar için
    // farklı bir yapı gerekebilir (örn: her düşman UI'ı kendi dropdown'ını yönetir).
    [Header("Raid Difficulty Controls (Opsiyonel Genel Butonlar)")]
    public List<Button> difficultyButtons; // Veya her düşman için ayrı Dropdown kullanılacaksa bu liste boş kalabilir.

    // Raid içeriğinin mevcut genel zorluğunu tutar.
    // Düşman bazlı zorluk için bu değişken yerine düşman verisinde zorluk tutulabilir.
    public Difficulty currentGlobalRaidDifficulty { get; private set; } = Difficulty.Easy;

    // Zorluk değiştiğinde (genel veya düşman bazlı) event tetiklenebilir.
    // Şimdilik genel bir event tanımlayalım.
    public event Action<Difficulty> OnGlobalRaidDifficultyChanged;
    // Düşman bazlı zorluk değişimi için event: public event Action<EnemyData, Difficulty> OnEnemyDifficultyChanged;

    void Awake()
    {
        // Singleton yapısı
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Raid paneliyle birlikte yönetiliyorsa bu gerekmeyebilir.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Eğer genel zorluk butonları kullanılacaksa:
        if (difficultyButtons != null && difficultyButtons.Count > 0)
        {
            for (int i = 0; i < difficultyButtons.Count; i++)
            {
                int difficultyIndex = i;
                difficultyButtons[i].onClick.AddListener(() => SetGlobalRaidDifficulty((Difficulty)difficultyIndex));
            }
            UpdateSelectedButtonVisuals(); // Başlangıç görselini ayarla
        }
    }

    /// <summary>
    /// Raid içeriğinin genel zorluğunu ayarlar (Eğer genel butonlar kullanılıyorsa).
    /// </summary>
    public void SetGlobalRaidDifficulty(Difficulty newDifficulty)
    {
        if (currentGlobalRaidDifficulty == newDifficulty) return;

        currentGlobalRaidDifficulty = newDifficulty;
        Debug.Log("Genel Raid Zorluk Seviyesi Değişti: " + newDifficulty);

        OnGlobalRaidDifficultyChanged?.Invoke(currentGlobalRaidDifficulty);
        UpdateSelectedButtonVisuals();
    }

    /// <summary>
    /// Belirli bir düşmanın zorluğunu ayarlar (Bu fonksiyon düşman UI script'inden çağrılacak).
    /// </summary>
    /// <param name="enemyData">Zorluğu değiştirilen düşman verisi (henüz oluşturulmadı).</param>
    /// <param name="newDifficulty">Yeni zorluk seviyesi.</param>
    public void SetEnemyDifficulty(/* EnemyData enemyData, */ Difficulty newDifficulty)
    {
        // TODO: Düşman verisi (EnemyData) oluşturulduğunda bu fonksiyonu güncelle.
        // enemyData.currentDifficulty = newDifficulty;
        Debug.Log($"Düşman [Düşman Adı] zorluğu ayarlandı: {newDifficulty}");
        // OnEnemyDifficultyChanged?.Invoke(enemyData, newDifficulty);
        // İlgili düşmanın UI'ını güncellemek için event tetiklenebilir veya doğrudan UI referansı güncellenebilir.
    }


    /// <summary>
    /// Genel zorluk butonlarının görsellerini günceller (Eğer kullanılıyorsa).
    /// </summary>
    void UpdateSelectedButtonVisuals()
    {
        if (difficultyButtons == null || difficultyButtons.Count == 0) return;

        for (int i = 0; i < difficultyButtons.Count; i++)
        {
            if(difficultyButtons[i] == null) continue; // Buton atanmamışsa geç

            var colors = difficultyButtons[i].colors;
            colors.colorMultiplier = (i == (int)currentGlobalRaidDifficulty) ? 1f : 0.7f;
            difficultyButtons[i].colors = colors;
        }
    }
}