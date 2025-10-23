using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class ExplorerQuestUI : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI rewardsText;
    public TextMeshProUGUI completionCountText;
    public Slider progressBar;
    public Button startButton;
    public TextMeshProUGUI startButtonText;
    public TextMeshProUGUI timerText;

    [Header("Kilit Paneli")]
    public GameObject lockPanel; // Görev kilitliyken gösterilecek panel

    [Header("Renk Ayarları")]
    public Color metColor = Color.green;
    public Color notMetColor = Color.red;

    private ExplorerQuestData _questData;
    private ExplorerManager _manager;
    private int _currentCompletions = 0;
    private bool _isMet = false; // Gereksinimler karşılandı mı?
    private string _metColorHex;
    private string _notMetColorHex;

    private void Awake()
    {
        _metColorHex = ColorUtility.ToHtmlStringRGB(metColor);
        _notMetColorHex = ColorUtility.ToHtmlStringRGB(notMetColor);
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    /// <summary>
    /// Bu UI öğesini ExplorerManager ve verisiyle kurar.
    /// </summary>
    public void Setup(ExplorerManager manager, ExplorerQuestData data, int currentCompletions)
    {
        _manager = manager;
        _questData = data;
        _currentCompletions = currentCompletions;

        if (_questData == null) return;

        if (descriptionText != null) 
            descriptionText.text = _questData.description;

        // Event'leri dinlemeye başla (Stat, Seviye vb. değişiklikleri için)
        SubscribeToEvents(true);
        
        // UI'ı mevcut duruma göre güncelle
        RefreshUI();
    }

    /// <summary>
    /// UI'daki tüm metinleri ve buton durumlarını mevcut verilere göre günceller.
    /// </summary>
    public void RefreshUI()
    {
        if (_questData == null) return;
        
        // 1. Gereksinimleri Kontrol Et ve Metni Formatla
        _isMet = CheckRequirementsMet(_questData.requirements);
        if (requirementsText != null)
            requirementsText.text = FormatRequirements(_questData.requirements);

        // 2. Tamamlanma Sayısını Güncelle
        bool limitReached = _currentCompletions >= _questData.completionLimit;
        if (completionCountText != null)
        {
            completionCountText.text = $"{_currentCompletions} / {_questData.completionLimit}";
            completionCountText.color = limitReached ? metColor : notMetColor;
        }
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false); // Varsayılan olarak gizle
        }

        // 3. Ödül Metnini Güncelle
        if (rewardsText != null)
            rewardsText.text = GetNextRewardDescription();

        // 4. Buton Durumunu Ayarla
        if (startButton != null)
        {
            if (limitReached)
            {
                SetButtonState("Tamamlandı", false);
                if (lockPanel != null) lockPanel.SetActive(true); // Tamamlandıysa kilitle
            }
            else if (_manager.IsQuestActive(_questData.questID))
            {
                SetButtonState("İptal Et", true);
            }
            else
            {
                SetButtonState(_questData.isTimerBased ? "Başla (Süre)" : "Başla", _isMet);
            }
        }
    }

    /// <summary>
    /// Bir sonraki ödülün açıklamasını döndürür.
    /// </summary>
    private string GetNextRewardDescription()
    {
        StringBuilder sb = new StringBuilder("<b>Ödüller:</b>\n");
        if (_questData.rewardsPerCompletion == null || _questData.rewardsPerCompletion.Count == 0)
        {
            return "<b>Ödül:</b> Yok";
        }

        for(int i=0; i < _questData.rewardsPerCompletion.Count; i++)
        {
            string color = (i < _currentCompletions) ? _metColorHex : _notMetColorHex; // Alınmışsa yeşil, alınmamışsa kırmızı
            string rewardDesc = _questData.rewardsPerCompletion[i].rewardDescription;
            if (string.IsNullOrEmpty(rewardDesc)) rewardDesc = "Ödül Yok";
            
            sb.AppendLine($"<color=#{color}>- Tamamlama {i + 1}: {rewardDesc}</color>");
        }
        return sb.ToString();
    }

    private void OnStartButtonClicked()
    {
        if (_manager == null || _questData == null) return;

        if (_manager.IsQuestActive(_questData.questID))
        {
            // Görevi İptal Et
            _manager.CancelExplorerQuest(_questData.questID);
            // UI anında güncellenir (event ile)
        }
        else if (_isMet)
        {
            // Görevi Başlat
            _manager.StartExplorerQuest(_questData);
            SetButtonState("Çalışıyor...", false); // Başlatıldıktan sonra butonu kilitle
            if (timerText != null)
            {
                timerText.gameObject.SetActive(true); // Zamanlayıcıyı göster
            }

        }
    }

    // ExplorerQuestUI.cs -> UpdateProgressBar:
