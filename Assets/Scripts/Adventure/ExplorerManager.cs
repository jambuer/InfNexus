using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Kayıt/Yükleme ve sıralama için
using UnityEngine.UI;
using Unity.VisualScripting; // Button için


/// <summary>
/// Adventure -> Explorer sekmesinin tüm mantığını yönetir.
/// Sol Panel (Perks) ve Sağ Panel (Görevler/Sunak) ilerlemesini kontrol eder.
/// </summary>
public class ExplorerManager : MonoBehaviour
{
    public static ExplorerManager Instance { get; private set; }

    [Header("Sol Panel (Perks)")]
    [Tooltip("Sol paneldeki ScrollView'un Content'i")]
    public Transform leftPanelContainer; 
    [Tooltip("ExplorerPerkUI script'ini içeren prefab")]
    public GameObject explorerPerkPrefab; 
    [Tooltip("Sol paneldeki tüm perk'lerin sıralı listesi (Inspector'dan atanır)")]
    public List<ExplorerPerkData> leftPanelPerks;

    [Header("Sağ Panel (Görevler)")]
    [Tooltip("Sağ paneldeki ScrollView'un Content'i")]
    public Transform rightPanelContainer; 
    [Tooltip("ExplorerQuestUI script'ini içeren prefab")]
    public GameObject explorerQuestPrefab; 
    [Tooltip("Sağ paneldeki tüm görevlerin sıralı listesi (Inspector'dan atanır)")]
    public List<ExplorerQuestData> rightPanelQuests;


    [Header("Sunak (Mağaza)")] // <-- YENİ BÖLÜM
    
    [Tooltip("Satın alma öğelerini içeren ana panel")]
    public GameObject sunakPanelObject;

    [Tooltip("Sunak panelini açan, kalıcı buton")]
    public Button openSunakButton;


    private bool isSunakOpen = false;
    
    public GameObject sunakPrefab; // Planındaki 2. görev için

    [Header("Alan (Area) Yönetimi")]
    public GameObject area1Panel;
    public GameObject area2Panel; // Başlangıçta deaktif olmalı
    [Tooltip("Haritadaki Area 2'ye geçişi sağlayan buton")]
    public Button area2UnlockButton; 

    [Header("Yandan Açılır Harita")]
    public RectTransform mapPanel; 
    public float mapAnimationSpeed = 0.5f;
    private bool isMapOpen = false;
    private Vector2 mapHiddenPos;
    private Vector2 mapVisiblePos;
    private Coroutine mapAnimationCoroutine;

    // --- Sol Panel Durum Takibi ---
    private int _currentLeftPerkIndex = 0; // Hangi perk'in kilidinin açılacağını takip eder
    private Dictionary<string, Coroutine> _activePerkTimers = new Dictionary<string, Coroutine>();
    private List<ExplorerPerkUI> _instantiatedPerkItems = new List<ExplorerPerkUI>();

    // --- Sağ Panel Durum Takibi ---
    private Dictionary<string, Coroutine> _activeExplorerQuests = new Dictionary<string, Coroutine>();
    private Dictionary<string, int> _explorerQuestCompletions = new Dictionary<string, int>();
    private HashSet<string> _unlockedExplorerQuests = new HashSet<string>();
    private List<ExplorerQuestUI> _instantiatedQuestItems = new List<ExplorerQuestUI>();

