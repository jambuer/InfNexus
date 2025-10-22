using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic; // List kullanmak için bu satırı ekle

public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Durum Ayarları")]
    public bool isLocked = true; // Obje kilitli mi? Başlangıçta kilitli olsun
    
    // YENİ EKLEDİĞİMİZ KISIM:
    [Tooltip("Bu objenin kilidini açmak için gereken şartlar")]
    public List<Requirement> unlockRequirements; // Gereksinim listesi

    [Header("Görsel Ayarlar")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    private Image objectImage;

    [Header("Tooltip (Bilgi Penceresi)")]
    public GameObject tooltipObject;
    public Text tooltipText;
    public string actionText = "Odun Topla";
    public string lockedTooltipText = "Kilitli"; // Kilitliyken gösterilecek metin

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
        // GetComponent'i buraya taşı
        objectImage = GetComponent<Image>();
        if (objectImage == null)
        {
            Debug.LogError($"InteractableObject: {gameObject.name} üzerinde Image component'i bulunamadı!", this);
        }
    }

    void Start()
    {
        

        // Başlangıçta gereksinimleri kontrol et ve görseli ayarla
        CheckRequirementsAndMaybeUnlock(); // Eskiden sadece UpdateObjectVisual() vardı

        if (tooltipObject != null) tooltipObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }
    
    

    // Objenin kilitli/açık durumuna göre görselini günceller
    void UpdateObjectVisual()
    {

        if (objectImage == null)
        {
            Debug.LogError($"UpdateObjectVisual çağrıldı ama objectImage null: {gameObject.name}", this);
            return;
        }
        
        if (isLocked)
        {
            objectImage.sprite = lockedSprite;
        }
        else
        {
            objectImage.sprite = unlockedSprite;
        }
    }

    // YENİ METOT: Tüm gereksinimlerin karşılanıp karşılanmadığını kontrol eder
    bool AreRequirementsMet()
    {
        if (unlockRequirements == null || unlockRequirements.Count == 0)
        {
            return true; // Hiç gereksinim yoksa, karşılanmış sayılır.
        }

        foreach (Requirement req in unlockRequirements)
        {
            bool requirementSatisfied = false;
            switch (req.requirementType.ToLower()) // Küçük/büyük harf duyarlılığını kaldır
            {
                case "level":
                    // TODO: LevelManager.Instance gibi bir yerden oyuncu seviyesini al
                     // Şimdilik varsayılan 1
                 requirementSatisfied = false; // Başlangıçta karşılanmadı olarak ayarla

               if (LevelManager.Instance != null)
               {
                   int currentLevel = LevelManager.Instance.currentLevel;
                   if (currentLevel >= req.requiredValue)
                   {
                       requirementSatisfied = true;
                   }
                   // Seviye yetmiyorsa false kalacak (başlangıçta false yaptık)
               }
               else
               {
                   Debug.LogError("LevelManager.Instance bulunamadı!");
                   // requirementSatisfied false kalır
               }
               // { // <-- Bu blok hala fazlalık görünüyor, SİLİNMELİ
               //     requirementSatisfied = true;
               // } // <-- Bu blok hala fazlalık görünüyor, SİLİNMELİ
               break;
                case "stat":
                    // TODO: StatManager.Instance gibi bir yerden ilgili stat'ı al
                    // Örnek: if (StatManager.Instance.GetStatValue(req.requirementName) >= req.requiredValue)
                    // Şimdilik varsayılan olarak karşılanmadı diyelim
                    // requirementSatisfied = false; // Veya true yap test için
                    if (StatManager.Instance != null) // StatManager örneği var mı?
                    {
                        // StatManager'dan ilgili stat'ın değerini al
                        float currentStatValue = StatManager.Instance.GetTotalStat(req.requirementName); // Yeni eklediğimiz fonksiyonu kullan
                        if (currentStatValue >= req.requiredValue)
                        {
                            requirementSatisfied = true;
                        }
                    }
                    else
                    {
                        Debug.LogError("StatManager.Instance bulunamadı!");
                        requirementSatisfied = false;
                    }
                    break;
                case "quest":
                    // TODO: QuestManager.Instance gibi bir yerden görevin tamamlanıp tamamlanmadığını kontrol et
                    // Örnek: if (QuestManager.Instance.IsQuestCompleted(req.requirementName)) // requirementName burada Quest ID olabilir
                    // Şimdilik varsayılan olarak karşılanmadı diyelim
                     if (QuestManager.Instance != null && QuestManager.Instance.GetCompletionCount(req.requirementName) > 0) 
                    {
                        requirementSatisfied = true;
                    }
                    break;
                case "item":
                     // Envanter kontrolü
                     if (Inventory.Instance != null && ItemManager.Instance != null)
                     {
                         // ItemManager'dan isme göre ItemData'yı bul
                         ItemData requiredItem = ItemManager.Instance.GetItemByName(req.requirementName);
                         if (requiredItem != null) // Item bulundu mu?
                         {
                             // Envanterde yeterli sayıda var mı kontrol et
                             if (Inventory.Instance.HasItem(requiredItem, req.requiredValue))
                             {
                                 requirementSatisfied = true;
                             }
                         }
                         else
                         {
                             Debug.LogWarning($"Item veritabanında bulunamadı: {req.requirementName}");
                         }
                     }
                     else
                     {
                         if(Inventory.Instance == null) Debug.LogError("Inventory.Instance bulunamadı!");
                         if(ItemManager.Instance == null) Debug.LogError("ItemManager.Instance bulunamadı!");
                         requirementSatisfied = false;
                     }
                    break;

                // Buraya başka gereksinim türleri (örn: "chapter") eklenebilir
                default:
                    Debug.LogWarning($"Bilinmeyen gereksinim türü: {req.requirementType}");
                    break;
            }

            // Eğer tek bir gereksinim bile karşılanmıyorsa, false döndür
            if (!requirementSatisfied)
            {
                return false;
            }
        }

        // Eğer döngü bittiyse ve hiç false dönmediyse, tüm gereksinimler karşılanmıştır
        return true;
    }

    // YENİ METOT: Gereksinimleri kontrol eder ve eğer karşılanıyorsa kilidi açar
    public void CheckRequirementsAndMaybeUnlock()
    {
        if (isLocked) // Sadece kilitliyken kontrol et
        {
            if (AreRequirementsMet())
            {
                UnlockObject();
            }
        }
         // Görseli her zaman güncelle (kilitli/açık duruma göre)
        UpdateObjectVisual();
    }


    // Dışarıdan bir script (örn: QuestManager, LevelManager) bu objenin kilidini açmak için çağırabilir
    // VEYA gereksinimler karşılandığında otomatik açılır
    public void UnlockObject()
    {
        if (isLocked) // Zaten açıksa tekrar işlem yapma
        {
            isLocked = false;
            UpdateObjectVisual();
            Debug.Log($"{gameObject.name} kilidi açıldı!");
            // İsteğe bağlı: Kilit açıldığında bir ses efekti veya görsel efekt oynatılabilir.
        }
    }

    // ---- Fare Olayları ----

    // Fare üzerine geldiğinde
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObject == null) return; // Tooltip atanmamışsa çık

        // CheckRequirementsAndMaybeUnlock(); // Fare üzerine gelince tekrar kontrol et (opsiyonel)

        if (isLocked)
        {
             tooltipText.text = lockedTooltipText; // Kilitli metnini göster
             tooltipObject.SetActive(true);
        }
        else if (!isGathering) // Kilitli değilse VE toplama yapmıyorsa
        {
            tooltipText.text = actionText;
            tooltipObject.SetActive(true);
        }
    }

    // Fare üzerinden çekildiğinde
    public void OnPointerExit(PointerEventData eventData)
    {
         if (tooltipObject == null) return;

        // Toplama yapmıyorsa tooltip'i kapat (Kilitliyse de kapanmalı)
        if (!isGathering)
        {
            tooltipObject.SetActive(false);
        }
    }

    // Objeye tıklandığında
    public void OnPointerClick(PointerEventData eventData)
    {
        // CheckRequirementsAndMaybeUnlock(); // Tıklayınca tekrar kontrol et (opsiyonel, ama güvenli)

        // Kilitli değilse ve zaten bir işlem yapmıyorsa
        if (!isLocked && !isGathering)
        {
            StartCoroutine(GatherResourceCoroutine());
        }
        // İsteğe bağlı: Kilitliyken tıklandığında bir ses efekti ("kilitli" sesi) çalınabilir.
        else if (isLocked)
        {
             Debug.Log($"{gameObject.name} şu anda kilitli.");
             // Örn: AudioManager.Instance.PlaySound("LockedObjectClick");
        }
    }

    // ---- Ana İşlem Fonksiyonu ----
    private IEnumerator GatherResourceCoroutine()
    {
        isGathering = true;
        if (tooltipObject != null) tooltipObject.SetActive(true); // İşlem sırasında tooltip açık kalsın
        if (progressBar != null) progressBar.gameObject.SetActive(true);
        if (progressBar != null) progressBar.value = 0;

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
            if (feedbackText != null) feedbackText.text = $"+{amountGained} {resourceGained}"; // resourceGained yerine itemToGive.itemName kullanmak daha doğru
            feedbackPanel.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (tooltipObject != null) tooltipObject.SetActive(false);
        isGathering = false;

        // Fare hala üzerindeyse tooltip'i tekrar açmak için kontrol et
        // Bu EventSystem tarafından otomatik yapılacaktır ama emin olmak için:
        // PointerEventData pointerData = new PointerEventData(EventSystem.current);
        // pointerData.position = Input.mousePosition;
        // List<RaycastResult> results = new List<RaycastResult>();
        // EventSystem.current.RaycastAll(pointerData, results);
        // bool pointerIsOver = results.Any(result => result.gameObject == gameObject);
        // if (pointerIsOver)
        // {
        //     OnPointerEnter(null); // Tekrar tooltip'i tetikle
        // }
    }
    
    // InteractableObject.cs içine:

