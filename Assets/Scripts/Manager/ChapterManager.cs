using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Chapter'ları (Bölüm) yönetir, kilitlerini kontrol eder ve UI'da gösterir.
/// [YENİ] Artık merkezi GameValidator ve Requirement sistemini kullanır.
/// </summary>
public class ChapterManager : MonoBehaviour, IGameDataSaveable<ChapterSaveData>
{
    public static ChapterManager Instance { get; private set; }

    [Header("UI Referansları")]
    public Button previousButton;
    public Button nextButton;
    public TextMeshProUGUI chapterNumberText;

    [Header("Chapter İçerikleri")]
    public List<ChapterContent> chapters;

    private int currentChapterIndex = 0;

    // ========================================================================
    // REFACTORING 1: 'ChapterRequirement' sınıfı silindi.
    // ========================================================================
    // [System.Serializable]
    // public class ChapterRequirement { ... } // Bu sınıf SİLİNDİ.

    [System.Serializable]
    public class ChapterContent
    {
        public string chapterName;
        public GameObject contentPanel;
        public GameObject lockPanel;
        public TextMeshProUGUI lockRequirementsText;
        public bool isUnlocked = false;

        // [YENİ] Artık merkezi List<Requirement> yapısını kullanıyor
        [Tooltip("Bölümün kilidini açmak için gereken merkezi gereksinim listesi.")]
        public List<Requirement> requirements; 
    }

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        } 
        else 
        { 
            Destroy(gameObject); 
        }
    }

    void Start()
    {
        // İlk chapter her zaman açık
        if (chapters.Count > 0) 
            chapters[0].isUnlocked = true; 
        
        previousButton.onClick.AddListener(GoToPreviousChapter);
        nextButton.onClick.AddListener(GoToNextChapter);
        
        UpdateChapterDisplay();
    }

    public void GoToPreviousChapter() 
    { 
        if (currentChapterIndex > 0) 
        { 
            currentChapterIndex--; 
            UpdateChapterDisplay(); 
        } 
    }
    public void GoToNextChapter() 
    { 
        if (currentChapterIndex < chapters.Count - 1) 
        { 
            currentChapterIndex++; 
            UpdateChapterDisplay(); 
        } 
    }

    void UpdateChapterDisplay()
    {
        if (chapterNumberText == null || previousButton == null || nextButton == null) return;

        chapterNumberText.text = $"{currentChapterIndex + 1} / {chapters.Count}";
        previousButton.interactable = currentChapterIndex > 0;
        nextButton.interactable = currentChapterIndex < chapters.Count - 1;

        for (int i = 0; i < chapters.Count; i++)
        {
            bool isActiveChapter = i == currentChapterIndex;
            if (chapters[i].contentPanel != null)
                chapters[i].contentPanel.SetActive(isActiveChapter);

            if (isActiveChapter)
                CheckChapterLock(chapters[i]);
        }
    }
    
    /// <summary>
    /// [YENİ] Belirtilen chapter index'inin kilidinin açık olup olmadığını döndürür.
    /// GameValidator tarafından kullanılır.
    /// </summary>
    public bool IsChapterUnlocked(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapters.Count)
        {
            Debug.LogWarning($"IsChapterUnlocked: Geçersiz chapter index: {chapterIndex}");
            return false;
        }
        // Not: Bu, 'CheckChapterLock' fonksiyonunu tetiklemez, sadece mevcut durumu okur.
        // Kilitlerin açılması 'UpdateChapterDisplay' çağrıldığında kontrol edilir.
        return chapters[chapterIndex].isUnlocked;
    }

    void CheckChapterLock(ChapterContent chapter)
    {
        if (chapter.isUnlocked) 
        { 
            if(chapter.lockPanel != null) 
                chapter.lockPanel.SetActive(false); 
            return; 
        }
        
        // [YENİ] Refactor edilmiş 'AreRequirementsMet' fonksiyonunu çağırır
        if (AreRequirementsMet(chapter.requirements))
        {
            chapter.isUnlocked = true;
            if(chapter.lockPanel != null) 
                chapter.lockPanel.SetActive(false);
                
            Debug.Log($"Chapter '{chapter.chapterName}' kilidi açıldı!");
        }
        else
        {
            if (chapter.lockPanel != null) 
            {
                chapter.lockPanel.SetActive(true);
                // [YENİ] Refactor edilmiş 'GetRequirementsText' fonksiyonunu çağırır
                if (chapter.lockRequirementsText != null) 
                    chapter.lockRequirementsText.text = GetRequirementsText(chapter.requirements);
            }
        }
    }

    // ========================================================================
    // REFACTORING 2: 'AreRequirementsMet' fonksiyonu refactor edildi.
    // ========================================================================

    /// <summary>
    /// [YENİ] Gerekli tüm gereksinimlerin karşılanıp karşılanmadığını merkezi GameValidator'a sorar.
    /// </summary>
    bool AreRequirementsMet(List<Requirement> reqs)
    {
        // Tüm "spagetti" 'if' blokları silindi.
        return GameValidator.Instance.AreRequirementsMet(reqs);
    }

    // ========================================================================
    // REFACTORING 3: 'GetRequirementsText' fonksiyonu refactor edildi.
    // ========================================================================

    /// <summary>
    /// [YENİ] Gereksinim metnini merkezi RequirementTooltipFormatter'dan alır.
    /// </summary>
    string GetRequirementsText(List<Requirement> reqs)
    {
        // Tüm "spagetti" string birleştirme kodları silindi.
        // Orijinal başlığı ("Kilitli! Gereksinimler:\n\n") koruyoruz.
        return RequirementTooltipFormatter.GetFormattedRequirementText(reqs, "Kilitli! Gereksinimler:\n\n");
    }

    // ====================================================================================================
    // KAYIT SİSTEMİ (Bu kısım değişmedi)
    // ====================================================================================================

    public ChapterSaveData GetSaveData()
    {
        List<int> unlockedIndices = new List<int>();
        for (int i = 0; i < chapters.Count; i++)
        {
            if (chapters[i].isUnlocked) 
                unlockedIndices.Add(i);
        }
        
        Debug.Log("ChapterManager: Kayıt verisi oluşturuluyor.");
        return new ChapterSaveData { unlockedChapterIndices = unlockedIndices };
    }

    public void LoadFromData(ChapterSaveData data)
    {
        if (data?.unlockedChapterIndices == null) 
        {
            Debug.LogWarning("ChapterManager LoadFromData: Yüklenecek veri bulunamadı.");
            return;
        }

        for (int i = 1; i < chapters.Count; i++) 
        { 
            chapters[i].isUnlocked = false; 
        }

        foreach (int index in data.unlockedChapterIndices) 
        { 
            if (index < chapters.Count) 
                chapters[index].isUnlocked = true; 
        }
        
        UpdateChapterDisplay();
        Debug.Log($"ChapterManager verisi yüklendi. {data.unlockedChapterIndices.Count} kilit açık.");
    }
}