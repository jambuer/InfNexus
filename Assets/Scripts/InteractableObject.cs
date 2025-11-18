using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic; // List kullanmak için
using System.Text; // StringBuilder için

public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Durum Ayarları")]
    public bool isLocked = true;
    
    [Tooltip("Kilidi açmak kaynakları harcar mı? (False ise gereksinimler karşılandığında otomatik açılır)")]
    public bool costsToUnlock = false;
    
    [Tooltip("Bu objenin kilidini açmak için gereken şartlar")]
    public List<Requirement> unlockRequirements; // Gereksinim listesi (Bu ismi kullanacağız)

    [Header("Görsel Ayarlar")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    private Image objectImage;

    [Header("Tooltip (Bilgi Penceresi)")]
    public GameObject tooltipObject;
    public Text tooltipText;
    public string actionText = "Odun Topla";
    public string lockedTooltipText = "Kilitli";

    [Header("İlerleme (Progress)")]
    public Slider progressBar;
    public float actionDuration = 3.0f;
    private bool isGathering = false;

    [Header("Kazanım Geri Bildirimi")]
    public GameObject feedbackPanel;
    public Text feedbackText;
    public string resourceGained = "Odun";
    public int amountGained = 5;

    [Header("Envanter Testi")]
    public ItemData itemToGive;

    void Awake()
    {
        objectImage = GetComponent<Image>();
        if (objectImage == null)
        {
            Debug.LogError($"InteractableObject: {gameObject.name} üzerinde Image component'i bulunamadı!", this);
        }
    }

    void Start()
    {
        CheckRequirementsAndMaybeUnlock(); 

        if (tooltipObject != null) tooltipObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }
    
    void OnEnable()
    {
        // Event'lere abone ol
        // Not: Stat, Currency vb. event'leri artık GameValidator dinlemeli.
        // Bu sınıfın sadece temel oyun olaylarını dinlemesi yeterli.
        LevelManager.OnPlayerLeveledUp += CheckRequirementsAndMaybeUnlock;
        QuestManager.OnQuestCompleted += HandleQuestCompleted;
        Inventory.OnInventoryChanged_Static += CheckRequirementsAndMaybeUnlock;

        CheckRequirementsAndMaybeUnlock();
    }

    void OnDisable()
    {
        // Event aboneliklerini iptal et
        LevelManager.OnPlayerLeveledUp -= CheckRequirementsAndMaybeUnlock;
        QuestManager.OnQuestCompleted -= HandleQuestCompleted;
        Inventory.OnInventoryChanged_Static -= CheckRequirementsAndMaybeUnlock;
    }

    // Objenin kilitli/açık durumuna göre görselini günceller
    void UpdateObjectVisual()
    {
        if (objectImage == null)
        {
            Debug.LogError($"UpdateObjectVisual çağrıldı ama objectImage null: {gameObject.name}", this);
            return;
        }
        objectImage.sprite = isLocked ? lockedSprite : unlockedSprite;
    }

    // ========================================================================
    // REFACTORING BURADA BAŞLIYOR
    // ========================================================================

    /// <summary>
    /// [YENİ / REFACTOR EDİLDİ]
    /// Gereksinimlerin karşılanıp karşılanmadığını merkezi GameValidator'a sorar.
    /// </summary>
    bool AreRequirementsMet()
    {
        // Tüm 'switch-case' mantığı buradan silindi.
        // Artık bu işi GameValidator yapıyor.
        // Not: 'unlockRequirements' kullandığımıza dikkat et.
        return GameValidator.Instance.AreRequirementsMet(unlockRequirements);
    }

    /// <summary>
    /// [YENİ / REFACTOR EDİLDİ]
    /// Gereksinimleri (maliyetleri) harcamak için merkezi GameCostConsumer'ı kullanır.
    /// </summary>
    private void ConsumeUnlockRequirements()
    {
        // Tüm 'switch-case' mantığı buradan silindi.
        // Artık bu işi GameCostConsumer yapıyor.
        // Not: 'unlockRequirements' kullandığımıza dikkat et.
        GameCostConsumer.Instance.ConsumeRequirements(unlockRequirements);
    }
    
    /// <summary>
    /// [YENİ / REFACTOR EDİLDİ]
    /// Gereksinim metnini oluşturmak için merkezi RequirementTooltipFormatter'ı kullanır.
    /// </summary>
    private string GetRequirementsTooltipText()
    {
        // Tüm 'StringBuilder' ve 'switch-case' mantığı buradan silindi.
        // Artık bu işi RequirementTooltipFormatter yapıyor.
        // Not: 'unlockRequirements' kullandığımıza dikkat et.
        
        // Formatter'a bu kilidin kaynakları harcayıp harcamadığını da bildiriyoruz,
        // böylece "Not: Kaynakları harcar" mesajını kendisi ekleyebilir.
        return RequirementTooltipFormatter.GetFormattedRequirementText(unlockRequirements, costsToUnlock.ToString());
    }

    // ========================================================================
    // REFACTORING BURADA BİTİYOR
    // ========================================================================


    /// <summary>
    /// Gereksinimleri kontrol eder ve eğer maliyetsizse ve karşılanıyorsa kilidi açar.
    /// </summary>
    public void CheckRequirementsAndMaybeUnlock()
    {
        if (isLocked) // Sadece kilitliyken kontrol et
        {
            // Maliyeti yoksa (costsToUnlock == false) ve gereksinimler tamamsa, otomatik aç
            if (!costsToUnlock && AreRequirementsMet())
            {
                UnlockObject();
            }
        }
        // Görseli her zaman güncelle (kilitli/açık duruma göre)
        UpdateObjectVisual();
    }

    /// <summary>
    /// Objenin kilidini açar ve görselini günceller.
    /// </summary>
    public void UnlockObject()
    {
        if (isLocked) // Zaten açıksa tekrar işlem yapma
        {
            isLocked = false;
            UpdateObjectVisual();
            Debug.Log($"{gameObject.name} kilidi açıldı!");
        }
    }

    // ---- Fare Olayları ----

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObject == null) return;

        if (isLocked)
        {
            tooltipText.text = GetRequirementsTooltipText(); // Refactor edilmiş metodu çağırır
            tooltipObject.SetActive(true);
        }
        else if (!isGathering)
        {
            tooltipText.text = actionText;
            tooltipObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipObject == null) return;
        if (!isGathering)
        {
            tooltipObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocked)
        {
            if (AreRequirementsMet()) // Refactor edilmiş metodu çağırır
            {
                if (costsToUnlock)
                {
                    ConsumeUnlockRequirements(); // Refactor edilmiş metodu çağırır
                }
                
                UnlockObject(); 
                OnPointerEnter(eventData); // Tooltip'i güncelle
            }
            else
            {
                Debug.Log($"{gameObject.name} şu anda kilitli. Gereksinimler karşılanmadı.");
            }
        }
        else if (!isGathering)
        {
            StartCoroutine(GatherResourceCoroutine());
        }
    }

    // ---- Ana İşlem Fonksiyonu (Değişiklik yok) ----
    private IEnumerator GatherResourceCoroutine()
    {
        isGathering = true;
        if (tooltipObject != null) tooltipObject.SetActive(true);
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0;
        }

        float timer = 0f;

        while (timer < actionDuration)
        {
            timer += Time.deltaTime;
            if (progressBar != null) progressBar.value = timer / actionDuration;
            yield return null;
        }

        if (progressBar != null) progressBar.gameObject.SetActive(false);

        if (itemToGive != null && Inventory.Instance != null)
        {
            Inventory.Instance.AddItem(itemToGive, amountGained);
        }
        else
        {
            if (itemToGive == null) Debug.LogWarning($"{gameObject.name}: Verilecek ItemData atanmamış!");
            if (Inventory.Instance == null) Debug.LogError("Inventory.Instance bulunamadı!");
        }

        if (feedbackPanel != null)
        {
            // resourceGained yerine itemToGive.itemName kullanmak daha doğru
            if (feedbackText != null && itemToGive != null) 
                feedbackText.text = $"+{amountGained} {itemToGive.itemName}";
            
            feedbackPanel.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (tooltipObject != null) tooltipObject.SetActive(false);
        isGathering = false;
    }

    // Quest event'i için özel handler (Değişiklik yok)
    private void HandleQuestCompleted(string completedQuestID)
    {
        if (!isLocked) return;

        // Bu objenin gereksinimleri arasında tamamlanan görev var mı?
        // Hızlı bir kontrol için GameValidator'a sormak yerine burada bırakabiliriz
        // veya CheckRequirementsAndMaybeUnlock() da çağırabiliriz.
        foreach (Requirement req in unlockRequirements)
        {
            if (req.reqType == RequirementType.Quest && req.requirementName == completedQuestID)
            {
                CheckRequirementsAndMaybeUnlock();
                return;
            }
        }
    }
}