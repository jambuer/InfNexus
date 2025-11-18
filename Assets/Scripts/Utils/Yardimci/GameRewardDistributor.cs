using UnityEngine;
using System.Collections.Generic; // List<T> için eklendi
using System;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Linq;

/// <summary>
/// Oyuncuya ödülleri (XP, Altın, Eşya, Stat Puanı, Perk vb.)
/// merkezi bir noktadan dağıtmak için kullanılan bir Singleton.
/// QuestManager, ExplorerManager, FightManager gibi sistemler ödül vermek
/// için doğrudan bu sınıfı çağırır.
/// </summary>
public class GameRewardDistributor : Singleton<GameRewardDistributor>
{
    // ========================================================================
    // YENİ MERKEZİ ÖDÜL SİSTEMİ
    // ========================================================================

    /// <summary>
    /// [YENİ] Bir 'GameReward' listesini alır ve hepsini dağıtır.
    /// Diğer tüm sistemler (QuestManager, ChapterManager vb.) SADECE bu fonksiyonu çağırmalıdır.
    /// </summary>
    /// <param name="rewards">GameReward struct listesi</param>
    public void DistributeRewards(List<GameReward> rewards)
    {
        if (rewards == null || rewards.Count == 0) return;

        foreach (var reward in rewards)
        {
            DistributeReward(reward);
        }
    }

    /// <summary>
    /// [YENİ] Tek bir 'GameReward' yapısını işler ve ilgili özel 'Award' fonksiyonunu çağırır.
    /// </summary>
    private void DistributeReward(GameReward reward)
    {
        // 'RewardData.cs' dosyasındaki RewardType enum'ını kullanır
        switch (reward.rewardType)
        {
            case RewardType.XP:
                // DEĞİŞTİ:
                var (finalXP, bonusXP) = AwardXP(reward.amount); // Hesapla ve ödülü ver
                // Şimdi loglamayı burada yap:
                if (bonusXP > 0)
                {
                    LogReward($"+{reward.amount:N0} XP (+{bonusXP:N0} Bonus) = +{finalXP:N0} Toplam XP");
                }
                else
                {
                    LogReward($"+{finalXP:N0} XP");
                }
                break;
                
            case RewardType.Gold:
                // DEĞİŞTİ:
                var (finalGold, bonusGold) = AwardGold((int)reward.amount); // Hesapla ve ödülü ver
                 // Şimdi loglamayı burada yap:
                if (bonusGold > 0)
                {
                    LogReward($"<color=yellow>Altın Bonusu: +{bonusGold} Altın (Toplam +{finalGold:N0})</color>");
                }
                else
                {
                    LogReward($"+{finalGold:N0} Altın");
                }
                break;
                
            case RewardType.NexusCoin:
                AwardNexusCoin(reward.amount, true);
                break;
                
            case RewardType.People:
                AwardPeople(reward.amount, true);
                break;
                
            case RewardType.PremiumCoin:
                // TODO: AwardPremiumCoin fonksiyonu eklenecek
                Debug.LogWarning("GameRewardDistributor: PremiumCoin ödülü henüz uygulanmadı.");
                break;
                
            case RewardType.Item:
                // Önce ItemData referansını kontrol et
                if (reward.itemData != null)
                {
                    AwardItem(reward.itemData, (int)reward.amount, true);
                }
                // ItemData yoksa stringParameter'ı (item name) kullan
                else if (!string.IsNullOrEmpty(reward.stringParameter))
                {
                    AwardItem(reward.stringParameter, (int)reward.amount, true);
                }
                break;
                
            case RewardType.Stat:
                // GameReward'daki stringParameter'ı (Stat Adı) kullan
                AwardStat(reward.stringParameter, reward.amount, true);
                break;
                
            case RewardType.Perk:
                // GameReward'daki stringParameter'ı (Perk Tag) kullan
                AwardPerk(reward.stringParameter, (int)reward.amount, true);
                break;
                
            case RewardType.LifeSkillXP:
                // [DEĞİŞTİ] TODO kaldırıldı. Artık 'Facade' yöneticisi olan
                // LifeJobsSkillsManager'ı çağırıyoruz.
                if (LifeJobsSkillsManager.Instance != null)
                {
                    // (GameReward'daki 'lifeSkill' enum'unu ve 'amount'u kullanır)
                    LifeJobsSkillsManager.Instance.AddSkillXP(reward.lifeSkill, reward.amount);
                }
                else
                {
                     Debug.LogError("GameRewardDistributor: LifeJobsSkillsManager bulunamadı!");
                }
                break;
                
            case RewardType.JobXP:
                // [DEĞİŞTİ] TODO kaldırıldı.
                if (LifeJobsSkillsManager.Instance != null)
                {
                    // (GameReward'daki 'job' enum'unu ve 'amount'u kullanır)
                    LifeJobsSkillsManager.Instance.AddJobXP(reward.job, reward.amount);
                }
                else
                {
                     Debug.LogError("GameRewardDistributor: LifeJobsSkillsManager bulunamadı!");
                }
                break;
        }
    }

