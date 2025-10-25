using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Gerekli (LINQ ve HashSet/Dictionary işlemleri için)
using UnityEngine.UI;
using System;
using NUnit.Framework; // Gerekli (Exception vb. için)

// GameSaveData ile uyumlu olması için IGameDataSaveable eklendi
public class ExplorerManager : MonoBehaviour, IGameDataSaveable<ExplorerSaveData>
{
    public static ExplorerManager Instance { get; private set; }

    [Header("UI Referansları")]
    public Transform leftPanelContainer;
    public Transform rightPanelContainer;
    public RectTransform mapPanel; // TÜR DEĞİŞTİ: Transform -> RectTransform
    public GameObject explorerPerkPrefab;
    public GameObject explorerQuestPrefab;
    public GameObject sunakPanelObject; // Eski koddaki referans
    public GameObject sunakPrefab;      // Eski koddaki referans
    public GameObject area1Panel;
    public GameObject area2Panel;
    public Button openSunakButton;      // Eski koddaki referans
    public Button area2UnlockButton;
    public float mapAnimationSpeed = 0.5f;

    [Header("Veri Listeleri (Inspector)")]
    public List<ExplorerPerkData> leftPanelPerks;
    public List<ExplorerQuestData> rightPanelQuests;
    public event Action<PerkReward> OnExplorerPerkCompleted;

    // --- Durum Takibi (Kaydedilecek Veriler) ---
    private int _currentLeftPerkIndex = 0;
    private HashSet<string> _unlockedExplorerQuests = new HashSet<string>();
    private Dictionary<string, int> _explorerQuestCompletions = new Dictionary<string, int>();

    // --- Geçici Durum Takibi (Kaydedilmez) ---
    private List<ExplorerPerkUI> _instantiatedPerkItems = new List<ExplorerPerkUI>();
    private Dictionary<string, Coroutine> _activeExplorerQuests = new Dictionary<string, Coroutine>();
    private List<ExplorerQuestUI> _instantiatedQuestItems = new List<ExplorerQuestUI>();
    private bool isMapOpen = false, isSunakOpen = false;
    private Vector2 mapHiddenPos, mapVisiblePos;
    private Coroutine mapAnimationCoroutine;

    public string _metColorHex => ColorUtility.ToHtmlStringRGB(Color.green); // Veya public değişken yap
    public string _notMetColorHex => ColorUtility.ToHtmlStringRGB(Color.red);


    // ========================================================================================
    // BAŞLANGIÇ VE YÖNETİM
    // ========================================================================================

    void Awake()
    {
        // Singleton ve DontDestroyOnLoad (Diğer yöneticiler gibi)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

         // Başlangıçta UI'yı yenile
    }

    void Start()
    {
        InitializeMapPanel();

        // LoadFromData çağrılacaksa, RefreshUI orada zaten çağrılıyor.
        // Eğer yeni oyunsa ve LoadFromData çağrılmayacaksa diye
        // başlangıç değerlerini ayarlayıp RefreshUI çağırmak gerekebilir.
        // LoadFromData içindeki mantık bunu zaten yapıyor (ilk görevi açma vs).
        // Bu yüzden Start içinde RefreshUI'a gerek kalmayabilir.
        RefreshUI(); // Muhtemelen gereksiz
    }

    /// <summary>
    /// Hem sol hem de sağ paneli mevcut duruma göre yeniler.
    /// LoadFromData tarafından veya gerektiğinde manuel çağrılır.
    /// </summary>
    public void RefreshUI()
    {
        Debug.Log("[ExplorerManager] RefreshUI çağrıldı.");
        RefreshLeftPanel();
        RefreshRightPanel();
        CheckArea2Unlock(); // Eski koddan
        UpdateSunakButtonVisibility(); // Eski koddan
    }

    #region UI & Panel Yönetimi (Harita, Sunak, Alan)

    private void InitializeMapPanel()
    {
        if (mapPanel == null) return;
        mapPanel.gameObject.SetActive(true);
        // RectTransform varsayılarak düzeltildi
        mapHiddenPos = new Vector2(-mapPanel.rect.width, mapPanel.anchoredPosition.y);
        mapVisiblePos = new Vector2(0, mapPanel.anchoredPosition.y);
        mapPanel.anchoredPosition = mapHiddenPos;
        isMapOpen = false;
    }

    public void ToggleMapPanel()
    {
        if (mapPanel == null) return;
        if (mapAnimationCoroutine != null) StopCoroutine(mapAnimationCoroutine);
        isMapOpen = !isMapOpen;
        Vector2 targetPos = isMapOpen ? mapVisiblePos : mapHiddenPos;
        // RectTransform gerektiren AnimatePanel çağrısı doğru
        mapAnimationCoroutine = StartCoroutine(AnimatePanel(mapPanel, targetPos, mapAnimationSpeed));
    }

    // Eski koddan alındı
    public void ToggleSunakPanel()
    {
        if (sunakPanelObject == null) return;

        isSunakOpen = !isSunakOpen;
        sunakPanelObject.SetActive(isSunakOpen);

        // Eski koddaki animasyon çağrısı
        RectTransform sunakRect = sunakPanelObject.GetComponent<RectTransform>();
        if (sunakRect != null)
        {
            AnimatePanel(sunakRect,
                         isSunakOpen ? Vector2.zero : new Vector2(0, -sunakRect.rect.height),
                         0.3f); // Süre eski koddan
        }


        if (GameConsole.Instance != null) // Eski koddan
            GameConsole.Instance.AddMessage(isSunakOpen ? "Sunak açıldı." : "Sunak kapandı.");
    }

