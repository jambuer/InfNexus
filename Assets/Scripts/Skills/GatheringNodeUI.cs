using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;

/// <summary>
/// Orta panelde görünen "Odun Topla" gibi bir eylem kartının UI script'i.
/// Artık "Sürekli Topla" (Auto-Gather) ve Ödül Gösterimi mantığını içerir.
/// </summary>
public class GatheringNodeUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Referansları")]
    [SerializeField]
    private Image nodeIcon;
    [SerializeField]
    private TextMeshProUGUI nodeNameText;
    [SerializeField]
    private TextMeshProUGUI nodeDescriptionText;
    [SerializeField]
    private Slider progressBar;
    [SerializeField]
    private Button gatherButton;
    [SerializeField]
    private Button autoGatherButton;
    [SerializeField]
    private TextMeshProUGUI requirementsText;
    [SerializeField]
    private Transform rewardIconContainer;
    
    [Header("Sürekli Toplama UI")]
    [SerializeField]
    private TextMeshProUGUI autoGatherButtonText; 
    [SerializeField]
    private Color autoGatherActiveColor = Color.green;
    private Color _autoGatherDefaultColor;

    // [YENİ] Adım 7.2'de oluşturduğunuz prefab'ı buraya sürükleyin
    [Header("Prefab Referansı")]
    [SerializeField]
    private GameObject rewardIconPrefab; 

    private GatheringNodeData _nodeData;
    private StringBuilder _sb = new StringBuilder();
    private Coroutine _gatheringCoroutine = null;
    private bool _isAutoGathering = false;

    // (Awake fonksiyonu Adım 6'daki ile aynı)
    private void Awake()
    {
        if (autoGatherButtonText != null)
        {
            _autoGatherDefaultColor = autoGatherButtonText.color;
        }
    }

    /// <summary>
    /// Bu kartı ilgili toplayıcılık verisi ile kurar.
    /// [DEĞİŞTİ] Artık ödül ikonlarını da oluşturur.
    /// </summary>
    public void Setup(GatheringNodeData nodeData)
    {
        _nodeData = nodeData;
        if (_nodeData == null) return;

        // 1. Verileri UI elemanlarına ata (Aynı)
        if (nodeIcon != null)
        {
            nodeIcon.sprite = _nodeData.icon;
            nodeIcon.gameObject.SetActive(_nodeData.icon != null);
        }
        if (nodeNameText != null)
            nodeNameText.text = _nodeData.displayName;
        if (nodeDescriptionText != null)
            nodeDescriptionText.text = _nodeData.description;

        // 2. Buton 'OnClick' fonksiyonlarını ayarla (Aynı)
        gatherButton.onClick.RemoveAllListeners();
        gatherButton.onClick.AddListener(OnGatherClicked);

        autoGatherButton.onClick.RemoveAllListeners();
        autoGatherButton.onClick.AddListener(OnAutoGatherClicked);
        
        // 3. [YENİ] Ödül İkonlarını Oluştur
        SetupRewardIcons();

        if (progressBar != null)
            progressBar.value = 0; 

        // 4. Başlangıç durumunu yenile (Aynı)
        _isAutoGathering = false;
        UpdateAutoGatherUI();
        RefreshState();
    }

    
    /// <summary>
    /// [DEĞİŞTİ] Adım 15 Öncesi: Artık 'itemLootTable' yerine 'rewards' listesini okur.
    /// </summary>
    private void SetupRewardIcons()
    {
        if (rewardIconContainer == null || rewardIconPrefab == null) return;

        foreach (Transform child in rewardIconContainer)
        {
            Destroy(child.gameObject);
        }

        if (_nodeData.rewards == null) return;

        foreach (GameReward reward in _nodeData.rewards)
        {
            // Sadece 'Item' tipi ödülleri gösteriyoruz
            if (reward.rewardType == RewardType.Item)
            {
                ItemData itemData = reward.itemData;
                if (itemData == null && !string.IsNullOrEmpty(reward.stringParameter))
                {
                    itemData = ItemManager.Instance?.GetItemByName(reward.stringParameter);
                }

                if (itemData != null)
                {
                    GameObject iconGO = Instantiate(rewardIconPrefab, rewardIconContainer);
                    
                    // Miktar metni (Temel miktarı gösterir, örn: "1")
                    string amountStr = reward.amount.ToString();

                    GatheringRewardIconUI iconUI = iconGO.GetComponent<GatheringRewardIconUI>();
                    if (iconUI != null)
                    {
                        iconUI.Setup(itemData.icon, amountStr);
                    }
                }
            }
            // (Artık Gold, XP ikonlarını da gösterebiliriz)
        }
    }
    
    // ===================================================================
    // DİNAMİK GÜNCELLEME (Refresh)
    // (Adım 6'daki ile aynı, değişiklik yok)
    // ===================================================================

    private void OnEnable()
    {
        SubscribeToEvents();
        ResetGatheringState(); 
        RefreshState();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
        ResetGatheringState(); 
    }

    private void ResetGatheringState()
    {
        if (_gatheringCoroutine != null)
        {
            StopCoroutine(_gatheringCoroutine);
            _gatheringCoroutine = null;
        }
        _isAutoGathering = false; 
        UpdateAutoGatherUI();
        if (progressBar != null) progressBar.value = 0;
    }

    private void SubscribeToEvents()
    {
        LevelManager.OnPlayerLeveledUp += OnRequirementEvent;
        QuestManager.OnQuestCompleted += OnRequirementEvent;
        Inventory.OnInventoryChanged_Static += OnRequirementEvent;
        CurrencyManager.Instance.OnCurrencyChanged += OnRequirementEvent;
        ResourceManager.Instance.OnValuesChanged += OnRequirementEvent;
        
        if (LifeSkillManager.Instance != null)
            LifeSkillManager.Instance.OnSkillLeveledUp += OnRequirementEvent;
        if (JobsManager.Instance != null)
            JobsManager.Instance.OnJobLeveledUp += OnRequirementEvent;
    }

    private void UnsubscribeFromEvents()
    {
        LevelManager.OnPlayerLeveledUp -= OnRequirementEvent;
        QuestManager.OnQuestCompleted -= OnRequirementEvent;
        Inventory.OnInventoryChanged_Static -= OnRequirementEvent;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= OnRequirementEvent;
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnValuesChanged -= OnRequirementEvent;
        if (LifeSkillManager.Instance != null)
            LifeSkillManager.Instance.OnSkillLeveledUp -= OnRequirementEvent;
        if (JobsManager.Instance != null)
            JobsManager.Instance.OnJobLeveledUp -= OnRequirementEvent;
    }

    private void OnRequirementEvent(string s) { RefreshState(); }
    private void OnRequirementEvent() { RefreshState(); }
    private void OnRequirementEvent(int i) { RefreshState(); }
    private void OnRequirementEvent(LifeSkill skill, int level) { RefreshState(); }
    private void OnRequirementEvent(Job job, int level) { RefreshState(); }
    private void OnRequirementEvent(CurrencyType type, double amount) { RefreshState(); }

    private void RefreshState()
    {
        if (_nodeData == null || !gameObject.activeInHierarchy) return;
        var validator = GameValidator.Instance;
        if (validator == null) return;
        bool isUnlocked = validator.CheckGameRequirements(_nodeData.requirementsToUnlock);
        bool canAffordCost = validator.CheckGameRequirements(_nodeData.costToPerform);
        gatherButton.interactable = isUnlocked && canAffordCost && (_gatheringCoroutine == null) && !_isAutoGathering;
        autoGatherButton.interactable = isUnlocked && (canAffordCost || _isAutoGathering);
        if (requirementsText != null)
        {
            _sb.Clear();
            if (!isUnlocked)
            {
                _sb.AppendLine("<b>KİLİTLİ:</b>");
                _sb.AppendLine(RequirementTooltipFormatter.GetGameRequirementsTooltipText(_nodeData.requirementsToUnlock));
            }
            else
            {
                _sb.AppendLine("<b>MALİYET:</b>");
                _sb.AppendLine(RequirementTooltipFormatter.GetGameRequirementsTooltipText(_nodeData.costToPerform));
            }
            requirementsText.text = _sb.ToString();
        }
    }

    // ===================================================================
    // BUTON EYLEMLERİ (SÜREKLİ TOPLAMA)
    // (Adım 6'daki ile aynı, değişiklik yok)
    // ===================================================================

    /// <summary>
    /// "Topla" butonuna tıklandığında (Tek seferlik).
    /// </summary>
    private void OnGatherClicked()
    {
        // [DEĞİŞTİ] Adım 11: Panele tıklandığını Sağ Panele bildir
        SkillDetailPanelUI.Instance?.ShowSkill(_nodeData.associatedSkill);

        if (_gatheringCoroutine == null && !_isAutoGathering)
        {
            _gatheringCoroutine = StartCoroutine(GatheringCoroutine());
        }
    }

    /// <summary>
    /// "Sürekli Topla" butonuna tıklandığında.
    /// </summary>
    private void OnAutoGatherClicked()
    {
        if (_isAutoGathering)
        {
            _isAutoGathering = false;
            UpdateAutoGatherUI();
            GameConsole.Instance?.AddMessage("<color=green>Sürekli toplama mevcut işlemden sonra duracak.</color>");
        }
        else
        {
            // [DEĞİŞTİ] Adım 11: Panele tıklandığını Sağ Panele bildir
            SkillDetailPanelUI.Instance?.ShowSkill(_nodeData.associatedSkill);

            _isAutoGathering = true;
            UpdateAutoGatherUI();
            GameConsole.Instance?.AddMessage("<color=green>Sürekli toplama başlatıldı.</color>");
            if (_gatheringCoroutine == null)
            {
                _gatheringCoroutine = StartCoroutine(GatheringCoroutine());
            }
        }
    }
    
    private void UpdateAutoGatherUI()
    {
        if (autoGatherButtonText == null) return;
        
        if (_isAutoGathering)
        {
            autoGatherButtonText.text = "STOP";
            autoGatherButtonText.color = autoGatherActiveColor;
        }
        else
        {
            autoGatherButtonText.text = "AUTO GATHER";
            autoGatherButtonText.color = _autoGatherDefaultColor;
        }
    }

    private IEnumerator GatheringCoroutine()
    {
        while (true) 
        {
            var validator = GameValidator.Instance;
            if (validator == null)
            {
                Debug.LogError("GameValidator bulunamadı!");
                break; 
            }
                
            if (!validator.CheckGameRequirements(_nodeData.costToPerform))
            {
                GameConsole.Instance?.AddMessage("<color=red>Sürekli toplama durdu! (Yetersiz kaynak)</color>");
                _isAutoGathering = false; 
                UpdateAutoGatherUI();
                break; 
            }

            gatherButton.interactable = false;
            autoGatherButton.interactable = true; 

            float baseTime = _nodeData.baseTimeInSeconds;

            // [DEĞİŞTİ] Adım 13: Artık 'GetTotalProductionBonus' çağrılıyor
            float totalProductionBonus = LifeJobsSkillsManager.Instance.GetTotalProductionBonus(_nodeData.associatedSkill);
            
            float actualTime = baseTime / (1f + totalProductionBonus);

            float timer = 0f;
            while (timer < actualTime)
            {
                timer += Time.deltaTime;
                progressBar.value = timer / actualTime;
                yield return null; 
            }

            progressBar.value = 0f;
            CompleteGathering(); 

            if (!_isAutoGathering)
            {
                break; 
            }
        }
        
        _gatheringCoroutine = null;
        _isAutoGathering = false;
        UpdateAutoGatherUI();
        RefreshState(); 
    }
    

    /// <summary>
    /// [DEĞİŞTİ] Adım 15 Öncesi: Artık 'DropRate' yerine 'Efficiency'
    /// mantığını (şans/miktar) uygular.
    /// </summary>
    private void CompleteGathering()
    {
        var validator = GameValidator.Instance;
        if (validator == null) return;
        
        // 1. Maliyetleri Harca (Hala Gerekli)
        // (Adım 10'da düzeltildi, artık çalışıyor olmalı)
        if (!validator.CheckGameRequirements(_nodeData.costToPerform))
        {
            GameConsole.Instance?.AddMessage("<color=red>Toplama iptal oldu! (Yetersiz kaynak)</color>");
            return;
        }
        GameCostConsumer.Instance?.ConsumeGameRequirements(_nodeData.costToPerform);

        // 2. [YENİ] Efficiency Hesaplaması
        var manager = LifeJobsSkillsManager.Instance;
        if (manager == null) return;

        // Oyuncunun o anki Efficiency'sini al (örn: 150)
        double playerEfficiency = manager.GetEfficiency(_nodeData.associatedSkill);
        // Eylemin zorluk eşiğini al (örn: 300)
        double nodeThreshold = _nodeData.efficiencyThreshold;

        // 3. [YENİ] Başarı Şansı Hesaplaması
        float successChance = 1f; // Varsayılan %100
        if (playerEfficiency < nodeThreshold)
        {
            // (150 / 300 = 0.5)
            successChance = (float)(playerEfficiency / nodeThreshold);
        }

        if (Random.Range(0f, 1f) > successChance)
        {
            GameConsole.Instance?.AddMessage("<color=orange>Toplama başarısız oldu... (Düşük Verimlilik)</color>");
            // (Yine de XP verilebilir, isteğe bağlı)
            // LifeJobsSkillsManager.Instance?.AddSkillXP(_nodeData.associatedSkill, _nodeData.baseSkillXP * 0.1); // (örn: %10 XP)
        }
        else
        {
            // BAŞARILI!
            
            // 4. [YENİ] Miktar Hesaplaması
            // (1000 / 300 = 3.33) -> Floor = 3
            int quantityMultiplier = (int)Mathf.Floor((float)(playerEfficiency / nodeThreshold));
            quantityMultiplier = Mathf.Max(1, quantityMultiplier); // En az 1

            // 5. Garantili Ödülleri Dağıt (XP, Gold vb.)
            // (Bu kod 'Item' olmayanları dağıtır)
            DistributeGuaranteedRewards(quantityMultiplier);

            // 6. Beceri XP'sini Ver (Bu hala aynı)
            LifeJobsSkillsManager.Instance?.AddSkillXP(_nodeData.associatedSkill, _nodeData.baseSkillXP);
        }

        // 7. Sağ Paneli Güncelle (Bu hala aynı)
        SkillDetailPanelUI.Instance?.ShowSkill(_nodeData.associatedSkill);
    }


    /// <summary>
    /// [DEĞİŞTİ] Adım 15: 'rewards' listesini işler ve public 'DistributeRewards'
    /// fonksiyonunu çağırır (CS0122 Hatası düzeltildi).
    /// </summary>
    private void DistributeGuaranteedRewards(int quantityMultiplier)
    {
        if (_nodeData.rewards == null) return;

        var distributor = GameRewardDistributor.Instance;
        if (distributor == null) return;

        // [YENİ] Ödülleri toplamak için yeni bir liste oluştur
        List<GameReward> finalRewardsList = new List<GameReward>();

        foreach (var reward in _nodeData.rewards)
        {
            if (reward.rewardType == RewardType.Item)
            {
                // Eşya ödüllerini 'quantityMultiplier' ile çarp
                GameReward itemReward = reward; // Kopyasını oluştur
                itemReward.amount *= quantityMultiplier; // Miktarı çarp
                
                finalRewardsList.Add(itemReward); // Listeye ekle
            }
            else
            {
                // XP, Gold vb. ödülleri 1 kez ekle
                finalRewardsList.Add(reward);
            }
        }

        // [DEĞİŞTİ] Artık 'public' olan çoğul fonksiyonu SADECE BİR KEZ çağır
        distributor.DistributeRewards(finalRewardsList);
    }
    

    /// <summary>
    /// [KALDIRILDI] Adım 15 Öncesi: Bu fonksiyon artık 'Efficiency'
    /// mantığı ile değiştirildiği için GEREKLİ DEĞİLDİR.
    /// </summary>
    // private void DistributeLootDrops() { ... }

    /// <summary>
    /// [YENİ] Kartın kendisine tıklandığında (butonlar hariç) tetiklenir.
    /// Adım 8: Sağdaki detay panelini günceller.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Eğer bir butona tıkladıysak (örn: Topla, Sürekli Topla), bu fonksiyonu tetikleme.
        // (Butonlar tıklamayı "tüketecektir")
        // Bu yüzden, tıklanan objenin bir buton olup olmadığını kontrol edebiliriz
        // VEYA daha basiti, butonların tıklamaları zaten almasını bekleriz.

        // Eğer tıklama hedefi bu objenin kendisiyse (veya altındaki bir ikon/arkaplan)
        // ve bir buton değilse, detay panelini göster.

        // En basit yöntem: Butonlar zaten tıklamayı yakalayacağı için
        // buraya gelen tıklama %99 kartın ana gövdesindendir.

        if (_nodeData != null && _nodeData.associatedSkill != LifeSkill.None)
        {
            SkillDetailPanelUI.Instance?.ShowSkill(_nodeData.associatedSkill);
        }
    }

}