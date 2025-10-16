using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text; // StringBuilder'ı kullanmak için

/// <summary>
/// Görev listesindeki tek bir görevin görsel temsilini ve kullanıcı etkileşimlerini yönetir.
/// </summary>
public class QuestItemUI : MonoBehaviour
{
    [Header("UI Referansları - Temel")]
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

    // --- YENİ EKLENEN REFERANSLAR ---
    [Header("UI Referansları - Detaylar")]
    [Tooltip("Görevin etiketlerini (MainTag, SubTag) gösteren TextMeshPro bileşeni.")]
    public TextMeshProUGUI tagsText;
    [Tooltip("Görevin açıklamasını gösteren TextMeshPro bileşeni.")]
    public TextMeshProUGUI descriptionText;
    [Tooltip("Görevin altın ödül aralığını gösteren TextMeshPro bileşeni.")]
    public TextMeshProUGUI goldRewardText;
    [Tooltip("Görevin olası eşya ödüllerini listeleyen TextMeshPro bileşeni.")]
    public TextMeshProUGUI itemRewardsText;
    [Tooltip("Görevin gereksinimlerini listeleyen TextMeshPro bileşeni.")]
    public TextMeshProUGUI requirementsText;
    public TextMeshProUGUI xpRewardText; // İleride eklenecekse
    private TextMeshProUGUI _startButtonText;
    // --- YENİ EKLENENLER BİTTİ ---

    private QuestData _questData;

    /// <summary>
    /// Bu UI elemanını belirtilen QuestData ile kurar ve görselini günceller.
    /// </summary>
    public void Setup(QuestData questToSetup)
    {
        _questData = questToSetup;

        // --- GÖRSELLERİ DOLDURMA (GÜNCELLENDİ) ---
        questIcon.sprite = _questData.questIcon;
        questNameText.text = _questData.questName;

        // Yeni eklenen UI elemanlarını doldur
        tagsText.text = $"[{_questData.mainTag}] - [{_questData.subTag}]";
        descriptionText.text = _questData.description;
        goldRewardText.text = FormatGoldReward();
        itemRewardsText.text = FormatItemRewards();
        requirementsText.text = FormatRequirements();
        xpRewardText.text = $"{_questData.experienceReward:F0} XP"; // İleride eklenecekse

        // Butonların tıklama olaylarını ayarla
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartButtonClicked);

        if (autoButton != null)
        {
            autoButton.gameObject.SetActive(_questData.automationData.canBeAutomated);
            autoButton.onClick.RemoveAllListeners();
            autoButton.onClick.AddListener(OnAutoButtonClicked);
        }

        QuestManager.Instance.OnQuestProgress += UpdateCompletionCount;
        QuestManager.Instance.OnQuestProgressUpdate += UpdateProgressBar;

        // QuestManager'dan gelen ilerleme güncellemelerini dinle
        QuestManager.Instance.OnQuestProgress += UpdateCompletionCount;

        // Başlangıçtaki tamamlama sayısını ayarla
        UpdateCompletionCount(questToSetup, QuestManager.Instance.GetCompletionCount(questToSetup.questID));

