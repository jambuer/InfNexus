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

    
    /* Kodları toparladıktan sonra devreye girecekler
    /// <summary>
    /// [YENİ] 'Requirement' listesini (yeni struct) tam, renkli bir metin bloğuna dönüştürür.
    /// </summary>
    
    public static string GetRequirementsMetin(List<Requirement> requirements, string colorMet = "green", string colorNotMet = "red")
    {
        if (requirements == null || requirements.Count == 0)
            return "";

        // Validator'un YENİ fonksiyonunu çağır
        if (GameValidator.Instance == null) return "Validator Yok"; 

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var req in requirements)
        {
            // YENİ CheckRequirement fonksiyonunu çağır
            bool isMet = GameValidator.Instance.CheckRequirement(req);
            // YENİ GetRequirementText fonksiyonunu çağır
            string line = GetRequirementText(req, isMet);

            sb.Append("<color=").Append(isMet ? colorMet : colorNotMet).Append(">");
            sb.Append(line).Append("</color>\n");
        }
        return sb.ToString().TrimEnd();
    }*/


/* daha sonra devreye girecekler
    /// <summary>
    /// [YENİ] Tek bir 'Requirement' (yeni struct) objesini metne dönüştürür.
    /// </summary>
    public static string GetRequirementText(Requirement req, bool isMet)
    {
        double currentValue;
        if (req.isHidden && !isMet) return "Gereksinim: ???";

        switch (req.requirementType)
        {
            case RequirementType.Level:
                currentValue = LevelManager.Instance?.currentLevel ?? 0;
                return $"Gereksinim: {currentValue:F0}/{req.amount:F0} Seviye";

            case RequirementType.Quest:
                return $"Görev Gerekli: {req.stringParameter}";

            case RequirementType.Item:
                currentValue = 0;
                ItemData item = req.itemData;
                if (item == null && !string.IsNullOrEmpty(req.stringParameter) && ItemManager.Instance != null)
                    item = ItemManager.Instance.GetItemByName(req.stringParameter);
                if (item != null && Inventory.Instance != null)
                    currentValue = Inventory.Instance.GetItemCount(item);
                return $"Gereken Eşya: {currentValue:F0}/{req.amount:F0} ({item?.itemName ?? req.stringParameter})";

            case RequirementType.Gold:
                currentValue = CurrencyManager.Instance?.gold ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Altın";

            case RequirementType.NexusCoin:
                currentValue = CurrencyManager.Instance?.nexusCoin ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Nexus Coin";

            case RequirementType.People:
                currentValue = CurrencyManager.Instance?.people ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Nüfus";



            case RequirementType.Stat:
                currentValue = StatManager.Instance?.GetTotalStat(req.stringParameter) ?? 0;
                return $"Gereken Stat: {currentValue:F0}/{req.amount:F0} ({req.stringParameter})";

            case RequirementType.Perk:
                currentValue = PerkManager.Instance?.GetPerkCount(req.stringParameter) ?? 0;
                return $"Gereken Perk: ({req.stringParameter}) (Mevcut: {currentValue:F0})";



            case RequirementType.Energy:
                currentValue = ResourceManager.Instance?.currentEnergy ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Enerji";

            case RequirementType.Health:
                currentValue = ResourceManager.Instance?.currentHealth ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Can";

            case RequirementType.Mana:
                currentValue = ResourceManager.Instance?.currentMana ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Mana";

            // === YENİ EKLENEN METİNLER ===
            case RequirementType.LifeSkillLevel:
                currentValue = LifeSkillManager.Instance?.GetLevel(req.lifeSkill) ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Seviye ({req.lifeSkill})";

            case RequirementType.JobLevel:
                currentValue = JobsManager.Instance?.GetLevel(req.job) ?? 0;
                return $"Gereken: {currentValue:F0}/{req.amount:F0} Seviye ({req.job})";

            case RequirementType.None:
                return "Gereksinim Yok";

            default:
                return $"Bilinmeyen Gereksinim: {req.requirementType}";
        }
    }*/
    
}

