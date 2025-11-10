using UnityEngine;
using TMPro; // TextMeshPro Dropdown için
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// City/Gathering panelinin tamamını yönetir.
/// Chapter Dropdown'unu, Bölge Butonlarını ve Node Kartlarını doldurur.
/// </summary>
public class GatheringUIManager : MonoBehaviour
{
    // Bu paneli yöneten tekil örnek (Instance)
    public static GatheringUIManager Instance { get; private set; }

    [Header("Veritabanı")]
    [SerializeField]
    private GatheringDatabase gatheringDatabase;

    [Header("UI Referansları - Sol Panel")]
    [SerializeField]
    private TMP_Dropdown chapterDropdown;
    
    [SerializeField]
    private Transform regionButtonContainer; // Bölge butonlarının (Kuzey, Güney..) oluşturulacağı yer

    [Header("UI Referansları - Orta Panel")]
    [SerializeField]
    private Transform nodeContainer; // GatheringNode prefablarının oluşturulacağı yer
    
    [Header("Prefab Referansları")]
    [SerializeField]
    private GameObject regionButtonPrefab; // Üzerinde 'GatheringRegionButton' script'i olan prefab
    
    [SerializeField]
    private GameObject gatheringNodePrefab; // Üzerinde 'GatheringNodeUI' script'i olan prefab

    private List<GatheringChapterData> _chapters;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        InitializeChapterDropdown();
    }

    /// <summary>
    /// Veritabanından Chapter'ları okur ve Dropdown'u doldurur.
    /// </summary>
    private void InitializeChapterDropdown()
    {
        if (gatheringDatabase == null || chapterDropdown == null)
        {
            Debug.LogError("[GatheringUIManager] Database veya Dropdown referansı eksik!");
            return;
        }

        _chapters = gatheringDatabase.allChapters;
        chapterDropdown.ClearOptions();

        List<string> chapterNames = new List<string>();
        foreach (var chapter in _chapters)
        {
            chapterNames.Add(chapter.chapterName);
        }
        
        chapterDropdown.AddOptions(chapterNames);
        
        // Dropdown değiştiğinde 'OnChapterSelected' fonksiyonunu çağır
        chapterDropdown.onValueChanged.AddListener(OnChapterSelected);

        // Paneli ilk Chapter ile başlat
        OnChapterSelected(0);
    }

    /// <summary>
    /// Dropdown'dan bir Chapter seçildiğinde tetiklenir.
    /// </summary>
    public void OnChapterSelected(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= _chapters.Count) return;

        GatheringChapterData selectedChapter = _chapters[chapterIndex];

        // 1. Mevcut bölge butonlarını temizle
        foreach (Transform child in regionButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Mevcut node kartlarını temizle
        ClearNodePanel();

        // 3. Yeni bölge butonlarını oluştur
        if (regionButtonPrefab == null) return;
        
        foreach (var region in selectedChapter.regions)
        {
            GameObject buttonObj = Instantiate(regionButtonPrefab, regionButtonContainer);
            GatheringRegionButton buttonScript = buttonObj.GetComponent<GatheringRegionButton>();
            
            if (buttonScript != null)
            {
                buttonScript.Setup(region, this);
            }
        }

        // 4. (İsteğe bağlı) İlk bölgeyi otomatik seç
        if (selectedChapter.regions.Count > 0)
        {
            OnRegionSelected(selectedChapter.regions[0]);
        }
    }

    /// <summary>
    /// Bir bölge butonu (Kuzey, Güney vb.) tıklandığında çağrılır.
    /// </summary>
    public void OnRegionSelected(GatheringRegion region)
    {
        // 1. Orta paneli temizle
        ClearNodePanel();

        // 2. Yeni Node Kartlarını (Prefabları) oluştur
        if (gatheringNodePrefab == null) return;

        foreach (var nodeData in region.gatheringNodes)
        {
            GameObject nodeObj = Instantiate(gatheringNodePrefab, nodeContainer);
            GatheringNodeUI nodeScript = nodeObj.GetComponent<GatheringNodeUI>();
            
            if (nodeScript != null)
            {
                nodeScript.Setup(nodeData); // 4. Adımdaki script'e veriyi gönder
            }
        }
    }

    /// <summary>
    /// Orta paneldeki tüm node kartlarını temizler.
    /// </summary>
    private void ClearNodePanel()
    {
        foreach (Transform child in nodeContainer)
        {
            Destroy(child.gameObject);
        }
    }
}