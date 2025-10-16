using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Belirli bir kategorideki görevleri (örn: Bölüm 1 Görevleri) listeleyen UI panelini yönetir.
/// QuestItemUI prefab'larını oluşturur ve içeriği doldurur.
/// </summary>
public class QuestPanelUI : MonoBehaviour
{
    [Header("Panel Ayarları")]
    [Tooltip("Bu panelde gösterilecek olan görevlerin QuestData listesi.")]
    public List<QuestData> questsToShow;

    [Header("UI Referansları")]
    [Tooltip("Tek bir görevi temsil eden QuestItemUI prefab'ı.")]
    public GameObject questItemPrefab;
    [Tooltip("Oluşturulan görev UI'larının ekleneceği container transform (Vertical Layout Group içeren).")]
    public Transform questContainer;

    /// <summary>
    /// Bu panel aktif olduğunda görev listesini oluşturur veya günceller.
    /// </summary>
    void Start()
    {
        // Başlangıçta görev listesini doldur.
        PopulateQuestList();
    }

    /// <summary>
    /// Mevcut görev listesini temizler ve questsToShow listesindeki her görev için yeni bir UI elemanı oluşturur.
    /// </summary>
    public void PopulateQuestList()
    {
        if (questItemPrefab == null || questContainer == null)
        {
            Debug.LogError("QuestPanelUI: Prefab veya Container atanmamış!");
            return;
        }

        // Önceki listeden kalanları temizle
        foreach (Transform child in questContainer)
        {
            Destroy(child.gameObject);
        }

        // Gösterilecek her görev için bir UI elemanı oluştur
        foreach (QuestData quest in questsToShow)
        {
            // Prefab'dan yeni bir GameObject oluştur ve container'ın içine yerleştir.
            GameObject questItemInstance = Instantiate(questItemPrefab, questContainer);

            // Oluşturulan objenin üzerindeki QuestItemUI script'ini al.
            QuestItemUI questItem = questItemInstance.GetComponent<QuestItemUI>();

            // Eğer script bulunduysa, görevin verileriyle kur.
            if (questItem != null)
            {
                questItem.Setup(quest);
            }
            else
            {
                Debug.LogError($"QuestItemUI script'i, '{questItemPrefab.name}' prefab'ında bulunamadı!");
            }
        }
    }
}