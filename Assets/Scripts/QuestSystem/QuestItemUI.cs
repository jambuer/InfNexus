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
        PopulateRewardFields();

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
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp += OnPlayerStatsChanged;
        // YÜKSEK FREKANSLI (Kasma Sebebi) - YORUMA AL:
        // if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged += OnPlayerStatsChanged;
        // if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += OnPlayerStatsChanged;

        UpdateRequirementsText(); // Sayfa açıldığında günceller (İSTEĞİN 2)
    }


    //
    private void OnDisable()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp -= OnPlayerStatsChanged;
        // YÜKSEK FREKANSLI (Kasma Sebebi) - YORUMA AL:
        // if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged -= OnPlayerStatsChanged;
        // if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= OnPlayerStatsChanged;
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
        // [YENİ] Artık _questData.requirements bir 'List<Requirement>'.
        // Bu listeyi doğrudan merkezi formatlayıcımıza gönderiyoruz.
        // Not: RequirementTooltipFormatter.GetFormattedRequirementText fonksiyonunun
        // ikinci parametresinin 'string' olduğundan bir önceki adımda emin olmuştuk.
        
        // (Eğer başlık istemiyorsan ikinci parametreyi "" yapabilirsin)
        return RequirementTooltipFormatter.GetFormattedRequirementText(_questData.requirements, "Gereksinimler:");
    }

    // --- Kalan Fonksiyonların Aynı ---

    /// <summary>
    /// [YENİ] _questData.rewards listesini okur ve ilgili tüm UI metin alanlarını doldurur.
    /// </summary>
    // --- YENİ FONKSİYON 1 ---
// (QuestItemUI.cs'e eklenecek)

    /// <summary>
    /// [YENİ] _questData.rewards listesini okur ve ilgili tüm UI metin alanlarını doldurur.
    /// </summary>
    private void PopulateRewardFields()
    {
        // Metinleri sıfırla
        if (xpRewardText != null) xpRewardText.text = "";
        if (goldRewardText != null) goldRewardText.text = "";
        if (itemRewardsText != null) itemRewardsText.text = "";

        if (_questData.rewards == null || _questData.rewards.Count == 0)
        {
            if (itemRewardsText != null) itemRewardsText.text = "Ödül Yok";
            return;
        }

        StringBuilder xpSB = new StringBuilder();
        StringBuilder goldSB = new StringBuilder();
        StringBuilder itemSB = new StringBuilder();

        bool hasItems = false;
        bool hasGold = false;
        bool hasXP = false;

        foreach (GameReward reward in _questData.rewards)
        {
            switch (reward.rewardType)
            {
                case RewardType.XP:
                    xpSB.Append($"XP: {reward.amount:N0}");
                    hasXP = true;
                    break;
                case RewardType.Gold:
                    goldSB.Append($"Gold: {reward.amount:N0}");
                    hasGold = true;
                    break;
                case RewardType.Item:
                    string itemName = reward.itemData != null ? reward.itemData.itemName : reward.stringParameter;
                    itemSB.AppendLine($"- {itemName} (x{reward.amount:N0})");
                    hasItems = true;
                    break;
                // Diğer ödülleri de buraya ekleyebilirsin
            }
        }

        if (xpRewardText != null) xpRewardText.text = hasXP ? xpSB.ToString() : "";
        if (goldRewardText != null) goldRewardText.text = hasGold ? goldSB.ToString() : "";
        if (itemRewardsText != null) itemRewardsText.text = hasItems ? itemSB.ToString().TrimEnd() : "";
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

    /// <summary>
    /// Tek bir GameReward yapısını UI'da gösterilecek basit bir metne dönüştürür.
    /// </summary>
    private string FormatReward(GameReward reward)
    {
        //
        switch (reward.rewardType)
        {
            case RewardType.XP:
                return $"+{reward.amount:N0} XP";
            case RewardType.Gold:
                return $"+{reward.amount:N0} Altın";
            case RewardType.NexusCoin:
                return $"+{reward.amount:N0} Nexus Coin";
            case RewardType.People:
                return $"+{reward.amount:N0} Nüfus";
            case RewardType.Item:
                string itemName = reward.itemData != null ? reward.itemData.itemName : reward.stringParameter;
                return $"+{reward.amount:N0} {itemName}";
            case RewardType.Stat:
                return $"+{reward.amount} {reward.stringParameter}";
            case RewardType.Perk:
                return $"Perk: {reward.stringParameter}";
            default:
                return $"+{reward.amount:N0} {reward.rewardType}";
        }
    }

    
    

}