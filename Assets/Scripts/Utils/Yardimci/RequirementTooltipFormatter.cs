using UnityEngine;
using System.Collections.Generic;
using System.Text; // StringBuilder için gerekli

/// <summary>
/// Bir 'Requirement' listesini alıp, oyuncunun mevcut durumuna göre
/// (örn: "Seviye: 5/10" <color=red>) formatlayan statik bir yardımcı sınıftır.
/// Tooltip (İpucu) metinleri oluşturmak için kullanılır.
/// </summary>
public static class RequirementTooltipFormatter
{
    private const string COLOR_MET_STR = "green"; // Veya "#00FF00"
    private const string COLOR_NOT_MET_STR = "red"; // Veya "#FF0000"

    /// <summary>
    /// Bir gereksinim listesini, oyuncunun mevcut durumunu da içerecek şekilde
    /// (örn: 5/10 Altın) renkli bir metin olarak formatlar.
    /// </summary>
    public static string GetFormattedRequirementText(List<Requirement> requirements, string title = "Gereksinimler:")
    {
        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(title))
        {
            sb.AppendLine($"<b>{title}</b>");
        }

        if (requirements == null || requirements.Count == 0)
        {
            sb.AppendLine("Yok");
            return sb.ToString();
        }

        // GameValidator'daki gibi tüm yöneticilere erişeceğiz
        // (Null check'ler eklemek iyi bir pratiktir)
        bool managersAvailable = GameValidator.Instance != null && // Validator'a ihtiyacımız var
                                 LevelManager.Instance != null &&
                                 StatManager.Instance != null &&
                                 Inventory.Instance != null &&
                                 ItemManager.Instance != null &&
                                 QuestManager.Instance != null &&
                                 CurrencyManager.Instance != null &&
                                 ResourceManager.Instance != null &&
                                 PerkManager.Instance != null;

        if (!managersAvailable)
        {
            Debug.LogError("RequirementTooltipFormatter: Gerekli tüm yöneticiler bulunamadı!");
            return "<color=red>Hata: Yöneticiler yüklenemedi.</color>";
        }

        foreach (var req in requirements)
        {
            bool requirementMet = false;
            string currentStatusText = ""; // "MevcutDeğer / GerekliDeğer"
            string requirementName = req.requirementName; // Eşya, Stat, Perk adı...

            switch (req.reqType)
            {
                case RequirementType.Level:
                    requirementMet = GameValidator.Instance.CheckLevelRequirement(req.requiredValue);
                    currentStatusText = $"{LevelManager.Instance.currentLevel} / {req.requiredValue}";
                    requirementName = "Seviye";
                    break;
                case RequirementType.Stat:
                    requirementMet = GameValidator.Instance.CheckStatRequirement(req.requirementName, req.requiredValue);
                    currentStatusText = $"{StatManager.Instance.GetTotalStat(req.requirementName)} / {req.requiredValue}";
                    // requirementName zaten doğru (örn: "Physical")
                    break;
                case RequirementType.Item:
                    ItemData itemData = ItemManager.Instance.GetItemByName(req.requirementName);
                    int currentAmount = (itemData == null) ? 0 : Inventory.Instance.GetItemCount(itemData);
                    requirementMet = currentAmount >= req.requiredValue;
                    currentStatusText = $"{currentAmount} / {req.requiredValue}";
                    // requirementName zaten doğru (örn: "Odun")
                    break;
                case RequirementType.Quest:
                    requirementMet = GameValidator.Instance.IsQuestCompleted(req.requirementName);
                    // Quest için "1/1" göstermek yerine sadece "Tamamlandı" veya "Tamamlanmadı"
                    currentStatusText = requirementMet ? "(Tamamlandı)" : "(Eksik)";
                    // requirementName zaten Quest ID (veya adı)
                    break;
                case RequirementType.Gold:
                    requirementMet = GameValidator.Instance.CheckCurrencyRequirement("Gold", req.requiredValue);
                    currentStatusText = $"{CurrencyManager.Instance.gold:N0} / {req.requiredValue:N0}";
                    requirementName = "Altın";
                    break;
                case RequirementType.NexusCoin:
                    requirementMet = GameValidator.Instance.CheckCurrencyRequirement("NexusCoin", req.requiredValue);
                    currentStatusText = $"{CurrencyManager.Instance.nexusCoin:N0} / {req.requiredValue:N0}";
                    requirementName = "NexusCoin";
                    break;
                case RequirementType.People:
                    requirementMet = GameValidator.Instance.CheckCurrencyRequirement("People", req.requiredValue);
                    currentStatusText = $"{CurrencyManager.Instance.people:N0} / {req.requiredValue:N0}";
                    requirementName = "Nüfus"; // (People)
                    break;
                case RequirementType.Health:
                    requirementMet = GameValidator.Instance.CheckResourceRequirement(PlayerFightMechanics.ResourceType.Health, (float)req.requiredValue);
                    currentStatusText = $"{ResourceManager.Instance.currentHealth:F0} / {(float)req.requiredValue:F0}";
                    requirementName = "Mevcut Can";
                    break;
                case RequirementType.Energy:
                     requirementMet = GameValidator.Instance.CheckResourceRequirement(PlayerFightMechanics.ResourceType.Energy, (float)req.requiredValue);
                     currentStatusText = $"{ResourceManager.Instance.currentEnergy:F0} / {(float)req.requiredValue:F0}";
                     requirementName = "Mevcut Enerji";
                    break;
                case RequirementType.Mana:
                     requirementMet = GameValidator.Instance.CheckResourceRequirement(PlayerFightMechanics.ResourceType.Mana, (float)req.requiredValue);
                     currentStatusText = $"{ResourceManager.Instance.currentMana:F0} / {(float)req.requiredValue:F0}";
                     requirementName = "Mevcut Mana";
                    break;
                case RequirementType.Perk:
                    requirementMet = GameValidator.Instance.HasPerk(req.requirementName);
                    // Perk için "1/1" göstermek yerine sadece "Var" veya "Yok"
                    currentStatusText = requirementMet ? "(Mevcut)" : "(Eksik)";
                    requirementName = $"Perk: {req.requirementName}";
                    break;
                default:
                    currentStatusText = "(Bilinmeyen Tip)";
                    requirementMet = false;
                    break;
            }

            // Rengi belirle
            string color = requirementMet ? COLOR_MET_STR : COLOR_NOT_MET_STR;
            
            // Satırı formatla
            sb.AppendLine($"<color={color}>{requirementName}: {currentStatusText}</color>");
        }

        return sb.ToString();
    }
}