using UnityEngine;

/// <summary>
/// Oyuncuya ödülleri (XP, Altın, Eşya, Stat Puanı, Perk vb.)
/// merkezi bir noktadan dağıtmak için kullanılan bir Singleton.
/// QuestManager, ExplorerManager, FightManager gibi sistemler ödül vermek
/// için doğrudan bu sınıfı çağırır.
/// </summary>
public class GameRewardDistributor : Singleton<GameRewardDistributor>
{
    // HATA 1 ve 2 DÜZELTMESİ:
    // GameConsole.cs dosyanızda 'LogColor' enum'u veya renk alan bir 'AddMessage' metodu yok.
    // Bu yüzden renk parametresini ve ilgili değişkeni kaldırıyoruz.
    // Konsola renkli yazdırmak istiyorsanız, önce GameConsole.cs'nin 'AddMessage' metodunu
    // zengin metin (rich text) kabul edecek şekilde güncellemeniz gerekir.
    // [Header("Ayarlar")]
    // public GameConsole.LogColor rewardLogColor = GameConsole.LogColor.Cyan; // BU SATIR KALDIRILDI

    // --- TEMEL ÖDÜL FONKSİYONLARI ---

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
    /// Oyuncuya XP verir.
    /// </summary>
    public void AwardXP(double amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (LevelManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: LevelManager bulunamadı!");
            return;
        }

        LevelManager.Instance.AddXP(amount);

        if (logToConsole)
        {
            LogReward($"+{amount:N0} XP");
        }
    }

    // --- PARA BİRİMİ ÖDÜLLERİ ---

    /// <summary>
    /// Oyuncuya Altın verir.
    /// </summary>
    public void AwardGold(double amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: CurrencyManager bulunamadı!");
            return;
        }

        // KULLANICI NOTU DÜZELTMESİ: 'ModifyGold' yerine 'AddGold' kullanılıyor.
        CurrencyManager.Instance.AddGold(amount); 

        if (logToConsole)
        {
            LogReward($"+{amount:N0} Altın");
        }
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

        // KULLANICI NOTU DÜZELTMESİ: 'ModifyNexusCoin' yerine 'AddNexusCoin' kullanılıyor.
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

        // KULLANICI NOTU DÜZELTMESİ: 'ModifyPeople' yerine 'AddPeople' kullanılıyor.
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
        if (amount <= 0) return;
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
        if (levels <= 0) return;
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

        // HATA 3 DÜZELTMESİ:
        // PerkDatabase.cs'deki metodun adı 'GetPerkDefinition' değil, 'GetPerkDefinitionByID'.
        PerkDefinition perkDef = PerkManager.Instance.perkDatabase.GetPerkDefinitionByID(perkTag); 

        if (perkDef == null)
        {
            Debug.LogWarning($"GameRewardDistributor: Perk veritabanında '{perkTag}' tag'ine sahip PerkDefinition bulunamadı.");
            return;
        }

        // PerkManager.AddPerk'in kendisi log atıyor olabilir, ancak biz yine de
        // displayName ile buradan log atalım.
        PerkManager.Instance.AddPerk(perkDef, levels);
        
        if (logToConsole) 
        { 
            // HATA 4 DÜZELTMESİ:
            // PerkDefinition.cs'de 'perkName' alanı yok, kullanıcıya gösterilecek ad 'displayName'.
            LogReward($"Perk Kazanıldı: {perkDef.displayName} (x{levels})"); 
        }
    }

    // --- KAYNAK ÖDÜLLERİ (ÖR: Can İksiri) ---

    /// <summary>
    /// Oyuncunun mevcut canını artırır (maksimumu geçemez).
    /// </summary>
    public void AwardHealth(float amount, bool logToConsole = true)
    {
        if (amount <= 0) return;
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("GameRewardDistributor: ResourceManager bulunamadı!");
            return;
        }
        
        ResourceManager.Instance.ModifyHealth(amount); 
        if(logToConsole) { LogReward($"+{amount:F0} Can"); }
    }
    
    // (Gerekiyorsa AwardEnergy ve AwardMana için de benzer fonksiyonlar eklenebilir)

    // --- KONSOL KAYDI ---

    /// <summary>
    /// Ödül mesajını GameConsole'a standart bir formatta gönderir.
    /// </summary>
    private void LogReward(string message)
    {
        if (GameConsole.Instance != null)
        {
            // HATA 1 ve 2 DÜZELTMESİ:
            // GameConsole.cs'deki 'AddMessage' metodu sadece 1 parametre (string) alıyor.
            GameConsole.Instance.AddMessage(message);
        }
        else
        {
            Debug.Log($"[ÖDÜL] {message}");
        }
    }
}