    // ========================================================================================
    // BAŞLANGIÇ VE YÖNETİM
    // ========================================================================================

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }

    void Start()
    {
        InitializeMapPanel();
        
        // Sol ve Sağ panellerin kayıtlı verilerini yükle
        LoadLeftPanelProgress();
        LoadExplorerQuestProgress();

        // UI'ları ilk defa doldur
        RefreshLeftPanel();
        RefreshRightPanel();

        // Alan (Area) kilitlerini kontrol et
        CheckArea2Unlock(); 
    }

    #region Harita Panel Animasyonu (Değişiklik Yok)

    private void InitializeMapPanel()
    {
        if (mapPanel == null) return;
        mapPanel.gameObject.SetActive(true);
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
        mapAnimationCoroutine = StartCoroutine(AnimatePanel(mapPanel, targetPos, mapAnimationSpeed));
    }
    /// <summary>
      /// Sunak (Mağaza) panelini açar veya kapatır.
      /// </summary>
    public void ToggleSunakPanel()
  {
    if (sunakPanelObject == null) return;

    isSunakOpen = !isSunakOpen;
        sunakPanelObject.SetActive(isSunakOpen);

    // TODO: Planına göre "aşağıdan yukarı açılma" animasyonunu
    // AnimatePanel coroutine'ine benzer bir mantıkla buraya ekleyebilirsin.
    AnimatePanel (sunakPanelObject.GetComponent<RectTransform>(), 
        isSunakOpen ? Vector2.zero : new Vector2(0, -sunakPanelObject.GetComponent<RectTransform>().rect.height), 0.3f);

    if (GameConsole.Instance != null)
        GameConsole.Instance.AddMessage(isSunakOpen ? "Sunak açıldı." : "Sunak kapandı.");
 }

    private IEnumerator AnimatePanel(RectTransform panel, Vector2 targetPos, float duration)
    {
        float time = 0;
        Vector2 startPos = panel.anchoredPosition;
        while (time < duration)
        {
            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        panel.anchoredPosition = targetPos;
        mapAnimationCoroutine = null;
    }

    public void GoToArea(GameObject areaPanelToShow)
    {
        area1Panel.SetActive(area1Panel == areaPanelToShow);
        area2Panel.SetActive(area2Panel == areaPanelToShow);
        ToggleMapPanel(); 
    }

    #endregion

    #region Sol Panel (Perks) Mantığı (Değişiklik Yok)

    public void RefreshLeftPanel()
    {
        // Önce tüm eski UI elemanlarını temizle
        foreach (Transform child in leftPanelContainer)
        {
            Destroy(child.gameObject);
        }
        

        // --- SORUN 1 ÇÖZÜMÜ: TÜM LİSTEYİ GÖSTER ---

        // Listenin tamamını dön
        for (int i = 0; i < leftPanelPerks.Count; i++)
        {
            ExplorerPerkData perkData = leftPanelPerks[i];

            // Bu perk zaten satın alındıysa, listeden kaybolsun (planına göre)
            if (i < _currentLeftPerkIndex)
            {
                continue; // Bu perk'i oluşturma, bir sonrakine geç
            }

            GameObject perkGO = Instantiate(explorerPerkPrefab, leftPanelContainer);
            ExplorerPerkUI perkUI = perkGO.GetComponent<ExplorerPerkUI>();

            if (perkUI != null)
            {
                PerkState stateToSet;

                // Bu, sıradaki aktif perk mi?
                if (i == _currentLeftPerkIndex)
                {
                    // Sıradaki perk. Durumunu kontrol et (Unlockable mı, Payable mı?)
                    bool unlockRequirementsMet = CheckRequirementsMet(perkData.unlockRequirements); // Bu yeni yardımcı fonksiyonu ekleyeceğiz
                    stateToSet = unlockRequirementsMet ? PerkState.Payable : PerkState.Unlockable;
                }
                else // i > _currentLeftPerkIndex
                {
                    // Bu, gelecek bir perk. Tamamen kilitli.
                    stateToSet = PerkState.Locked;
                }

                perkUI.Setup(perkData, stateToSet);
            }
        }

        CheckArea2Unlock();
        _instantiatedPerkItems.Clear();
    }


    public void StartExplorerTimer(ExplorerPerkData perkData, ExplorerPerkUI perkUI)
{
    string timerID = perkData.name + "_Timer";
    if (_activePerkTimers.ContainsKey(timerID)) return; 

    if (!CheckAndConsumeRequirements(perkData.purchasePrice, true))
    {
        GameConsole.Instance.AddMessage("<color=red>Explorer görevi için kaynakların yetersiz!</color>");
        perkUI.ResetButton(); 
        return;
    }

    float reduction = PerkManager.Instance.GetExplorerTimeReduction(); // -180 saniye
    float durationInSeconds = 0f; 

    // Örnek 3: "30 dakika bekleme süresi"
    // Örnek 8: "15 dakika bekleme süresi"
    // NOT: Bu kontrolü ID veya daha güvenilir bir yolla yapmalısın.
    // Şimdilik perk adına göre yapıyorum (çok kırılgandır!):
    if (perkData.name.Contains("3_ExplorerTime")) // Asset adının "Perk_03_ExplorerTime" olduğunu varsayalım
    {
        durationInSeconds = 30 * 60; // 30 dakika
    }
    else if (perkData.name.Contains("8_ExplorerTime")) // Asset adının "Perk_08_ExplorerTime" olduğunu varsayalım
    {
        durationInSeconds = 15 * 60; // 15 dakika
    }

    float finalDuration = durationInSeconds - reduction;
    if (finalDuration <= 0)
    {
        CompletePerk(perkData);
    }
    else
    {
        // Coroutine'e perkUI'ı da ver
        Coroutine timerCoroutine = StartCoroutine(ExplorerTimerCoroutine(perkData, perkUI, finalDuration)); // perkUI eklendi
        _activePerkTimers.Add(timerID, timerCoroutine);
    }
}

    private IEnumerator ExplorerTimerCoroutine(ExplorerPerkData perkData, ExplorerPerkUI perkUI, float duration)
{
    Debug.Log($"{perkData.name} zamanlayıcısı başladı: {duration} saniye.");

    float timer = duration;
    while (timer > 0)
    {
        timer -= Time.deltaTime;
        if (perkUI != null)
        {
            perkUI.UpdateTimerText(timer); // UI'daki metni güncelle
        }
        yield return null; // Bir sonraki frame'e kadar bekle
    }

    if (perkUI != null)
    {
        perkUI.UpdateTimerText(0);
    }

    _activePerkTimers.Remove(perkData.name + "_Timer");
    CompletePerk(perkData);
}

    public void PurchasePerk(ExplorerPerkData perkData)
    {
        if (!CheckAndConsumeRequirements(perkData.purchasePrice, true)) // Maliyeti harca
        {
            GameConsole.Instance.AddMessage("<color=red>Satın almak için kaynakların yetersiz!</color>");
            ExplorerPerkUI perkUI = leftPanelContainer.GetComponentInChildren<ExplorerPerkUI>();
            if(perkUI != null) perkUI.ResetButton();
            return;
        }
        CompletePerk(perkData);
    }

    private void CompletePerk(ExplorerPerkData perkData)
    {
        Debug.Log($"Perk tamamlandı/satın alındı: {perkData.name}");
        if (PerkManager.Instance != null && perkData.reward != null && !string.IsNullOrEmpty(perkData.reward.masteryName))
        {
            PerkManager.Instance.AddPerk(perkData.reward.masteryName, perkData.reward.amount);
        }
        _currentLeftPerkIndex++;
        PlayerPrefs.SetInt("ExplorerLeftPerkIndex", _currentLeftPerkIndex); // İlerlemeyi kaydet
        RefreshLeftPanel();
    }
    
    private void LoadLeftPanelProgress()
    {
         _currentLeftPerkIndex = PlayerPrefs.GetInt("ExplorerLeftPerkIndex", 0);
    }

    #endregion

    #region Sağ Panel (Görevler) Mantığı (YENİ EKLENDİ)

    /// <summary>
    /// Sağ paneli mevcut görev ilerlemesine göre doldurur.
    /// </summary>
    public void RefreshRightPanel()
    {
        foreach (Transform child in rightPanelContainer) { Destroy(child.gameObject); }
        _instantiatedQuestItems.Clear();


        foreach (ExplorerQuestData questData in rightPanelQuests)
        {
            if (!_unlockedExplorerQuests.Contains(questData.questID) && questData != rightPanelQuests[0])
            {
                 // Eğer bu görev kilitliyse ve ilk görev değilse, gösterme VEYA kilitli göster
                 // Şimdilik, sadece kilidi açılmış olanları gösterelim
                 // (Daha sonra kilitli göstermeyi ekleyebiliriz)
                 
                 // Plan: "bir önceki tamamlanmadan kilitli ekran"
                 // Bu UI'ı oluşturup SetLocked(true) diyelim
                 GameObject questGO = Instantiate(explorerQuestPrefab, rightPanelContainer);
                 ExplorerQuestUI questUI = questGO.GetComponent<ExplorerQuestUI>();
                 if(questUI != null)
                 {
                     questUI.Setup(this, questData, GetExplorerQuestCompletionCount(questData.questID));
                     questUI.SetLocked(true); // Kilidi göster
                     _instantiatedQuestItems.Add(questUI);
                 }
                 continue; // Bir sonraki göreve geç
            }
            
            // --- Özel Durum: Sunak (Altar) ---
            // Eğer "Sunak" görevi (Tag: Unlock) tamamlandıysa, görev prefab'ı yerine sunak prefab'ını göster
            if (questData.tag == ExplorerTag.Unlock && GetExplorerQuestCompletionCount(questData.questID) > 0)
            {
                if (sunakPrefab != null)
                {
                    Instantiate(sunakPrefab, rightPanelContainer);
                    // (Sunak prefab'ının kendi açılış/tıklama script'i olmalı)
                }
                continue; // Görevi gösterme, sunağı göster
            }
            // --- Sunak Bitiş ---

            // Görevi oluştur
            GameObject questItemGO = Instantiate(explorerQuestPrefab, rightPanelContainer);
            ExplorerQuestUI questItemUI = questItemGO.GetComponent<ExplorerQuestUI>();
            if (questItemUI != null)
            {
                questItemUI.Setup(this, questData, GetExplorerQuestCompletionCount(questData.questID));
                questItemUI.SetLocked(false); // Kilit açık
                _instantiatedQuestItems.Add(questItemUI);
            }
        }
        CheckArea2Unlock();
    }

    /// <summary>
    /// Sağ panelden bir görevi başlatır (ExplorerQuestUI tarafından çağrılır).
    /// </summary>
    public void StartExplorerQuest(ExplorerQuestData quest)
    {
        if (quest == null || IsQuestActive(quest.questID)) return;
        
        int completions = GetExplorerQuestCompletionCount(quest.questID);
        if (quest.completionLimit > 0 && completions >= quest.completionLimit)
        {
            Debug.Log($"Explorer Görevi {quest.questID} limitine ulaştı.");
            return;
        }

        // Maliyetleri/Gereksinimleri KONTROL ET ve TÜKET
        if (!CheckAndConsumeRequirements(quest.requirements, true)) // true = harca
        {
            GameConsole.Instance.AddMessage($"<color=red>{quest.questID} için kaynaklar yetersiz!</color>");
            return;
        }

        // --- Süre Hesabı ---
        float finalDuration = quest.baseCompletionTime;
        
        if (quest.isTimerBased) // "ExplorerTime" tag'li ise (Plan Örnek 4)
        {
            float reduction = PerkManager.Instance.GetExplorerTimeReduction();
            finalDuration = quest.baseCompletionTime - reduction;
        }
        else // Normal görevse (Plan Örnek 1, 3, 6)
        {
            // Sadece Stat bonusları etki etsin
            float statBonus = (StatCalculator.Instance != null) ? (float)StatCalculator.Instance.currentStats.PercentCooldownReduction : 0f;
            finalDuration = quest.baseCompletionTime / (1 + statBonus);
        }

        if (finalDuration <= 0f)
        {
            // Süre 0 veya daha azsa, anında tamamla
            CompleteExplorerQuest(quest);
        }
        else
        {
            // Zamanlayıcıyı başlat
            Coroutine questCoroutine = StartCoroutine(ProcessExplorerQuestCoroutine(quest, finalDuration));
            _activeExplorerQuests.Add(quest.questID, questCoroutine);
        }
    }

    /// <summary>
    /// Sağ paneldeki bir görevi iptal eder.
    /// </summary>
    public void CancelExplorerQuest(string questID)
    {
        if (_activeExplorerQuests.TryGetValue(questID, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            _activeExplorerQuests.Remove(questID);
            UpdateExplorerQuestProgress(questID, 0f); // UI'ı sıfırla
            UpdateExplorerQuestTimerText(questID, 0f); // Zamanlayıcı metnini sıfırla
            Debug.Log($"Explorer Görevi iptal edildi: {questID}");
            // Not: Harcanan kaynaklar iade edilmez.
        }
    }

    /// <summary>
    /// Sağ panel görev zamanlayıcısını yönetir.
    /// </summary>
    private IEnumerator ProcessExplorerQuestCoroutine(ExplorerQuestData quest, float duration)
{
    float timer = duration;
    while (timer > 0)
    {
        timer -= Time.deltaTime;
        if (timer < 0) timer = 0;

        float progress = 1.0f - (timer / duration); // İlerlemeyi (0 -> 1) hesapla

        // UI'ı GÜNCELLE
        UpdateExplorerQuestProgress(quest.questID, progress); // Slider'ı güncelle
        UpdateExplorerQuestTimerText(quest.questID, timer); // Zamanlayıcı METNİNİ güncelle

        yield return null;
    }

    // Bittiğinden emin ol
    UpdateExplorerQuestProgress(quest.questID, 1f);
    UpdateExplorerQuestTimerText(quest.questID, 0f);

    CompleteExplorerQuest(quest);
}

    /// <summary>
    /// Sağ panel görevini tamamlar, ödülleri dağıtır ve bir sonrakini açar.
    /// </summary>
    private void CompleteExplorerQuest(ExplorerQuestData quest)
    {
        if (quest == null) return;

        _activeExplorerQuests.Remove(quest.questID); // Aktif görevlerden çıkar
        
        // İlk tamamlamada ödülleri dağıt

        // Tamamlama sayısını al ve 1 artır
        int oldCompletions = GetExplorerQuestCompletionCount(quest.questID);
        int newCompletions = oldCompletions + 1;
        _explorerQuestCompletions[quest.questID] = newCompletions;

        Debug.Log($"Explorer Görevi tamamlandı: {quest.questID} (Toplam: {newCompletions})");

        // Ödülleri Dağıt
        DistributeExplorerRewards(quest, newCompletions);

        // Bir sonraki görevin kilidini aç
        if (!string.IsNullOrEmpty(quest.unlocksQuestID) && !_unlockedExplorerQuests.Contains(quest.unlocksQuestID))
        {
            _unlockedExplorerQuests.Add(quest.unlocksQuestID);
            Debug.Log($"Yeni Explorer Görevi açıldı: {quest.unlocksQuestID}");
        }

        // Veriyi kaydet ve UI'ı yenile
        SaveExplorerQuestProgress();
        RefreshRightPanel();
        UpdateSunakButtonVisibility();
    }

    /// <summary>
    /// Sağ panel görevi için doğru sıradaki ödülü verir.
    /// </summary>
    private void DistributeExplorerRewards(ExplorerQuestData quest, int newCompletionCount)
    {
        // Ödül listesinin index'i (1. tamamlama = index 0)
        int rewardIndex = newCompletionCount - 1;

        if (quest.rewardsPerCompletion == null || quest.rewardsPerCompletion.Count <= rewardIndex)
        {
            Debug.LogWarning($"Explorer Görevi ({quest.questID}) için {newCompletionCount}. tamamlamada ödül tanımlanmamış.");
            return;
        }

        ExplorerReward reward = quest.rewardsPerCompletion[rewardIndex];
        if (reward == null) return;

        // 1. Stat Ödülleri
        if (reward.statRewards != null)
        {
            foreach (var statReward in reward.statRewards)
            {
                StatManager.Instance.AddStat(statReward.statToReward.ToString(), statReward.amount);
                GameConsole.Instance.AddMessage($"<color=cyan>+{statReward.amount} {statReward.statToReward}</color> kazanıldı ({quest.questID}).");
            }
        }

        // 2. Eşya Ödülleri
        if (reward.itemRewards != null)
        {
            foreach (var itemDrop in reward.itemRewards)
            {
                // (Sağ panel görevlerinde drop chance olmadığını varsayıyoruz, planına göre garantili)
                Inventory.Instance.AddItem(itemDrop.itemToDrop, itemDrop.amount);
                GameConsole.Instance.AddMessage($"<color=orange>+{itemDrop.amount} {itemDrop.itemToDrop.itemName}</color> elde edildi ({quest.questID}).");
            }
        }
        
        // 3. Perk Ödülleri
        if (reward.perkRewards != null)
        {
             foreach (var perkReward in reward.perkRewards)
             {
                 PerkManager.Instance.AddPerk(perkReward.masteryName, perkReward.amount);
                 // (PerkManager zaten konsola yazdırıyor)
             }
        }
    }

    /// <summary>
    /// İlgili UI progress bar'ını günceller.
    /// </summary>
    public void UpdateExplorerQuestProgress(string questID, float progress)
    {
        if (_instantiatedQuestItems == null) return;
        ExplorerQuestUI itemToUpdate = _instantiatedQuestItems.Find(item => item.GetQuestID() == questID);
        if (itemToUpdate != null)
        {
            itemToUpdate.UpdateProgressBar(progress);
        }
    }

    /// <summary>
/// İlgili UI zamanlayıcı metnini günceller.
/// </summary>
public void UpdateExplorerQuestTimerText(string questID, float remainingTime)
{
    if (_instantiatedQuestItems == null) return;
    ExplorerQuestUI itemToUpdate = _instantiatedQuestItems.Find(item => item != null && item.GetQuestID() == questID);
    if (itemToUpdate != null)
    {
        itemToUpdate.UpdateTimerText(remainingTime);
    }
}

    // Public sorgu fonksiyonları
    public bool IsQuestActive(string questID)
    {
        return _activeExplorerQuests.ContainsKey(questID);
    }

    public int GetExplorerQuestCompletionCount(string questID)
    {
        _explorerQuestCompletions.TryGetValue(questID, out int count);
        return count;
    }

    #endregion

    #region Kayıt & Yükleme (Sağ Panel - YENİ EKLENDİ)
    
    [System.Serializable]
    private class ExplorerQuestSaveData
    {
        // Hangi görevlerin kilidinin açıldığını kaydet
        public List<string> unlockedQuestIDs;
        // Hangi görevin kaç kez tamamlandığını kaydet
        public List<string> questCompletionKeys;
        public List<int> questCompletionValues;
    }

    public void SaveExplorerQuestProgress()
    {
        ExplorerQuestSaveData saveData = new ExplorerQuestSaveData();
        saveData.unlockedQuestIDs = _unlockedExplorerQuests.ToList();
        saveData.questCompletionKeys = _explorerQuestCompletions.Keys.ToList();
        saveData.questCompletionValues = _explorerQuestCompletions.Values.ToList();

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("ExplorerQuestProgress", json);
    }

    public void LoadExplorerQuestProgress()
    {
        if (PlayerPrefs.HasKey("ExplorerQuestProgress"))
        {
            string json = PlayerPrefs.GetString("ExplorerQuestProgress");
            ExplorerQuestSaveData saveData = JsonUtility.FromJson<ExplorerQuestSaveData>(json);

            _unlockedExplorerQuests = new HashSet<string>(saveData.unlockedQuestIDs);
            _explorerQuestCompletions.Clear();
            for (int i = 0; i < saveData.questCompletionKeys.Count; i++)
            {
                _explorerQuestCompletions[saveData.questCompletionKeys[i]] = saveData.questCompletionValues[i];
            }
            Debug.Log("Explorer Sağ Panel (Görev) ilerlemesi yüklendi.");
        }
        else
        {
            // İlk görev her zaman açıktır
            if (rightPanelQuests.Count > 0)
            {
                _unlockedExplorerQuests.Add(rightPanelQuests[0].questID);
            }

            UpdateSunakButtonVisibility();

            if (sunakPanelObject != null)
            {
                sunakPanelObject.SetActive(false); // Başlangıçta kapalı
            }
        }


    }
    
    private void UpdateSunakButtonVisibility()
{
    if (openSunakButton == null || rightPanelQuests.Count < 2) return;

    // Planına göre Sunak görevi 2. sıradaki görev (index 1)
    string sunakQuestID = rightPanelQuests[1].questID; 

    bool sunakUnlocked = GetExplorerQuestCompletionCount(sunakQuestID) > 0;
    openSunakButton.gameObject.SetActive(sunakUnlocked);
}

    #endregion

    #region Genel Yardımcı Fonksiyonlar
    

    /// <summary>
    /// Verilen gereksinim listesinin (kilit açma veya fiyat) tamamının karşılanıp karşılanmadığını KONTROL EDER (HARCAMA YAPMAZ).
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
    /// <summary>
/// Tek bir gereksinimin karşılanıp karşılanmadığını KONTROL EDER (HARCAMA YAPMAZ).
/// </summary>
private bool IsRequirementMet(Requirement req)
{
    // Bu, ExplorerPerkUI'da da olacak olan (ve Sorun 2'yi çözen) ana kontrol mantığıdır.
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

        // --- SORUN 2 İÇİN EKLENEN KONTROLLER ---
        case "gold":
            return (CurrencyManager.Instance != null) && CurrencyManager.Instance.gold >= req.requiredValue;
        case "nexuscoin":
            return (CurrencyManager.Instance != null) && CurrencyManager.Instance.nexusCoin >= req.requiredValue;
        case "people":
             // CurrencyManager'daki değişkenin adını 'people' varsayıyoruz
            return (CurrencyManager.Instance != null) && CurrencyManager.Instance.people >= req.requiredValue;
        case "health": // Fiyat olarak Can
            return (ResourceManager.Instance != null) && ResourceManager.Instance.currentHealth > req.requiredValue; // Eşitse ölür
        case "energy": // Fiyat olarak Enerji
                return (ResourceManager.Instance != null) && ResourceManager.Instance.currentEnergy >= req.requiredValue;
        case "maxenergy": // "Bad" olan, -10 max energy (bu bir maliyet değil, gereksinim)
            return true; // Bu her zaman "karşılanabilir" bir maliyettir
        case "maxhealth": // "Bad" olan, -10 max health (bu bir maliyet değil, gereksinim)
                return true; // Bu her zaman "karşılanabilir" bir maliyettir
        case "mana": // Fiyat olarak Mana
            return (ResourceManager.Instance != null) && ResourceManager.Instance.currentMana >= req.requiredValue;
        case "maxmana": // "Bad" olan, -10 max mana (bu bir maliyet değil, gereksinim)
            return true; // Bu her zaman "karşılanabilir" bir maliyettir
        // --- BİTTİ ---

        default:
            Debug.LogWarning($"Bilinmeyen gereksinim tipi (IsRequirementMet): {req.requirementType}");
            return false;
    }
}
    
    

    /// <summary>
    /// Verilen gereksinim listesini KONTROL EDER ve 'consume' true ise harcar.
    /// Hem Sol (Perk) hem de Sağ (Quest) panel tarafından kullanılır.
    /// </summary>
    public bool CheckAndConsumeRequirements(List<Requirement> requirements, bool consume)
    {
        if (requirements == null || requirements.Count == 0) return true;

        // 1. Adım: Önce Hepsini KONTROL ET
        foreach (Requirement req in requirements)
        {
            switch (req.requirementType.ToLower())
            {
                case "item":
                    ItemData item = ItemManager.Instance.GetItemByName(req.requirementName);
                    if (item == null || !Inventory.Instance.HasItem(item, req.requiredValue)) return false;
                    break;
                case "gold":
                    if (CurrencyManager.Instance.gold < req.requiredValue) return false;
                    break;
                case "nexuscoin":
                    if (CurrencyManager.Instance.nexusCoin < req.requiredValue) return false;
                    break;
                case "health": 
                    if (ResourceManager.Instance.currentHealth <= req.requiredValue) return false; // Eşitse ölür
                    break;
                case "energy": 
                    if (ResourceManager.Instance.currentEnergy < req.requiredValue) return false;
                    break;
                case "people":
                    // CurrencyManager'da 'people' adında bir değişken olduğunu varsayıyoruz
                    // Eğer değişkenin adı farklıysa (örn: currentPeople) onu kullan.
                    if (CurrencyManager.Instance.people < req.requiredValue) return false;
                    break;
                case "mana":
                    if (ResourceManager.Instance.currentMana < req.requiredValue) return false;
                    break;
                case "maxhealth":
                // Çıkarma işlemi yapıyorsak (değer negatifse), max health'in 1'in altına düşmeyeceğini kontrol et
                    if (req.requiredValue < 0 && (ResourceManager.Instance.maxHealth + req.requiredValue) < 1) 
                    return false; // Yeterli max health yok (örn: 5 max varken -10 istenemez)
                break; // Pozitifse (artırıyorsa) her zaman OK.
                case "maxenergy":
                    if (req.requiredValue < 0 && (ResourceManager.Instance.maxEnergy + req.requiredValue) < 1) 
                    return false;
                    break;
                case "maxmana":
                    if (req.requiredValue < 0 && (ResourceManager.Instance.maxMana + req.requiredValue) < 1) 
                    return false;
                    break;
            }
        }

        // 2. Adım: Eğer 'consume' true ise, şimdi HARCA
        if (consume)
        {
            foreach (Requirement req in requirements)
            {
                switch (req.requirementType.ToLower())
                {
                    case "item":
                        ItemData item = ItemManager.Instance.GetItemByName(req.requirementName);
                        Inventory.Instance.RemoveItem(item, req.requiredValue);
                        GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} {item.itemName}</color> harcandı.");
                        break;
                    case "gold":
                        CurrencyManager.Instance.SpendGold(req.requiredValue);
                        GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Altın</color> harcandı.");
                        break;
                    case "nexuscoin":
                        CurrencyManager.Instance.SpendNexusCoin(req.requiredValue);
                        GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Nexus Coin</color> harcandı.");
                        break;
                    case "health":
                        ResourceManager.Instance.ModifyHealth(-req.requiredValue);
                        GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Can</color> harcandı.");
                        break;
                    case "energy":
                        ResourceManager.Instance.ModifyEnergy(-req.requiredValue);
                        GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Enerji</color> harcandı.");
                        break;
                    case "mana":
                        ResourceManager.Instance.ModifyMana(-req.requiredValue);
                        GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Mana</color> harcandı.");
                        break;
                    case "people":
                        // CurrencyManager'a SpendPeople(double amount) fonksiyonu eklediğini varsayıyoruz.
                        // Eğer adı ModifyPeople ise: CurrencyManager.Instance.ModifyPeople(-req.requiredValue);
                        CurrencyManager.Instance.SpendPeople(req.requiredValue); 
                        GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Nüfus</color> harcandı.");
                        break;
                    case "maxmana":
                        ResourceManager.Instance.ModifyMaxMana(req.requiredValue); // Yeni fonksiyonu çağır
                        GameConsole.Instance.AddMessage($"Maksimum Mana {req.requiredValue} değişti.");
                    break;
                    case "maxhealth":
                        ResourceManager.Instance.ModifyMaxHealth(req.requiredValue); // Yeni fonksiyonu çağır
                        GameConsole.Instance.AddMessage($"Maksimum Can {req.requiredValue} değişti.");
                    break;
                    case "maxenergy":
                        ResourceManager.Instance.ModifyMaxEnergy(req.requiredValue); // Yeni fonksiyonu çağır
                        GameConsole.Instance.AddMessage($"Maksimum Enerji {req.requiredValue} değişti.");
                    break;
                }
            }
        }

        return true; // Kontrol başarılı (ve gerekirse harcama yapıldı)
    }

    #endregion
    
    #region Alan Kilidi (Area 2) Güncellemesi

    public void CheckArea2Unlock()
    {
        // Sol panel bitti mi?
        bool leftPanelDone = _currentLeftPerkIndex >= leftPanelPerks.Count;
        
        // Sağ panel bitti mi?
        bool rightPanelDone = true; 
        foreach(var questData in rightPanelQuests)
        {
             // Bir görev bile limitine ulaşmadıysa, panel bitmemiştir.
             if (GetExplorerQuestCompletionCount(questData.questID) < questData.completionLimit)
             {
                 rightPanelDone = false;
                 break;
             }
        }
        
        bool canUnlock = leftPanelDone && rightPanelDone;
        
        if (area2UnlockButton != null)
        {
            area2UnlockButton.interactable = canUnlock;
            // TODO: Area 2 açıldığında butona bir "GİT" fonksiyonu ekle
            // area2UnlockButton.onClick.AddListener(() => GoToArea(area2Panel));
        }
    }
    
    #endregion
}