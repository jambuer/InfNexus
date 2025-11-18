using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System; // TimeSpan ve Exception için eklendi

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
    public GameObject lockPanel; // Görev kilitliyken gösterilecek panel

    [Header("Renk Ayarları")]
    public Color metColor = Color.green;
    public Color notMetColor = Color.red;

    private ExplorerQuestData _questData;
    private ExplorerManager _manager; // Manager referansını tutalım
    private int _currentCompletions = 0;
    // _isMet kaldırıldı, kontrol artık Manager'da

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
        else
        {
            Debug.LogError($"[{_questData?.questID ?? gameObject.name}] Start Button atanmamış!", this.gameObject);
        }
    }

    /// <summary>
    /// Bu UI öğesini ExplorerManager ve verisiyle kurar.
    /// </summary>
    public void Setup(ExplorerManager manager, ExplorerQuestData data, int currentCompletions)
    {
        _manager = manager; // Manager referansını sakla
        _questData = data;
        _currentCompletions = currentCompletions;

        if (_questData == null)
        {
             Debug.LogError("Setup için QuestData null!", this.gameObject);
             Destroy(gameObject);
             return;
        }
        if (_manager == null)
        {
             Debug.LogError($"[{_questData.questID}] Setup için ExplorerManager null!", this.gameObject);
        }


        // UI Referans Kontrolleri
        if (descriptionText == null) Debug.LogWarning($"[{_questData.questID}] DescriptionText atanmamış!", this.gameObject);
        if (requirementsText == null) Debug.LogWarning($"[{_questData.questID}] RequirementsText atanmamış!", this.gameObject);
        if (rewardsText == null) Debug.LogWarning($"[{_questData.questID}] RewardsText atanmamış!", this.gameObject);
        if (completionCountText == null) Debug.LogWarning($"[{_questData.questID}] CompletionCountText atanmamış!", this.gameObject);
        if (progressBar == null) Debug.LogWarning($"[{_questData.questID}] ProgressBar atanmamış!", this.gameObject);
        if (timerText == null) Debug.LogWarning($"[{_questData.questID}] TimerText atanmamış!", this.gameObject);
        if (lockPanel == null) Debug.LogWarning($"[{_questData.questID}] LockPanel atanmamış!", this.gameObject);
        // --- Kontroller Bitti ---

        if (descriptionText != null)
            descriptionText.text = _questData.description;

        // UI'ı mevcut duruma göre ilk kez güncelle
        RefreshUI();

        // Eventleri dinlemeye başla (OnEnable çağrılacak)
    }

    /// <summary>
    /// UI'daki tüm metinleri ve buton durumlarını mevcut verilere göre günceller.
    /// </summary>
    public void RefreshUI()
    {
        if (_questData == null || _manager == null) return; // Manager kontrolü eklendi

        // 1. Gereksinimleri Manager'a sor ve Metni Formatla
        bool requirementsMet = GameValidator.Instance.AreRequirementsMet(_questData.requirements);
        if (requirementsText != null)
            requirementsText.text = BuildFormattedRequirementsString(_questData.requirements); // Sadece listeyi gönder

        // 2. Tamamlanma Sayısını Güncelle
        // Manager'dan güncel sayıyı al (Setup sonrası değişmiş olabilir)
        _currentCompletions = _manager.GetExplorerQuestCompletionCount(_questData.questID);
        bool limitReached = _questData.completionLimit > 0 && _currentCompletions >= _questData.completionLimit; // limit 0 ise sınırsız kabul edelim

        if (completionCountText != null)
        {
            // Limiti 0 ise "X / ∞" gibi gösterilebilir
            string limitText = _questData.completionLimit > 0 ? _questData.completionLimit.ToString() : "∞";
            completionCountText.text = $"{_currentCompletions} / {limitText}";
            completionCountText.color = limitReached ? metColor : notMetColor;
        }

        // Zamanlayıcıyı başlangıçta gizle (UpdateProgressBar/UpdateTimerText yönetecek)
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);


        // 3. Ödül Metnini Güncelle
        if (rewardsText != null)
            rewardsText.text = GetNextRewardDescription();

        // 4. Buton Durumunu Ayarla
        if (startButton != null)
        {
            if (limitReached)
            {
                SetButtonState("Tamamlandı", false);
                // Tamamlandıysa kilit panelini tekrar göstermeye gerek yok,
                // SetLocked metodu kilit durumunu yönetmeli.
            }
            else if (_manager.IsQuestActive(_questData.questID))
            {
                // Görev aktifse butonu "İptal Et" yap ve progress bar/timer göster
                SetButtonState("İptal Et", true);
                // Not: Aktif görevin mevcut ilerlemesi UI'a yansıtılmalı
                // Bu, Manager'da Coroutine'leri takip edip Load'da geri yükleyerek veya
                // UI'ın periyodik olarak Manager'dan progress sormasıyla yapılabilir.
                // Şimdilik sadece buton metnini ayarlıyoruz.
            }
            else // Görev aktif değil ve limit dolmadı
            {
                SetButtonState(_questData.isTimerBased ? "Başla" : "Tamamla", requirementsMet); // Süre yazısını kaldırdık
            }
        }
    }

    /// <summary>
    /// Bir sonraki ödülün açıklamasını döndürür (renklendirme ile).
    /// </summary>
    private string GetNextRewardDescription()
    {
        StringBuilder sb = new StringBuilder("<b>Ödüller:</b>\n");
        if (_questData.rewardsPerCompletion == null || _questData.rewardsPerCompletion.Count == 0)
        {
            return sb.Append("- Yok").ToString();
        }

        // Her tamamlama seviyesi için döngü (T1, T2, T3...)
        for(int i = 0; i < _questData.rewardsPerCompletion.Count; i++)
        {
            // Limit kontrolü
            if (_questData.completionLimit > 0 && i >= _questData.completionLimit) break;

            // Ödülün alınıp alınmadığına göre rengi belirle
            string colorHex = (i < _currentCompletions) ? _metColorHex : _notMetColorHex;
            sb.Append($"<color=#{colorHex}>- T{i + 1}: ");

            // [YENİ] Ödül listesini al (GameRewardList)
            GameRewardList rewardListWrapper = _questData.rewardsPerCompletion[i];
            
            if (rewardListWrapper == null || rewardListWrapper.rewards == null || rewardListWrapper.rewards.Count == 0)
            {
                sb.AppendLine("Ödül Yok</color>");
                continue;
            }

            // Ödül listesindeki her bir ödülü formatla
            List<string> rewardStrings = new List<string>();
            foreach (GameReward reward in rewardListWrapper.rewards) //
            {
                // FormatReward isimli yeni yardımcı fonksiyonumuzu kullan (Adım B'de eklenecek)
                rewardStrings.Add(FormatReward(reward));
            }

            // Ödülleri virgülle birleştir (örn: "+50 XP, +10 Odun")
            sb.Append(string.Join(", ", rewardStrings));
            sb.AppendLine("</color>");
        }
        return sb.ToString().TrimEnd();
    }

    private void OnStartButtonClicked()
    {
        if (_manager == null || _questData == null) return;

        if (_manager.IsQuestActive(_questData.questID))
        {
            // Görevi İptal Et (Manager yapar)
            _manager.CancelExplorerQuest(_questData.questID);
            // UI, Manager'dan gelen UpdateProgress/UpdateTimer ile güncellenir.
        }
        else // Görev aktif değilse başlatmayı dene
        {
             // Gereksinimleri TEKRAR Manager'a sor (buton aktif olsa bile arada değişmiş olabilir)
            if (GameValidator.Instance.AreRequirementsMet(_questData.requirements))
            {
                SetButtonState("Başlatılıyor...", false); // Geçici olarak kilitle
                _manager.StartExplorerQuest(_questData); // Manager görevi başlatır (ve kaynakları harcar)
                // UI, Manager'dan gelen UpdateProgress/UpdateTimer ile güncellenir.
            }
            else
            {
                 Debug.LogWarning($"[{_questData.questID}] Buton aktif ama gereksinimler karşılanmıyor?");
                 RefreshUI(); // UI'ı en son duruma göre yenile
            }
        }
    }

    /// <summary>
    /// Manager tarafından çağrılır, progress bar'ı ve buton durumunu günceller.
    /// </summary>
    public void UpdateProgressBar(float progress)
    {
        bool isRunning = progress > 0 && progress < 1;

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(isRunning && _questData.isTimerBased); // Sadece zaman bazlıysa göster
            progressBar.value = progress;
        }

        // Görev bittiğinde (progress 1) veya iptal edildiğinde (progress 0)
        // buton durumunu RefreshUI ile yenile.
        if (!isRunning && startButton != null && gameObject.activeInHierarchy) // Aktifse yenile
        {
            RefreshUI();
        }
    }

    /// <summary>
    /// Manager tarafından çağrılır, zamanlayıcı metnini günceller.
    /// </summary>
    public void UpdateTimerText(float remainingSeconds)
    {
        if (timerText == null) return;
        if (remainingSeconds < 0) remainingSeconds = 0;

        // Zamanlayıcıyı sadece çalışıyorsa ve görev zaman bazlıysa göster
        timerText.gameObject.SetActive(remainingSeconds > 0 && _questData.isTimerBased);

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


    /// <summary>
    /// Görevin kilitli olup olmadığını ayarlar. Manager tarafından çağrılır.
    /// </summary>
    public void SetLocked(bool isLocked)
    {
        if (lockPanel != null)
        {
            lockPanel.SetActive(isLocked);
        }
        // Kilitliyken butonları ve diğer etkileşimli elemanları gizle
        if (startButton != null) startButton.gameObject.SetActive(!isLocked);
        if (progressBar != null) progressBar.gameObject.SetActive(!isLocked);
        if (timerText != null) timerText.gameObject.SetActive(!isLocked);
        // Gereksinim/Ödül metinleri kilitliyken de görünebilir veya gizlenebilir, tasarıma bağlı.
    }

    public string GetQuestID()
    {
        return (_questData != null) ? _questData.questID : string.Empty;
    }

    #region Gereksinim Formatlama (Kontrol Manager'da)

    // CheckRequirementsMet ve IsRequirementMet metotları SİLİNDİ.


    private string BuildFormattedRequirementsString(List<Requirement> requirements)
    {
        // [YENİ] Artık ExplorerManager'a sormak yerine,
        // merkezi RequirementTooltipFormatter'a soruyoruz.
        return RequirementTooltipFormatter.GetFormattedRequirementText(requirements, "<b>Gereksinimler:</b>");
    
    }

    #endregion

    private void SetButtonState(string text, bool interactable)
    {
        if (startButtonText != null) startButtonText.text = text;
        if (startButton != null) startButton.interactable = interactable;
    }

    #region Event Abonelikleri (Dinamik UI Güncellemesi için)

    private bool _isSubscribed = false;

    private void OnEnable()
    {
        // Setup henüz çalışmadıysa veya Manager yoksa dinlemeye başlama
        if (_questData == null || _manager == null) return;
        if (_isSubscribed) return;

        SubscribeToEvents(true);
        RefreshUI(); // Panel açıldığında UI'ı yenile
    }

    private void OnDisable()
    {
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
            catch (Exception ex) { Debug.LogWarning($"[{_questData?.questID ?? "Bilinmeyen Görev"}] Event aboneliği kaldırılırken hata: {ex.Message}"); }
            _isSubscribed = false;
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
                // ItemData referansı varsa ismini kullan, yoksa string parametresini kullan
                string itemName = reward.itemData != null ? reward.itemData.itemName : reward.stringParameter;
                return $"+{reward.amount:N0} {itemName}";
            case RewardType.Stat:
                return $"+{reward.amount} {reward.stringParameter}"; // Örn: "+5 Physical"
            case RewardType.Perk:
                return $"Perk: {reward.stringParameter}";
            // Henüz uygulanmayanları ekle (GameRewardDistributor'dakilerle aynı olmalı)
            case RewardType.PremiumCoin:
            case RewardType.LifeSkillXP:
            case RewardType.JobXP:
                return $"+{reward.amount:N0} {reward.rewardType}"; // Örn: "+50 LifeSkillXP"
            default:
                return "Bilinmeyen Ödül";
        }
    }
    


    // Gelen herhangi bir değişiklik anonsunda, UI durumunu yenile.
    private void OnPlayerStatsChanged() => RefreshUI();
    private void OnStatManagerChanged(string statName, double value) => RefreshUI();
    private void OnCurrencyChanged(CurrencyType type, double amount) => RefreshUI();
    private void OnQuestProgressChanged(QuestData questData, int completionCount) => RefreshUI(); // Ana görevler değiştiğinde
    // private void OnExplorerQuestCompleted(ExplorerQuestData questData) => RefreshUI(); // Başka bir explorer görevi bittiğinde

    

    #endregion
}