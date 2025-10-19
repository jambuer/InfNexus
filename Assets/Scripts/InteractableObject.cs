using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Fare olayları için bu kütüphane şart
using System.Collections;

// Hatanızın nedeni bu satırın veya bir sonraki {'in eksik olmasıydı.
public class InteractableObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Durum Ayarları")]
    public bool isLocked = true; // Obje kilitli mi?
    // public GerekliKosul kosul; // Buraya obje kilidini açacak koşulu eklersin

    [Header("Görsel Ayarlar")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    private Image objectImage;

    [Header("Tooltip (Bilgi Penceresi)")]
    public GameObject tooltipObject; // Üzerine gelince açılacak panel (Slider'ı da içermeli)
    public Text tooltipText;
    public string actionText = "Odun Topla"; // Inspector'dan her obje için değiştirilebilir

    [Header("İlerleme (Progress)")]
    public Slider progressBar; // Tooltip'in içindeki slider
    public float actionDuration = 3.0f; // Eylemin süresi (saniye)
    private bool isGathering = false; // Şu an toplama yapılıyor mu?

    [Header("Kazanım Geri Bildirimi")]
    public GameObject feedbackPanel; // Yeşil renkte "+5 Odun" yazacak panel
    public Text feedbackText;
    public string resourceGained = "Odun";
    public int amountGained = 5;

    [Header("Envanter Testi")]
    // TEST İÇİN EKLEDİĞİMİZ SATIR:
    public ItemData itemToGive; 

    void Start()
    {
        objectImage = GetComponent<Image>();
        UpdateObjectVisual();
        
        // Başlangıçta tüm pencereleri kapat
        if (tooltipObject != null) tooltipObject.SetActive(false);
        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }

    // Objenin kilitli/açık durumuna göre görselini günceller
    void UpdateObjectVisual()
    {
        if (isLocked)
        {
            objectImage.sprite = lockedSprite;
        }
        else
        {
            objectImage.sprite = unlockedSprite;
        }
    }

    // Dışarıdan bir script (örn: QuestManager) bu objenin kilidini açmak için çağırabilir
    public void UnlockObject()
    {
        isLocked = false;
        UpdateObjectVisual();
    }

    // ---- Fare Olayları ----

    // Fare üzerine geldiğinde
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Obje kilitli değilse ve şu an bir işlem yapmıyorsa
        if (!isLocked && !isGathering)
        {
            tooltipText.text = actionText;
            tooltipObject.SetActive(true);
        }
    }

    // Fare üzerinden çekildiğinde
    public void OnPointerExit(PointerEventData eventData)
    {
        // Toplama yapmıyorsa tooltip'i kapat
        if (!isGathering)
        {
            tooltipObject.SetActive(false);
        }
    }

    // Objeye tıklandığında
    public void OnPointerClick(PointerEventData eventData)
    {
        // Kilitli değilse ve zaten bir işlem yapmıyorsa
        if (!isLocked && !isGathering)
        {
            StartCoroutine(GatherResourceCoroutine());
        }
    }

    // ---- Ana İşlem Fonksiyonu ----

    private IEnumerator GatherResourceCoroutine()
    {
        isGathering = true;
        tooltipObject.SetActive(true); // İşlem sırasında tooltip açık kalsın
        progressBar.gameObject.SetActive(true);
        progressBar.value = 0;

        float timer = 0f;

        // Slider'ı doldur
        while (timer < actionDuration)
        {
            timer += Time.deltaTime;
            progressBar.value = timer / actionDuration;
            yield return null; // Bir sonraki frame'e kadar bekle
        }

        // İşlem bitti
        progressBar.gameObject.SetActive(false);
        
        // --- ENVANTER SİSTEMİ ---
        // itemToGive'in atanıp atanmadığını kontrol et (Null hata vermemesi için)
        if (itemToGive != null)
        {
            Inventory.Instance.AddItem(itemToGive, amountGained);
        }
        else
        {
            Debug.LogWarning("InteractableObject'a verilecek ItemData atanmamış!");
        }
        // ------------------------------------

        // Kazanım geri bildirimini göster
        feedbackText.text = $"+{amountGained} {resourceGained}";
        feedbackPanel.SetActive(true);

        // 2 saniye bekle
        yield return new WaitForSeconds(2f);

        // Geri bildirimi ve tooltip'i kapat
        feedbackPanel.SetActive(false);
        tooltipObject.SetActive(false);
        isGathering = false;
    }
}