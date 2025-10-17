using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;

public class QuestItemUI : MonoBehaviour
{
    [Header("UI Referansları - Temel")]
    public Image questIcon;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI completionCountText;
    public Slider progressBar;
    public Button startButton;
    public Button autoButton;

    [Header("UI Referansları - Detaylar")]
    public TextMeshProUGUI tagsText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI goldRewardText;
    public TextMeshProUGUI itemRewardsText;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI xpRewardText;
    private TextMeshProUGUI _startButtonText;
    
    // --- YENİ EKLENENLER (Sadece Renk İçin) ---
    [Header("Renk Ayarları")]
    public Color metColor = Color.green;
    public Color notMetColor = Color.red;
    private string _metColorHex;
    private string _notMetColorHex;
    // --- BİTTİ ---

    private QuestData _questData;

    // Sadece renk kodlarını hazırlamak için Awake eklendi.
    private void Awake()
    {
        _metColorHex = ColorUtility.ToHtmlStringRGB(metColor);
        _notMetColorHex = ColorUtility.ToHtmlStringRGB(notMetColor);
    }

    public void Setup(QuestData questToSetup)
    {
        _questData = questToSetup;
        
        questIcon.sprite = _questData.questIcon;
        questNameText.text = _questData.questName;
        tagsText.text = $"[{_questData.mainTag}] - [{_questData.subTag}]";
        descriptionText.text = _questData.description;
        goldRewardText.text = FormatGoldReward();
        itemRewardsText.text = FormatItemRewards();
        xpRewardText.text = $"{_questData.experienceReward:F0} XP";

        // requirementsText.text = FormatRequirements(); satırı UpdateRequirementsText() ile değiştirildi
        UpdateRequirementsText();

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartButtonClicked);

        if (autoButton != null)
        {
            autoButton.gameObject.SetActive(_questData.automationData.canBeAutomated);
            autoButton.onClick.RemoveAllListeners();
            autoButton.onClick.AddListener(OnAutoButtonClicked);
        }

        QuestManager.Instance.OnQuestProgress += UpdateCompletionCount;
        QuestManager.Instance.OnQuestProgressUpdate += UpdateProgressBar;

        UpdateCompletionCount(questToSetup, QuestManager.Instance.GetCompletionCount(questToSetup.questID));

