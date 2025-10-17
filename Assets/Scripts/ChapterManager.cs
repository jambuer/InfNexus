using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ChapterManager : MonoBehaviour
{
    [Header("UI Referansları")]
    public Button previousButton;
    public Button nextButton;
    public TextMeshProUGUI chapterNumberText;

    [Header("Chapter İçerikleri")]
    public List<ChapterContent> chapters;

    private int currentChapterIndex = 0;

    // Gereksinimlerin daha esnek ve Inspector'dan yönetilebilir olması için yeni class yapısı
    [System.Serializable]
    public class ChapterRequirement
    {
        public int requiredLevel = 0;
        public double requiredGold = 0;
        public double requiredPhysical = 0;
        public double requiredMental = 0;
        public double requiredHealth = 0;
        public double requiredMana = 0;
        public double requiredEnergy = 0;
        public double requiredNexusCoin = 0;
        public double requiredLuck = 0;
        public double requiredSocial = 0;
        public double requiredPerception = 0;
        public double requiredSpiritual = 0;
        // Buraya diğer stat gereksinimlerini de ekleyebilirsin
        public List<QuestData> requiredQuests; // Tamamlanması gereken görevler
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

    void Start()
    {
        // İlk chapter her zaman açık
        if (chapters.Count > 0)
        {
            chapters[0].isUnlocked = true;
        }

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
        chapterNumberText.text = $"{currentChapterIndex + 1} / {chapters.Count}";

        previousButton.interactable = currentChapterIndex > 0;
        nextButton.interactable = currentChapterIndex < chapters.Count - 1;

        for (int i = 0; i < chapters.Count; i++)
        {
            if (i == currentChapterIndex)
            {
                // Mevcut chapter'ı göster ve kilidini kontrol et
                chapters[i].contentPanel.SetActive(true);
                CheckChapterLock(chapters[i]);
            }
            else
            {
                // Diğer chapter'ları gizle
                chapters[i].contentPanel.SetActive(false);
                if (chapters[i].lockPanel != null)
                {
                    chapters[i].lockPanel.SetActive(false);
                }
            }
        }
    }

    void CheckChapterLock(ChapterContent chapter)
    {
        if (chapter.isUnlocked)
        {
            if (chapter.lockPanel != null)
                chapter.lockPanel.SetActive(false);
            return;
        }

        bool requirementsMet = AreRequirementsMet(chapter.requirements);

        if (requirementsMet)
        {
            chapter.isUnlocked = true;
            if (chapter.lockPanel != null)
                chapter.lockPanel.SetActive(false);
            Debug.Log($"Chapter '{chapter.chapterName}' kilidi açıldı!");
        }
        else
        {
            if (chapter.lockPanel != null)
            {
                chapter.lockPanel.SetActive(true);
                // Gereksinim metnini dinamik olarak oluştur
                if (chapter.lockRequirementsText != null)
                {
                    chapter.lockRequirementsText.text = GetRequirementsText(chapter.requirements);
                }
            }
        }
    }

    bool AreRequirementsMet(ChapterRequirement req)
    {
        // Gerçek oyun verilerini kontrol et
        if (LevelManager.Instance.currentLevel < req.requiredLevel) return false;
        if (CurrencyManager.Instance.gold < req.requiredGold) return false;
        if (StatManager.Instance.GetTotalPhysical() < req.requiredPhysical) return false;
        if (StatManager.Instance.GetTotalMental() < req.requiredMental) return false;

        // Gerekli görevlerin tamamlanıp tamamlanmadığını kontrol et
        if (req.requiredQuests != null && req.requiredQuests.Count > 0)
        {
            foreach (var quest in req.requiredQuests)
            {
                if (QuestManager.Instance.GetCompletionCount(quest.questID) <= 0)
                {
                    return false; // Gerekli görevlerden biri bile tamamlanmamışsa
                }
            }
        }
        
        return true;
    }

    string GetRequirementsText(ChapterRequirement req)
    {
        string text = "Kilitli! Gereksinimler:\n\n";
        if (req.requiredLevel > 0) text += $"- Seviye: {req.requiredLevel}\n";
        if (req.requiredGold > 0) text += $"- Altın: {req.requiredGold}\n";
        if (req.requiredPhysical > 0) text += $"- Physical Stat: {req.requiredPhysical}\n";
        if (req.requiredMental > 0) text += $"- Mental Stat: {req.requiredMental}\n";
        if (req.requiredQuests != null && req.requiredQuests.Count > 0)
        if (req.requiredLuck > 0) text += $"- Luck Stat: {req.requiredLuck}\n";
        if (req.requiredSocial > 0) text += $"- Social Stat: {req.requiredSocial}\n";
        if (req.requiredPerception > 0) text += $"- Perception Stat: {req.requiredPerception}\n";
        if (req.requiredSpiritual > 0) text += $"- Spiritual Stat: {req.requiredSpiritual}\n";
        if (req.requiredHealth > 0) text += $"- Health: {req.requiredHealth}\n";
        if (req.requiredMana > 0) text += $"- Mana: {req.requiredMana}\n";
        if (req.requiredEnergy > 0) text += $"- Energy: {req.requiredEnergy}\n";
        if (req.requiredNexusCoin > 0) text += $"- Nexus Coin: {req.requiredNexusCoin}\n";

        {
            text += "- Görevleri Tamamla:\n";
            foreach (var quest in req.requiredQuests)
            {
                text += $"  • {quest.questName}\n";
            }
        }
        return text;
    }
}