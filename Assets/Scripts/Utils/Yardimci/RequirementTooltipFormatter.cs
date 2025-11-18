using UnityEngine;
using System.Collections.Generic;
using System.Text; // StringBuilder için gerekli

/// <summary>
/// 'Requirement' (Eski) ve 'GameRequirement' (Yeni) listelerini alıp,
/// oyuncunun mevcut durumuna göre (örn: "Seviye: 5/10" <color=red>) formatlayan
/// statik bir yardımcı sınıftır.
/// </summary>
public static class RequirementTooltipFormatter
{
    private const string COLOR_MET_STR = "green"; // Veya "#00FF00"
    private const string COLOR_NOT_MET_STR = "red"; // Veya "#FF0000"

    // Not: Her fonksiyon kendi StringBuilder'ını kullanarak
    // static bir sb'nin yaratabileceği çakışmaları önler.

    /// <summary>
    /// [ESKİ SİSTEM] - 'Requirement' (ItemDataModels.cs'den) listesini formatlar.
    /// InteractableObject gibi eski scriptler tarafından kullanılır.
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

        foreach (var req in requirements)
        {
            bool isMet = false;
            string reqText = "Bilinmeyen Gereksinim";
            string displayName;

            try
            {
                switch (req.reqType)
                {
                    case RequirementType.None:
                        isMet = true;
                        reqText = "Gereksinim Yok";
                        break;

                    case RequirementType.Level:
                        int currentLevel = (LevelManager.Instance != null) ? LevelManager.Instance.currentLevel : 0;
                        isMet = currentLevel >= req.requiredValue;
                        reqText = $"Seviye: {currentLevel} / {req.requiredValue}";
                        break;

                    case RequirementType.Stat:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        double currentStat = (StatManager.Instance != null) ? StatManager.Instance.GetTotalStat(req.requirementName) : 0;
                        isMet = currentStat >= (double)req.requiredValue;
                        reqText = $"{displayName}: {NumberFormatter.FormatNumber(currentStat)} / {NumberFormatter.FormatNumber(req.requiredValue)}";
                        break;

                    case RequirementType.Item:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        ItemData item = (ItemManager.Instance != null) ? ItemManager.Instance.GetItemByName(req.requirementName) : null;
                        int currentAmount = (item != null && Inventory.Instance != null) ? Inventory.Instance.GetItemCount(item) : 0;
                        isMet = currentAmount >= req.requiredValue;
                        reqText = $"{currentAmount} / {req.requiredValue} x {displayName}";
                        break;

                    case RequirementType.Quest:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        isMet = (QuestManager.Instance != null) && QuestManager.Instance.GetCompletionCount(req.requirementName) > 0;
                        reqText = isMet ? $"Tamamlandı: '{displayName}'" : $"Tamamla: '{displayName}'";
                        break;

                    case RequirementType.Gold:
                        double currentGold = (CurrencyManager.Instance != null) ? CurrencyManager.Instance.gold : 0;
                        isMet = currentGold >= (double)req.requiredValue;
                        reqText = $"Altın: {NumberFormatter.FormatNumber(currentGold)} / {NumberFormatter.FormatNumber(req.requiredValue)}";
                        break;

                    case RequirementType.NexusCoin:
                        double currentNexus = (CurrencyManager.Instance != null) ? CurrencyManager.Instance.nexusCoin : 0;
                        isMet = currentNexus >= (double)req.requiredValue;
                        reqText = $"Nexus Coin: {NumberFormatter.FormatNumber(currentNexus)} / {NumberFormatter.FormatNumber(req.requiredValue)}";
                        break;

                    case RequirementType.People:
                        double currentPeople = (CurrencyManager.Instance != null) ? CurrencyManager.Instance.people : 0;
                        isMet = currentPeople >= (double)req.requiredValue;
                        reqText = $"Nüfus: {NumberFormatter.FormatNumber(currentPeople)} / {NumberFormatter.FormatNumber(req.requiredValue)}";
                        break;

                    case RequirementType.Health:
                        float currentHealth = (ResourceManager.Instance != null) ? ResourceManager.Instance.currentHealth : 0;
                        isMet = currentHealth >= (float)req.requiredValue;
                        reqText = $"Mevcut Can: {currentHealth:N0} / {req.requiredValue}";
                        break;

                    case RequirementType.Energy:
                        float currentEnergy = (ResourceManager.Instance != null) ? ResourceManager.Instance.currentEnergy : 0;
                        isMet = currentEnergy >= (float)req.requiredValue;
                        reqText = $"Mevcut Enerji: {currentEnergy:N0} / {req.requiredValue}";
                        break;

                    case RequirementType.Mana:
                        float currentMana = (ResourceManager.Instance != null) ? ResourceManager.Instance.currentMana : 0;
                        isMet = currentMana >= (float)req.requiredValue;
                        reqText = $"Mevcut Mana: {currentMana:N0} / {req.requiredValue}";
                        break;

                    case RequirementType.Perk:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        isMet = (PerkManager.Instance != null) && PerkManager.Instance.GetPerkCount(req.requirementName) > 0;
                        reqText = isMet ? $"Perk Mevcut: '{displayName}'" : $"Perk Gerekli: '{displayName}'";
                        break;

                    case RequirementType.Chapter:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        isMet = (ChapterManager.Instance != null) && ChapterManager.Instance.IsChapterUnlocked(req.requiredValue);
                        reqText = isMet ? $"Bölüm {req.requiredValue} Açık" : $"Bölüm {req.requiredValue} Gerekli";
                        break;

                    case RequirementType.MaxHealth:
                        float currentMaxHealth = (ResourceManager.Instance != null) ? ResourceManager.Instance.maxHealth : 0;
                        isMet = currentMaxHealth >= (float)req.requiredValue;
                        reqText = $"Maks. Can: {currentMaxHealth:N0} / {req.requiredValue}";
                        break;

                    case RequirementType.MaxEnergy:
                        float currentMaxEnergy = (ResourceManager.Instance != null) ? ResourceManager.Instance.maxEnergy : 0;
                        isMet = currentMaxEnergy >= (float)req.requiredValue;
                        reqText = $"Maks. Enerji: {currentMaxEnergy:N0} / {req.requiredValue}";
                        break;

                    case RequirementType.MaxMana:
                        float currentMaxMana = (ResourceManager.Instance != null) ? ResourceManager.Instance.maxMana : 0;
                        isMet = currentMaxMana >= (float)req.requiredValue;
                        reqText = $"Maks. Mana: {currentMaxMana:N0} / {req.requiredValue}";
                        break;

                    case RequirementType.ExplorerQuest:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        isMet = (ExplorerManager.Instance != null) && ExplorerManager.Instance.GetExplorerQuestCompletionCount(req.requirementName) > 0;
                        reqText = isMet ? $"Keşif Tamamlandı: '{displayName}'" : $"Keşif Gerekli: '{displayName}'";
                        break;

                    case RequirementType.ExplorerTime:
                        isMet = false; // TODO
                        reqText = "Keşif Süresi (Uygulanmadı)";
                        break;

                    // --- [YENİ EKLENEN KISIM] ---
                    // TODO'lar, LifeSkillManager ve JobsManager kullanılarak düzeltildi.
                    case RequirementType.LifeSkillLevel:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        int currentSkillLevel = (LifeSkillManager.Instance != null) ? LifeSkillManager.Instance.GetSkillLevel(req.requirementName) : 0;
                        isMet = currentSkillLevel >= req.requiredValue;
                        reqText = $"{displayName}: {currentSkillLevel} / {req.requiredValue}";
                        break;

                    case RequirementType.JobLevel:
                        displayName = !string.IsNullOrEmpty(req.displayName) ? req.displayName : req.requirementName;
                        int currentJobLevel = (JobsManager.Instance != null) ? JobsManager.Instance.GetJobLevel(req.requirementName) : 0;
                        isMet = currentJobLevel >= req.requiredValue;
                        reqText = $"{displayName}: {currentJobLevel} / {req.requiredValue}";
                        break;
                    // --- [YENİ EKLENEN KISIM SONU] ---

                    default:
                        isMet = false;
                        reqText = $"({req.reqType} kontrolü eksik)";
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[RequirementTooltipFormatter] Gereksinim metni oluşturulurken hata! Tip: {req.reqType}, İsim: {req.requirementName}. Hata: {e.Message}");
                isMet = false;
                reqText = "<color=red>HATA</color>";
            }

            string color = isMet ? COLOR_MET_STR : COLOR_NOT_MET_STR;
            sb.AppendLine($"<color={color}>- {reqText}</color>");
        }
        
        return sb.ToString().TrimEnd();
    }
    
    
    /// <summary>
    /// [YENİ SİSTEM] - 'GameRequirement' (GatheringNodeUI için)
    /// (Tüm 'requirementName' hataları düzeltildi -> 'stringParameter' kullanıldı)
    /// </summary>
    /// <summary>
    /// [YENİ SİSTEM] - 'GameRequirement' (GatheringNodeUI için)
    /// (Tüm 'requirementName' hataları düzeltildi -> 'stringParameter' kullanıldı)
    /// </summary>
    public static string GetGameRequirementsTooltipText(List<GameRequirement> requirements)
    {
        StringBuilder sb = new StringBuilder();

        if (requirements == null || requirements.Count == 0)
        {
            return "Gereksinim Yok.";
        }

        var validator = GameValidator.Instance;
        if (validator == null) return "Validator bulunamadı.";

        bool first = true;
        foreach (var req in requirements)
        {
            if (!first) sb.AppendLine();

            // GameValidator'a eklediğimiz YENİ (ve artık düzeltilmiş) fonksiyonu çağırır.
            bool isMet = validator.CheckSingleGameRequirement(req);
            string color = isMet ? COLOR_MET_STR : COLOR_NOT_MET_STR;

            sb.Append($"<color={color}>- ");

            switch (req.requirementType)
            {
                case RequirementType.Level:
                    sb.Append($"Oyuncu Seviyesi: {LevelManager.Instance.currentLevel}/{req.amount}");
                    break;
                case RequirementType.Item:
                    // [DEĞİŞTİ] requirementName -> stringParameter
                    ItemData item = (ItemManager.Instance != null) ? ItemManager.Instance.GetItemByName(req.stringParameter) : null;
                    int currentAmount = (item != null && Inventory.Instance != null) ? Inventory.Instance.GetItemCount(item) : 0;
                    string itemLabel = (item != null && !string.IsNullOrEmpty(item.itemName)) ? item.itemName : req.stringParameter;
                    sb.Append($"{itemLabel}: {currentAmount}/{req.amount}");
                    break;
                case RequirementType.Quest:
                    // [DEĞİŞTİ] requirementName -> stringParameter
                    int completedCount = QuestManager.Instance.GetCompletionCount(req.stringParameter);
                    sb.Append($"Göv. Tamamla ({req.stringParameter}): {completedCount}/{req.amount}");
                    break;
                case RequirementType.Gold:
                    sb.Append($"Altın: {NumberFormatter.FormatNumber(CurrencyManager.Instance.gold)}/{NumberFormatter.FormatNumber(req.amount)}");
                    break;
                case RequirementType.NexusCoin:
                    sb.Append($"Nexus Coin: {NumberFormatter.FormatNumber(CurrencyManager.Instance.nexusCoin)}/{NumberFormatter.FormatNumber(req.amount)}");
                    break;
                case RequirementType.People:
                    sb.Append($"Nüfus: {NumberFormatter.FormatNumber(CurrencyManager.Instance.people)}/{NumberFormatter.FormatNumber(req.amount)}");
                    break;
                case RequirementType.Energy:
                    sb.Append($"Enerji: {ResourceManager.Instance.currentEnergy:N0}/{req.amount}");
                    break;
                case RequirementType.Health:
                    sb.Append($"Can: {ResourceManager.Instance.currentHealth:N0}/{req.amount}");
                    break;
                case RequirementType.LifeSkillLevel:
                // [DEĞİŞTİ] requirementName -> stringParameter
                int currentSkillLevel = LifeSkillManager.Instance.GetSkillLevel(req.stringParameter);
                sb.Append($"{req.stringParameter} Seviyesi: {currentSkillLevel}/{req.amount}");
                break;
            case RequirementType.JobLevel:
                // [DEĞİŞTİ] requirementName -> stringParameter
                int currentJobLevel = JobsManager.Instance.GetJobLevel(req.stringParameter);
                sb.Append($"{req.stringParameter} Seviyesi: {currentJobLevel}/{req.amount}");
                break;
                default:
                    sb.Append($"Bilinmeyen Gereksinim: {req.requirementType}");
                    break;
            }
            sb.Append("</color>");
            first = false;
        }
        return sb.ToString().TrimEnd();
    }
}