void OnEnable()
{
    // Event'lere abone ol
    LevelManager.OnPlayerLeveledUp += CheckRequirementsAndMaybeUnlock;
    QuestManager.OnQuestCompleted += HandleQuestCompleted; // Quest için ayrı bir handler
    Inventory.OnInventoryChanged_Static += CheckRequirementsAndMaybeUnlock; // Static event'i dinle

    // Başlangıç kontrolü OnEnable içinde de yapılabilir (Start yerine)
     CheckRequirementsAndMaybeUnlock();
}

void OnDisable()
{
    // Event aboneliklerini iptal et (hafıza sızıntısını önlemek için ÇOK ÖNEMLİ)
    LevelManager.OnPlayerLeveledUp += CheckRequirementsAndMaybeUnlock;
    QuestManager.OnQuestCompleted -= HandleQuestCompleted;
    Inventory.OnInventoryChanged_Static -= CheckRequirementsAndMaybeUnlock;
}

// Quest event'i için özel handler (ID kontrolü yapmak için)
private void HandleQuestCompleted(string completedQuestID)
{
     if (!isLocked) return; // Zaten açıksa kontrol etme

    // Bu objenin gereksinimleri arasında tamamlanan görev var mı?
    foreach (Requirement req in unlockRequirements)
    {
        if (req.requirementType.ToLower() == "quest" && req.requirementName == completedQuestID)
        {
             // Eğer bu görev bir gereksinimse, tüm gereksinimleri tekrar kontrol et
             CheckRequirementsAndMaybeUnlock();
             return; // Bir tane bulmak yeterli
        }
    }
}
}