    // ========================================================================
    // ESKİ ÖDÜL FONKSİYONLARI (Artık 'private' olabilirler veya 'internal' kalabilirler)
    // Bu fonksiyonlar artık yeni 'DistributeReward' switch-case'i tarafından çağrılıyor.
    // ========================================================================

    /// <summary>
    /// Oyuncuya belirtilen ItemData'dan belirtilen miktarda verir.
    /// </summary>
    public void AwardItem(ItemData itemData, int amount = 1, bool logToConsole = true)
    {
        if (itemData == null)
        {
            Debug.LogWarning("GameRewardDistributor: Ödül verilemedi (ItemData null).");
            return;
        }
        if (Inventory.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: Inventory bulunamadı!");
            return;
        }

        Inventory.Instance.AddItem(itemData, amount);

        if (logToConsole)
        {
            LogReward($"+{amount} {itemData.itemName}");
        }
    }

    /// <summary>
    /// Oyuncuya isme göre bir eşya verir (ItemManager'ı kullanarak).
    /// </summary>
    public void AwardItem(string itemName, int amount = 1, bool logToConsole = true)
    {
        if (ItemManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: ItemManager bulunamadı!");
            return;
        }
        ItemData itemData = ItemManager.Instance.GetItemByName(itemName);
        if (itemData == null)
        {
            Debug.LogWarning($"GameRewardDistributor: '{itemName}' isminde bir eşya bulunamadı, ödül verilemedi.");
            return;
        }
        AwardItem(itemData, amount, logToConsole);
    }

    /// <summary>
    /// Oyuncuya XP verir. (Stat bonusları hesaplanır)
    /// HESAPLANAN DEĞERLERİ GERİ DÖNDÜRÜR. LOGLAMA YAPMAZ.
    /// </summary>
    /// <returns>(double finalAmount, double bonusAmount) tuple'ı.</returns>
    public (double finalAmount, double bonusAmount) AwardXP(double baseAmount) // logToConsole parametresi kaldırıldı
    {
        if (baseAmount <= 0) return (0, 0);
        if (LevelManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: LevelManager bulunamadı!");
            return (0, 0);
        }

        double totalBonusPercentage = 0;
        if (StatCalculator.Instance != null)
        {
            totalBonusPercentage = StatCalculator.Instance.currentStats.ExpBonus; //
        }

        double bonusMultiplier = 1.0 + totalBonusPercentage;
        double finalXPAmount = baseAmount * bonusMultiplier;
        double bonusAmount = finalXPAmount - baseAmount;
        
        LevelManager.Instance.AddXP(finalXPAmount); // Ödülü ver

        return (finalXPAmount, bonusAmount); // Sonuçları döndür
    }

    // --- PARA BİRİMİ ÖDÜLLERİ ---

