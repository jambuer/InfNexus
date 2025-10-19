using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // ScrollRect için eklendi

/// <summary>
/// Görev listesini yönetir. Belirtilen zorluk seviyesine göre görevleri oluşturur ve gösterir.
/// </summary>
public class QuestPanelUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [Tooltip("QuestItemUI prefab'ının ekleneceği content objesi.")]
    public Transform questContentParent;
    [Tooltip("Oluşturulacak görev arayüzü prefab'ı.")]
    public GameObject questItemPrefab;
    [Tooltip("Görev listesini içeren ScrollRect bileşeni.")]
    public ScrollRect scrollRect; // En üste kaydırmak için referans eklendi

    [Header("Görev Listeleri (Zorluğa Göre)")]
    public List<QuestData> easyQuests;
    public List<QuestData> normalQuests;
    public List<QuestData> hardQuests;
    public List<QuestData> veryHardQuests;
    public List<QuestData> nightmareQuests;

    private List<GameObject> _instantiatedQuestItems = new List<GameObject>();

    void Start()
    {
        // DifficultyManager'ı dinlemeye başla
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnDifficultyChanged += PopulateQuestsForDifficulty;
        }

        // Başlangıçta mevcut zorluk seviyesine göre görevleri doldur
        PopulateQuestsForDifficulty(DifficultyManager.Instance.currentDifficulty);
    }

    private void OnDestroy()
    {
        // Dinlemeyi bırak
        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnDifficultyChanged -= PopulateQuestsForDifficulty;
        }
    }

    /// <summary>
    /// Belirtilen zorluk seviyesi için görev listesini temizler ve yeniden oluşturur.
    /// </summary>
    private void PopulateQuestsForDifficulty(DifficultyManager.Difficulty difficulty)
    {
        // 1. Mevcut görev arayüzlerini temizle
        foreach (GameObject item in _instantiatedQuestItems)
        {
            Destroy(item);
        }
        _instantiatedQuestItems.Clear();

        // 2. Doğru görev listesini seç
        List<QuestData> questsToDisplay = GetQuestListForDifficulty(difficulty);

        // 3. Seçilen listedeki her görev için yeni arayüz elemanı oluştur
        if (questsToDisplay != null)
        {
            foreach (QuestData quest in questsToDisplay)
            {
                GameObject newQuestItem = Instantiate(questItemPrefab, questContentParent);
                QuestItemUI questItemUI = newQuestItem.GetComponent<QuestItemUI>();
                if (questItemUI != null)
                {
                    questItemUI.Setup(quest);
                }
                _instantiatedQuestItems.Add(newQuestItem);
            }
        }

        // 4. Scroll bar'ı en üste sıfırla (isteğe bağlı ama önerilir)
        if (scrollRect != null)
        {
            // Canvas'ın güncellenmesini beklemek için bir frame gecikme ekliyoruz.
            StartCoroutine(ResetScrollPosition());
        }
    }

    /// <summary>
    /// Zorluk seviyesine göre ilgili görev listesini döndürür.
    /// </summary>
    private List<QuestData> GetQuestListForDifficulty(DifficultyManager.Difficulty difficulty)
    {
        switch (difficulty)
        {
            case DifficultyManager.Difficulty.Easy:
                return easyQuests;
            case DifficultyManager.Difficulty.Normal:
                return normalQuests;
            case DifficultyManager.Difficulty.Hard:
                return hardQuests;
            case DifficultyManager.Difficulty.VeryHard:
                return veryHardQuests;
            case DifficultyManager.Difficulty.Nightmare:
                return nightmareQuests;
            default:
                return new List<QuestData>(); // Boş liste
        }
    }
    
    private System.Collections.IEnumerator ResetScrollPosition()
    {
        // Bir frame bekle, layout'un güncellenmesini sağla
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}