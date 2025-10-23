using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq; // `Any` gibi fonksiyonlar için eklendi (veya gerekmeyebilir)

// Bu enum'u ya buraya ya da ExplorerPerkData.cs'e (class'ın dışına) taşıyalım.
// Zaten ExplorerPerkData.cs'de varsa bu bloğu silebilirsin.


public class ExplorerPerkUI : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI rewardText;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;
    public TextMeshProUGUI timerText;

    [Header("Grup Panelleri")]
    public GameObject lockedPanel;
    public GameObject unlockedPanel;
    public GameObject priceRewardSection; 
    public GameObject requirementsSection; 

    [Header("Renk Ayarları")]
    public Color metColor = Color.green;
    public Color notMetColor = Color.red;

    // Perk verisini public yapalım ki Manager dışarıdan okuyabilsin (gerekirse)
    public ExplorerPerkData _perkData { get; private set; }
    private PerkState _currentState;
    private bool _isTimerTask = false;

    private string _metColorHex;
    private string _notMetColorHex;

    private void Awake()
    {
        _metColorHex = ColorUtility.ToHtmlStringRGB(metColor);
        _notMetColorHex = ColorUtility.ToHtmlStringRGB(notMetColor);
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }
    }

    /// <summary>
/// Zamanlayıcı görünümünü açar/kapatır.
/// </summary>
public void SetActiveTimerView(bool isTimerRunning)
{
    // Zamanlayıcı çalışırken butonu gizle, metni göster
    if (actionButton != null) actionButton.gameObject.SetActive(!isTimerRunning);
    if (timerText != null) timerText.gameObject.SetActive(isTimerRunning);
}