    // RectTransform alacak şekilde imzası doğru
    private IEnumerator AnimatePanel(RectTransform panel, Vector2 targetPos, float duration)
    {
        if (panel == null) yield break; // Null kontrolü eklendi
        float time = 0;
        Vector2 startPos = panel.anchoredPosition;
        while (time < duration)
        {
            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        panel.anchoredPosition = targetPos;
        if (panel == mapPanel) mapAnimationCoroutine = null; // Sadece map için null yap
    }

    // Eski koddan alındı
    public void GoToArea(GameObject areaPanelToShow)
    {
        if (area1Panel != null) area1Panel.SetActive(area1Panel == areaPanelToShow);
        if (area2Panel != null) area2Panel.SetActive(area2Panel == areaPanelToShow);
        if (isMapOpen) ToggleMapPanel(); // Harita açıksa kapat
    }
    #endregion

    #region Sol Panel (Perks) Mantığı

    // Eski kodun mantığı büyük ölçüde korundu
    public void RefreshLeftPanel()
    {
        foreach (Transform child in leftPanelContainer) Destroy(child.gameObject);
        _instantiatedPerkItems.Clear();

        for (int i = 0; i < leftPanelPerks.Count; i++)
        {
            ExplorerPerkData perkData = leftPanelPerks[i];

            // Satın alınmışsa gösterme
            if (i < _currentLeftPerkIndex) continue;

            GameObject perkGO = Instantiate(explorerPerkPrefab, leftPanelContainer);
            ExplorerPerkUI perkUI = perkGO.GetComponent<ExplorerPerkUI>();

            if (perkUI != null)
            {
                PerkState stateToSet;
                if (i == _currentLeftPerkIndex) // Sıradaki aktif perk
                {
                    // ÖNEMLİ: UI script'i yerine Manager'daki doğru kontrolü kullan
                    stateToSet = CheckRequirementsMet(perkData.unlockRequirements) ? PerkState.Payable : PerkState.Unlockable;
                }
                else // Gelecek perk
                {
                    stateToSet = PerkState.Locked;
                }
                perkUI.Setup(perkData, stateToSet);
                _instantiatedPerkItems.Add(perkUI);
            }
        }
    }

    // Eski kodun mantığı büyük ölçüde korundu
    public void StartExplorerTimer(ExplorerPerkData perkData, ExplorerPerkUI perkUI)
    {
        // Benzersiz ID oluştur (Asset adı + "_Timer" kullanmaya devam edebiliriz ama dikkatli olunmalı)
        string timerID = perkData.name + "_Timer";
        if (ExplorerTimerManager.Instance.IsTimerActive(timerID)) // Yeni kontrol
        {
            Debug.LogWarning($"[{timerID}] Zamanlayıcısı zaten aktif.");
            return;
        }


        // Maliyeti KONTROL ET ve TÜKET
        if (!CheckAndConsumeRequirements(perkData.purchasePrice, true)) // true = harca
        {
            GameConsole.Instance?.AddMessage("<color=red>Explorer görevi için kaynakların yetersiz!</color>");
            perkUI?.ResetButton();
            return;
        }

        // Süreyi hesapla
        float reduction = (PerkManager.Instance != null) ? PerkManager.Instance.GetBonusFromPerks(PerkEffectType.GetExplorerTimeReduction) : 0f;
        float durationInSeconds = 0f; // TODO: Süreyi perkData'dan (örn: yeni bir float alanı) okuyun!
                                      // if (perkData.name.Contains...) kısmı çok kırılgandı, kaldırıp doğrudan veri okuyun.
                                      // Örnek: durationInSeconds = perkData.baseDuration; // ExplorerPerkData'ya eklenecek yeni alan
                                      // Şimdilik test için varsayılan süreler:
        if (perkData.tag == ExplorerTag.ExplorerTime) durationInSeconds = 15 * 60; // 15dk varsayalım
        else { durationInSeconds = 10; Debug.LogWarning($"[{perkData.name}] Zamanlayıcı süresi tanımlanmamış, 10sn kullanılıyor."); }


        float finalDuration = durationInSeconds - reduction;

        // Zamanlayıcıyı ExplorerTimerManager üzerinden başlat
        ExplorerTimerManager.Instance.StartTimer(
            timerID,                                    // Benzersiz ID
            finalDuration,                              // Hesaplanan süre
            () => CompletePerkAfterTimer(perkData),     // Süre dolunca çalışacak metot
            (remainingTime) => UpdateUITimer(timerID, remainingTime) // Süre güncellendikçe çalışacak metot
        );

        // UI'ı "çalışıyor" durumuna getir (SetActiveTimerView zaten UpdateUITimer içinde çağrılabilir)
        // perkUI?.SetActiveTimerView(true); // Bu satır UpdateUITimer'a taşınabilir
    }


    // Eski kodun mantığı korundu
    

    // Eski kodun mantığı korundu
    public void PurchasePerk(ExplorerPerkData perkData)
    {
        // Maliyeti KONTROL ET ve TÜKET (Manager'daki doğru metot ile)
        if (!CheckAndConsumeRequirements(perkData.purchasePrice, true)) // true = harca
        {
            GameConsole.Instance?.AddMessage("<color=red>Satın almak için kaynakların yetersiz!</color>"); // Eski koddan
            // UI'ı bulup sıfırlama (bu biraz karmaşık, UI eventi dinlese daha iyi olurdu)
            ExplorerPerkUI perkUI = _instantiatedPerkItems.Find(ui => ui._perkData == perkData);
            perkUI?.ResetButton();
            return;
        }
        CompletePerk(perkData);
    }

    // Eski kodun mantığı korundu
    private void CompletePerk(ExplorerPerkData perkData)
    {
        Debug.Log($"Perk tamamlandı/satın alındı: {perkData.name}");

        // Perk ödülünü ver
        if (perkData?.reward?.perkToGrant != null) // perkToGrant null değilse
        {
            OnExplorerPerkCompleted?.Invoke(perkData.reward); // Event'i tetikle (PerkReward gönder)
        }
        else
        {
            Debug.LogWarning($"[{perkData?.name ?? "Bilinmeyen Perk"}] Tamamlandı ama geçerli bir PerkReward/PerkDefinition bulunamadı.");
        }
    


        _currentLeftPerkIndex++;

        // KAYIT ARTIK BURADA YAPILMIYOR
        // PlayerPrefs.SetInt("ExplorerLeftPerkIndex", _currentLeftPerkIndex);

        RefreshLeftPanel(); // UI'ı yenile
        CheckArea2Unlock(); // Alan kilidini kontrol et (eski kodda yoktu ama mantıklı)
    }
    #endregion

    #region Sağ Panel (Quests) Mantığı

    // Eski kodun mantığı korundu
    public void RefreshRightPanel()
    {
        foreach (Transform child in rightPanelContainer) { Destroy(child.gameObject); }
        _instantiatedQuestItems.Clear();

        foreach (ExplorerQuestData questData in rightPanelQuests)
        {
            // --- Sunak Özel Durumu (Eski koddan) ---
            bool isSunakQuest = questData.tag == ExplorerTag.Unlock; // Tag kontrolü
            bool sunakCompleted = GetExplorerQuestCompletionCount(questData.questID) > 0;

            if (isSunakQuest && sunakCompleted)
            {
                if (sunakPrefab != null)
                {
                    Instantiate(sunakPrefab, rightPanelContainer);
                }
                continue; // Sunak gösterildiyse görevi gösterme
            }
            // --- Sunak Bitiş ---

            bool isUnlocked = _unlockedExplorerQuests.Contains(questData.questID);

            // Kilidi açılmamışsa veya ilk görev değilse (ve kilitliyse)
            if (!isUnlocked && questData != rightPanelQuests[0])
            {
                // Kilitli göster (Eski kod mantığı)
                GameObject questGO = Instantiate(explorerQuestPrefab, rightPanelContainer);
                ExplorerQuestUI questUI = questGO.GetComponent<ExplorerQuestUI>();
                if(questUI != null)
                {
                    questUI.Setup(this, questData, GetExplorerQuestCompletionCount(questData.questID));
                    questUI.SetLocked(true); // Kilidi göster
                    _instantiatedQuestItems.Add(questUI);
                }
                continue;
            }

            // Kilidi açıksa normal görevi oluştur
            GameObject questItemGO = Instantiate(explorerQuestPrefab, rightPanelContainer);
            ExplorerQuestUI questItemUI = questItemGO.GetComponent<ExplorerQuestUI>();
            if (questItemUI != null)
            {
                questItemUI.Setup(this, questData, GetExplorerQuestCompletionCount(questData.questID));
                questItemUI.SetLocked(false); // Kilit açık
                _instantiatedQuestItems.Add(questItemUI);

                // Eğer görev zaten aktifse UI'ı güncelle (örn: oyun yüklenince)
                if (_activeExplorerQuests.ContainsKey(questData.questID))
                {
                    // TODO: Aktif görevin kalan süresini alıp UI'ı güncellemek gerekebilir.
                    // Bu, Coroutine'leri kaydetmediğimiz için zor. Şimdilik sadece
                    // progress bar'ı 0'da başlatıp tekrar başlatılmasını bekleyebiliriz.
                    // Veya Load anında aktif görevleri sıfırlayabiliriz.
                }
            }
        }
    }

    // Eski kodun mantığı korundu
    public bool IsQuestActive(string questID) => _activeExplorerQuests.ContainsKey(questID);

    // Eski kodun mantığı korundu
    public void CancelExplorerQuest(string questID)
    {
        // Coroutine yerine TimerManager'dan iptal et
        if (ExplorerTimerManager.Instance.IsTimerActive(questID))
        {
            ExplorerTimerManager.Instance.CancelTimer(questID); // Bu, OnUpdate(0) çağırarak UI'ı da sıfırlamalı
            _activeExplorerQuests.Remove(questID); // Aktif listesinden de çıkaralım
            Debug.Log($"Explorer Görevi (Zamanlayıcı) iptal edildi: {questID}");
        }
        // _activeExplorerQuests listesinde olup TimerManager'da olmayan durumlar olabilir mi? Kontrol edelim.
        else if (_activeExplorerQuests.ContainsKey(questID))
        {
            // Zamanlayıcısı yok ama listede var? Bu durum olmamalı ama varsa temizleyelim.
            _activeExplorerQuests.Remove(questID);
            Debug.LogWarning($"Explorer Görevi ({questID}) aktif listedeydi ama zamanlayıcısı yoktu? Yine de kaldırıldı.");
            // UI sıfırlama
            UpdateExplorerQuestProgress(questID, 0f);
            UpdateExplorerQuestTimerText(questID, 0f);
        }
    }


    // Eski kodun mantığı korundu
    public void StartExplorerQuest(ExplorerQuestData questData)
    {
        if (questData == null || ExplorerTimerManager.Instance.IsTimerActive(questData.questID)) // Coroutine yerine TimerManager kontrolü
        {
            // Eğer normal (zaman bazlı olmayan) bir görevse ve zaten aktif değilse,
            // yine de tamamlanmasına izin verilebilir mi? Tasarıma bağlı.
            // Şimdilik zamanlayıcı aktifse başlatmıyoruz.
            if (ExplorerTimerManager.Instance.IsTimerActive(questData.questID))
                Debug.LogWarning($"[{questData.questID}] Zamanlayıcısı zaten aktif.");
            return;
        }


        int completions = GetExplorerQuestCompletionCount(questData.questID);
        if (questData.completionLimit > 0 && completions >= questData.completionLimit)
        {
            Debug.Log($"Explorer Görevi {questData.questID} limitine ulaştı.");
            return;
        }

        if (!CheckAndConsumeRequirements(questData.requirements, true))
        {
            GameConsole.Instance?.AddMessage($"<color=red>{questData.questID} için kaynaklar yetersiz!</color>");
            return;
        }

        // --- Süre Hesabı ---
        float finalDuration;
        if (questData.isTimerBased) // Sadece zaman bazlıysa süre hesapla
        {
            float reduction = (PerkManager.Instance != null) ? PerkManager.Instance.GetBonusFromPerks(PerkEffectType.GetExplorerTimeReduction) : 0f;
            finalDuration = questData.baseCompletionTime - reduction;
            if (finalDuration < 0.2f && questData.baseCompletionTime > 0) finalDuration = 0.2f; // Minimum süre
        }
        else // Zaman bazlı değilse süre 0'dır
        {
            finalDuration = 0f;
        }
        // --------------------

        if (finalDuration <= 0f) // Süre yoksa veya zaman bazlı değilse anında tamamla
        {
            CompleteExplorerQuest(questData);
        }
        else // Zaman bazlıysa TimerManager'ı başlat
        {
            _activeExplorerQuests.Add(questData.questID, null); // Coroutine yerine sadece ID'yi ekle (aktif olduğunu bilmek için)

            ExplorerTimerManager.Instance.StartTimer(
                questData.questID,                          // Benzersiz ID
                finalDuration,                              // Hesaplanan süre
                () => CompleteExplorerQuestAfterTimer(questData), // Süre dolunca çalışacak metot
                (remainingTime) => UpdateUITimer(questData.questID, remainingTime) // Süre güncellendikçe çalışacak metot
            );

            // UI'ı "çalışıyor" durumuna getir (UpdateUITimer yapacak)
            // UpdateExplorerQuestProgress(questData.questID, 0.01f); // Başladığını göstermek için küçük bir ilerleme?
            // UpdateExplorerQuestTimerText(questData.questID, finalDuration);
        }
    }


    // Eski kodun mantığı korundu (timer hesaplaması düzeltildi)
    

    // Eski kodun mantığı korundu
    private void CompleteExplorerQuest(ExplorerQuestData questData)
    {
        if (questData == null) return;

        int oldCompletions = GetExplorerQuestCompletionCount(questData.questID);
        int newCompletions = oldCompletions + 1;
        _explorerQuestCompletions[questData.questID] = newCompletions;

        Debug.Log($"Explorer Görevi tamamlandı: {questData.questID} (Toplam: {newCompletions})");

        // Ödülleri Dağıt
        DistributeExplorerRewards(questData, newCompletions);

        // Bir sonraki görevin kilidini aç
        if (!string.IsNullOrEmpty(questData.unlocksQuestID) && !_unlockedExplorerQuests.Contains(questData.unlocksQuestID))
        {
            _unlockedExplorerQuests.Add(questData.unlocksQuestID);
            Debug.Log($"Yeni Explorer Görevi açıldı: {questData.unlocksQuestID}");
        }

        // KAYIT ARTIK BURADA YAPILMIYOR
        // SaveExplorerQuestProgress();

        RefreshRightPanel(); // UI'ı yenile
        UpdateSunakButtonVisibility(); // Sunak butonunu kontrol et
        CheckArea2Unlock(); // Alan kilidini kontrol et
    }
    

    // Eski kodun mantığı korundu
    private void DistributeExplorerRewards(ExplorerQuestData quest, int newCompletionCount)
    {
        int rewardIndex = newCompletionCount - 1;

        if (quest.rewardsPerCompletion == null || quest.rewardsPerCompletion.Count <= rewardIndex)
        {
            Debug.LogWarning($"Explorer Görevi ({quest.questID}) için {newCompletionCount}. tamamlamada ödül tanımlanmamış.");
            return;
        }

        ExplorerReward reward = quest.rewardsPerCompletion[rewardIndex];
        if (reward == null) return;

        // Stat Ödülleri
        if (reward.statRewards != null)
        {
            foreach (var statReward in reward.statRewards)
            {
                if (StatManager.Instance != null) StatManager.Instance.AddStat(statReward.statToReward.ToString(), statReward.amount);
                GameConsole.Instance?.AddMessage($"<color=cyan>+{statReward.amount} {statReward.statToReward}</color> kazanıldı ({quest.questID}).");
            }
        }

        // Eşya Ödülleri
        if (reward.itemRewards != null)
        {
            foreach (var itemDrop in reward.itemRewards)
            {
                if (Inventory.Instance != null && itemDrop.itemToDrop != null) Inventory.Instance.AddItem(itemDrop.itemToDrop, itemDrop.amount);
                GameConsole.Instance?.AddMessage($"<color=orange>+{itemDrop.amount} {itemDrop.itemToDrop?.itemName ?? "Bilinmeyen Eşya"}</color> elde edildi ({quest.questID}).");
            }
        }

        // Perk Ödülleri
        if (reward.perkRewards != null)
        {
            foreach (var perkReward in reward.perkRewards) // perkReward'ın türü ExplorerQuestData.ExplorerReward içindeki List<PerkReward>'dan geliyor
            {
                // Null kontrolleri
                if (PerkManager.Instance != null && perkReward?.perkToGrant != null && perkReward.amount > 0)
                {
                    // PerkManager'ın doğru AddPerk metodunu çağır (PerkDefinition ve int ile)
                    PerkManager.Instance.AddPerk(perkReward.perkToGrant, perkReward.amount); // Düzeltilmiş çağrı
                                                                                             // PerkManager zaten konsola yazdırıyor, buradaki log'a gerek yok.
                }
                else
                {
                    Debug.LogWarning($"[{quest.questID}] Geçersiz PerkReward veya PerkManager bulunamadı.");
                }
            }
        }
    
    }

    // Eski koddaki UI güncelleme metotları
    public void UpdateExplorerQuestProgress(string questID, float progress)
    {
        ExplorerQuestUI itemToUpdate = _instantiatedQuestItems.Find(item => item != null && item.GetQuestID() == questID);
        itemToUpdate?.UpdateProgressBar(progress);
    }

    public void UpdateExplorerQuestTimerText(string questID, float remainingTime)
    {
        ExplorerQuestUI itemToUpdate = _instantiatedQuestItems.Find(item => item != null && item.GetQuestID() == questID);
        itemToUpdate?.UpdateTimerText(remainingTime);
    }

    // Eski koddan
    public int GetExplorerQuestCompletionCount(string questID)
    {
        _explorerQuestCompletions.TryGetValue(questID, out int count);
        return count;
    }

    


    #endregion

    #region Genel Yardımcı Fonksiyonlar (Gereksinim Kontrolü - ESKİ KODDAN ALINDI)

    /// <summary>
    /// Verilen gereksinim listesinin tamamının karşılanıp karşılanmadığını KONTROL EDER (HARCAMA YAPMAZ).
    /// </summary>
    public bool CheckRequirementsMet(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return true;
        foreach (Requirement req in requirements)
        {
            if (!IsRequirementMet(req)) return false;
        }
        return true;
    }

    /// <summary>
    /// Tek bir gereksinimin karşılanıp karşılanmadığını KONTROL EDER (HARCAMA YAPMAZ).
    /// </summary>
    private bool IsRequirementMet(Requirement req)
    {

        switch (req.reqType)
        {
            case RequirementType.Level:
                return LevelManager.Instance != null && LevelManager.Instance.currentLevel >= req.requiredValue;
            case RequirementType.Quest: // Ana görev sistemindeki görevler
                return QuestManager.Instance != null && QuestManager.Instance.GetCompletionCount(req.requirementName) > 0;
            case RequirementType.Item:
                ItemData item = ItemManager.Instance?.GetItemByName(req.requirementName);
                return item != null && Inventory.Instance != null && Inventory.Instance.HasItem(item, req.requiredValue);
            case RequirementType.Stat:
                return StatManager.Instance != null && StatManager.Instance.GetTotalStat(req.requirementName) >= req.requiredValue;
            case RequirementType.Gold:
                return CurrencyManager.Instance != null && CurrencyManager.Instance.gold >= req.requiredValue;
            case RequirementType.NexusCoin:
                return CurrencyManager.Instance != null && CurrencyManager.Instance.nexusCoin >= req.requiredValue;
            case RequirementType.People:
                return CurrencyManager.Instance != null && CurrencyManager.Instance.people >= req.requiredValue;
            case RequirementType.Health: // Gereksinim olarak can (örn: kilidi açmak için >50 can)
                return ResourceManager.Instance != null && ResourceManager.Instance.currentHealth >= req.requiredValue;
            case RequirementType.Energy: // Gereksinim olarak enerji
                return ResourceManager.Instance != null && ResourceManager.Instance.currentEnergy >= req.requiredValue;
            case RequirementType.Mana: // Gereksinim olarak mana
                return ResourceManager.Instance != null && ResourceManager.Instance.currentMana >= req.requiredValue;

            // Maksimum değer GEREKSİNİMLERİ (örn: kilidi açmak için max health > 100)
            case RequirementType.MaxHealth:
                 return ResourceManager.Instance != null && ResourceManager.Instance.maxHealth >= req.requiredValue;
            case RequirementType.MaxEnergy:
                 return ResourceManager.Instance != null && ResourceManager.Instance.maxEnergy >= req.requiredValue;
            case RequirementType.MaxMana:
                 return ResourceManager.Instance != null && ResourceManager.Instance.maxMana >= req.requiredValue;

             // Özel Explorer Görev Tamamlama Gereksinimi
            case RequirementType.ExplorerQuest: // Başka bir explorer görevinin tamamlanmış olması
                return GetExplorerQuestCompletionCount(req.requirementName) > 0; // Kendi içindeki sayaca bak
            case RequirementType.ExplorerTime:

                return true;

             // Özel Perk Satın Alma Gereksinimi
             case RequirementType.Perk:
                // requirementName alanına yazılan string'i PerkName enum'una çevirmeyi dene
                if (Enum.TryParse<PerkName>(req.requirementName, true, out PerkName requiredPerkEnum)) // true: büyük/küçük harf duyarsız
                {
                    // PerkManager'daki yeni GetPerkCount(PerkName) metodunu çağır
                    return PerkManager.Instance != null && PerkManager.Instance.GetPerkCount(requiredPerkEnum) > 0;
                }
                else
                {
                    // Eğer requirementName geçerli bir PerkName enum değeri değilse uyar ve false dön
                    Debug.LogWarning($"IsRequirementMet: Geçersiz Perk adı '{req.requirementName}' requirementName olarak belirtilmiş.");
                    return false;
                }
       
            default:
                Debug.LogWarning($"Bilinmeyen gereksinim tipi (IsRequirementMet): {req.reqType}");
                return false;
        }
    }

    /// <summary>
    /// Verilen gereksinim listesini KONTROL EDER ve 'consume' true ise HARCAR.
    /// </summary>
    public bool CheckAndConsumeRequirements(List<Requirement> requirements, bool consume)
    {
        if (requirements == null || requirements.Count == 0) return true;

        // 1. Adım: Önce Hepsini KONTROL ET
        foreach (Requirement req in requirements)
        {

            switch (req.reqType)
            {
                case RequirementType.Item:
                    ItemData item = ItemManager.Instance?.GetItemByName(req.requirementName);
                    if (item == null || Inventory.Instance == null || !Inventory.Instance.HasItem(item, req.requiredValue)) return false;
                    break;
                case RequirementType.Gold:
                    if (CurrencyManager.Instance == null || CurrencyManager.Instance.gold < req.requiredValue) return false;
                    break;
                case RequirementType.NexusCoin:
                    if (CurrencyManager.Instance == null || CurrencyManager.Instance.nexusCoin < req.requiredValue) return false;
                    break;
                case RequirementType.Health: // Maliyet olarak can
                    if (ResourceManager.Instance == null || ResourceManager.Instance.currentHealth <= req.requiredValue) return false; // Eşitse ölür
                    break;
                case RequirementType.Energy: // Maliyet olarak enerji
                    if (ResourceManager.Instance == null || ResourceManager.Instance.currentEnergy < req.requiredValue) return false;
                    break;
                case RequirementType.Mana: // Maliyet olarak mana
                    if (ResourceManager.Instance == null || ResourceManager.Instance.currentMana < req.requiredValue) return false;
                    break;
                case RequirementType.People: // Maliyet olarak nüfus
                    if (CurrencyManager.Instance == null || CurrencyManager.Instance.people < req.requiredValue) return false;
                    break;
                // Max değerleri HARCAMA (Negatif etki)
                case RequirementType.MaxHealth:
                    if (req.requiredValue < 0 && (ResourceManager.Instance == null || (ResourceManager.Instance.maxHealth + req.requiredValue) < 1)) return false;
                    break;
                case RequirementType.MaxEnergy:
                    if (req.requiredValue < 0 && (ResourceManager.Instance == null || (ResourceManager.Instance.maxEnergy + req.requiredValue) < 1)) return false;
                    break;
                case RequirementType.ExplorerTime:
                    if (!IsRequirementMet(req)) return false; // Zaman bazlı görevler için kontrol
                    break;
                case RequirementType.MaxMana:
                    if (req.requiredValue < 0 && (ResourceManager.Instance == null || (ResourceManager.Instance.maxMana + req.requiredValue) < 1)) return false;
                    break;
                // Stat, Level, Quest gibi şeyler harcanamaz, sadece kontrol edilir (IsRequirementMet içinde)
                case RequirementType.Stat:
                case RequirementType.Level:
                case RequirementType.Quest:
                case RequirementType.ExplorerQuest:
                case RequirementType.Perk:
                    if (!IsRequirementMet(req)) return false; // Harcanmaz ama yine de kontrol edilmeli
                    break;
            }
        }

        // 2. Adım: Eğer 'consume' true ise, şimdi HARCA
        if (consume)
        {
            foreach (Requirement req in requirements)
            {

                switch (req.reqType)
                {
                    case RequirementType.Item:
                        ItemData item = ItemManager.Instance?.GetItemByName(req.requirementName);
                        if (item != null && Inventory.Instance != null) Inventory.Instance.RemoveItem(item, req.requiredValue);
                        GameConsole.Instance?.AddMessage($"<color=red>-{req.requiredValue} {item?.itemName ?? req.requirementName}</color> harcandı.");
                        break;
                    case RequirementType.Gold:
                        if (CurrencyManager.Instance != null) CurrencyManager.Instance.SpendGold(req.requiredValue);
                        GameConsole.Instance?.AddMessage($"<color=red>-{req.requiredValue} Altın</color> harcandı.");
                        break;
                    case RequirementType.NexusCoin:
                        if (CurrencyManager.Instance != null) CurrencyManager.Instance.SpendNexusCoin(req.requiredValue);
                        GameConsole.Instance?.AddMessage($"<color=red>-{req.requiredValue} Nexus Coin</color> harcandı.");
                        break;
                    case RequirementType.Health:
                        if (ResourceManager.Instance != null) ResourceManager.Instance.ModifyHealth(-req.requiredValue);
                        GameConsole.Instance?.AddMessage($"<color=red>-{req.requiredValue} Can</color> harcandı.");
                        break;
                    case RequirementType.Energy:
                        if (ResourceManager.Instance != null) ResourceManager.Instance.ModifyEnergy(-req.requiredValue);
                        GameConsole.Instance?.AddMessage($"<color=red>-{req.requiredValue} Enerji</color> harcandı.");
                        break;
                    case RequirementType.Mana:
                        if (ResourceManager.Instance != null) ResourceManager.Instance.ModifyMana(-req.requiredValue);
                        GameConsole.Instance?.AddMessage($"<color=red>-{req.requiredValue} Mana</color> harcandı.");
                        break;
                    case RequirementType.People:
                        if (CurrencyManager.Instance != null) CurrencyManager.Instance.SpendPeople(req.requiredValue);
                        GameConsole.Instance?.AddMessage($"<color=red>-{req.requiredValue} Nüfus</color> harcandı.");
                        break;
                    case RequirementType.MaxHealth: // Negatif etkiyi uygula
                        if (ResourceManager.Instance != null) ResourceManager.Instance.ModifyMaxHealth(req.requiredValue);
                        GameConsole.Instance?.AddMessage($"Maksimum Can {(req.requiredValue > 0 ? "+" : "")}{req.requiredValue} değişti.");
                        break;
                    case RequirementType.MaxEnergy:
                        if (ResourceManager.Instance != null) ResourceManager.Instance.ModifyMaxEnergy(req.requiredValue);
                        GameConsole.Instance?.AddMessage($"Maksimum Enerji {(req.requiredValue > 0 ? "+" : "")}{req.requiredValue} değişti.");
                        break;
                    case RequirementType.MaxMana:
                        if (ResourceManager.Instance != null) ResourceManager.Instance.ModifyMaxMana(req.requiredValue);
                        GameConsole.Instance?.AddMessage($"Maksimum Mana {(req.requiredValue > 0 ? "+" : "")}{req.requiredValue} değişti.");
                        break;
                    case RequirementType.ExplorerTime:

                        break;
                        // Stat, Level, Quest harcanmaz
                }
            }
        }

        return true; // Kontrol başarılı (ve gerekirse harcama yapıldı)
    }

    /// <summary>
    /// Tek bir gereksinimi kontrol eder ve UI için formatlanmış, renkli bir string döndürür.
    /// </summary>
    /// <param name="req">Formatlanacak gereksinim.</param>
    /// <param name="forceMetColor">Rengin her zaman 'karşılandı' olarak mı gösterileceği.</param>
    /// <returns>Formatlanmış string (örn: "<color=#00FF00>- Seviye 5 (3)</color>")</returns>
    public string GetFormattedRequirementString(Requirement req, bool forceMetColor = false)
    {

        // Gereksinimin karşılanıp karşılanmadığını kontrol et
        bool isMet = forceMetColor || IsRequirementMet(req); // Kendi içindeki metodu kullan
        string colorHex = isMet ? _metColorHex : _notMetColorHex;
        string reqText = "";
        string currentVal = "";

        // Metin oluşturma (UI script'lerindeki switch-case mantığı buraya taşındı)
        try
        {
            switch (req.reqType)
            {
                case RequirementType.Level:
                    reqText = $"Seviye {req.requiredValue}";
                    currentVal = $"({LevelManager.Instance?.currentLevel ?? 0})";
                    break;
                case RequirementType.Quest:
                    reqText = $"Görevi tamamla: '{req.requirementName}'";
                    break;
                case RequirementType.MaxHealth:
                    reqText = $"Keşif Görevini Tamamla: '{req.requirementName}'";
                    // İsteğe bağlı: Tamamlanma sayısını gösterebiliriz
                    // currentVal = $"({GetExplorerQuestCompletionCount(req.requirementName)}/1)";
                    break;
                case RequirementType.Item:
                    ItemData item = ItemManager.Instance?.GetItemByName(req.requirementName);
                    int currentAmount = (item != null && Inventory.Instance != null) ? Inventory.Instance.GetItemCount(item) : 0;
                    reqText = $"{req.requiredValue} x {item?.itemName ?? req.requirementName}";
                    currentVal = $"({currentAmount})";
                    break;
                case RequirementType.Stat:
                    float currentStat = StatManager.Instance != null ? StatManager.Instance.GetTotalStat(req.requirementName) : 0;
                    reqText = $"{req.requiredValue} {req.requirementName} Stat";
                    currentVal = $"({currentStat:F0})";
                    break;
                case RequirementType.Gold:
                    reqText = $"{req.requiredValue:F0} Altın";
                    currentVal = $"({CurrencyManager.Instance?.gold ?? 0:F0})";
                    break;
                case RequirementType.NexusCoin:
                    reqText = $"{req.requiredValue:F0} Nexus Coin";
                    currentVal = $"({CurrencyManager.Instance?.nexusCoin ?? 0:F0})";
                    break;
                case RequirementType.People:
                    reqText = $"{req.requiredValue:F0} Nüfus";
                    currentVal = $"({CurrencyManager.Instance?.people ?? 0:F0})";
                    break;
                case RequirementType.Health: // Hem gereksinim hem maliyet olabilir
                    reqText = $"{req.requiredValue:F0} Can";
                    currentVal = $"({ResourceManager.Instance?.currentHealth ?? 0:F0})";
                    break;
                case RequirementType.Energy:
                    reqText = $"{req.requiredValue:F0} Enerji";
                    currentVal = $"({ResourceManager.Instance?.currentEnergy ?? 0:F0})";
                    break;
                case RequirementType.Mana:
                    reqText = $"{req.requiredValue:F0} Mana";
                    currentVal = $"({ResourceManager.Instance?.currentMana ?? 0:F0})";
                    break;
                //* case RequirementType.MaxHealth:
                //    reqText = $"{(req.requiredValue > 0 ? "Maks. Can >= " : "")}{req.requiredValue:F0} Maks. Can"; // Maliyetse sadece değeri yaz
                 //   if (req.requiredValue > 0) currentVal = $"({ResourceManager.Instance?.maxHealth ?? 0:F0})"; // Sadece gereksinimse mevcut max'ı göster
                 //   isMet = forceMetColor || IsRequirementMet(req); // Rengi tekrar kontrol et (özellikle negatif maliyet için)
                //    break;//
                case RequirementType.MaxEnergy:
                    reqText = $"{(req.requiredValue > 0 ? "Maks. Enerji >= " : "")}{req.requiredValue:F0} Maks. Enerji";
                    if (req.requiredValue > 0) currentVal = $"({ResourceManager.Instance?.maxEnergy ?? 0:F0})";
                    isMet = forceMetColor || IsRequirementMet(req);
                    break;
                case RequirementType.MaxMana:
                    reqText = $"{(req.requiredValue > 0 ? "Maks. Mana >= " : "")}{req.requiredValue:F0} Maks. Mana";
                    if (req.requiredValue > 0) currentVal = $"({ResourceManager.Instance?.maxMana ?? 0:F0})";
                    isMet = forceMetColor || IsRequirementMet(req);
                    break;
                case RequirementType.Perk:
                    reqText = $"Perk'e Sahip Ol: '{req.requirementName}'";
                    // İsteğe bağlı: Enum'a çevirip kontrol edip (Var)/(Yok) yazdırılabilir
                    // bool hasPerk = false;
                    // if (Enum.TryParse<PerkName>(req.requirementName, true, out PerkName perkEnum)) {
                    //     hasPerk = PerkManager.Instance != null && PerkManager.Instance.GetPerkCount(perkEnum) > 0;
                    // }
                    // currentVal = hasPerk ? "(Var)" : "(Yok)";
                    break;
       
                case RequirementType.ExplorerTime:
                    reqText = $"Keşif Süresi Görevi";
                    break;
                default:
                    reqText = $"{req.requiredValue} {req.requirementName}";
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"GetFormattedRequirementString hatası ({req.reqType}): {ex.Message}");
            reqText = $"HATA: {req.reqType}";
            isMet = false;
        }

        return $"<color=#{colorHex}>- {reqText} {currentVal}</color>";
    }



    #endregion

    #region Alan Kilidi (Area 2) Güncellemesi (Eski koddan)

    public void CheckArea2Unlock()
    {
        // Sol panel bitti mi?
        bool leftPanelDone = _currentLeftPerkIndex >= leftPanelPerks.Count;

        // Sağ panel bitti mi?
        bool rightPanelDone = true;
        foreach(var questData in rightPanelQuests)
        {
            // Bir görev bile limitine ulaşmadıysa, panel bitmemiştir.
            if (questData.completionLimit > 0 && GetExplorerQuestCompletionCount(questData.questID) < questData.completionLimit)
            {
                rightPanelDone = false;
                break;
            }
        }

        bool canUnlock = leftPanelDone && rightPanelDone;

        if (area2UnlockButton != null)
        {
            area2UnlockButton.interactable = canUnlock;
            // TODO: Butonun onClick event'ini burada dinamik olarak ayarlamak daha iyi olabilir.
            // area2UnlockButton.onClick.RemoveAllListeners();
            // if(canUnlock) area2UnlockButton.onClick.AddListener(() => GoToArea(area2Panel));
        }
    }

    // Eski koddan
    private void UpdateSunakButtonVisibility()
    {
        if (openSunakButton == null || rightPanelQuests.Count < 2) return;

        // Planına göre Sunak görevi 2. sıradaki görev (index 1) idi
        string sunakQuestID = rightPanelQuests.FirstOrDefault(q => q.tag == ExplorerTag.Unlock)?.questID; // Daha güvenli bulma

        if (!string.IsNullOrEmpty(sunakQuestID))
        {
            bool sunakCompleted = GetExplorerQuestCompletionCount(sunakQuestID) > 0;
            openSunakButton.gameObject.SetActive(sunakCompleted);
        }
        else
        {
            openSunakButton.gameObject.SetActive(false); // Sunak görevi bulunamazsa butonu gizle
        }
    }

    #endregion

    #region Zamanlayıcı Geri Çağrıları (Callbacks)

    /// <summary>
    /// ExplorerTimerManager tarafından bir PERK zamanlayıcısı bittiğinde çağrılır.
    /// </summary>
    public void CompletePerkAfterTimer(ExplorerPerkData perkData)
    {
        Debug.Log($"[ExplorerManager] Zamanlayıcı bitti, CompletePerk çağrılıyor: {perkData?.name ?? "NULL"}");
        // Zaten var olan CompletePerk metodunu çağırıyoruz.
        if (perkData != null)
        {
            CompletePerk(perkData);
        }
    }


    /// <summary>
    /// ExplorerTimerManager tarafından bir QUEST zamanlayıcısı bittiğinde çağrılır.
    /// </summary>
    public void CompleteExplorerQuestAfterTimer(ExplorerQuestData questData)
    {
        Debug.Log($"[ExplorerManager] Zamanlayıcı bitti, CompleteExplorerQuest çağrılıyor: {questData?.questID ?? "NULL"}");
        // Zaten var olan CompleteExplorerQuest metodunu çağırıyoruz.
        if (questData != null)
        {
            CompleteExplorerQuest(questData);
        }
    }


    /// <summary>
    /// ExplorerTimerManager tarafından bir zamanlayıcı güncellendiğinde çağrılır.
    /// İlgili UI elemanını bulup günceller.
    /// </summary>
    public void UpdateUITimer(string uniqueID, float remainingTime)
    {
        // Gelen ID'ye göre doğru UI elemanını bulmamız lazım.
        // ID'nin yapısına göre Perk mi Quest mi olduğunu anlayabiliriz.

        if (uniqueID.EndsWith("_Timer")) // Perk zamanlayıcısı olduğunu varsayalım (QperkName_Timer gibi)
        {
            string perkName = uniqueID.Replace("_Timer", "");
            ExplorerPerkUI perkUI = _instantiatedPerkItems.Find(ui => ui != null && ui._perkData != null && ui._perkData.name == perkName); // İsme göre bul
            if (perkUI != null)
            {
                perkUI.UpdateTimerText(remainingTime);
                // Zamanlayıcı çalışıyorsa arka planı/butonu ayarla
                perkUI.SetActiveTimerView(remainingTime > 0);
            }
            // else { Debug.LogWarning($"[ExplorerManager] UpdateUITimer: Perk UI bulunamadı: {perkName}"); }
        }
        else if (uniqueID.StartsWith("EXPLORER_QUEST_")) // Quest zamanlayıcısı olduğunu varsayalım
        {
            ExplorerQuestUI questUI = _instantiatedQuestItems.Find(ui => ui != null && ui.GetQuestID() == uniqueID); // ID'ye göre bul
            if (questUI != null)
            {
                questUI.UpdateTimerText(remainingTime);
                // Progress bar'ı da güncelleyebiliriz (eğer toplam süreyi biliyorsak)
                // float totalDuration = ... ; // Toplam süreyi bir yerden almamız lazım
                // float progress = (totalDuration > 0) ? 1.0f - (remainingTime / totalDuration) : (remainingTime > 0 ? 0 : 1);
                // questUI.UpdateProgressBar(progress);

                // Sadece zamanlayıcı metnini göster/gizle
                questUI.timerText?.gameObject.SetActive(remainingTime > 0);
                questUI.progressBar?.gameObject.SetActive(remainingTime > 0); // Progress barı da gösterelim
            }
            // else { Debug.LogWarning($"[ExplorerManager] UpdateUITimer: Quest UI bulunamadı: {uniqueID}"); }
        }
        else
        {
            Debug.LogWarning($"[ExplorerManager] UpdateUITimer: Tanımlanamayan ID formatı: {uniqueID}");
        }

    }

    #endregion


    #region Kayıt Sistemi (IGameDataSaveable Uygulaması)

    public ExplorerSaveData GetSaveData()
    {
        Debug.Log("[ExplorerManager] Kayıt verisi oluşturuluyor.");
        return new ExplorerSaveData
        {
            currentLeftPerkIndex = _currentLeftPerkIndex,
            // HashSet'i List'e dönüştürmeye gerek yok, Newtonsoft.Json halleder.
            unlockedQuestIDs = new HashSet<string>(_unlockedExplorerQuests),
            questCompletionCounts = new Dictionary<string, int>(_explorerQuestCompletions)
        };
    }

    public void LoadFromData(ExplorerSaveData data)
    {
        if (data == null)
        {
            Debug.Log("[ExplorerManager] Yüklenecek veri yok, varsayılanlar kullanılıyor.");
            // Yeni oyun durumu
            _currentLeftPerkIndex = 0;
            _unlockedExplorerQuests = new HashSet<string>();
            _explorerQuestCompletions = new Dictionary<string, int>();
            // İlk görevin kilidini aç
            if (rightPanelQuests.Count > 0 && !string.IsNullOrEmpty(rightPanelQuests[0].questID))
            {
                _unlockedExplorerQuests.Add(rightPanelQuests[0].questID);
            }
        }
        else
        {
            Debug.Log("[ExplorerManager] Veri yükleniyor.");
            _currentLeftPerkIndex = data.currentLeftPerkIndex;
            _unlockedExplorerQuests = data.unlockedQuestIDs ?? new HashSet<string>();
            _explorerQuestCompletions = data.questCompletionCounts ?? new Dictionary<string, int>();

            // Yükleme sonrası ilk görevin hala açık olduğundan emin ol (isteğe bağlı)
             if (_unlockedExplorerQuests.Count == 0 && rightPanelQuests.Count > 0 && !string.IsNullOrEmpty(rightPanelQuests[0].questID))
             {
                 _unlockedExplorerQuests.Add(rightPanelQuests[0].questID);
             }
        }

        // Aktif görevleri ve zamanlayıcıları temizle (Coroutine'ler kaydedilmediği için)
        _activeExplorerQuests.Clear();

        // UI'ı yüklenen verilere göre yenile
        // Start metodu zaten çağrıldıysa bile, yüklenen veriye göre tekrar yenilemek önemli.
        if (gameObject.activeInHierarchy) // Obje aktifse UI'ı yenile
        {
             RefreshUI();
        }
        Debug.Log($"[ExplorerManager] Veri yüklendi. Perk Index: {_currentLeftPerkIndex}, Açık Görevler: {_unlockedExplorerQuests.Count}");
    }
    #endregion
}