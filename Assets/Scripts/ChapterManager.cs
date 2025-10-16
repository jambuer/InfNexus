using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChapterManager : MonoBehaviour
{
    [Header("UI References")]
    public Button previousButton;
    public Button nextButton;
    public TextMeshProUGUI chapterNumberText;
    public GameObject lockPanel; // Siyah yarı saydam panel
    public TextMeshProUGUI lockRequirementsText;

    [Header("Chapter Contents")]
    public GameObject[] chapterContents; // Her chapter'ın içeriği

    [Header("Chapter Settings")]
    public int currentChapter = 1;
    public int totalChapters = 3;

    // Chapter kilitleme durumları
    private bool[] chapterUnlocked;

    // Chapter gereksinimleri (test için - sonra değiştirebiliriz)
    [System.Serializable]
    public class ChapterRequirement
    {
        public int chapterNumber;
        public string requirementText;
        public bool isMet; // Gereksinim karşılandı mı?
    }

    public List<ChapterRequirement> chapterRequirements = new List<ChapterRequirement>();

    void Start()
    {

        // previousButton.onClick.AddListener(GoToPreviousChapter);
        // nextButton.onClick.AddListener(GoToNextChapter);
         
        // Chapter kilitleme array'ini oluştur
        chapterUnlocked = new bool[totalChapters + 1]; // +1 çünkü 0. index kullanmıyoruz
        chapterUnlocked[1] = true; // Chapter 1 her zaman açık

        // Test gereksinimleri ekle
        InitializeRequirements();

        // Butonlara fonksiyon bağla
        previousButton.onClick.AddListener(GoToPreviousChapter);
        nextButton.onClick.AddListener(GoToNextChapter);

        // İlk chapter'ı göster
        UpdateChapterDisplay();
    }

    void InitializeRequirements()
    {
        // Chapter 2 gereksinimleri
        chapterRequirements.Add(new ChapterRequirement
        {
            chapterNumber = 2,
            requirementText = "• Physical: 10 veya üzeri\n• 1000 Altın",
            isMet = false
        });

        // Chapter 3 gereksinimleri
        chapterRequirements.Add(new ChapterRequirement
        {
            chapterNumber = 3,
            requirementText = "• Mental: 15 veya üzeri\n• Chapter 2'deki tüm görevleri tamamla\n• 5000 Altın",
            isMet = false
        });
    }

    public void GoToPreviousChapter()
    {
        if (currentChapter > 1)
        {
            currentChapter--;
            UpdateChapterDisplay();
        }
    }

    public void GoToNextChapter()
    {
        if (currentChapter < totalChapters)
        {
            currentChapter++;
            UpdateChapterDisplay();
        }
    }

    void UpdateChapterDisplay()
    {
        // Sayfa numarasını güncelle
        chapterNumberText.text = $"{currentChapter} / {totalChapters}";

        // Butonları aktif/pasif yap
        previousButton.interactable = currentChapter > 1;
        nextButton.interactable = currentChapter < totalChapters;

        // Chapter içeriklerini göster/gizle
        for (int i = 0; i < chapterContents.Length; i++)
        {
            if (chapterContents[i] != null)
            {
                chapterContents[i].SetActive(i == currentChapter - 1);
            }
        }

        // Kilit kontrolü
        CheckChapterLock();
    }

    void CheckChapterLock()
    {
        // Chapter 1 ise kilit yok
        if (currentChapter == 1)
        {
            lockPanel.SetActive(false);
            return;
        }

        // Chapter kilidi kontrol et
        if (!chapterUnlocked[currentChapter])
        {
            // Gereksinimleri kontrol et
            bool requirementsMet = CheckRequirements(currentChapter);

            if (requirementsMet)
            {
                // Gereksanimler karşılandı, kilidi aç
                UnlockChapter(currentChapter);
                lockPanel.SetActive(false);
            }
            else
            {
                // Kilidi göster
                ShowLockPanel(currentChapter);
            }
        }
        else
        {
            // Chapter zaten açık
            lockPanel.SetActive(false);
        }
    }

    bool CheckRequirements(int chapter)
    {
        // İlgili chapter'ın gereksinimlerini bul
        ChapterRequirement req = chapterRequirements.Find(r => r.chapterNumber == chapter);
        
        if (req != null)
        {
            // Burada gerçek kontroller yapılacak
            // Şimdilik test için false döndürüyoruz
            return req.isMet;
        }

        return false; // Gereksinim bulunamadı
    }

    void ShowLockPanel(int chapter)
    {
        lockPanel.SetActive(true);

        // Gereksinimleri bul ve göster
        ChapterRequirement req = chapterRequirements.Find(r => r.chapterNumber == chapter);
        
        if (req != null)
        {
            lockRequirementsText.text = $"Chapter {chapter} Gereksinimleri:\n\n{req.requirementText}";
        }
        else
        {
            lockRequirementsText.text = $"Chapter {chapter} kilitli.";
        }
    }

    public void UnlockChapter(int chapter)
    {
        if (chapter > 0 && chapter <= totalChapters)
        {
            chapterUnlocked[chapter] = true;
            Debug.Log($"Chapter {chapter} açıldı!");
        }
    }

    // Test için - Inspector'dan çağrılabilir
    public void TestUnlockChapter2()
    {
        chapterRequirements.Find(r => r.chapterNumber == 2).isMet = true;
        UnlockChapter(2);
        UpdateChapterDisplay();
    }

    public void TestUnlockChapter3()
    {
        chapterRequirements.Find(r => r.chapterNumber == 3).isMet = true;
        UnlockChapter(3);
        UpdateChapterDisplay();
    }
}