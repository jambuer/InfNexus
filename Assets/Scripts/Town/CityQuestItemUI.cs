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
        if (goldRewardText != null) goldRewardText.text = FormatGoldReward();
        if (itemRewardsText != null) itemRewardsText.text = FormatItemRewards();
        if (xpRewardText != null) xpRewardText.text = $"{CurrentQuestData.experienceReward:F0} XP";

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
        // Düzeltme: Statik event'lere doğru şekilde abone ol
        LevelManager.OnPlayerLeveledUp += OnPlayerStatsChanged; // Instance olmadan eriş
        if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged += OnPlayerStatsChanged;
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += OnPlayerStatsChanged;
        // Düzeltme: StatManager event adı ve aboneliği
        if (StatManager.Instance != null) StatManager.Instance.OnStatChanged += OnStatManagerChanged; // Doğru event adı ve ayrı handler

        if (CurrentQuestData != null) // Setup yapıldıysa güncelle
        {
             UpdateUIOnStatChange();
        }
    }

    private void OnDisable()
    {
        // Düzeltme: Statik event aboneliğini doğru kaldır
        LevelManager.OnPlayerLeveledUp -= OnPlayerStatsChanged; // Instance olmadan eriş
        if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged -= OnPlayerStatsChanged;
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= OnPlayerStatsChanged;
        // Düzeltme: StatManager event adı ve abonelik iptali
        if (StatManager.Instance != null) StatManager.Instance.OnStatChanged -= OnStatManagerChanged; // Doğru event adı ve handler
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
         if (CurrentQuestData == null || CurrentQuestData.requirements == null) return "Gereksinim Yok";

        QuestRequirements req = CurrentQuestData.requirements;
        StringBuilder sb = new StringBuilder();
        bool hasRequirements = false;

        // Düzeltme: Manager Instance'larının null olup olmadığını kontrol et
        bool levelMet = (LevelManager.Instance != null && LevelManager.Instance.currentLevel >= req.requiredLevel);
        bool healthMet = (ResourceManager.Instance != null && ResourceManager.Instance.currentHealth >= req.requiredHealth);
        bool energyMet = (ResourceManager.Instance != null && ResourceManager.Instance.currentEnergy >= req.requiredEnergy); // Basit kontrol
        bool goldMet = (CurrencyManager.Instance != null && CurrencyManager.Instance.gold >= req.requiredGold);
        bool manaMet = (ResourceManager.Instance != null && ResourceManager.Instance.currentMana >= req.requiredMana);
        bool nexusMet = (CurrencyManager.Instance != null && CurrencyManager.Instance.nexusCoin >= req.requiredNexusCoin);
        bool physicalMet = (StatManager.Instance != null && StatManager.Instance.GetTotalPhysical() >= req.requiredPhysical);
        bool mentalMet = (StatManager.Instance != null && StatManager.Instance.GetTotalMental() >= req.requiredMental);
        bool spiritualMet = (StatManager.Instance != null && StatManager.Instance.GetTotalSpiritual() >= req.requiredSpiritual);
        bool perceptionMet = (StatManager.Instance != null && StatManager.Instance.GetTotalPerception() >= req.requiredPerception);
        bool luckMet = (StatManager.Instance != null && StatManager.Instance.GetTotalLuck() >= req.requiredLuck);
        bool socialMet = (StatManager.Instance != null && StatManager.Instance.GetTotalSocial() >= req.requiredSocial);


        // ... (Gereksinim metnini oluşturma kısmı aynı kalabilir, yukarıdaki bool'ları kullanır) ...
         if (req.requiredLevel > 1) { sb.AppendLine($"<color=#{ (levelMet ? _metColorHex : _notMetColorHex) }>- LVL {req.requiredLevel}</color>"); hasRequirements = true; }
         if (req.requiredHealth > 0) { sb.AppendLine($"<color=#{ (healthMet ? _metColorHex : _notMetColorHex) }>- Health {req.requiredHealth:F0}</color>"); hasRequirements = true; }
         if (req.requiredEnergy > 0) { sb.AppendLine($"<color=#{ (energyMet ? _metColorHex : _notMetColorHex) }>- Energy {req.requiredEnergy:F0}</color>"); hasRequirements = true; } // Enerji maliyeti için daha doğru hesaplama gerekebilir
         if (req.requiredGold > 0) { sb.AppendLine($"<color=#{(goldMet ? _metColorHex : _notMetColorHex)}>- Gold {req.requiredGold:F0}</color>"); hasRequirements = true; }
         if (req.requiredMana > 0) { sb.AppendLine($"<color=#{(manaMet ? _metColorHex : _notMetColorHex)}>- Mana {req.requiredMana:F0}</color>"); hasRequirements = true; }
         if (req.requiredNexusCoin > 0) { sb.AppendLine($"<color=#{(nexusMet ? _metColorHex : _notMetColorHex)}>- Nexus Coin {req.requiredNexusCoin:F0}</color>"); hasRequirements = true; }
         if (req.requiredPhysical > 0) { sb.AppendLine($"<color=#{ (physicalMet ? _metColorHex : _notMetColorHex) }>- Physical: {req.requiredPhysical:F0}</color>"); hasRequirements = true; }
         if (req.requiredMental > 0) { sb.AppendLine($"<color=#{ (mentalMet ? _metColorHex : _notMetColorHex) }>- Mental: {req.requiredMental:F0}</color>"); hasRequirements = true; }
         if (req.requiredSpiritual > 0) { sb.AppendLine($"<color=#{ (spiritualMet ? _metColorHex : _notMetColorHex) }>- Spiritual: {req.requiredSpiritual:F0}</color>"); hasRequirements = true; }
         if (req.requiredPerception > 0) { sb.AppendLine($"<color=#{ (perceptionMet ? _metColorHex : _notMetColorHex) }>- Perception: {req.requiredPerception:F0}</color>"); hasRequirements = true; }
         if (req.requiredLuck > 0) { sb.AppendLine($"<color=#{ (luckMet ? _metColorHex : _notMetColorHex) }>- Luck: {req.requiredLuck:F0}</color>"); hasRequirements = true; }
         if (req.requiredSocial > 0) { sb.AppendLine($"<color=#{ (socialMet ? _metColorHex : _notMetColorHex) }>- Social: {req.requiredSocial:F0}</color>"); hasRequirements = true; }


        return hasRequirements ? sb.ToString().TrimEnd() : "Gereksinim Yok";
    }

    private string FormatGoldReward()
    {
         if (CurrentQuestData == null || CurrentQuestData.goldRewardTiers == null || CurrentQuestData.goldRewardTiers.Count == 0) return "Altın: Yok";
         // ... (içerik aynı) ...
         double minGold = double.MaxValue, maxGold = double.MinValue;
         bool isRange = false;
         foreach (var tier in CurrentQuestData.goldRewardTiers)
         {
             if (tier.minAmount < minGold) minGold = tier.minAmount;
             if (tier.maxAmount > maxGold) maxGold = tier.maxAmount;
             if (tier.minAmount != tier.maxAmount) isRange = true;
         }
         if (!isRange && CurrentQuestData.goldRewardTiers.Count > 1) { isRange = CurrentQuestData.goldRewardTiers.Any(t => t.minAmount != minGold || t.maxAmount != maxGold); }
         return isRange ? $"Altın: {minGold:F0} - {maxGold:F0}" : $"Altın: {minGold:F0}";
    }

    private string FormatItemRewards()
    {
         if (CurrentQuestData == null || CurrentQuestData.itemRewards == null || CurrentQuestData.itemRewards.Count == 0) return "";
         // ... (içerik aynı) ...
         StringBuilder sb = new StringBuilder("Eşyalar:\n");
         foreach (var itemDrop in CurrentQuestData.itemRewards)
         {
             if (itemDrop.itemToDrop != null)
             {
                 if (itemDrop.dropChance >= 1f) { sb.AppendLine($"- {itemDrop.itemToDrop.itemName} (x{itemDrop.amount})"); }
                 else { sb.AppendLine($"- {itemDrop.itemToDrop.itemName} (x{itemDrop.amount}, %{itemDrop.dropChance * 100:F0})"); }
             }
         }
         return sb.ToString().TrimEnd();
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
    private bool CheckAllRequirementsMet()
    {
         if (CurrentQuestData == null || CurrentQuestData.requirements == null) return true;
         QuestRequirements req = CurrentQuestData.requirements;

         // Null kontrolleri eklendi
         if (LevelManager.Instance != null && LevelManager.Instance.currentLevel < req.requiredLevel) return false;
         if (ResourceManager.Instance != null)
         {
             if (ResourceManager.Instance.currentHealth < req.requiredHealth) return false;
             // Enerji maliyetini de kontrol et (daha doğru hesaplama gerekebilir)
             if (ResourceManager.Instance.currentEnergy < req.requiredEnergy) return false;
             if (ResourceManager.Instance.currentMana < req.requiredMana) return false;
         } else if (req.requiredHealth > 0 || req.requiredEnergy > 0 || req.requiredMana > 0) return false; // Kaynak yöneticisi yoksa ve gerekiyorsa false

         if (CurrencyManager.Instance != null)
         {
             if (CurrencyManager.Instance.gold < req.requiredGold) return false;
             if (CurrencyManager.Instance.nexusCoin < req.requiredNexusCoin) return false;
         } else if (req.requiredGold > 0 || req.requiredNexusCoin > 0) return false;

         if (StatManager.Instance != null)
         {
             if (StatManager.Instance.GetTotalPhysical() < req.requiredPhysical) return false;
             if (StatManager.Instance.GetTotalMental() < req.requiredMental) return false;
             if (StatManager.Instance.GetTotalSpiritual() < req.requiredSpiritual) return false;
             if (StatManager.Instance.GetTotalPerception() < req.requiredPerception) return false;
             if (StatManager.Instance.GetTotalLuck() < req.requiredLuck) return false;
             if (StatManager.Instance.GetTotalSocial() < req.requiredSocial) return false;
         } else if (req.requiredPhysical > 0 || req.requiredMental > 0 /* ... diğer statlar ... */) return false;

         return true;
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
}