    /// <summary>
    /// Oyuncuya Altın verir. (Bonus hesaplaması içerir)
    /// HESAPLANAN DEĞERLERİ GERİ DÖNDÜRÜR. LOGLAMA YAPMAZ.
    /// </summary>
    /// <returns>(int finalAmount, int bonusAmount) tuple'ı.</returns>
    public (int finalAmount, int bonusAmount) AwardGold(int baseGoldAmount)
    {
        if (baseGoldAmount <= 0) return (0, 0);

        double totalBonusPercentage = 0;
        if (StatCalculator.Instance != null)
        {
            totalBonusPercentage = StatCalculator.Instance.currentStats.GoldBonus; //
        }

        float bonusMultiplier = 1.0f + (float)totalBonusPercentage;
        int finalGoldAmount = Mathf.RoundToInt(baseGoldAmount * bonusMultiplier);
        int bonusAmount = finalGoldAmount - baseGoldAmount;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(finalGoldAmount); // Ödülü ver
        }
        else
        {
            Debug.LogError("[GameRewardDistributor] CurrencyManager bulunamadı!");
        }

        return (finalGoldAmount, bonusAmount); // Sonuçları döndür
    }


    /// <summary>
    /// Oyuncuya NexusCoin verir.
    /// </summary>
    public void AwardNexusCoin(double amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: CurrencyManager bulunamadı!");
            return;
        }

        CurrencyManager.Instance.AddNexusCoin(amount);

        if (logToConsole)
        {
            LogReward($"+{amount:N0} NexusCoin");
        }
    }

    /// <summary>
    /// Oyuncuya Nüfus (People) verir.
    /// </summary>
    public void AwardPeople(double amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: CurrencyManager bulunamadı!");
            return;
        }

        CurrencyManager.Instance.AddPeople(amount);

        if (logToConsole)
        {
            LogReward($"+{amount:N0} Nüfus");
        }
    }

    // --- STAT VE PERK ÖDÜLLERİ ---

    /// <summary>
    /// Oyuncuya kalıcı (base) stat bonusu verir.
    /// </summary>
    public void AwardStat(string statName, double amount, bool logToConsole = true)
    {
        if (amount <= 0 || string.IsNullOrEmpty(statName)) return;
        if (StatManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: StatManager bulunamadı!");
            return;
        }

        StatManager.Instance.AddStat(statName, amount); 

        if (logToConsole)
        {
            LogReward($"+{amount} {statName} Stat");
        }
    }
    
    /// <summary>
    /// Oyuncunun tüm kalıcı (base) stat'larına bonus verir.
    /// </summary>
    public void AwardAllStats(double amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (StatManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: StatManager bulunamadı!");
            return;
        }

        StatManager.Instance.AddAllStats(amount); 

        if (logToConsole)
        {
            LogReward($"+{amount} Tüm Stat'lar");
        }
    }

    /// <summary>
    /// Oyuncuya harcanmamış stat puanı verir.
    /// </summary>
    public void AwardUnspentStatPoints(int amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (LevelManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: LevelManager bulunamadı!");
            return;
        }

        LevelManager.Instance.AddUnspentStatPoints(amount); 

        if (logToConsole)
        {
            LogReward($"+{amount} Stat Puanı");
        }
    }

    /// <summary>
    /// Oyuncuya yeni bir Perk veya mevcut bir Perk'e seviye verir.
    /// </summary>
    public void AwardPerk(string perkTag, int levels = 1, bool logToConsole = true)
    {
        if (levels <= 0 || string.IsNullOrEmpty(perkTag)) return;
        if (PerkManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: PerkManager bulunamadı!");
            return;
        }
        
        if (PerkManager.Instance.perkDatabase == null)
        {
            Debug.LogError("GameRewardDistributor: PerkManager'a bağlı 'perkDatabase' referansı bulunamadı!");
            return;
        }

        PerkDefinition perkDef = PerkManager.Instance.perkDatabase.GetPerkDefinitionByID(perkTag); 

        if (perkDef == null)
        {
            Debug.LogWarning($"GameRewardDistributor: Perk veritabanında '{perkTag}' tag'ine sahip PerkDefinition bulunamadı.");
            return;
        }

        PerkManager.Instance.AddPerk(perkDef, levels);
        
        if (logToConsole) 
        { 
            LogReward($"Perk Kazanıldı: {perkDef.displayName} (x{levels})"); 
        }
    }

    // --- KAYNAK ÖDÜLLERİ (ÖR: Can İksiri) ---
    public void AwardHealth(float amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: ResourceManager bulunamadı!");
            return;
        }

        ResourceManager.Instance.ModifyHealth(amount);
        if (logToConsole) { LogReward($"+{amount:F0} Can"); }
    }

    public void AwardMana(float amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: ResourceManager bulunamadı!");
            return;
        }

        ResourceManager.Instance.ModifyMana(amount);
        if (logToConsole) { LogReward($"+{amount:F0} Mana"); }
    }

    public void AwardEnergy(float amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: ResourceManager bulunamadı!");
            return;
        }

        ResourceManager.Instance.ModifyEnergy(amount);
        if (logToConsole) { LogReward($"+{amount:F0} Enerji"); }
    }

    // RewardTier listesinden ağırlıklı rastgele ödül seçen yardımcı fonksiyon (QuestManager'dan kopyalandı)
    private double GetWeightedReward(List<RewardTier> tiers)
    {
        if (tiers == null || tiers.Count == 0) return 0;
        float totalWeight = tiers.Sum(t => t.probabilityWeight);
        if (totalWeight <= 0) return tiers.LastOrDefault()?.GetRandomAmount() ?? 0;
        float randomPoint = UnityEngine.Random.Range(0, totalWeight);
        foreach (var tier in tiers)
        {
            if (randomPoint < tier.probabilityWeight) return tier.GetRandomAmount();
            randomPoint -= tier.probabilityWeight;
        }
        return tiers.Last().GetRandomAmount(); // Hata durumunda sonuncuyu ver
    }
    
    // --- BU YENİ FONKSİYONU GameRewardDistributor.cs SINIFININ İÇİNE EKLE ---