/// <summary>
/// Zamanlayıcı metnini günceller (SS:DD:SS formatında).
/// </summary>
public void UpdateTimerText(float remainingSeconds)
{
    if (timerText == null) return;
    System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(remainingSeconds);
    timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                    (int)timeSpan.TotalHours,
                                    timeSpan.Minutes,
                                    timeSpan.Seconds);
}

    /// <summary>
    /// Bu UI öğesini ExplorerPerkData ve başlangıç durumu ile kurar.
    /// </summary>
    public void Setup(ExplorerPerkData data, PerkState initialState)
    {
        _perkData = data;
        if (_perkData == null) { Destroy(gameObject); return; }

        if (descriptionText != null)
            descriptionText.text = _perkData.description;

        _isTimerTask = _perkData.tag == ExplorerTag.ExplorerTime;

        // UI'ı ayarla
        SetState(initialState);
    }

    /// <summary>
    /// UI'ı mevcut duruma göre günceller.
    /// </summary>
    public void SetState(PerkState newState)
    {
        _currentState = newState;
        
        lockedPanel.SetActive(newState == PerkState.Locked);
        unlockedPanel.SetActive(newState == PerkState.Unlockable || newState == PerkState.Payable);

        if (newState == PerkState.Locked)
        {
            return;
        }
        
        bool requirementsMet;
        switch (newState)
        {
            case PerkState.Unlockable: // Aşama 2: UNLOCK
                requirementsSection.SetActive(true);
                priceRewardSection.SetActive(false); 
                
                requirementsMet = AreRequirementsMet(_perkData.unlockRequirements);
                requirementsText.text = FormatRequirements(_perkData.unlockRequirements, false, "<b>Kilidi Açmak İçin:</b>");
                
                actionButton.gameObject.SetActive(true);
                actionButtonText.text = "UNLOCK";
                actionButton.interactable = requirementsMet;
                break;

            case PerkState.Payable: // Aşama 3: PAY / START
                requirementsSection.SetActive(true);
                priceRewardSection.SetActive(true); 

                requirementsText.text = FormatRequirements(_perkData.unlockRequirements, true, "<b>Kilit Açıldı:</b>"); 
                rewardText.text = $"<b>Ödül:</b> {(_perkData.reward != null ? _perkData.reward.description : "Yok")}";
                
                requirementsMet = AreRequirementsMet(_perkData.purchasePrice);
                requirementsText.text += "\n" + FormatRequirements(_perkData.purchasePrice, false, "<b>Ödenecek Maliyet:</b>");

                actionButton.gameObject.SetActive(true);
                actionButtonText.text = _isTimerTask ? "START" : "PAY";
                actionButton.interactable = requirementsMet; 
                
                SetActiveTimerView(false); // Zamanlayıcı görünümünü kapat
                break;
        }
    }
    
    private void OnActionButtonClicked()
    {
        if (_perkData == null || ExplorerManager.Instance == null) return;

        switch (_currentState)
        {
            case PerkState.Unlockable:
                if (AreRequirementsMet(_perkData.unlockRequirements))
                {
                    SetState(PerkState.Payable); // Aşama 3'e geçir
                }
                break;
                
            case PerkState.Payable:
                actionButton.interactable = false; // Çift tıklamayı önle
                
                if (_isTimerTask)
                {
                    SetActiveTimerView(true); // Zamanlayıcı görünümünü aç
                    ExplorerManager.Instance.StartExplorerTimer(_perkData, this);
                }
                else
                {
                    ExplorerManager.Instance.PurchasePerk(_perkData);
                }
                break;
        }
    }

    public void ResetButton()
    {
        SetActiveTimerView(false); // Zamanlayıcı görünümünü kapat
        if (_currentState == PerkState.Payable)
        {
            actionButton.interactable = AreRequirementsMet(_perkData.purchasePrice);
        }
    }

    #region Gereksinim Kontrolü (Sorun 2 Çözümü)

    // Bu fonksiyonlar, ExplorerManager'daki yardımcı fonksiyonların BİREBİR AYNISIDIR.
    // Bu, UI'ın Manager ile aynı kontrolleri yapmasını sağlar.

    private bool AreRequirementsMet(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return true;
        foreach (Requirement req in requirements)
        {
            if (!IsRequirementMet(req)) return false;
        }
        return true;
    }
    
    private bool IsRequirementMet(Requirement req)
    {
        // Manager'ların null olma ihtimaline karşı güvenli kontrol yap
        switch (req.requirementType.ToLower())
        {
            case "level":
                return (LevelManager.Instance != null) && LevelManager.Instance.currentLevel >= req.requiredValue;
            case "quest":
                return (QuestManager.Instance != null) && QuestManager.Instance.GetCompletionCount(req.requirementName) > 0;
            case "item":
                ItemData item = (ItemManager.Instance != null) ? ItemManager.Instance.GetItemByName(req.requirementName) : null;
                return (item != null && Inventory.Instance != null) && Inventory.Instance.HasItem(item, req.requiredValue);
            case "stat":
                return (StatManager.Instance != null) && StatManager.Instance.GetTotalStat(req.requirementName) >= req.requiredValue;
            case "gold":
                return (CurrencyManager.Instance != null) && CurrencyManager.Instance.gold >= req.requiredValue;
            case "nexuscoin":
                return (CurrencyManager.Instance != null) && CurrencyManager.Instance.nexusCoin >= req.requiredValue;
            case "people":
                // CurrencyManager'daki değişkenin adını 'people' olarak varsayıyoruz
                return (CurrencyManager.Instance != null) && CurrencyManager.Instance.people >= req.requiredValue;
            case "health": 
                return (ResourceManager.Instance != null) && ResourceManager.Instance.currentHealth > req.requiredValue;
            case "energy":
                return (ResourceManager.Instance != null) && ResourceManager.Instance.currentEnergy >= req.requiredValue;
            case "mana":
                return (ResourceManager.Instance != null) && ResourceManager.Instance.currentMana >= req.requiredValue; 
            case "maxhealth":
            // Eğer -10 gibi bir maliyetse, yeterli max health'imiz var mı?
            if (req.requiredValue < 0)
                return (ResourceManager.Instance != null) && (ResourceManager.Instance.maxHealth + req.requiredValue) >= 1;
                 return true; // Eğer +10 gibi bir bonus ise, her zaman "karşılanmıştır" (bu bir maliyet değil)
            case "maxenergy":
            if (req.requiredValue < 0)
                return (ResourceManager.Instance != null) && (ResourceManager.Instance.maxEnergy + req.requiredValue) >= 1;
                return true;
            case "maxmana":
            if (req.requiredValue < 0)
                return (ResourceManager.Instance != null) && (ResourceManager.Instance.maxMana + req.requiredValue) >= 1;
                return true;
            default:
                Debug.LogWarning($"Bilinmeyen gereksinim tipi (IsRequirementMet): {req.requirementType}");
                return false;
        }
    }

    private string FormatRequirements(List<Requirement> requirements, bool forceMetColor, string header = "")
    {
        if (requirements == null || requirements.Count == 0)
        {
            if(header.Contains("Maliyet")) return "<b>Maliyet:</b> Yok";
            if(header.Contains("Kilit")) return "<b>Gereksinim:</b> Yok";
            return "";
        }

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(header))
        {
             sb.AppendLine(header);
        }

        foreach (Requirement req in requirements)
        {
            bool isMet = forceMetColor || IsRequirementMet(req);
            string reqText = "";
            string currentVal = ""; // Mevcut durumu göstermek için

            switch (req.requirementType.ToLower())
            {
                case "level":
                    reqText = $"Seviye {req.requiredValue}";
                    currentVal = $"({(LevelManager.Instance != null ? LevelManager.Instance.currentLevel : 0)})";
                    break;
                case "quest":
                    reqText = $"Görevi tamamla: '{req.requirementName}'";
                    break;
                case "item":
                    ItemData item = (ItemManager.Instance != null) ? ItemManager.Instance.GetItemByName(req.requirementName) : null;
                    int currentAmount = (item != null && Inventory.Instance != null) ? Inventory.Instance.GetItemCount(item) : 0;
                    reqText = $"{req.requiredValue} x {req.requirementName}";
                    currentVal = $"({currentAmount})";
                    break;
                case "stat":
                    float currentStat = (StatManager.Instance != null) ? StatManager.Instance.GetTotalStat(req.requirementName) : 0;
                    reqText = $"{req.requiredValue} {req.requirementName} Stat";
                    currentVal = $"({currentStat:F0})";
                    break;
                case "gold":
                    reqText = $"{req.requiredValue} Altın";
                    currentVal = $"({(CurrencyManager.Instance != null ? CurrencyManager.Instance.gold : 0):F0})";
                    break;
                case "nexuscoin":
                    reqText = $"{req.requiredValue} Nexus Coin";
                    currentVal = $"({(CurrencyManager.Instance != null ? CurrencyManager.Instance.nexusCoin : 0):F0})";
                    break;
                case "people":
                    reqText = $"{req.requiredValue} Nüfus";
                    currentVal = $"({(CurrencyManager.Instance != null ? CurrencyManager.Instance.people : 0):F0})";
                    break;
                case "health":
                    reqText = $"{req.requiredValue} Can";
                    currentVal = $"({(ResourceManager.Instance != null ? ResourceManager.Instance.currentHealth : 0):F0})";
                    break;
                case "energy":
                    reqText = $"{req.requiredValue} Enerji";
                    currentVal = $"({(ResourceManager.Instance != null ? ResourceManager.Instance.currentEnergy : 0):F0})";
                    break;
                case "mana":
                    reqText = $"{req.requiredValue} Mana";
                    currentVal = $"({(ResourceManager.Instance != null ? ResourceManager.Instance.currentMana : 0):F0})";
                    break;
                case "maxhealth":
                    reqText = $"{(req.requiredValue > 0 ? "+" : "")}{req.requiredValue} Maksimum Can";
                    currentVal = $"({(ResourceManager.Instance != null ? ResourceManager.Instance.maxHealth : 0):F0})";
                    isMet = IsRequirementMet(req); // Rengi doğru ayarlamak için kontrolü tekrar yap
                    break;
                case "maxenergy":
                    reqText = $"{(req.requiredValue > 0 ? "+" : "")}{req.requiredValue} Maksimum Enerji";
                    currentVal = $"({(ResourceManager.Instance != null ? ResourceManager.Instance.maxEnergy : 0):F0})";
                    isMet = IsRequirementMet(req);
                    break;
                case "maxmana":
                    reqText = $"{(req.requiredValue > 0 ? "+" : "")}{req.requiredValue} Maksimum Mana";
                    currentVal = $"({(ResourceManager.Instance != null ? ResourceManager.Instance.maxMana : 0):F0})";
                    isMet = IsRequirementMet(req);
                    break;
                default:
                    reqText = $"{req.requiredValue} {req.requirementName}";
                    break;
            }
            
            sb.AppendLine($"<color=#{ (isMet ? _metColorHex : _notMetColorHex) }>- {reqText} {currentVal}</color>");
        }
        return sb.ToString().TrimEnd();
    }
    
    #endregion

    #region Event Abonelikleri (Sorun 2 Çözümü)

    // UI'ın gereksinimleri DİNAMİK olarak güncellemesi için event dinleme
    private void OnEnable()
    {
        if (_perkData == null) return; // Setup henüz çalışmadıysa dinlemeye başlama
        SubscribeToEvents(true);
        RefreshState(); // Panel açıldığında UI'ı yenile
    }

    private void OnDisable()
    {
        SubscribeToEvents(false);
    }
    
    private void SubscribeToEvents(bool subscribe)
    {
        if (subscribe)
        {
            // Statik event'lere abone ol
            LevelManager.OnPlayerLeveledUp += OnPlayerStatsChanged; 
            Inventory.OnInventoryChanged_Static += OnPlayerStatsChanged;

            // Instance'ı olan event'lere abone ol (null kontrolü önemli)
            if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged += OnPlayerStatsChanged;
            if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
            if (StatManager.Instance != null) StatManager.Instance.OnStatChanged += OnStatManagerChanged;
        }
        else
        {
            // Abonelikleri iptal et
            LevelManager.OnPlayerLeveledUp -= OnPlayerStatsChanged;
            Inventory.OnInventoryChanged_Static -= OnPlayerStatsChanged;

            if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged -= OnPlayerStatsChanged;
            if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
            if (StatManager.Instance != null) StatManager.Instance.OnStatChanged -= OnStatManagerChanged;
        }
    }
    
    // Gelen herhangi bir değişiklik anonsunda, gereksinim metnini yeniden çiz.
    private void OnPlayerStatsChanged() => RefreshState();
    private void OnStatManagerChanged(string statName, double value) => RefreshState();
    private void OnCurrencyChanged(CurrencyType type, double amount) => RefreshState();

    /// <summary>
    /// Kaynaklar, statlar veya envanter değiştiğinde UI'ı günceller.
    /// </summary>
    private void RefreshState()
    {
        if (_perkData == null || !gameObject.activeInHierarchy) return;

        // Sadece 'Kilitli Değil' durumlarını güncelle, 'Kilitli' ise dokunma
        if (_currentState == PerkState.Unlockable)
        {
            bool unlockRequirementsMet = AreRequirementsMet(_perkData.unlockRequirements);
            requirementsText.text = FormatRequirements(_perkData.unlockRequirements, false, "<b>Kilidi Açmak İçin:</b>");
            actionButton.interactable = unlockRequirementsMet;
            
            // Otomatik olarak PAYABLE durumuna geç, eğer şartlar aniden karşılanırsa
            if (unlockRequirementsMet)
            {
                SetState(PerkState.Payable);
            }
        }
        else if (_currentState == PerkState.Payable)
        {
            // Fiyat metnini yeniden oluştur (örn: 10/50 Odun) ve butonu güncelle
            requirementsText.text = FormatRequirements(_perkData.unlockRequirements, true, "<b>Kilit Açıldı:</b>");
            requirementsText.text += "\n" + FormatRequirements(_perkData.purchasePrice, false, "<b>Ödenecek Maliyet:</b>");
            
            actionButton.interactable = AreRequirementsMet(_perkData.purchasePrice);
        }
    }

    #endregion
}