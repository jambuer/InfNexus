using UnityEngine;
using PlayerFightMechanics; // PlayerAttackEffectType için gerekli
using System.Collections.Generic; // List için gerekli
using System.Linq; // LINQ gerekirse
using System.Text; // StringBuilder için gerekli
using System;

/// <summary>
/// Oyun içindeki tüm durum kontrolleri, şartlar ve gereksinimler için merkezi Singleton sınıfı.
/// Diğer sınıflar (InteractableObject, ChapterManager vb.) doğrudan diğer manager'lara değil,
/// bu sınıfa başvurur.
/// </summary>
public class GameValidator : Singleton<GameValidator>
{
    // ========================================================================
    // YÜKSEK SEVİYE KONTROLLER (Diğer sistemlerin doğrudan çağırabilmesi için)
    // ========================================================================

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
        // TODO: QuestManager'da IsQuestActive fonksiyonu olduğundan emin ol
        // return QuestManager.Instance.IsQuestActive(questID);
        Debug.LogWarning("GameValidator: IsQuestActive kontrolü henüz uygulanmadı.");
        return false; // Şimdilik
    }

    /// <summary>
    /// Belirtilen saldırı efektinin oyuncu için kilidinin açık (kullanılabilir) olup olmadığını kontrol eder.
    /// </summary>
    public bool IsAttackEffectUnlocked(PlayerAttackEffectType effectType)
    {
        switch (effectType)
        {
            case PlayerAttackEffectType.NormalHit:
            case PlayerAttackEffectType.CriticalHit:
                return true; // Bu efektler her zaman açık
        }

        if (FightManager.Instance == null || FightManager.Instance.playerAttackEffectDatabase == null)
        {
            Debug.LogError($"GameValidator: FightManager veya PlayerAttackEffectDatabase bulunamadı! Efekt kontrolü yapılamıyor: {effectType}");
            return false;
        }

        PlayerAttackEffectData effectData = FightManager.Instance.playerAttackEffectDatabase.GetAttackEffectData(effectType);
        if (effectData == null)
        {
            Debug.LogWarning($"GameValidator: PlayerAttackEffectDatabase'de '{effectType}' için veri bulunamadı.");
            return false;
        }

        // Perk gereksinimini kontrol et
        if (!string.IsNullOrEmpty(effectData.requiredPerkTag))
        {
            return HasPerk(effectData.requiredPerkTag);
        }

        // Özel bir perk gerektirmiyorsa kilidi açıktır.
        return true;
    }

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
        return PerkManager.Instance.GetPerkCount(perkName) > 0;
    }

    /// <summary>
    /// Oyuncunun envanterinde belirtilen isimdeki eşyadan yeterli miktarda olup olmadığını kontrol eder.
    /// </summary>
    public bool HasItem(string itemName, int requiredAmount = 1)
    {
        if (ItemManager.Instance == null) { Debug.LogError("GameValidator: ItemManager bulunamadı!"); return false; }

        ItemData itemData = ItemManager.Instance.GetItemByName(itemName);
        if (itemData == null)
        {
            Debug.LogWarning($"GameValidator: '{itemName}' isminde bir eşya ItemManager'da bulunamadı.");
            return false;
        }
        // HasItem'ın ItemData overload'ını çağır
        return HasItem(itemData, requiredAmount);
    }

    /// <summary>
    /// Oyuncunun envanterinde belirtilen ItemData asset'inden yeterli miktarda olup olmadığını kontrol eder.
    /// </summary>
    public bool HasItem(ItemData itemData, int requiredAmount = 1)
    {
        if (itemData == null) { Debug.LogWarning("GameValidator: HasItem(ItemData) null itemData ile çağrıldı."); return false; }
        if (Inventory.Instance == null) { Debug.LogError("GameValidator: Inventory bulunamadı!"); return false; }
        
        return Inventory.Instance.GetItemCount(itemData) >= requiredAmount;
    }

    
    // ========================================================================
    // MERKEZİ GEREKSİNİM SİSTEMİ (Refactor Edildi)
    // ========================================================================

    /// <summary>
    /// [YENİ] Verilen Requirement listesindeki tüm gereksinimlerin karşılanıp karşılanmadığını kontrol eder.
    /// Bu, InteractableObject, ChapterManager vb. sınıfların ana çağrı fonksiyonudur.
    /// </summary>
    public bool AreRequirementsMet(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return true;

        // Her bir gereksinim için IsRequirementMet (tekil) fonksiyonunu çağırır.
        foreach (var req in requirements)
        {
            if (!IsRequirementMet(req)) 
                return false; // Bir tanesi bile sağlanmazsa false dön
        }
        
        return true; // Hepsi sağlandıysa true dön
    }

    /// <summary>
    /// [YENİ] Sadece tek bir 'Requirement' yapısının karşılanıp karşılanmadığını kontrol eder.
    /// 'AreRequirementsMet' fonksiyonu tarafından kullanılır.
    /// </summary>
    /// <summary>
    /// [GÜNCELLENDİ] Sadece tek bir 'Requirement' yapısının karşılanıp karşılanmadığını kontrol eder.
    /// 'AreRequirementsMet' fonksiyonu tarafından kullanılır.
    /// </summary>
    /// <summary>
    /// [GÜNCELLENDİ] Sadece tek bir 'Requirement' yapısının karşılanıp karşılanmadığını kontrol eder.
    /// </summary>
    private bool IsRequirementMet(Requirement req)
    {
        // Manager'ların varlığını kontrol et (Gerektiğinde)
        //
        try
        {
            switch (req.reqType)
            {
                case RequirementType.None:
                    return true; // 'None' gereksinimi her zaman karşılanır

                case RequirementType.Level:
                    return LevelManager.Instance.currentLevel >= req.requiredValue;

                case RequirementType.Stat:
                    return StatManager.Instance.GetTotalStat(req.requirementName) >= (double)req.requiredValue;

                case RequirementType.Item:
                    ItemData itemData = ItemManager.Instance.GetItemByName(req.requirementName);
                    if (itemData == null)
                    {
                        Debug.LogWarning($"GameValidator: Gereksinim için Item bulunamadı: {req.requirementName}");
                        return false;
                    }
                    return Inventory.Instance.GetItemCount(itemData) >= req.requiredValue;

                case RequirementType.Quest:
                    return QuestManager.Instance.GetCompletionCount(req.requirementName) > 0;

                case RequirementType.Gold:
                    return CurrencyManager.Instance.gold >= (double)req.requiredValue;

                case RequirementType.NexusCoin:
                    return CurrencyManager.Instance.nexusCoin >= (double)req.requiredValue;

                case RequirementType.People:
                    return CurrencyManager.Instance.people >= (double)req.requiredValue;

                case RequirementType.Health:
                    return ResourceManager.Instance.currentHealth >= (float)req.requiredValue;

                case RequirementType.Energy:
                    return ResourceManager.Instance.currentEnergy >= (float)req.requiredValue;

                case RequirementType.Mana:
                    return ResourceManager.Instance.currentMana >= (float)req.requiredValue;

                case RequirementType.Perk:
                    return PerkManager.Instance.GetPerkCount(req.requirementName) > 0;

                case RequirementType.Chapter:
                    return ChapterManager.Instance.IsChapterUnlocked(req.requiredValue);

                case RequirementType.MaxHealth:
                    return ResourceManager.Instance.maxHealth >= (float)req.requiredValue;

                case RequirementType.MaxEnergy:
                    return ResourceManager.Instance.maxEnergy >= (float)req.requiredValue;

                case RequirementType.MaxMana:
                    return ResourceManager.Instance.maxMana >= (float)req.requiredValue;

                case RequirementType.ExplorerQuest:
                    return ExplorerManager.Instance.GetExplorerQuestCompletionCount(req.requirementName) > 0;

                case RequirementType.ExplorerTime:
                    // TODO: ExplorerManager'a Toplam Explorer Süresini döndüren bir fonksiyon eklenmeli.
                    Debug.LogWarning("GameValidator: ExplorerTime kontrolü henüz uygulanmadı.");
                    return false;

                case RequirementType.LifeSkillLevel:
                    // TODO: LifeSkillManager eklendiğinde bu yorumu kaldırın.
                    Debug.LogWarning("GameValidator: LifeSkillLevel kontrolü henüz uygulanmadı.");
                    return false;

                case RequirementType.JobLevel:
                    // TODO: JobsManager eklendiğinde bu yorumu kaldırın.
                    Debug.LogWarning("GameValidator: JobLevel kontrolü henüz uygulanmadı.");
                    return false;

                default:
                    Debug.LogWarning($"GameValidator: IsRequirementMet içinde işlenmeyen RequirementType: {req.reqType}");
                    return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameValidator] IsRequirementMet hatası! Tip: {req.reqType}, İsim: {req.requirementName}. Hata: {e.Message}");
            return false;
        }
    }
    
    // using System.Text; // Hata CS0246 ('StringBuilder' not found) için en üste ekleyin


    // ====================================================================
    // YENİ GATHERING SİSTEMİ İÇİN NİHAİ DÜZELTİLMİŞ FONKSİYONLAR
    // ====================================================================

    /// <summary>
    /// [DÜZELTİLDİ] Verilen GameRequirement listesinin tamamının karşılanıp karşılanmadığını kontrol eder.
    /// </summary>
    public bool CheckGameRequirements(List<GameRequirement> requirements, bool showMessage = true)
    {
        if (requirements == null || requirements.Count == 0) return true;

        foreach (var req in requirements)
        {
            switch (req.requirementType)
            {
                case RequirementType.Level:
                    // HATA DÜZELTMESİ: 'Level' -> 'currentLevel'
                    if (LevelManager.Instance.currentLevel < req.amount)
                    {
                        if (showMessage) LogMissing($"Seviye {req.amount} gerekli.");
                        return false;
                    }
                    break;
                
                case RequirementType.Item:
                    ItemData item = ItemManager.Instance.GetItemByName(req.stringParameter);
                    if (item == null || Inventory.Instance.GetItemCount(item) < req.amount)
                    {
                        if (showMessage) LogMissing($"{req.amount} x {req.stringParameter} gerekli.");
                        return false;
                    }
                    break;
                
                case RequirementType.Stat:
                    if (StatManager.Instance.GetTotalStat(req.stringParameter) < req.amount)
                    {
                        if (showMessage) LogMissing($"{req.amount} {req.stringParameter} stat'ı gerekli.");
                        return false;
                    }
                    break;
                
                case RequirementType.Quest:
                    // HATA DÜZELTMESİ: 'GetQuestByName' veya 'IsQuestCompleted' yerine
                    // QuestManager'ınızdaki 'GetCompletionCount' fonksiyonunu kullanın.
                    int completionCount = QuestManager.Instance.GetCompletionCount(req.stringParameter);
                    
                    if (req.amount > 0 && completionCount <= 0) // (Tamamlanmış olmalı)
                    {
                        if (showMessage) LogMissing($"'{req.stringParameter}' görevi tamamlanmalı.");
                        return false;
                    }
                    if (req.amount == 0 && completionCount > 0) // (Tamamlanmamış olmalı)
                    {
                        if (showMessage) LogMissing($"'{req.stringParameter}' görevi aktif olmamalı.");
                        return false;
                    }
                    break;
                
                case RequirementType.Gold:
                    // HATA DÜZELTMESİ: 'currentGold' -> 'gold'
                    if (CurrencyManager.Instance.gold < req.amount)
                    {
                        if (showMessage) LogMissing($"{req.amount:N0} Altın gerekli.");
                        return false;
                    }
                    break;

                case RequirementType.NexusCoin:
                    // HATA DÜZELTMESİ: 'currentNexusCoin' -> 'nexusCoin'
                    if (CurrencyManager.Instance.nexusCoin < req.amount)
                    {
                        if (showMessage) LogMissing($"{req.amount:N0} NexusCoin gerekli.");
                        return false;
                    }
                    break;

                case RequirementType.People:
                    // HATA DÜZELTMESİ: 'currentPeople' -> 'people'
                    if (CurrencyManager.Instance.people < req.amount)
                    {
                        if (showMessage) LogMissing($"{req.amount:N0} Nüfus gerekli.");
                        return false;
                    }
                    break;
                
                case RequirementType.LifeSkillLevel:
                    // (Adım 1'de düzelttiğimiz 'GetSkillLevel(string)' fonksiyonunu çağırır)
                    if (LifeSkillManager.Instance == null || LifeSkillManager.Instance.GetSkillLevel(req.stringParameter) < req.amount)
                    {
                        if (showMessage) LogMissing($"{req.stringParameter} Seviye {req.amount} gerekli.");
                        return false;
                    }
                    break;
                    
                case RequirementType.JobLevel:
                     // (Adım 2'de düzelttiğimiz 'GetJobLevel(string)' fonksiyonunu çağırır)
                     if (JobsManager.Instance == null || JobsManager.Instance.GetJobLevel(req.stringParameter) < req.amount)
                    {
                        if (showMessage) LogMissing($"{req.stringParameter} Seviye {req.amount} gerekli.");
                        return false;
                    }
                    break;
            }
        }
        return true;
    }

    /// <summary>
    /// [DÜZELTİLDİ] Yeni GameRequirement gereksinimlerini kontrol eder ve eksik olanları zengin metin (renkli) olarak döndürür.
    /// </summary>
    public string GetMissingGameRequirementsText(List<GameRequirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return "Gereksinim yok.";

        StringBuilder sb = new StringBuilder();
        bool allMet = true;

        foreach (var req in requirements)
        {
            bool isMet = false;
            string reqText = "";

            switch (req.requirementType)
            {
                case RequirementType.Level:
                    // HATA DÜZELTMESİ: 'Level' -> 'currentLevel'
                    isMet = LevelManager.Instance.currentLevel >= req.amount;
                    reqText = $"Seviye {req.amount}";
                    break;

                case RequirementType.Item:
                    ItemData item = ItemManager.Instance.GetItemByName(req.stringParameter);
                    int currentItemCount = (item != null) ? Inventory.Instance.GetItemCount(item) : 0;
                    isMet = currentItemCount >= req.amount;
                    reqText = $"{req.stringParameter}: {currentItemCount}/{req.amount}";
                    break;

                case RequirementType.Stat:
                    double currentStat = StatManager.Instance.GetTotalStat(req.stringParameter);
                    isMet = currentStat >= req.amount;
                    reqText = $"{req.stringParameter} Stat: {currentStat:N0}/{req.amount:N0}";
                    break;

                case RequirementType.Quest:
                    // HATA DÜZELTMESİ: 'GetQuestByName' veya 'IsQuestCompleted' yerine
                    // QuestManager'ınızdaki 'GetCompletionCount' fonksiyonunu kullanın.
                    int completionCount = QuestManager.Instance.GetCompletionCount(req.stringParameter);
                    isMet = (req.amount > 0) ? (completionCount > 0) : (completionCount == 0);
                    reqText = (req.amount > 0) ? $"Görev: {req.stringParameter} (Tamamlandı)" : $"Görev: {req.stringParameter} (Aktif)";
                    break;

                case RequirementType.Gold:
                    // HATA DÜZELTMESİ: 'currentGold' -> 'gold'
                    isMet = CurrencyManager.Instance.gold >= req.amount;
                    reqText = $"Altın: {CurrencyManager.Instance.gold:N0}/{req.amount:N0}";
                    break;

                case RequirementType.NexusCoin:
                    // HATA DÜZELTMESİ: 'currentNexusCoin' -> 'nexusCoin'
                    isMet = CurrencyManager.Instance.nexusCoin >= req.amount;
                    reqText = $"NexusCoin: {CurrencyManager.Instance.nexusCoin:N0}/{req.amount:N0}";
                    break;

                case RequirementType.People:
                    // HATA DÜZELTMESİ: 'currentPeople' -> 'people'
                    isMet = CurrencyManager.Instance.people >= req.amount;
                    reqText = $"Nüfus: {CurrencyManager.Instance.people:N0}/{req.amount:N0}";
                    break;

                case RequirementType.LifeSkillLevel:
                    // (Adım 1'de düzelttiğimiz 'GetSkillLevel(string)' fonksiyonunu çağırır)
                    int currentSkillLevel = (LifeSkillManager.Instance != null) ? LifeSkillManager.Instance.GetSkillLevel(req.stringParameter) : 0;
                    isMet = currentSkillLevel >= req.amount;
                    reqText = $"{req.stringParameter} Seviye: {currentSkillLevel}/{req.amount}";
                    break;

                case RequirementType.JobLevel:
                    // (Adım 2'de düzelttiğimiz 'GetJobLevel(string)' fonksiyonunu çağırır)
                    int currentJobLevel = (JobsManager.Instance != null) ? JobsManager.Instance.GetJobLevel(req.stringParameter) : 0;
                    isMet = currentJobLevel >= req.amount;
                    reqText = $"{req.stringParameter} Seviye: {currentJobLevel}/{req.amount}";
                    break;
            }

            if (!isMet) allMet = false;

            // Format requirement text with color based on whether it's met
            string formattedText = isMet ? $"<color=green>{reqText}</color>" : $"<color=red>{reqText}</color>";
            sb.AppendLine(formattedText);
        }

        if (allMet)
        {
            return "<color=green>Tüm gereksinimler karşılandı.</color>";
        }

        return sb.ToString();
    }

    // (LogMissing fonksiyonu aynı kalır, o zaten düzeltilmişti)
    
    // ===================================================================
    // [YENİ SİSTEM] - GameRequirement (RequirementType.cs) KONTROLLERİ
    // GatheringNodeUI ve yeni sistemler tarafından kullanılır.
    // (requirementName hataları düzeltildi -> stringParameter kullanıldı)
    // ===================================================================

    /// <summary>
    /// [YENİ] 'GameRequirement' (yeni struct) listesinin tamamının karşılanıp karşılanmadığını kontrol eder.
    /// </summary>
    public bool CheckGameRequirements(List<GameRequirement> requirements)
    {
        if (requirements == null || requirements.Count == 0)
            return true; // Gereksinim yoksa, karşılanmış sayılır.

        foreach (var req in requirements)
        {
            if (!CheckSingleGameRequirement(req))
                return false; // Herhangi biri karşılanmazsa 'false' döner.
        }
        return true; // Hepsi karşılandı.
    }

    /// <summary>
    /// [YENİ] Tek bir 'GameRequirement' (yeni struct) objesinin karşılanıp karşılanmadığını kontrol eder.
    /// RequirementTooltipFormatter tarafından çağrılır.
    /// </summary>
    public bool CheckSingleGameRequirement(GameRequirement req)
    {
        switch (req.requirementType)
        {
            case RequirementType.Level:
                return LevelManager.Instance.currentLevel >= (int)req.amount;
            
            case RequirementType.Item:
                // [DEĞİŞTİ] requirementName -> stringParameter
                ItemData itemData = ItemManager.Instance.GetItemByName(req.stringParameter);
                if (itemData == null) return false;
                return Inventory.Instance.GetItemCount(itemData) >= (int)req.amount; 
            
            
            case RequirementType.Quest:
                // [DEĞİŞTİ] requirementName -> stringParameter
                return QuestManager.Instance.GetCompletionCount(req.stringParameter) >= (int)req.amount;

            case RequirementType.Gold:
                return CurrencyManager.Instance.gold >= req.amount;
            
            case RequirementType.NexusCoin:
                return CurrencyManager.Instance.nexusCoin >= req.amount;
            
            case RequirementType.People:
                return CurrencyManager.Instance.people >= req.amount;

            case RequirementType.Energy:
                return ResourceManager.Instance.currentEnergy >= (float)req.amount;
            
            case RequirementType.Health:
                return ResourceManager.Instance.currentHealth >= (float)req.amount;
            
            case RequirementType.Mana:
                return ResourceManager.Instance.currentMana >= (float)req.amount;

            // --- Beceri/Meslek Kontrolleri ---
            case RequirementType.LifeSkillLevel:
                // [DEĞİŞTİ] requirementName -> stringParameter
                return LifeSkillManager.Instance.GetSkillLevel(req.stringParameter) >= (int)req.amount;
            
            case RequirementType.JobLevel:
                // [DEĞİŞTİ] requirementName -> stringParameter
                return JobsManager.Instance.GetJobLevel(req.stringParameter) >= (int)req.amount;
            
            case RequirementType.None:
                return true;
        }
        return false;
    }



    // HATA CS0103: 'LogMissing' does not exist... için eklendi
    private void LogMissing(string message)
    {
        // TODO: GameConsole'a veya oyuncuya özel bir mesaj gösterme
        Debug.LogWarning($"Gereksinim Karşılanmadı: {message}");
    }
    


}