// (Tüm mantık FightManager.EnemyDefeated'dan taşındı)
//
    /// <summary>
    /// Düşman ödülleri için karmaşık (Tier, DropRate) hesaplamaları yapar ve dağıtır.
    /// </summary>
    /// <summary>
    /// Düşman ödülleri için karmaşık (Tier, DropRate) hesaplamaları yapar ve dağıtır.
    /// (BONUS HESAPLAMALARINI İÇERİR VE ÖZEL KONSOL MESAJI YAZAR)
    /// </summary>
    public void DistributeEnemyRewards(EnemyData enemyData)
    {
        if (enemyData == null) return;
        if (LevelManager.Instance == null || CurrencyManager.Instance == null || Inventory.Instance == null || StatCalculator.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: Düşman ödülleri dağıtılamadı...");
            return;
        }

        ComputedStats playerStats = StatCalculator.Instance.currentStats; // Sadece eşya dropları için lazım

        // 1. XP Ödülü
        double baseExp = enemyData.experienceReward;
        // DEĞİŞTİ: Bonus hesaplamasını AwardXP'ye yaptır
        var (finalExp, bonusExp) = AwardXP(baseExp);

        string xpMessage;
        double totalExpBonusPercent = (baseExp > 0) ? (bonusExp / baseExp) : 0;
        if (bonusExp > 0.1) // Küsurat hatalarını engelle
        {
            xpMessage = $"<color=green>+{finalExp:N0} XP</color> kazanıldı <color=cyan>(+%{(totalExpBonusPercent * 100):F0} Bonus)</color>";
        }
        else
        {
            xpMessage = $"<color=green>+{finalExp:N0} XP</color> kazanıldı.";
        }
        FightConsole.Instance?.AddMessage(xpMessage);
        GameConsole.Instance?.AddMessage(xpMessage);


        // 2. Altın Ödülü
        if (enemyData.goldRewardTiers != null && enemyData.goldRewardTiers.Count > 0)
        {
            double baseGold = GetWeightedReward(enemyData.goldRewardTiers);
            
            // DEĞİŞTİ: Bonus hesaplamasını AwardGold'a yaptır
            var (finalGold, bonusGold) = AwardGold((int)baseGold);

            string goldMessage;
            double totalGoldBonusPercent = (baseGold > 0) ? ((double)bonusGold / baseGold) : 0;
            if (bonusGold > 0)
            {
                goldMessage = $"<color=yellow>+{finalGold:N0} Altın</color> kazanıldı <color=cyan>(+%{(totalGoldBonusPercent * 100):F0} Bonus)</color>";
            }
            else
            {
                goldMessage = $"<color=yellow>+{finalGold:N0} Altın</color> kazanıldı.";
            }
            FightConsole.Instance?.AddMessage(goldMessage);
            GameConsole.Instance?.AddMessage(goldMessage);
        }


        // 3. Nexus Coin Ödülü (Henüz bonus yok, mevcut kod kalabilir)
        if (enemyData.nexusCoinRewardTiers != null && enemyData.nexusCoinRewardTiers.Count > 0)
        {
            double baseNexusCoin = GetWeightedReward(enemyData.nexusCoinRewardTiers);
            // TODO: NexusCoinBonus eklendiğinde StatCalculator'dan çekilip buraya eklenecek
            double finalNexusCoin = baseNexusCoin; 
            if (finalNexusCoin > 0) CurrencyManager.Instance.AddNexusCoin(finalNexusCoin);
            string nexuscoinMessage = $"<color=blue>+{finalNexusCoin:N0} Nexus Coin</color> kazanıldı.";
            FightConsole.Instance?.AddMessage(nexuscoinMessage);
            GameConsole.Instance?.AddMessage(nexuscoinMessage);
        }
        

        // 4. Eşya Ödülleri (Bu kod zaten StatCalculator'dan DropRate alıyordu, doğru çalışıyor)
        foreach (var dropInfo in enemyData.itemDrops)
        {
            if (dropInfo.itemToDrop == null) continue;

            float actualDropChance = dropInfo.baseDropChance;
            // Eşik kontrolü
            if (playerStats.DropRate < dropInfo.dropRateThreshold)
            {
                actualDropChance *= dropInfo.chanceMultiplierBelowThreshold;
            }
            // Oyuncu bonusunu ekle
            actualDropChance *= (1.0f + (float)playerStats.DropRate);
            actualDropChance = Mathf.Clamp01(actualDropChance);

            if (UnityEngine.Random.Range(0f, 1f) <= actualDropChance)
            {
                int quantity = 1;
                if (dropInfo.quantityScalesWithDropRate && dropInfo.dropRateThreshold > 0)
                {
                    quantity = (int)Math.Floor(playerStats.DropRate / dropInfo.dropRateThreshold);
                    quantity = Math.Max(1, quantity);
                }

                // TODO: Maksimum düşme sayısını (maxDrops) kontrol et

                Inventory.Instance.AddItem(dropInfo.itemToDrop, quantity);
                string itemMessage = $"<color=orange>+{quantity} {dropInfo.itemToDrop.itemName}</color> düştü!";
                FightConsole.Instance?.AddMessage(itemMessage);
                GameConsole.Instance?.AddMessage(itemMessage);
            }
        }
    }
    
    // (Gerekiyorsa AwardEnergy ve AwardMana için de benzer fonksiyonlar eklenebilir)

    // --- KONSOL KAYDI ---
    private void LogReward(string message)
    {
        if (GameConsole.Instance != null)
        {
            GameConsole.Instance.AddMessage(message);
        }
        else
        {
            Debug.Log($"[ÖDÜL] {message}");
        }
    }
}