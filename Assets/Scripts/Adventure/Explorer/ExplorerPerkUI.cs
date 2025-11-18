using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System; // TimeSpan ve Exception için eklendi

// Enum tanımları ExplorerPerkData.cs içinde olduğu varsayılıyor.

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
        else
        {
            Debug.LogError($"[{_perkData?.name ?? gameObject.name}] Action Button atanmamış!", this.gameObject);
        }
    }

    /// <summary>
    /// Bu UI öğesini ExplorerPerkData ve başlangıç durumu ile kurar.
    /// </summary>
    public void Setup(ExplorerPerkData data, PerkState initialState)
    {
        _perkData = data;
        if (_perkData == null)
        {
            Debug.LogError("Setup için PerkData null!", this.gameObject);
            Destroy(gameObject);
            return;
        }

        // UI Referans Kontrolleri (Setup aşamasında yapmak daha iyi)
        if (descriptionText == null) Debug.LogWarning($"[{_perkData.name}] DescriptionText atanmamış!", this.gameObject);
        if (requirementsText == null) Debug.LogWarning($"[{_perkData.name}] RequirementsText atanmamış!", this.gameObject);
        if (rewardText == null) Debug.LogWarning($"[{_perkData.name}] RewardText atanmamış!", this.gameObject);
        if (timerText == null) Debug.LogWarning($"[{_perkData.name}] TimerText atanmamış!", this.gameObject);
        if (lockedPanel == null) Debug.LogWarning($"[{_perkData.name}] LockedPanel atanmamış!", this.gameObject);
        if (unlockedPanel == null) Debug.LogWarning($"[{_perkData.name}] UnlockedPanel atanmamış!", this.gameObject);
        if (priceRewardSection == null) Debug.LogWarning($"[{_perkData.name}] PriceRewardSection atanmamış!", this.gameObject);
        if (requirementsSection == null) Debug.LogWarning($"[{_perkData.name}] RequirementsSection atanmamış!", this.gameObject);
        // --- Kontroller Bitti ---

        if (descriptionText != null)
            descriptionText.text = _perkData.description;

        _isTimerTask = (_perkData.tag == ExplorerTag.ExplorerTime);

        // Başlangıç UI durumunu ayarla
        SetState(initialState);

        // Eventleri dinlemeye başla
        // Obje aktifse OnEnable zaten çağrılmıştır, değilse Awake/Start sonrası aktifleşince çağrılır.
        // Bu yüzden SubscribeToEvents'i OnEnable içine taşıyoruz.
    }

    /// <summary>
    /// UI'ı mevcut duruma göre günceller.
    /// </summary>
    public void SetState(PerkState newState)
    {
        _currentState = newState;

        // UI Referanslarının var olduğundan emin ol (Awake'de kontrol edildi ama yine de güvenli)
        if (lockedPanel == null || unlockedPanel == null || requirementsSection == null || priceRewardSection == null || actionButton == null || actionButtonText == null || requirementsText == null || rewardText == null)
        {
             Debug.LogError($"[{_perkData?.name ?? "Bilinmeyen Perk"}] UI Referans hatası nedeniyle SetState yapılamıyor!");
            return;
        }


        lockedPanel.SetActive(newState == PerkState.Locked);
        unlockedPanel.SetActive(newState == PerkState.Unlockable || newState == PerkState.Payable);

        // Zamanlayıcıyı başlangıçta gizle
        SetActiveTimerView(false);

        if (newState == PerkState.Locked)
        {
            // Kilitliyken event dinlemeye gerek yok
            SubscribeToEvents(false);
            return;
        }
        else
        {
            // Kilit açıldığında veya açılabilir olduğunda dinlemeye başla/devam et
            SubscribeToEvents(true); // OnEnable zaten yapacak ama emin olmak için
        }

        bool requirementsMetForButton;
        switch (newState)
        {
            case PerkState.Unlockable: // Aşama 2: UNLOCK
                requirementsSection.SetActive(true);
                priceRewardSection.SetActive(false);

                // Kontrolü Manager'a sor
                requirementsMetForButton = GameValidator.Instance != null && GameValidator.Instance.AreRequirementsMet(_perkData.unlockRequirements);
                requirementsText.text = BuildFormattedRequirementsString(_perkData.unlockRequirements, false, "<b>Kilidi Açmak İçin:</b>");

                actionButton.gameObject.SetActive(true);
                actionButtonText.text = "UNLOCK";
                actionButton.interactable = requirementsMetForButton;
                break;

            case PerkState.Payable: // Aşama 3: PAY / START
                requirementsSection.SetActive(true);
                priceRewardSection.SetActive(true);

                requirementsText.text = BuildFormattedRequirementsString(_perkData.unlockRequirements, true, "<b>Kilit Açıldı:</b>");
                requirementsText.text += "\n" + BuildFormattedRequirementsString(_perkData.purchasePrice, false, "<b>Ödenecek Maliyet:</b>");


                string descToShow = "Yok";
                if (_perkData.reward?.perkToGrant != null) // perkToGrant null değilse
                {
                    // Önce override'a bak, yoksa definition'daki açıklamayı al
                    descToShow = !string.IsNullOrEmpty(_perkData.reward.descriptionOverride)
                                 ? _perkData.reward.descriptionOverride
                                 : _perkData.reward.perkToGrant.description;
                }

                rewardText.text = $"<b>Ödül:</b> {descToShow}";


                // Buton için maliyet kontrolünü Manager'a sor
                requirementsMetForButton = GameValidator.Instance != null && GameValidator.Instance.AreRequirementsMet(_perkData.purchasePrice);

                actionButton.gameObject.SetActive(true);
                actionButtonText.text = _isTimerTask ? "START" : "PAY";
                actionButton.interactable = requirementsMetForButton;
                break;
        }
    }

    private void OnActionButtonClicked()
    {
        if (_perkData == null || ExplorerManager.Instance == null)
        {
             Debug.LogError("PerkData veya ExplorerManager bulunamadığı için butona tıklanamıyor!");
             return;
        }


        switch (_currentState)
        {
            case PerkState.Unlockable:
                // Kontrolü Manager'a sor
                //
                if (GameValidator.Instance.AreRequirementsMet(_perkData.unlockRequirements))
                {
                    // Sadece UI durumunu değiştir, Manager'a gitmeye gerek yok
                    SetState(PerkState.Payable);
                }
            
                break;

            case PerkState.Payable:
                actionButton.interactable = false; // Çift tıklamayı önle

                if (_isTimerTask)
                {
                    SetActiveTimerView(true); // Zamanlayıcı UI'ını göster
                    ExplorerManager.Instance.StartExplorerTimer(_perkData, this); // Manager'a başlatmasını söyle
                }
                else
                {
                    // Satın alma işlemini Manager'a yaptır
                    ExplorerManager.Instance.PurchasePerk(_perkData);
                }
                break;
        }
    }

    /// <summary>
    /// Zamanlayıcı bittiğinde veya harcama başarısız olduğunda Manager tarafından çağrılabilir.
    /// </summary>
    public void ResetButton()
    {
        SetActiveTimerView(false);
        if (_currentState == PerkState.Payable && actionButton != null && ExplorerManager.Instance != null)
        {
            // Buton durumunu tekrar Manager'a sorarak güncelle
            //
            actionButton.interactable = GameValidator.Instance.AreRequirementsMet(_perkData.purchasePrice);

        }
    }

    /// <summary>
    /// Zamanlayıcı görünümünü açar/kapatır.
    /// </summary>
    public void SetActiveTimerView(bool isTimerRunning)
    {
        if (actionButton != null) actionButton.gameObject.SetActive(!isTimerRunning);
        if (timerText != null) timerText.gameObject.SetActive(isTimerRunning);
    }

    /// <summary>
    /// Zamanlayıcı metnini günceller (Formatlı).
    /// </summary>
    public void UpdateTimerText(float remainingSeconds)
    {
        if (timerText == null) return;
        if (remainingSeconds < 0) remainingSeconds = 0;

        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(remainingSeconds);
        if (timeSpan.TotalHours >= 1)
        {
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }
        else
        {
             timerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
    }


    #region Gereksinim Formatlama (Kontrol Manager'da)

    // AreRequirementsMet ve IsRequirementMet metotları SİLİNDİ.

    /// <summary>
    /// Gereksinim listesini okunabilir metne çevirir, renkleri Manager'a sorarak ayarlar.
    /// </summary>

    #endregion

    #region Event Abonelikleri (Dinamik UI Güncellemesi - ESKİ KODDAN ALINDI)

    private bool _isSubscribed = false; // Tekrar abone olmayı önlemek için

    private void OnEnable()
    {
        // Setup henüz çalışmadıysa veya Manager yoksa dinlemeye başlama
        if (_perkData == null || ExplorerManager.Instance == null) return;
        // Zaten aboneysek tekrar abone olma
        if (_isSubscribed) return;

        SubscribeToEvents(true);
        RefreshState(); // Panel açıldığında UI'ı yenile
    }

    private void OnDisable()
    {
        // Sadece aboneysek abonelikten çık
        if (!_isSubscribed) return;
        SubscribeToEvents(false);
    }

    private void SubscribeToEvents(bool subscribe)
    {
        if (subscribe)
        {
            if (_isSubscribed) return;
            LevelManager.OnPlayerLeveledUp += OnPlayerStatsChanged;
            if (QuestManager.Instance != null) QuestManager.Instance.OnQuestProgress += OnQuestProgressChanged;
            // YÜKSEK FREKANSLI (Kasma Sebebi) - YORUMA AL:
            // Inventory.OnInventoryChanged_Static += OnPlayerStatsChanged;
            // if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged += OnPlayerStatsChanged;
            // if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
            // if (StatManager.Instance != null) StatManager.Instance.OnStatChanged += OnStatManagerChanged;
            _isSubscribed = true;
        }
        else
        {
            if (!_isSubscribed) return;
            LevelManager.OnPlayerLeveledUp -= OnPlayerStatsChanged;
            if (QuestManager.Instance != null) QuestManager.Instance.OnQuestProgress -= OnQuestProgressChanged;
            // YÜKSEK FREKANSLI (Kasma Sebebi) - YORUMA AL:
            // Inventory.OnInventoryChanged_Static -= OnPlayerStatsChanged;
            try
            {
                // if (ResourceManager.Instance != null) ResourceManager.Instance.OnValuesChanged -= OnPlayerStatsChanged;
                // if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
                // if (StatManager.Instance != null) StatManager.Instance.OnStatChanged -= OnStatManagerChanged;
            }
            catch (Exception ex) { Debug.LogWarning($"[{_perkData?.name ?? "Bilinmeyen Perk"}] Event aboneliği kaldırılırken hata: {ex.Message}"); }
            _isSubscribed = false;
        }
    }

    // Gelen herhangi bir değişiklik anonsunda, UI durumunu yenile.
    private void OnPlayerStatsChanged() => RefreshState();
    private void OnStatManagerChanged(string statName, double value) => RefreshState();
    private void OnCurrencyChanged(CurrencyType type, double amount) => RefreshState();
    private void OnQuestProgressChanged(QuestData questData, int completionCount) => RefreshState();

    /// <summary>
    /// Kaynaklar, statlar, envanter veya görevler değiştiğinde UI'ı günceller.
    /// </summary>
    private void RefreshState()
    {
        // Obje aktif değilse, verisi yoksa veya Manager yoksa işlem yapma
        if (_perkData == null || !gameObject.activeInHierarchy || ExplorerManager.Instance == null) return;
        // Henüz Setup çağrılmadıysa (_currentState null olabilir)
        if (_currentState == 0 && _perkData != null)
        {
            // Başlangıç durumunu Manager'dan alarak ayarla (güvenlik için)
            bool isPurchased = ExplorerManager.Instance.GetExplorerQuestCompletionCount(_perkData.name) > 0; // Veya perk index'ine göre kontrol
            if (isPurchased) _currentState = PerkState.Purchased; // Bu UI zaten görünmemeli
            else
            {
                //
                bool canUnlock = GameValidator.Instance.AreRequirementsMet(_perkData.unlockRequirements);

                _currentState = canUnlock ? PerkState.Payable : PerkState.Unlockable;
            }
        }


        // Sadece 'Kilitli Değil' durumlarını güncelle
        if (_currentState == PerkState.Unlockable)
        {
            //
            bool unlockRequirementsMet = GameValidator.Instance.AreRequirementsMet(_perkData.unlockRequirements);

            if (requirementsText != null) requirementsText.text = BuildFormattedRequirementsString(_perkData.unlockRequirements, false, "<b>Kilidi Açmak İçin:</b>");
            if (actionButton != null) actionButton.interactable = unlockRequirementsMet;

            // Eğer gereksinimler ANİDEN karşılanırsa, durumu otomatik PAYABLE yap
            if (unlockRequirementsMet)
            {
                SetState(PerkState.Payable);
            }
        }
        else if (_currentState == PerkState.Payable)
        {
            //
            bool purchaseRequirementsMet = GameValidator.Instance.AreRequirementsMet(_perkData.purchasePrice);

            // Fiyat metnini yeniden oluştur ve butonu güncelle
            if (requirementsText != null)
            {
                requirementsText.text = BuildFormattedRequirementsString(_perkData.unlockRequirements, true, "<b>Kilit Açıldı:</b>");
                requirementsText.text += "\n" + BuildFormattedRequirementsString(_perkData.purchasePrice, false, "<b>Ödenecek Maliyet:</b>");

            }
            if (actionButton != null)
            {
                actionButton.interactable = purchaseRequirementsMet;
            }
        }
        // Locked veya Purchased durumunda bir şey yapmaya gerek yok.
    }

    private string BuildFormattedRequirementsString(List<Requirement> requirements, bool forceMetColor, string header)
    {
        // [YENİ] Artık ExplorerManager'a sormak yerine,
        // merkezi RequirementTooltipFormatter'a soruyoruz.
        return RequirementTooltipFormatter.GetFormattedRequirementText(requirements, header);
    
    }


    #endregion
}