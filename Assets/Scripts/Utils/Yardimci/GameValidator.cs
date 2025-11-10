using UnityEngine;
using PlayerFightMechanics; // PlayerAttackEffectType için gerekli
using System.Collections.Generic; // List için gerekli
using System.Linq; // LINQ gerekirse (örn: perk kontrolü için)

// Oyunun genel durumunu kontrol etmek için merkezi bir Singleton sınıfı.
// Örn: Bir yetenek aktif mi? Bir görev tamamlandı mı? Yeterli kaynak var mı?
public class GameValidator : Singleton<GameValidator> // Kullandığın Singleton<T> sınıfından miras al
{
    // --- Görev Kontrolleri ---

    /// <summary>
    /// Belirtilen ID'ye sahip görevin tamamlanıp tamamlanmadığını kontrol eder.
    /// </summary>
    public bool IsQuestCompleted(string questID)
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("GameValidator: QuestManager bulunamadı!");
            return false;
        }
        // QuestManager'daki GetCompletionCount metodu, görevin tamamlanma sayısını verir. 0'dan büyükse tamamlanmıştır.
        return QuestManager.Instance.GetCompletionCount(questID) > 0;
    }

    /// <summary>
    /// Belirtilen ID'ye sahip görevin şu anda aktif (kabul edilmiş) olup olmadığını kontrol eder.
    /// </summary>
    public bool IsQuestActive(string questID)
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("GameValidator: QuestManager bulunamadı!");
            return false;
        }
        // QuestManager'da IsQuestActive fonksiyonu olduğunu varsayıyoruz.
        return QuestManager.Instance.IsQuestActive(questID);
    }


    // --- Saldırı Efekti Kontrolleri ---

    /// <summary>
    /// Belirtilen saldırı efektinin oyuncu için kilidinin açık (kullanılabilir) olup olmadığını kontrol eder.
    /// Artık PlayerAttackEffectData assetindeki requiredPerkTag alanını kullanır.
    /// </summary>
    public bool IsAttackEffectUnlocked(PlayerAttackEffectType effectType)
    {
        // 1. Her zaman açık olan temel efektler:
        switch (effectType)
        {
            case PlayerAttackEffectType.NormalHit:
            case PlayerAttackEffectType.CriticalHit:

                return true; // Bu efektler her zaman açık kabul edilir.
        }

        // 2. Efektin verisini veritabanından al
        PlayerAttackEffectData effectData = null;
        // FightManager instance'ı üzerinden veritabanına erişim
        if (FightManager.Instance != null && FightManager.Instance.playerAttackEffectDatabase != null)
        {
            effectData = FightManager.Instance.playerAttackEffectDatabase.GetAttackEffectData(effectType);
        }
        else
        {
            Debug.LogError($"GameValidator: FightManager veya PlayerAttackEffectDatabase bulunamadı! Efekt kontrolü yapılamıyor: {effectType}");
            return false; // Veritabanına ulaşılamıyorsa kilitli varsay
        }

        // Veritabanında efekt yoksa (ve temel değilse) kilitlidir.
        if (effectData == null)
        {
            Debug.LogWarning($"GameValidator: PlayerAttackEffectDatabase'de '{effectType}' için veri bulunamadı.");
            return false;
        }

        // 3. Perk Gereksinimini Kontrol Et
        // 'requiredPerkTag' boş değilse (bir perk gerektiriyorsa)
        if (!string.IsNullOrEmpty(effectData.requiredPerkTag))
        {
            // Gerekli perk'e sahip miyiz? (HasPerk fonksiyonunu kullanarak)
            bool hasRequiredPerk = HasPerk(effectData.requiredPerkTag);
            if (!hasRequiredPerk)
            {
                // Gerekli perk yoksa, efekt kilitlidir.
                return false;
            }
            // Gerekli perk varsa, efekt açıktır.
            return true;
        }
        else
        {
            // 'requiredPerkTag' boşsa VE temel bir efekt değilse,
            // efektin kilidi varsayılan olarak açıktır (özel perk gerektirmiyor).
            return true;
        }
    }


    // --- Perk Kontrolü ---

    /// <summary>
    /// Oyuncunun belirtilen perke sahip olup olmadığını kontrol eder.
    /// </summary>
    public bool HasPerk(string perkName)
    {
        if (PerkManager.Instance == null)
        {
            Debug.LogError("GameValidator: PerkManager bulunamadı!");
            return false;
        }
        // PerkManager'daki GetPerkCount'un, perk mevcutsa 0'dan büyük döndürdüğünü varsayıyoruz.
        return PerkManager.Instance.GetPerkCount(perkName) > 0;
    }


    // --- Envanter Kontrolleri ---

    /// <summary>
    /// Oyuncunun envanterinde belirtilen isimdeki eşyadan yeterli miktarda olup olmadığını kontrol eder.
    /// </summary>
    public bool HasItem(string itemName, int requiredAmount = 1)
    {
        if (Inventory.Instance == null) { Debug.LogError("GameValidator: Inventory bulunamadı!"); return false; }
        if (ItemManager.Instance == null) { Debug.LogError("GameValidator: ItemManager bulunamadı!"); return false; }

        ItemData itemData = ItemManager.Instance.GetItemByName(itemName);
        if (itemData == null)
        {
            Debug.LogWarning($"GameValidator: '{itemName}' isminde bir eşya ItemManager'da bulunamadı.");
            return false;
        }
        // Envanterdeki sayıyı ItemData ile kontrol et
        return Inventory.Instance.GetItemCount(itemData) >= requiredAmount;
    }

    /// <summary>
    /// Oyuncunun envanterinde belirtilen ItemData asset'inden yeterli miktarda olup olmadığını kontrol eder.
    /// </summary>
    public bool HasItem(ItemData itemData, int requiredAmount = 1)
    {
        if (itemData == null) { Debug.LogWarning("GameValidator: HasItem(ItemData) null itemData ile çağrıldı."); return false; }
        if (Inventory.Instance == null) { Debug.LogError("GameValidator: Inventory bulunamadı!"); return false; }
        // Doğrudan ItemData ile envanteri kontrol et
        return Inventory.Instance.GetItemCount(itemData) >= requiredAmount;
    }


    // --- Stat, Seviye, Kaynak ve Para Birimi Kontrolleri ---

    /// <summary>
    /// Oyuncunun belirli bir stat değerinin gereken minimum değere ulaşıp ulaşmadığını kontrol eder.
    /// </summary>
    public bool CheckStatRequirement(string statName, double requiredValue)
    {
        if (StatManager.Instance == null) { Debug.LogError("GameValidator: StatManager bulunamadı!"); return false; }
        return StatManager.Instance.GetTotalStat(statName) >= requiredValue;
    }

    /// <summary>
    /// Oyuncunun seviyesinin gereken minimum seviyeye ulaşıp ulaşmadığını kontrol eder.
    /// </summary>
    public bool CheckLevelRequirement(int requiredLevel)
    {
        if (LevelManager.Instance == null) { Debug.LogError("GameValidator: LevelManager bulunamadı!"); return false; }
        return LevelManager.Instance.currentLevel >= requiredLevel;
    }

    /// <summary>
    /// Oyuncunun belirli bir kaynağının (Can, Enerji, Mana) gereken minimum miktarda olup olmadığını kontrol eder.
    /// </summary>
    public bool CheckResourceRequirement(PlayerFightMechanics.ResourceType resourceType, float requiredValue)
    {
        if (ResourceManager.Instance == null) { Debug.LogError("GameValidator: ResourceManager bulunamadı!"); return false; }
        switch (resourceType)
        {
            case PlayerFightMechanics.ResourceType.Health: return ResourceManager.Instance.currentHealth >= requiredValue;
            case PlayerFightMechanics.ResourceType.Energy: return ResourceManager.Instance.currentEnergy >= requiredValue;
            case PlayerFightMechanics.ResourceType.Mana: return ResourceManager.Instance.currentMana >= requiredValue;
            case PlayerFightMechanics.ResourceType.None: return true;
            default: Debug.LogWarning($"GameValidator: Bilinmeyen kaynak türü: {resourceType}"); return false;
        }
    }

    /// <summary>
    /// Oyuncunun belirli bir para biriminden (Gold, NexusCoin, People) gereken minimum miktarda olup olmadığını kontrol eder.
    /// </summary>
    public bool CheckCurrencyRequirement(string currencyType, double requiredValue)
    {
        if (CurrencyManager.Instance == null) { Debug.LogError("GameValidator: CurrencyManager bulunamadı!"); return false; }
        switch (currencyType.ToLowerInvariant())
        {
            case "gold": return CurrencyManager.Instance.gold >= requiredValue;
            case "nexuscoin": return CurrencyManager.Instance.nexusCoin >= requiredValue;
            case "people": return CurrencyManager.Instance.people >= requiredValue;
            default: Debug.LogWarning($"GameValidator: CheckCurrencyRequirement için bilinmeyen para birimi türü: {currencyType}"); return false;
        }
    }

    /*
    // ===================================================================
    // === GATHERING SİSTEMİ İÇİN YENİ (BOZMAYAN) KONTROL FONKSİYONLARI ===
    // === Bu fonksiyonlar RequirementType.cs'teki YENİ struct'ı kullanır ===
    // ===================================================================

    /// <summary>
    /// [YENİ] RequirementType.cs'teki 'Requirement' yapısını kullanan listeyi kontrol eder.
    /// </summary>
    public bool CheckRequirements(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return true;

        foreach (var req in requirements)
        {
            if (!CheckRequirement(req))
                return false; // Bir tanesi bile sağlanmazsa false dön
        }
        return true; // Hepsi sağlandıysa true dön
    }*/

    /*
    /// <summary>
    /// [YENİ] RequirementType.cs'teki 'Requirement' yapısını kullanan tekil gereksinimi kontrol eder.
    /// </summary>
    public bool CheckRequirement(Requirement req)
    {
        // YENİ struct'ın alan adlarını kullan (requirementType, amount, stringParameter)
        switch (req.requirementType)
        {
            case RequirementType.None:
                return true;

            case RequirementType.Level:
                if (LevelManager.Instance == null) return false;
                return LevelManager.Instance.currentLevel >= (int)req.amount;

            case RequirementType.Quest:
                if (QuestManager.Instance == null) return false;
                // GameValidator'daki mevcut fonksiyonu kullan
                return IsQuestCompleted(req.stringParameter); 

            case RequirementType.Item:
                if (Inventory.Instance == null) return false;
                ItemData item = req.itemData;
                if (item == null && !string.IsNullOrEmpty(req.stringParameter) && ItemManager.Instance != null)
                {
                    item = ItemManager.Instance.GetItemByName(req.stringParameter);
                }
                if (item == null) return false;
                return Inventory.Instance.GetItemCount(item) >= (int)req.amount;

            case RequirementType.Gold:
                if (CurrencyManager.Instance == null) return false;
                // GameValidator'daki mevcut fonksiyonu kullan
                return CheckCurrencyRequirement("gold", req.amount); 

            case RequirementType.NexusCoin:
                if (CurrencyManager.Instance == null) return false;
                return CheckCurrencyRequirement("nexuscoin", req.amount);

            case RequirementType.People:
                if (CurrencyManager.Instance == null) return false;
                return CheckCurrencyRequirement("people", req.amount);

           

            case RequirementType.Stat:
                if (StatManager.Instance == null) return false;
                // GameValidator'daki mevcut fonksiyonu kullan
                return CheckStatRequirement(req.stringParameter, req.amount);

            case RequirementType.Perk:
                if (PerkManager.Instance == null) return false;
                // GameValidator'daki mevcut fonksiyonu kullan
                return HasPerk(req.stringParameter);

            

            case RequirementType.Energy:
                if (ResourceManager.Instance == null) return false;
                // GameValidator'daki mevcut fonksiyonu kullan
                return CheckResourceRequirement(PlayerFightMechanics.ResourceType.Energy, (float)req.amount);

            case RequirementType.Health:
                if (ResourceManager.Instance == null) return false;
                return CheckResourceRequirement(PlayerFightMechanics.ResourceType.Health, (float)req.amount);

            case RequirementType.Mana:
                if (ResourceManager.Instance == null) return false;
                return CheckResourceRequirement(PlayerFightMechanics.ResourceType.Mana, (float)req.amount);

            // === YENİ EKLEDİĞİMİZ KONTROLLER ===
            case RequirementType.LifeSkillLevel:
                if (LifeSkillManager.Instance == null) return false;
                return LifeSkillManager.Instance.GetLevel(req.lifeSkill) >= (int)req.amount;

            case RequirementType.JobLevel:
                if (JobsManager.Instance == null) return false;
                return JobsManager.Instance.GetLevel(req.job) >= (int)req.amount;
            
            default:
                Debug.LogWarning($"[GameValidator] CheckRequirement (Yeni): Bilinmeyen RequirementType '{req.requirementType}'.");
                return false;
        }
    } */


    // --- Genel Gereksinim Listesi Kontrolü ---

    /// <summary>
    /// Verilen Requirement listesindeki tüm gereksinimlerin karşılanıp karşılanmadığını kontrol eder.
    /// </summary>
    public bool AreRequirementsMet(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return true;

        foreach (var req in requirements)
        {
            bool requirementMet = false;
            // Requirement struct'ındaki alan adlarını kullan (reqType, requiredValue, requirementName)
            switch (req.reqType)
            {
                case RequirementType.Level:
                    requirementMet = CheckLevelRequirement(req.requiredValue); // 'int' dönüşümü varsayılıyor
                    break;
                case RequirementType.Stat:
                    requirementMet = CheckStatRequirement(req.requirementName, req.requiredValue); // 'double' dönüşümü varsayılıyor
                    break;
                case RequirementType.Item:
                    // ItemData'yı requirementName'den alıp kontrol et
                    requirementMet = HasItem(req.requirementName, req.requiredValue); // 'int' dönüşümü varsayılıyor
                    break;
                case RequirementType.Quest:
                    requirementMet = IsQuestCompleted(req.requirementName); // requirementName'in Quest ID olduğunu varsayıyoruz
                    break;
                case RequirementType.Gold:
                    requirementMet = CheckCurrencyRequirement("Gold", req.requiredValue); // 'double' dönüşümü varsayılıyor
                    break;
                case RequirementType.NexusCoin:
                    requirementMet = CheckCurrencyRequirement("NexusCoin", req.requiredValue);
                    break;
                case RequirementType.People:
                    requirementMet = CheckCurrencyRequirement("People", req.requiredValue);
                    break;
                case RequirementType.Health:
                    requirementMet = CheckResourceRequirement(PlayerFightMechanics.ResourceType.Health, (float)req.requiredValue); // float dönüşümü
                    break;
                case RequirementType.Energy:
                    requirementMet = CheckResourceRequirement(PlayerFightMechanics.ResourceType.Energy, (float)req.requiredValue);
                    break;
                case RequirementType.Mana:
                    requirementMet = CheckResourceRequirement(PlayerFightMechanics.ResourceType.Mana, (float)req.requiredValue);
                    break;
                case RequirementType.Perk:
                    // requirementName'in Perk adı/tag'i olduğunu varsayıyoruz
                    requirementMet = HasPerk(req.requirementName);
                    break;
                case RequirementType.ExplorerQuest:
                    // ExplorerQuest kontrolü eklenecek (AdventureManager gerekli)
                    Debug.LogWarning("GameValidator: AreRequirementsMet içinde ExplorerQuest kontrolü henüz uygulanmadı.");
                    requirementMet = false;
                    break;
                case RequirementType.ExplorerTime:
                    // ExplorerTime kontrolü eklenecek (AdventureManager gerekli)
                    Debug.LogWarning("GameValidator: AreRequirementsMet içinde ExplorerTime kontrolü henüz uygulanmadı.");
                    requirementMet = false;
                    break;
                // case RequirementType.ExplorerQuest: // AdventureManager varsa eklenecek
                //     break;
                default:
                    Debug.LogWarning($"GameValidator: AreRequirementsMet içinde işlenmeyen RequirementType: {req.reqType}");
                    requirementMet = false;
                    break;
            }

            if (!requirementMet) return false; // Bir tanesi bile sağlanmazsa false dön
        }
        return true; // Hepsi sağlandıysa true dön
    }
}


