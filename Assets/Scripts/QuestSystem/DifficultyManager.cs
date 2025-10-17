using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    // Zorluk seviyelerini tanımlayan enum
    public enum Difficulty { Easy, Normal, Hard, VeryHard, Nightmare }

    [Header("Difficulty Buttons")]
    public List<Button> difficultyButtons; // Inspector'a 5 butonu sürükle

    public Difficulty currentDifficulty { get; private set; } = Difficulty.Easy;

    // Zorluk değiştiğinde diğer script'lere haber vermek için event
    public event Action<Difficulty> OnDifficultyChanged;

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
        // Butonlara tıklama olaylarını ata
        for (int i = 0; i < difficultyButtons.Count; i++)
        {
            int difficultyIndex = i; // Lambda expression için bu gerekli
            difficultyButtons[i].onClick.AddListener(() => SetDifficulty((Difficulty)difficultyIndex));
        }

        // Başlangıçta seçili olanı vurgula
        UpdateSelectedButton();
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        if (currentDifficulty == newDifficulty) return;

        currentDifficulty = newDifficulty;
        Debug.Log("Zorluk seviyesi değişti: " + newDifficulty);

        // Değişikliği anons et
        OnDifficultyChanged?.Invoke(currentDifficulty);

        UpdateSelectedButton();
    }

    void UpdateSelectedButton()
    {
        for (int i = 0; i < difficultyButtons.Count; i++)
        {
            // Seçili butonu farklı bir renkte göster (örneğin)
            // Bu kısım butonların 'colors' ayarına göre değişir.
            var colors = difficultyButtons[i].colors;
            colors.colorMultiplier = (i == (int)currentDifficulty) ? 1f : 0.7f; // Seçili olan parlak, diğerleri soluk
            difficultyButtons[i].colors = colors;
        }
    }
}