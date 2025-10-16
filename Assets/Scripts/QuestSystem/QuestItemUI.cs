using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Görev listesindeki tek bir görevin görsel temsilini ve kullanıcı etkileşimlerini yönetir.
/// </summary>
public class QuestItemUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [Tooltip("Görevin ikonunu gösteren Image bileşeni.")]
    public Image questIcon;
    [Tooltip("Görevin adını gösteren TextMeshPro bileşeni.")]
    public TextMeshProUGUI questNameText;
    [Tooltip("Görevin tamamlanma sayısını ve limitini gösteren TextMeshPro bileşeni.")]
    public TextMeshProUGUI completionCountText;
    [Tooltip("Görevin ilerlemesini gösteren Slider bileşeni.")]
    public Slider progressBar;
    [Tooltip("Görevi başlatan Button bileşeni.")]
    public Button startButton;
    [Tooltip("Görevi otomatik tekrar modunda başlatan Button bileşeni.")]
    public Button autoButton;

    private QuestData _questData;
    private Coroutine _activeCoroutine;
    private bool _isAutoRepeating = false;

    /// <summary>
    /// Bu UI elemanını belirtilen QuestData ile kurar ve görselini günceller.
    /// </summary>
    /// <param name="questToSetup">Görüntülenecek görevin verisi.</param>
    public void Setup(QuestData questToSetup)
    {
        _questData = questToSetup;
        questIcon.sprite = _questData.questIcon;
        questNameText.text = _questData.questName;

        // Butonların tıklama olaylarını ayarla
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartButtonClicked);
        
        // Şimdilik autoButton'ı devre dışı bırakalım, bu mantığı daha sonra ekleyebiliriz.
        if (autoButton != null)
        {
            autoButton.gameObject.SetActive(_questData.automationData.canBeAutomated);
            autoButton.onClick.RemoveAllListeners();
            autoButton.onClick.AddListener(OnAutoButtonClicked);
        }

        // QuestManager'dan gelen ilerleme güncellemelerini dinle
        QuestManager.Instance.OnQuestProgress += UpdateCompletionCount;

        UpdateCompletionCount(questToSetup, QuestManager.Instance.GetCompletionCount(questToSetup.questID));
    }

    /// <summary>
    /// Görevin tamamlanma sayısını günceller.
    /// </summary>
    private void UpdateCompletionCount(QuestData updatedQuest, int newCount)
    {
        // Sadece bu UI elemanına ait görev güncellendiğinde işlem yap
        if (updatedQuest.questID != _questData.questID) return;

        if (_questData.completionLimit > 0)
        {
            completionCountText.text = $"{newCount} / {_questData.completionLimit}";
            // Limite ulaşıldıysa butonları pasif yap
            if (newCount >= _questData.completionLimit)
            {
                startButton.interactable = false;
                if(autoButton != null) autoButton.interactable = false;
            }
        }
        else
        {
            completionCountText.text = $"x{newCount}"; // Sınırsız ise sadece sayıyı göster
        }
    }

    private void OnStartButtonClicked()
    {
        // QuestManager'a görevi başlatma isteği gönder
        QuestManager.Instance.StartQuest(_questData);
    }
    
    private void OnAutoButtonClicked()
    {
        // TODO: Otomasyon mantığı buraya eklenecek.
        Debug.Log($"Otomasyon başlatılıyor: {_questData.questName}");
    }

    // Bu UI elemanı yok olduğunda, event dinlemeyi bırakır.
    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestProgress -= UpdateCompletionCount;
        }
    }
}