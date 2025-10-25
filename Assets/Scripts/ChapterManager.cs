using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Chapter'ları (Bölüm) yönetir, kilitlerini kontrol eder ve UI'da gösterir.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// (Kritik gereksinim kontrol hatası düzeltildi)
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

    [System.Serializable]
    public class ChapterRequirement
    {
        public int requiredLevel = 0;
        public double requiredGold = 0;
        public double requiredNexusCoin = 0;
        public double requiredHealth = 0;
        public double requiredMana = 0;
        public double requiredEnergy = 0;
        public double requiredPhysical = 0;
        public double requiredMental = 0;
        public double requiredPerception = 0;
        public double requiredSpiritual = 0;
        public double requiredLuck = 0;
        public double requiredSocial = 0;
        public List<QuestData> requiredQuests;
    }

    [System.Serializable]
    public class ChapterContent
    {
        public string chapterName;
        public GameObject contentPanel;
        public GameObject lockPanel;
        public TextMeshProUGUI lockRequirementsText;
        public bool isUnlocked = false;
        public ChapterRequirement requirements;
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
        
        // LoadFromData çağrılmadıysa bile (yeni oyun) UI'ı ilk kez ayarla
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

    /// <summary>
    /// Aktif chapter'ı gösterir, kilit durumunu kontrol eder ve UI'ı günceller.
    /// </summary>
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
    /// Belirtilen chapter'ın kilidini kontrol eder ve gerekiyorsa açar.
    /// </summary>
    void CheckChapterLock(ChapterContent chapter)
    {
        if (chapter.isUnlocked) 
        { 
            if(chapter.lockPanel != null) 
                chapter.lockPanel.SetActive(false); 
            return; 
        }
        
        if (AreRequirementsMet(chapter.requirements))
        {
            chapter.isUnlocked = true;
            // Kayıt işlemi artık GameDataManager tarafından toplu yapılacak.
            // 'SaveChapterUnlocks()' burada ÇAĞRILMAZ.
            
            if(chapter.lockPanel != null) 
                chapter.lockPanel.SetActive(false);
                
            Debug.Log($"Chapter '{chapter.chapterName}' kilidi açıldı!");
        }
        else
        {
            if (chapter.lockPanel != null) 
            {
                chapter.lockPanel.SetActive(true);
                if (chapter.lockRequirementsText != null) 
                    chapter.lockRequirementsText.text = GetRequirementsText(chapter.requirements);
            }
        }
    }

    /// <summary>
    /// Gerekli tüm stat, kaynak ve görevlerin tamamlanıp tamamlanmadığını KONTROL EDER.
    /// (TÜM GEREKSİNİMLER EKLENDİ - HATA DÜZELTİLDİ)
    /// </summary>
    bool AreRequirementsMet(ChapterRequirement req)
    {
        if (LevelManager.Instance.currentLevel < req.requiredLevel) return false;
        
        // Para birimleri
        if (CurrencyManager.Instance.gold < req.requiredGold) return false;
        if (CurrencyManager.Instance.nexusCoin < req.requiredNexusCoin) return false;

        // Kaynaklar
        if (ResourceManager.Instance.currentHealth < req.requiredHealth) return false;
        if (ResourceManager.Instance.currentMana < req.requiredMana) return false;
        if (ResourceManager.Instance.currentEnergy < req.requiredEnergy) return false;

        // Stat'lar
        if (StatManager.Instance.GetTotalPhysical() < req.requiredPhysical) return false;
        if (StatManager.Instance.GetTotalMental() < req.requiredMental) return false;
        if (StatManager.Instance.GetTotalPerception() < req.requiredPerception) return false;
        if (StatManager.Instance.GetTotalSpiritual() < req.requiredSpiritual) return false;
        if (StatManager.Instance.GetTotalLuck() < req.requiredLuck) return false;
        if (StatManager.Instance.GetTotalSocial() < req.requiredSocial) return false;

        // Görevler
        if (req.requiredQuests != null) 
        { 
            foreach (var quest in req.requiredQuests) 
            { 
                if (QuestManager.Instance.GetCompletionCount(quest.questID) <= 0) 
                    return false; 
            } 
        }
        
        return true;
    }

    /// <summary>
    /// Gereksinim metnini oyuncuya GÖSTERMEK için oluşturur.
    /// (TÜM GEREKSİNİMLER EKLENDİ - "ESKİ KOD"DAN ALINDI)
    /// </summary>
    string GetRequirementsText(ChapterRequirement req)
    {
        string text = "Kilitli! Gereksinimler:\n\n";
        if (req.requiredLevel > 0) text += $"- Seviye: {req.requiredLevel}\n";
        
        if (req.requiredGold > 0) text += $"- Altın: {req.requiredGold}\n";
        if (req.requiredNexusCoin > 0) text += $"- Nexus Coin: {req.requiredNexusCoin}\n";
        
        if (req.requiredHealth > 0) text += $"- Health: {req.requiredHealth}\n";
        if (req.requiredMana > 0) text += $"- Mana: {req.requiredMana}\n";
        if (req.requiredEnergy > 0) text += $"- Energy: {req.requiredEnergy}\n";
        
        if (req.requiredPhysical > 0) text += $"- Physical Stat: {req.requiredPhysical}\n";
        if (req.requiredMental > 0) text += $"- Mental Stat: {req.requiredMental}\n";
        if (req.requiredPerception > 0) text += $"- Perception Stat: {req.requiredPerception}\n";
        if (req.requiredSpiritual > 0) text += $"- Spiritual Stat: {req.requiredSpiritual}\n";
        if (req.requiredLuck > 0) text += $"- Luck Stat: {req.requiredLuck}\n";
        if (req.requiredSocial > 0) text += $"- Social Stat: {req.requiredSocial}\n";
        
        if (req.requiredQuests != null && req.requiredQuests.Count > 0)
        {
            text += "- Görevleri Tamamla:\n";
            foreach (var quest in req.requiredQuests) 
            { 
                if (quest != null)
                    text += $"  • {quest.questName}\n"; 
            }
        }
        return text;
    }

    // ====================================================================================================
    // KAYIT SİSTEMİ (GameDataManager UYUMLU)
    // ====================================================================================================

    /// <summary>
    /// GameDataManager'a kaydedilecek verileri toplar ve döndürür.
    /// </summary>
    public ChapterSaveData GetSaveData()
    {
        List<int> unlockedIndices = new List<int>();
        for (int i = 0; i < chapters.Count; i++)
        {
            if (chapters[i].isUnlocked) 
                unlockedIndices.Add(i);
        }
        
        Debug.Log("ChapterManager: Kayıt verisi oluşturuluyor."); // Eski koddan Debug log eklendi
        return new ChapterSaveData { unlockedChapterIndices = unlockedIndices };
    }

    /// <summary>
    /// GameDataManager'dan gelen verileri bu yöneticiye yükler.
    /// </summary>
    public void LoadFromData(ChapterSaveData data)
    {
        if (data?.unlockedChapterIndices == null) 
        {
            Debug.LogWarning("ChapterManager LoadFromData: Yüklenecek veri bulunamadı.");
            return;
        }

        // Kilitleri sıfırla (ilk chapter hariç)
        for (int i = 1; i < chapters.Count; i++) 
        { 
            chapters[i].isUnlocked = false; 
        }

        // Kayıttan gelen kilitleri aç
        foreach (int index in data.unlockedChapterIndices) 
        { 
            if (index < chapters.Count) 
                chapters[index].isUnlocked = true; 
        }
        
        UpdateChapterDisplay();
        Debug.Log($"ChapterManager verisi yüklendi. {data.unlockedChapterIndices.Count} kilit açık."); // Eski koddan Debug log eklendi
    }
}