public void UpdateProgressBar(float progress)
{
    bool isRunning = progress > 0 && progress < 1;

    if (progressBar != null)
    {
        progressBar.gameObject.SetActive(isRunning);
        progressBar.value = progress;
    }

    // Eğer görev BİTTİ veya İPTAL EDİLDİ ise (progress 0 veya 1 ise)
    if (!isRunning)
    {
        if (timerText != null) timerText.gameObject.SetActive(false); // Zamanlayıcı metnini gizle
        if (startButton != null) RefreshUI(); // Buton durumunu ("Başla") yenile
    }
}

    public void SetLocked(bool isLocked)
    {
        if (lockPanel != null)
        {
            lockPanel.SetActive(isLocked);
        }
        // Kilitliyken butonları da kapat
        if (startButton != null) startButton.gameObject.SetActive(!isLocked);
        // ... diğer UI elemanları ...
    }
    
    /// <summary>
/// Kalan süreyi SS:DD:SS formatında gösterir.
/// </summary>
public void UpdateTimerText(float remainingSeconds)
{
    if (timerText == null) return;

    // Zamanlayıcıyı sadece çalışıyorsa (0'dan büyükse) göster
    timerText.gameObject.SetActive(remainingSeconds > 0);

    System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(remainingSeconds);
    timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                    (int)timeSpan.TotalHours,
                                    timeSpan.Minutes,
                                    timeSpan.Seconds);
}

    public string GetQuestID()
    {
        return (_questData != null) ? _questData.questID : string.Empty;
    }

    #region Gereksinim Kontrolü (ExplorerPerkUI'dan Kopyalandı)

    private bool CheckRequirementsMet(List<Requirement> requirements)
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
        // Not: Bu fonksiyon ExplorerManager'ın maliyet kontrolünden farklıdır,
        // Sadece "Gereken" şarta bakılır (örn: 50 Enerjiye SAHİP olmak)
        switch (req.requirementType.ToLower())
        {
            case "level": return (LevelManager.Instance != null) && LevelManager.Instance.currentLevel >= req.requiredValue;
            case "quest": return (QuestManager.Instance != null) && QuestManager.Instance.GetCompletionCount(req.requirementName) > 0;
            case "item":
                ItemData item = (ItemManager.Instance != null) ? ItemManager.Instance.GetItemByName(req.requirementName) : null;
                return (item != null && Inventory.Instance != null) && Inventory.Instance.HasItem(item, req.requiredValue);
            case "stat": return (StatManager.Instance != null) && StatManager.Instance.GetTotalStat(req.requirementName) >= req.requiredValue;
            case "energy": return (ResourceManager.Instance != null) && ResourceManager.Instance.currentEnergy >= req.requiredValue;
            // Diğer kaynaklar...
            default: return false;
        }
    }

    private string FormatRequirements(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return "<b>Gereksinim:</b> Yok";

        StringBuilder sb = new StringBuilder("<b>Gereksinimler:</b>\n");
        foreach (Requirement req in requirements)
        {
            bool isMet = IsRequirementMet(req);
            string reqText = "";
            string currentVal = "";

            switch (req.requirementType.ToLower())
            {
                case "level": reqText = $"Seviye {req.requiredValue}"; break;
                case "quest": reqText = $"Görevi tamamla: '{req.requirementName}'"; break;
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
                case "energy":
                    reqText = $"{req.requiredValue} Enerji";
                    currentVal = $"({ResourceManager.Instance.currentEnergy:F0})";
                    break;
                default: reqText = $"{req.requiredValue} {req.requirementName}"; break;
            }
            sb.AppendLine($"<color=#{ (isMet ? _metColorHex : _notMetColorHex) }>- {reqText} {currentVal}</color>");
        }
        return sb.ToString().TrimEnd();
    }

    #endregion

    private void SetButtonState(string text, bool interactable)
    {
        if (startButtonText != null) startButtonText.text = text;
        if (startButton != null) startButton.interactable = interactable;
    }

    #region Event Abonelikleri

    // UI'ın gereksinimleri dinamik olarak güncellemesi için
    private void OnEnable()
    {
        SubscribeToEvents(true);
        if (_questData != null) RefreshUI(); // Panel tekrar açıldığında UI'ı yenile
    }

    private void OnDisable()
    {
        SubscribeToEvents(false);
    }
    
    private void SubscribeToEvents(bool subscribe)
    {
        // Event'ler null olabilir, null kontrolü yap
        if (subscribe)
        {
            if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp += OnPlayerStatsChanged;
            if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged += OnPlayerStatsChanged;
            if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += OnPlayerStatsChanged;
            if (StatManager.Instance != null) StatManager.Instance.OnStatChanged += OnStatManagerChanged;
            if (Inventory.Instance != null) Inventory.OnInventoryChanged_Static += OnPlayerStatsChanged; // Statik eventi dinle
        }
        else
        {
            if (LevelManager.Instance != null) LevelManager.Instance.OnLevelUp -= OnPlayerStatsChanged;
            if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged -= OnPlayerStatsChanged;
            if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= OnPlayerStatsChanged;
            if (StatManager.Instance != null) StatManager.Instance.OnStatChanged -= OnStatManagerChanged;
            if (Inventory.Instance != null) Inventory.OnInventoryChanged_Static -= OnPlayerStatsChanged; // Statik event aboneliğini kaldır
        }
    }

    private void OnPlayerStatsChanged() => RefreshUI();
    private void OnStatManagerChanged(string statName, double value) => RefreshUI();
    private void OnPlayerStatsChanged(CurrencyType type, double amount) => RefreshUI();

    #endregion
}