using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic; // List kullanmak için bu satırı ekle
using System.Text; // StringBuilder için

public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Durum Ayarları")]
    public bool isLocked = true; // Obje kilitli mi? Başlangıçta kilitli olsun
    
    [Tooltip("Kilidi açmak kaynakları harcar mı? (False ise gereksinimler karşılandığında otomatik açılır)")]
    public bool costsToUnlock = false; // Varsayılan olarak 'false'
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
            switch (req.reqType)
            {
                case RequirementType.Level:
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
                    
                    break;
                case RequirementType.Stat:
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
                case RequirementType.Quest:
                    // TODO: QuestManager.Instance gibi bir yerden görevin tamamlanıp tamamlanmadığını kontrol et
                    // Örnek: if (QuestManager.Instance.IsQuestCompleted(req.requirementName)) // requirementName burada Quest ID olabilir
                    // Şimdilik varsayılan olarak karşılanmadı diyelim
                    if (QuestManager.Instance != null && QuestManager.Instance.GetCompletionCount(req.requirementName) > 0)
                    {
                        requirementSatisfied = true;
                    }
                    break;
                case RequirementType.Item:
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
                        if (Inventory.Instance == null) Debug.LogError("Inventory.Instance bulunamadı!");
                        if (ItemManager.Instance == null) Debug.LogError("ItemManager.Instance bulunamadı!");
                        requirementSatisfied = false;
                    }
                    break;

                // Buraya başka gereksinim türleri (örn: "chapter") eklenebilir
                default:
                    Debug.LogWarning($"Bilinmeyen gereksinim türü: {req.reqType}");
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
            if (!costsToUnlock && AreRequirementsMet())
            {
                UnlockObject();
            }
        }
        // Görseli her zaman güncelle (kilitli/açık duruma göre)
        UpdateObjectVisual();
    }

    private void ConsumeUnlockRequirements()
    {
        if (unlockRequirements == null) return;

        Debug.Log($"Gereksinimler harcanıyor: {gameObject.name}");

        foreach (Requirement req in unlockRequirements)
        {
            // Gereksinim tipine göre ilgili yöneticiden harcama yap
            switch (req.reqType)
            {
                case RequirementType.Item:
                    ItemData item = ItemManager.Instance.GetItemByName(req.requirementName);
                    if (item != null && Inventory.Instance != null)
                    {
                        Inventory.Instance.RemoveItem(item, req.requiredValue);
                        if (GameConsole.Instance != null)
                        {
                            GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} {item.itemName}</color> harcandı ({gameObject.name} kilidi açıldı).");
                        }
                    }
                    break;

                case RequirementType.Gold:
                    if (CurrencyManager.Instance != null)
                    {
                        CurrencyManager.Instance.SpendGold(req.requiredValue); // Değer int'den double'a otomatik dönüşecektir
                        if (GameConsole.Instance != null)
                        {
                            GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Altın</color> harcandı ({gameObject.name} kilidi açıldı).");
                        }
                    }
                    break;

                case RequirementType.NexusCoin:
                    if (CurrencyManager.Instance != null)
                    {
                        CurrencyManager.Instance.SpendNexusCoin(req.requiredValue);
                        if (GameConsole.Instance != null)
                        {
                            GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Nexus Coin</color> harcandı ({gameObject.name} kilidi açıldı).");
                        }
                    }
                    break;

                case RequirementType.People: // Varsayılan isim, Inspector'da "people" yazdığından emin ol
                    if (CurrencyManager.Instance != null)
                    {
                        // CurrencyManager'da SpendPeople(double amount) olduğunu varsayıyoruz
                        // Eğer fonksiyon adı SpendPeople değilse (örn: ModifyPeople), aşağıdaki satırı ona göre değiştir:
                        // CurrencyManager.Instance.ModifyPeople(-req.requiredValue); 
                        
                        CurrencyManager.Instance.SpendPeople(req.requiredValue); // <-- Fonksiyonun adını doğrulamamız lazım*
                        
                        // *EĞER CurrencyManager'da SpendPeople yoksa, bana haber ver, ilgili fonksiyonu ekleriz.
                        // Şimdilik bu satırı yorumda bırakıyorum, ancak mantığı buraya gelecek.
                        
                        Debug.Log($"{req.requiredValue} People harcanması gerekiyor (fonksiyonu eklemeliyiz).");

                        if (GameConsole.Instance != null)
                         {
                             GameConsole.Instance.AddMessage($"<color=red>-{req.requiredValue} Nüfus</color> harcandı ({gameObject.name} kilidi açıldı).");
                         }
                    }
                    break;
            }
        }
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
            tooltipText.text = GetRequirementsTooltipText(); // Kilitli metnini göster
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
        if (isLocked)
        {
            // Obje kilitliyse, önce gereksinimlerin karşılanıp karşılanmadığını kontrol et
            if (AreRequirementsMet())
            {
                // Gereksinimler tamamsa:
                
                // Eğer bu kilidi açmanın bir maliyeti varsa, kaynakları harca
                if (costsToUnlock)
                {
                    ConsumeUnlockRequirements();
                }
                
                // Kilidi aç
                UnlockObject(); 
                
                // Tooltip'i hemen güncelle (artık "Odun Topla" yazsın)
                OnPointerEnter(eventData); 
            }
            else
            {
                // Gereksinimler tam değilse
                Debug.Log($"{gameObject.name} şu anda kilitli. Gereksinimler karşılanmadı.");
                // İsteğe bağlı: Kilitli sesi çal
            }
        }
        else if (!isGathering) // Kilitli değilse ve toplama yapmıyorsa
        {
            // Normal eylemi başlat (örn: Odun Topla)
            StartCoroutine(GatherResourceCoroutine());
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
        LevelManager.OnPlayerLeveledUp -= CheckRequirementsAndMaybeUnlock;
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
            if (req.reqType == RequirementType.Quest && req.requirementName == completedQuestID)
            {
                // Eğer bu görev bir gereksinimse, tüm gereksinimleri tekrar kontrol et
                CheckRequirementsAndMaybeUnlock();
                return; // Bir tane bulmak yeterli
            }
        }
    }

    private string GetRequirementsTooltipText()
    {
        if (unlockRequirements == null || unlockRequirements.Count == 0)
        {
            return "Kilitli (Gereksinim Gerekli)"; // Hata durumu
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>Kilidi Açmak İçin Gerekenler:</b>"); // Başlık

        // Renkleri tanımlayalım (CityQuestItemUI'dakine benzer)
        string metColorHex = ColorUtility.ToHtmlStringRGB(Color.green);
        string notMetColorHex = ColorUtility.ToHtmlStringRGB(Color.red);

        foreach (Requirement req in unlockRequirements)
        {
            bool isMet = false;
            string reqText = "";

            switch (req.reqType)
            {
                case RequirementType.Level:
                    int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 0;
                    isMet = currentLevel >= req.requiredValue;
                    reqText = $"Seviye {req.requiredValue}";
                    break;
                case RequirementType.Quest:
                    isMet = (QuestManager.Instance != null) && QuestManager.Instance.GetCompletionCount(req.requirementName) > 0;
                    reqText = $"Görevi tamamla: '{req.requirementName}'"; // Tırnak içine aldık
                    break;
                case RequirementType.Item:
                    ItemData item = (ItemManager.Instance != null) ? ItemManager.Instance.GetItemByName(req.requirementName) : null;
                    int currentAmount = (item != null && Inventory.Instance != null) ? Inventory.Instance.GetItemCount(item) : 0; // GetItemCount fonksiyonun olduğunu varsayıyoruz*
                    isMet = currentAmount >= req.requiredValue;
                    reqText = $"{currentAmount} / {req.requiredValue} x {req.requirementName}";
                    break;
                case RequirementType.Stat:
                    float currentStat = (StatManager.Instance != null) ? StatManager.Instance.GetTotalStat(req.requirementName) : 0;
                    isMet = currentStat >= req.requiredValue;
                    reqText = $"{req.requirementName} Stat: {currentStat:F0} / {req.requiredValue}";
                    break;

                case RequirementType.Gold:
                    double currentGold = (CurrencyManager.Instance != null) ? CurrencyManager.Instance.gold : 0;
                    isMet = currentGold >= req.requiredValue;
                    reqText = $"{currentGold:F0} / {req.requiredValue} Altın";
                    break;
                case RequirementType.NexusCoin:
                    double currentNexus = (CurrencyManager.Instance != null) ? CurrencyManager.Instance.nexusCoin : 0;
                    isMet = currentNexus >= req.requiredValue;
                    reqText = $"{currentNexus:F0} / {req.requiredValue} Nexus Coin";
                    break;
                case RequirementType.People:
                    double currentPeople = 0; // Varsayılan
                    // if (CurrencyManager.Instance != null) currentPeople = CurrencyManager.Instance.people; //* Değişkenin adını doğrula
                    isMet = currentPeople >= req.requiredValue;
                    reqText = $"{currentPeople:F0} / {req.requiredValue} Nüfus";
                    break;
                default:
                    reqText = $"{req.requirementName} ({req.reqType})";
                    break;
            }

            sb.AppendLine($"<color=#{(isMet ? metColorHex : notMetColorHex)}>- {reqText}</color>");
        }

        if (costsToUnlock)
        {
            sb.AppendLine("\n<color=yellow><b>Not:</b> Kilidi açmak bu kaynakları harcar.</color>");
        }
        
        sb.AppendLine("\n<size=18>(Gereksinimler tamamsa kilidi açmak için tıkla)</size>");
        return sb.ToString();
    }




}