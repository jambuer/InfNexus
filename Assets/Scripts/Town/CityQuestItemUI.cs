using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using System.Linq; // Null check için eklendi

public class CityQuestItemUI : MonoBehaviour
{
    [Header("UI Referansları - Temel")]
    public Image questIcon;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI completionCountText;
    public Slider progressBar;
    public Button startButton;
    public Button autoButton; // Bu null olabilir, kontrol ekleyelim

    [Header("UI Referansları - Detaylar")]
    public TextMeshProUGUI tagsText; // QuestData'da bu alanlar yok gibi duruyor
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI goldRewardText;
    public TextMeshProUGUI itemRewardsText;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI xpRewardText;
    private TextMeshProUGUI _startButtonText;

    [Header("Renk Ayarları")]
    public Color metColor = Color.green;
    public Color notMetColor = Color.red;
    private string _metColorHex;
    private string _notMetColorHex;

    // Düzeltme: Property'i doğru tanımlayalım
    public QuestData CurrentQuestData { get; private set; }
    // Not: Kodun içinde bazı yerlerde _questData kullanılmış, onu da CurrentQuestData ile değiştireceğiz.

    private void Awake()
    {
        _metColorHex = ColorUtility.ToHtmlStringRGB(metColor);
        _notMetColorHex = ColorUtility.ToHtmlStringRGB(notMetColor);
        if (startButton != null)
        {
            _startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void Setup(QuestData questToSetup)
    {
        CurrentQuestData = questToSetup; // Property'e ata
        if (CurrentQuestData == null)
        {
            Debug.LogError("Setup fonksiyonuna null QuestData geldi!", this);
            gameObject.SetActive(false); // Hatalı UI'ı gizle
            return;
        }

        gameObject.SetActive(true); // Önceden hata nedeniyle gizlenmiş olabilir.

        if (questIcon != null)
        {
            questIcon.sprite = CurrentQuestData.questIcon;
            questIcon.enabled = (questIcon.sprite != null);
        }
        if (questNameText != null) questNameText.text = CurrentQuestData.questName;
        // if (tagsText != null) tagsText.text = $"[{CurrentQuestData.mainTag}] - [{CurrentQuestData.subTag}]"; // Yorumlu kalsın
        if (tagsText != null) tagsText.text = ""; // Boş bırakalım
        if (descriptionText != null) descriptionText.text = CurrentQuestData.description;
        PopulateRewardFields();

        UpdateRequirementsText();

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (autoButton != null)
        {
            // autoButton.gameObject.SetActive(CurrentQuestData.automationData.canBeAutomated); // Yorumlu kalsın
            autoButton.gameObject.SetActive(false);
            autoButton.onClick.RemoveAllListeners();
            autoButton.onClick.AddListener(OnAutoButtonClicked);
        }

        // Event aboneliklerini buradan kaldıralım (Panel yönetecek)

        UpdateCompletionCount(CurrentQuestData, QuestManager.Instance != null ? QuestManager.Instance.GetCompletionCount(CurrentQuestData.questID) : 0);
        UpdateButtonState();
    }

    // Bu fonksiyon doğruydu, sadece CurrentQuestData kullanmalı
    public string GetQuestID()
    {
        return (CurrentQuestData != null) ? CurrentQuestData.questID : string.Empty;
    }

    private void OnEnable()
    {
        LevelManager.OnPlayerLeveledUp += OnPlayerStatsChanged;
        // YÜKSEK FREKANSLI (Kasma Sebebi) - YORUMA AL:
        // if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged += OnPlayerStatsChanged;
        // if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += OnPlayerStatsChanged;
        // if (StatManager.Instance != null) StatManager.Instance.OnStatChanged += OnStatManagerChanged;

        if (CurrentQuestData != null) UpdateUIOnStatChange(); // Sayfa açıldığında günceller (İSTEĞİN 2)
    }

    private void OnDisable()
    {
        LevelManager.OnPlayerLeveledUp -= OnPlayerStatsChanged;
        // YÜKSEK FREKANSLI (Kasma Sebebi) - YORUMA AL:
        // if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged -= OnPlayerStatsChanged;
        // if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= OnPlayerStatsChanged;
        // if (StatManager.Instance != null) StatManager.Instance.OnStatChanged -= OnStatManagerChanged;
    }


    // StatManager event'i için ayrı handler (parametreleri farklı)
    private void OnStatManagerChanged(string statName, double value) => UpdateUIOnStatChange();
    // Diğer handler'lar (parametreleri farklı veya yok)
    private void OnPlayerStatsChanged() => UpdateUIOnStatChange();
    private void OnPlayerStatsChanged(CurrencyType type, double amount) => UpdateUIOnStatChange();


    private void UpdateUIOnStatChange()
    {
        if (CurrentQuestData != null && gameObject.activeInHierarchy) // Aktifse güncelle
        {
             UpdateRequirementsText();
             UpdateButtonState();
        }
    }

    // Düzeltme: Public UpdateProgress fonksiyonu
    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
            // Aktif değilse veya bittiyse barı gizle/göster
            // progressBar.gameObject.SetActive(progress > 0f && progress < 1f);
        }
    }

    // Event'ten gelen progress güncellemesini işleyen private fonksiyon
    private void HandleProgressBarUpdateFromEvent(string questID, float progress)
    {
        if (CurrentQuestData == null || questID != CurrentQuestData.questID) return;
        UpdateProgress(progress); // Public olanı çağır
    }


    private void UpdateRequirementsText()
    {
        if (CurrentQuestData != null && requirementsText != null)
        {
            requirementsText.text = FormatRequirements();
        }
    }

