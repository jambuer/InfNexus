using UnityEngine;
using UnityEngine.UI;
using TMPro;
// using System.Collections.Generic; // Gelecekte gereksinim/ödül listesi için

/// <summary>
/// Orta panelde görünen "Odun Topla" gibi bir eylem kartının UI script'i.
/// </summary>
public class GatheringNodeUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField]
    private Image nodeIcon; // Odun görseli

    [SerializeField]
    private TextMeshProUGUI nodeNameText; // Eylemin ismi

    [SerializeField]
    private TextMeshProUGUI nodeDescriptionText; // Açıklaması

    [SerializeField]
    private Slider progressBar; // Toplama süresi progress bar'ı

    [SerializeField]
    private Button gatherButton; // Toplama butonu

    [SerializeField]
    private Button autoGatherButton; // Sürekli toplama butonu
    
    [SerializeField]
    private TextMeshProUGUI requirementsText; // Gereksinimleri gösterecek text alanı
    
    [SerializeField]
    private Transform rewardIconContainer; // Düşecek item'ların görselleri için

    private GatheringNodeData _nodeData;

    /// <summary>
    /// Bu kartı ilgili toplayıcılık verisi ile kurar.
    /// </summary>
    public void Setup(GatheringNodeData nodeData)
    {
        _nodeData = nodeData;

        if (_nodeData == null) return;

        // Verileri UI elemanlarına ata
        if (nodeIcon != null)
        {
            nodeIcon.sprite = _nodeData.icon;
            nodeIcon.gameObject.SetActive(_nodeData.icon != null);
        }

        if (nodeNameText != null)
            nodeNameText.text = _nodeData.displayName;

        if (nodeDescriptionText != null)
            nodeDescriptionText.text = _nodeData.description;

        // TODO: "requirementsText" alanını doldur
        // _nodeData.costToPerform listesini kullanarak (GameValidator'ı çağırarak)
        // dinamik bir gereksinim metni oluştur.
        
        // TODO: "rewardIconContainer" alanını doldur
        // _nodeData.rewards listesindeki ItemData'ları kullanarak
        // ödül ikonlarını (prefab) oluştur.
        
        // TODO: Butonlara 'OnClick' fonksiyonları ekle
        // gatherButton.onClick.AddListener(StartGathering);
        // autoGatherButton.onClick.AddListener(ToggleAutoGather);

        if (progressBar != null)
            progressBar.value = 0; // Başlangıçta 0
    }
    
    // TODO: Aşağıdaki fonksiyonları ileride dolduracağız
    // private void StartGathering() { /* Tıklandığında toplama başlat */ }
    // private void ToggleAutoGather() { /* Sürekli toplamayı aç/kapat */ }
}