        if (QuestManager.Instance.IsQuestActive(_questData.questID))
        {
            // Eğer görev zaten aktifse (örneğin sahne yeniden yüklendiğinde)
            // Bu kısım ileride daha da geliştirilebilir. Şimdilik sadece buton metnini ayarlar.
            if (_startButtonText != null) _startButtonText.text = "İptal Et";
        }
        else
        {
            progressBar.value = 0;
            if (_startButtonText != null) _startButtonText.text = "Başla";
        }
    }
    
    /// <summary>
    /// QuestManager'dan gelen ilerleme verisiyle progress bar'ı günceller.
    /// </summary>
    private void UpdateProgressBar(string questID, float progress)
    {
        if (_questData == null || questID != _questData.questID) return;

        progressBar.value = progress;
    }

    // --- YARDIMCI FORMATLAMA FONKSİYONLARI ---

    private string FormatGoldReward()
    {
        if (_questData.goldRewardTiers == null || _questData.goldRewardTiers.Count == 0)
        {
            return "Ödül: Yok";
        }

        double minGold = double.MaxValue;
        double maxGold = double.MinValue;

        foreach (var tier in _questData.goldRewardTiers)
        {
            if (tier.minAmount < minGold) minGold = tier.minAmount;
            if (tier.maxAmount > maxGold) maxGold = tier.maxAmount;
        }

        if (minGold >= maxGold)
        {
            return $"{minGold:F0}";
        }

        return $"{minGold:F0} - {maxGold:F0}";
    }

    private string FormatItemRewards()
    {
        if (_questData.itemRewards == null || _questData.itemRewards.Count == 0)
        {
            return "No Item";
        }

        StringBuilder sb = new StringBuilder("Olası Eşyalar:\n");
        foreach (var itemDrop in _questData.itemRewards)
        {
            if (itemDrop.itemToDrop != null)
            {
                sb.AppendLine($"- {itemDrop.itemToDrop.itemName} (%{itemDrop.dropChance * 100})");
            }
        }
        return sb.ToString();
    }

    private string FormatRequirements()
    {
        QuestRequirements req = _questData.requirements;
        StringBuilder sb = new StringBuilder("\n");
        
        if (req.requiredLevel > 1) sb.AppendLine($"- LVL {req.requiredLevel}");
        if (req.requiredHealth > 0) sb.AppendLine($"- Health {req.requiredHealth:F0}");
        if (req.requiredEnergy > 0) sb.AppendLine($"- Energy {req.requiredEnergy:F0}");
        if (req.requiredGold > 0) sb.AppendLine($"- Gold {req.requiredGold:F0}");
        // Diğer gereksinimleri de buraya ekleyebilirsiniz (NexusCoin, Statlar vb.)

        if (sb.Length <= "Gereksinimler:\n".Length)
        {
            return "";
        }
        
        return sb.ToString();
    }


    // --- MEVCUT FONKSİYONLAR (DEĞİŞİKLİK YOK) ---

    private void UpdateCompletionCount(QuestData updatedQuest, int newCount)
    {
        if (_questData == null || updatedQuest.questID != _questData.questID) return;

        // Görev tamamlandığında buton metnini "Başla" yap
        if (_startButtonText != null) _startButtonText.text = "Başla";

        if (_questData.completionLimit > 0)
        {
            completionCountText.text = $"{newCount} / {_questData.completionLimit}";
            if (newCount >= _questData.completionLimit)
            {
                startButton.interactable = false;
                if(autoButton != null) autoButton.interactable = false;
            }
        }
        else
        {
            completionCountText.text = $"x{newCount}";
        }
    }

    private void OnStartButtonClicked()
    {
        // --- GÜNCELLENEN BUTON MANTIĞI ---
        if (QuestManager.Instance.IsQuestActive(_questData.questID))
        {
            // Görev zaten çalışıyorsa, iptal etme isteği gönder
            QuestManager.Instance.CancelQuest(_questData.questID);
            if (_startButtonText != null) _startButtonText.text = "Başla";
        }
        else
        {
            // Görev çalışmıyorsa, başlatma isteği gönder
            QuestManager.Instance.StartQuest(_questData);
            if (_startButtonText != null) _startButtonText.text = "İptal Et";
        }
        // --- BİTTİ ---
    }
    
    private void OnAutoButtonClicked()
    {
        Debug.Log($"Otomasyon başlatılıyor: {_questData.questName}");
    }

    private void OnDestroy()
    {
        // --- GÜNCELLENEN KISIM ---
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestProgress -= UpdateCompletionCount;
            QuestManager.Instance.OnQuestProgressUpdate -= UpdateProgressBar; // Abonelikten çık
        }
        // --- BİTTİ ---
    }
}