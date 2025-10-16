using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public Slider xpSlider;
    public TextMeshProUGUI unspentPointsText;

    void Start()
    {
        // LevelManager'daki event'lere abone ol
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnXPChanged += UpdateXPUI;
            LevelManager.Instance.OnLevelUp += UpdateLevelUI;
        }

        // Başlangıçta tüm UI'ı bir kez güncelle
        UpdateLevelUI();
        UpdateXPUI();
    }

    void OnDestroy()
    {
        // Obje yok olduğunda event aboneliklerini iptal et (hafıza sızıntısını önler)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnXPChanged -= UpdateXPUI;
            LevelManager.Instance.OnLevelUp -= UpdateLevelUI;
        }
    }

    void UpdateLevelUI()
    {
        if (LevelManager.Instance == null) return;
        
        levelText.text = $"Lvl: {LevelManager.Instance.currentLevel}";
        unspentPointsText.text = $"SP: {LevelManager.Instance.unspentStatPoints}";
    }

    void UpdateXPUI()
    {
        if (LevelManager.Instance == null) return;

        // Slider ve Text'i güncelle
        xpSlider.maxValue = (float)LevelManager.Instance.xpToNextLevel;
        xpSlider.value = (float)LevelManager.Instance.currentXP;
        xpText.text = $"{LevelManager.Instance.currentXP:F0} / {LevelManager.Instance.xpToNextLevel:F0}";
    }
}