    private string FormatRequirements()
    {
        // [YENİ] Merkezi formatlayıcıyı çağırıyoruz. Başlık istemediğimiz için "" gönderiyoruz.
        return RequirementTooltipFormatter.GetFormattedRequirementText(CurrentQuestData.requirements, "");
    }


    // --- YENİ FONKSİYON --- (CityQuestItemUI.cs'e eklenecek)

    /// <summary>
    /// [YENİ] CurrentQuestData.rewards listesini okur ve ilgili tüm UI metin alanlarını doldurur.
    /// </summary>
    private void PopulateRewardFields()
    {
        // Metinleri sıfırla
        if (xpRewardText != null) xpRewardText.text = "";
        if (goldRewardText != null) goldRewardText.text = "";
        if (itemRewardsText != null) itemRewardsText.text = "";

        if (CurrentQuestData.rewards == null || CurrentQuestData.rewards.Count == 0)
        {
            if (itemRewardsText != null) itemRewardsText.text = "Ödül Yok";
            return;
        }

        // Metin birleştiricileri hazırlayalım
        StringBuilder xpSB = new StringBuilder();
        StringBuilder goldSB = new StringBuilder();
        StringBuilder itemSB = new StringBuilder(); 

        bool hasItems = false;
        bool hasGold = false;
        bool hasXP = false;

        foreach (GameReward reward in CurrentQuestData.rewards)
        {
            switch (reward.rewardType)
            {
                case RewardType.XP:
                    xpSB.Append($"{reward.amount:N0} XP"); // xpRewardText için başlığa gerek yok
                    hasXP = true;
                    break;
                case RewardType.Gold:
                    goldSB.Append($"Altın: {reward.amount:N0}");
                    hasGold = true;
                    break;
                case RewardType.Item:
                    string itemName = reward.itemData != null ? reward.itemData.itemName : reward.stringParameter;
                    // Orijinal kod "Eşyalar:" başlığını kendi içinde ekliyordu
                    itemSB.AppendLine($"- {itemName} (x{reward.amount:N0})");
                    hasItems = true;
                    break;
                // Diğer ödülleri de buraya ekleyebilirsin
            }
        }

        // Metin alanlarını doldur
        if (xpRewardText != null) xpRewardText.text = hasXP ? xpSB.ToString() : "";
        if (goldRewardText != null) goldRewardText.text = hasGold ? goldSB.ToString() : "";
        
        // itemRewardsText alanı "Eşyalar:" başlığını bekliyorsa:
        if (itemRewardsText != null) 
        {
            if (hasItems)
            {
                itemSB.Insert(0, "Eşyalar:\n");
                itemRewardsText.text = itemSB.ToString().TrimEnd();
            }
            else
            {
                itemRewardsText.text = ""; // Eşya yoksa boş bırak
            }
        }
    }

    // QuestManager event'i için handler
    private void UpdateCompletionCount(QuestData updatedQuest, int newCount)
    {
        if (CurrentQuestData == null || updatedQuest.questID != CurrentQuestData.questID) return;

        if (completionCountText != null)
        {
            if (CurrentQuestData.completionLimit > 0)
            {
                completionCountText.text = $"{newCount} / {CurrentQuestData.completionLimit}";
            }
            else
            {
                completionCountText.text = $"x{newCount}";
            }
        }
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (startButton == null || CurrentQuestData == null || QuestManager.Instance == null) return; // QuestManager null kontrolü

        int currentCompletions = QuestManager.Instance.GetCompletionCount(CurrentQuestData.questID);
        bool limitReached = CurrentQuestData.completionLimit > 0 && currentCompletions >= CurrentQuestData.completionLimit;
        bool isActive = QuestManager.Instance.IsQuestActive(CurrentQuestData.questID);
        bool requirementsMet = CheckAllRequirementsMet();

        if (limitReached)
        {
            startButton.interactable = false;
            if (_startButtonText != null) _startButtonText.text = "Limit Dolu";
            if (autoButton != null) autoButton.interactable = false;
        }
        else if (isActive)
        {
            startButton.interactable = true;
            if (_startButtonText != null) _startButtonText.text = "İptal Et";
        }
        else
        {
            startButton.interactable = requirementsMet;
            if (_startButtonText != null) _startButtonText.text = "Başla";
        }
    }

    // Gereksinim kontrolü (kaynaklar dahil)
    // CityQuestItemUI.cs - CheckAllRequirementsMet YENİ İÇERİĞİ
    private bool CheckAllRequirementsMet()
    {
        // [YENİ] Merkezi doğrulayıcıyı (Validator) çağırıyoruz.
        return GameValidator.Instance.AreRequirementsMet(CurrentQuestData.requirements);
    }



    private void OnStartButtonClicked()
    {
        if (CurrentQuestData == null || QuestManager.Instance == null) return;

        if (QuestManager.Instance.IsQuestActive(CurrentQuestData.questID))
        {
            QuestManager.Instance.CancelQuest(CurrentQuestData.questID);
            // Buton durumu ve progress bar event ile güncellenecek
        }
        else
        {
            QuestManager.Instance.StartQuest(CurrentQuestData);
            // Buton durumu event ile güncellenecek
        }
    }

    private void OnAutoButtonClicked()
    {
        Debug.Log($"Otomasyon tıklandı: {CurrentQuestData?.questName}");
    }

    /// <summary>
    /// Tek bir GameReward yapısını UI'da gösterilecek basit bir metne dönüştürür.
    /// (Şu an PopulateRewardFields tarafından kullanılmıyor, ancak referans için durabilir)
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