        if (QuestManager.Instance.IsQuestActive(_questData.questID))
        {
            if (_startButtonText != null) _startButtonText.text = "İptal Et";
        }
        else
        {
            progressBar.value = 0;
            if (_startButtonText != null) _startButtonText.text = "Başla";
        }
    }

    // --- YENİ EKLENENLER (Event Dinleme) ---
    private void OnEnable()
    {
        // Gerekli yöneticiler varsa, onların değişikliklerini dinlemeye başla.
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp += OnPlayerStatsChanged;
        if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged += OnPlayerStatsChanged;
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += OnPlayerStatsChanged;
        // İlk açılışta da bir kez güncelle
        UpdateRequirementsText();
    }

    private void OnDisable()
    {
        // Dinlemeyi bırak ki hata olmasın.
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp -= OnPlayerStatsChanged;
        if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged -= OnPlayerStatsChanged;
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= OnPlayerStatsChanged;
    }
    
    // Gelen herhangi bir değişiklik anonsunda, gereksinim metnini yeniden çiz.
    private void OnPlayerStatsChanged(int level, int statPoints) => UpdateRequirementsText();
    private void OnPlayerStatsChanged() => UpdateRequirementsText();
    private void OnPlayerStatsChanged(CurrencyType type, double amount) => UpdateRequirementsText();
    // --- BİTTİ ---

    private void UpdateProgressBar(string questID, float progress)
    {
        if (_questData == null || questID != _questData.questID) return;
        progressBar.value = progress;
    }

    private void UpdateRequirementsText()
    {
        if (_questData != null && requirementsText != null)
        {
            requirementsText.text = FormatRequirements();
        }
    }

    private string FormatRequirements()
    {
        // Bu metodun içeriği, senin çalışan kodundakiyle neredeyse aynı,
        // Sadece renk ekleme mantığı eklendi.
        QuestRequirements req = _questData.requirements;
        StringBuilder sb = new StringBuilder();
        bool hasRequirements = false;

        if (req.requiredLevel > 1)
        {
            bool isMet = LevelManager.Instance.currentLevel >= req.requiredLevel;
            sb.AppendLine($"<color=#{ (isMet ? _metColorHex : _notMetColorHex) }>- LVL {req.requiredLevel}</color>");
            hasRequirements = true;
        }
        if (req.requiredHealth > 0)
        {
            bool isMet = ResourceManager.Instance.currentHealth >= req.requiredHealth;
            sb.AppendLine($"<color=#{ (isMet ? _metColorHex : _notMetColorHex) }>- Health {req.requiredHealth:F0}</color>");
            hasRequirements = true;
        }
        if (req.requiredEnergy > 0)
        {
            bool isMet = ResourceManager.Instance.currentEnergy >= req.requiredEnergy;
            sb.AppendLine($"<color=#{ (isMet ? _metColorHex : _notMetColorHex) }>- Energy {req.requiredEnergy:F0}</color>");
            hasRequirements = true;
        }
        if (req.requiredGold > 0)
        {
            bool isMet = CurrencyManager.Instance.gold >= req.requiredGold;
            sb.AppendLine($"<color=#{ (isMet ? _metColorHex : _notMetColorHex) }>- Gold {req.requiredGold:F0}</color>");
            hasRequirements = true;
        }

        if (hasRequirements)
        {
            sb.Insert(0, "\n"); 
            return sb.ToString();
        }
        
        return "";
    }
    
    // --- Kalan Fonksiyonların Aynı ---
    
    private string FormatGoldReward()
    {
        if (_questData.goldRewardTiers == null || _questData.goldRewardTiers.Count == 0) return "Ödül: Yok";
        double minGold = double.MaxValue, maxGold = double.MinValue;
        foreach (var tier in _questData.goldRewardTiers)
        {
            if (tier.minAmount < minGold) minGold = tier.minAmount;
            if (tier.maxAmount > maxGold) maxGold = tier.maxAmount;
        }
        return minGold >= maxGold ? $"{minGold:F0}" : $"Gold: {minGold:F0} - {maxGold:F0}";
    }

    private string FormatItemRewards()
    {
        if (_questData.itemRewards == null || _questData.itemRewards.Count == 0) return "";
        StringBuilder sb = new StringBuilder("Olası Eşyalar:\n");
        foreach (var itemDrop in _questData.itemRewards)
        {
            if (itemDrop.itemToDrop != null)
            {
                sb.AppendLine($"- {itemDrop.itemToDrop.itemName} (%{itemDrop.dropChance * 100})");
            }
        }
        return sb.ToString();
    }
    
    private void UpdateCompletionCount(QuestData updatedQuest, int newCount)
    {
        if (_questData == null || updatedQuest.questID != _questData.questID) return;
        if (_startButtonText != null) _startButtonText.text = "Başla";

        progressBar.value = 0;
        // Eğer tamamlanma limiti varsa, "mevcut / limit" şeklinde göster

        if (_questData.completionLimit > 0)
        {
            completionCountText.text = $"{newCount} / {_questData.completionLimit}";
            if (newCount >= _questData.completionLimit)
            {
                startButton.interactable = false;
                if(autoButton != null) autoButton.interactable = false;
            }
        }
        else
        {
            completionCountText.text = $"x{newCount}";
        }
    }

    private void OnStartButtonClicked()
    {
        if (QuestManager.Instance.IsQuestActive(_questData.questID))
        {
            QuestManager.Instance.CancelQuest(_questData.questID);
            if (_startButtonText != null) _startButtonText.text = "Başla";
        }
        else
        {
            QuestManager.Instance.StartQuest(_questData);
            if (_startButtonText != null) _startButtonText.text = "İptal Et";
        }
    }
    
    private void OnAutoButtonClicked()
    {
        Debug.Log($"Otomasyon başlatılıyor: {_questData.questName}");
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestProgress -= UpdateCompletionCount;
            QuestManager.Instance.OnQuestProgressUpdate -= UpdateProgressBar;
        }
    }
}