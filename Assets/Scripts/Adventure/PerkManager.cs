using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Oyuncunun sahip olduğu Perk'lerin (PerkDefinition) stack sayılarını yönetir.
/// Anlık efektleri uygular ve pasif bonusları sorgulamak için arayüz sağlar.
/// GameDataManager ile uyumlu "pasif" modda çalışır.
/// ExplorerManager'dan gelen event'leri dinler.
/// </summary>
public class PerkManager : MonoBehaviour, IGameDataSaveable<PerkSaveData> // IGameDataSaveable doğru
{
    public static PerkManager Instance { get; private set; }

    [Header("Veritabanı Referansı")]
    [Tooltip("Kullanılacak Perk Database asset'i.")]
    public PerkDatabase perkDatabase; // Bu doğru

    // DEĞİŞİKLİK: Anahtar artık string (perkID)
    private Dictionary<string, int> _perkCounts = new Dictionary<string, int>();
    public event System.Action OnPerkUpdated;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // --- EVENT ABONELİĞİ (Değişiklik Yok) ---
    void Start()
    {
        if (ExplorerManager.Instance != null)
        {
            ExplorerManager.Instance.OnExplorerPerkCompleted += HandleExplorerPerkCompleted;
            Debug.Log("[PerkManager] ExplorerManager event'ine abone olundu.");
        }
        else { Debug.LogWarning("[PerkManager] ExplorerManager başlangıçta bulunamadı!"); }
    }

    void OnDestroy()
    {
        if (ExplorerManager.Instance != null)
        {
            try { ExplorerManager.Instance.OnExplorerPerkCompleted -= HandleExplorerPerkCompleted; }
            catch (Exception ex) { Debug.LogWarning($"[PerkManager] Event aboneliği kaldırılırken hata: {ex.Message}");}
        }
    }

    /// <summary>
    /// ExplorerManager'dan gelen 'OnExplorerPerkCompleted' event'ini yakalar.
    /// </summary>
    /// <summary>
    /// ExplorerManager'dan gelen 'OnExplorerPerkCompleted' event'ini yakalar.
    /// </summary>
    private void HandleExplorerPerkCompleted(PerkReward completedExplorerReward) // Parametre ExplorerPerkData.PerkReward
    {
        // Gelen ExplorerReward içindeki PerkDefinition referansını ve amount'u al
        PerkDefinition perkDefToGrant = completedExplorerReward?.perkToGrant;
        int amountToAdd = completedExplorerReward?.amount ?? 0;

        // Sadece perkDefToGrant ve amountToAdd'ın geçerli olup olmadığını kontrol etmemiz yeterli
        if (perkDefToGrant != null && amountToAdd > 0)
        {
            // Doğru log mesajı
            Debug.Log($"[PerkManager] HandleExplorerPerkCompleted tetiklendi: {perkDefToGrant.perkID} (x{amountToAdd})");
            // Kendi AddPerk metodunu çağır (Instance olmadan!)
            AddPerk(perkDefToGrant, amountToAdd); // <<<--- DÜZELTİLMİŞ ÇAĞRI
        }
        else
        {
            // Doğru log mesajı
            Debug.LogWarning("[PerkManager] HandleExplorerPerkCompleted: Geçersiz PerkReward veya PerkDefinition alındı.");
        }
    }
    

    // --- EVENT ABONELİĞİ BİTTİ ---

    /// <summary>
    /// Bir Perk'in stack sayısını artırır ve anlık ödülünü uygular.
    /// </summary>
    /// <param name="perkDef">Eklenen Perk'in PerkDefinition verisi.</param>
    /// <param name="amountToAdd">Eklenecek stack sayısı.</param>
    public void AddPerk(PerkDefinition perkDef, int amountToAdd) // İmza değişti
    {
        if (perkDef == null || string.IsNullOrEmpty(perkDef.perkID) || amountToAdd <= 0)
        {
             Debug.LogError("[PerkManager] AddPerk: Geçersiz PerkDefinition veya miktar!");
             return;
        }

        string perkID = perkDef.perkID; // Anahtar olarak perkID kullanılıyor

        _perkCounts.TryGetValue(perkID, out int currentCount);
        _perkCounts[perkID] = currentCount + amountToAdd;

        Debug.Log($"[PerkManager] Perk eklendi/güncellendi: {perkID}, Yeni Stack: {_perkCounts[perkID]} (+{amountToAdd})");
        OnPerkUpdated?.Invoke();

        // Anlık efekti uygula (PerkDefinition kullanarak)
        HandlePerkReward(perkDef, amountToAdd);

        // KAYIT ARTIK BURADA YAPILMIYOR
    }

    /// <summary>
    /// Bir Perk'in (ID'sine göre) mevcut stack sayısını döndürür.
    /// </summary>
    public int GetPerkCount(string perkID) => _perkCounts.TryGetValue(perkID, out int count) ? count : 0;
    /// <summary>
    /// Bir Perk'in (Enum değerine göre) mevcut stack sayısını döndürür.
    /// </summary>
    public int GetPerkCount(PerkName perkNameValue)
    {
        // Enum None ise 0 döndür
        if (perkNameValue == PerkName.None) return 0;

        // Veritabanından bu enum'a karşılık gelen PerkDefinition'ı bul
        if (perkDatabase == null)
        {
            Debug.LogError("[PerkManager] GetPerkCount(Enum): PerkDatabase atanmamış!");
            return 0;
        }
        // PerkDatabase'e enum ile arama metodu eklemek daha iyi olurdu,
        // Şimdilik listede arayalım (biraz yavaş olabilir):
        PerkDefinition perkDef = perkDatabase.allPerkDefinitions?.Find(p => p != null && p.perkNameValue == perkNameValue);

        if (perkDef != null && !string.IsNullOrEmpty(perkDef.perkID))
        {
            // Bulunan perkDef'in ID'si ile _perkCounts'tan sayıyı al
            return GetPerkCount(perkDef.perkID);
        }
        else
        {
            // Enum değeri veritabanında bulunamadıysa uyarı ver
            Debug.LogWarning($"[PerkManager] GetPerkCount(Enum): Veritabanında '{perkNameValue}' enum değerine sahip PerkDefinition bulunamadı.");
            return 0;
        }
    }



    /// <summary>
    /// Yeni eklenen Perk'in PerkDefinition verisine göre anlık ödülünü uygular.
    /// </summary>
    private void HandlePerkReward(PerkDefinition perkDef, int amountApplied) // İmza değişti
    {
        if (perkDef == null || amountApplied <= 0) return;

        string consoleMessage = "";

        bool statManagerExists = StatManager.Instance != null;
        bool levelManagerExists = LevelManager.Instance != null;
        bool resourceManagerExists = ResourceManager.Instance != null;
        bool inventoryManagerExists = Inventory.Instance != null;
        bool itemManagerExists = ItemManager.Instance != null;
        bool gameConsoleExists = GameConsole.Instance != null;
        bool explorerManagerExists = ExplorerManager.Instance != null; // Bunu da ekleyelim, belki lazım olur
        bool currencyManagerExists = CurrencyManager.Instance != null; // İstediniz

        // Diğer manager kontrolleri...

        try
        {
            // Artık perkDef'teki EffectType, Value, Parameter kullanılıyor
            switch (perkDef.effectType)
            {
                case PerkEffectType.AddAllStats:
                    if (statManagerExists) StatManager.Instance.AddAllStats(perkDef.effectValue * amountApplied);
                    else Debug.LogWarning($"[{perkDef.perkID}] StatManager bulunamadı!");
                    consoleMessage = $"<color=cyan>Perk Ödülü: +{perkDef.effectValue * amountApplied:F0} Tüm Statlar</color> ({perkDef.displayName})";
                    break;

                case PerkEffectType.AddStatPoints:
                    int pointsToAdd = (int)Math.Round(perkDef.effectValue * amountApplied);
                    if (levelManagerExists) LevelManager.Instance.AddUnspentStatPoints(pointsToAdd);
                    else Debug.LogWarning($"[{perkDef.perkID}] LevelManager bulunamadı!");
                    consoleMessage = $"<color=green>Perk Ödülü: +{pointsToAdd} Stat Puanı</color> ({perkDef.displayName})";
                    break;

                case PerkEffectType.AddStat:
                    if (!string.IsNullOrEmpty(perkDef.effectParameter))
                    {
                        float statToAdd = perkDef.effectValue * amountApplied;
                        if (statManagerExists) StatManager.Instance.AddStat(perkDef.effectParameter, statToAdd);
                        else Debug.LogWarning($"[{perkDef.perkID}] StatManager bulunamadı!");
                        consoleMessage = $"<color=cyan>Perk Ödülü: +{statToAdd:F0} {perkDef.effectParameter}</color> ({perkDef.displayName})";
                    }
                    else { Debug.LogWarning($"[{perkDef.perkID}] AddStat efekti için 'effectParameter' (Stat adı) eksik."); }
                    break;

                case PerkEffectType.ModifyResourceMaxHealth:
                     if (resourceManagerExists) ResourceManager.Instance.ModifyMaxHealth(perkDef.effectValue * amountApplied);
                     else Debug.LogWarning($"[{perkDef.perkID}] ResourceManager bulunamadı!");
                     consoleMessage = $"<color=lightblue>Perk Etkisi: Maks. Can {(perkDef.effectValue > 0 ? "+" : "")}{perkDef.effectValue * amountApplied:F0}</color> ({perkDef.displayName})";
                     break;
                case PerkEffectType.ModifyResourceMaxEnergy:
                     if (resourceManagerExists) ResourceManager.Instance.ModifyMaxEnergy(perkDef.effectValue * amountApplied);
                     else Debug.LogWarning($"[{perkDef.perkID}] ResourceManager bulunamadı!");
                     consoleMessage = $"<color=lightblue>Perk Etkisi: Maks. Enerji {(perkDef.effectValue > 0 ? "+" : "")}{perkDef.effectValue * amountApplied:F0}</color> ({perkDef.displayName})";
                     break;
                case PerkEffectType.ModifyResourceMaxMana:
                     if (resourceManagerExists) ResourceManager.Instance.ModifyMaxMana(perkDef.effectValue * amountApplied);
                     else Debug.LogWarning($"[{perkDef.perkID}] ResourceManager bulunamadı!");
                     consoleMessage = $"<color=lightblue>Perk Etkisi: Maks. Mana {(perkDef.effectValue > 0 ? "+" : "")}{perkDef.effectValue * amountApplied:F0}</color> ({perkDef.displayName})";
                     break;

                case PerkEffectType.GrantItem:
                     if (!string.IsNullOrEmpty(perkDef.effectParameter) && inventoryManagerExists && itemManagerExists)
                     {
                         ItemData itemToGrant = ItemManager.Instance.GetItemByName(perkDef.effectParameter);
                         int quantity = (int)Math.Round(perkDef.effectValue * amountApplied);
                         if (itemToGrant != null && quantity > 0)
                         {
                             Inventory.Instance.AddItem(itemToGrant, quantity);
                             consoleMessage = $"<color=orange>Perk Ödülü: +{quantity} {itemToGrant.itemName}</color> ({perkDef.displayName})";
                         }
                         else { Debug.LogWarning($"[{perkDef.perkID}] GrantItem eşyası '{perkDef.effectParameter}' bulunamadı veya miktar ({quantity}) geçersiz."); }
                     }
                     else { Debug.LogWarning($"[{perkDef.perkID}] GrantItem için 'effectParameter' (Eşya adı) eksik veya Inventory/ItemManager yok."); }
                     break;

                case PerkEffectType.UnlockFeature:
                    // TODO: Özellik açma mantığı (ilgili manager'ın null kontrolünü burada yapın)
                    Debug.Log($"[PerkManager] ÖZELLİK AÇILDI (TODO): {perkDef.effectParameter} ({perkDef.perkID})");
                    consoleMessage = $"<color=yellow>Özellik Açıldı: {perkDef.effectParameter}</color> ({perkDef.displayName})";
                    break;

                // --- SORGULANAN BONUSLAR VEYA ANLIK ETKİSİ OLMAYANLAR ---
                case PerkEffectType.None:
                case PerkEffectType.AddGoldBonus: // Bunların etkisi StatCalculator'da uygulanacak
                case PerkEffectType.AddXPBonus:
                case PerkEffectType.ModifyResourceHealthRecovery:
                case PerkEffectType.ModifyResourceEnergyRecovery:
                case PerkEffectType.ModifyResourceManaRecovery:
                case PerkEffectType.AddCriticalChance:
                case PerkEffectType.AddCriticalDamage:
                case PerkEffectType.AddDropRate:
                case PerkEffectType.ReduceEnemyHealth:
                case PerkEffectType.ReduceEnemyDamage:
                case PerkEffectType.ReduceEnemyArmor:
                case PerkEffectType.AddHitRate:
                case PerkEffectType.AddProduction:
                case PerkEffectType.AddCooldownReduction:
                case PerkEffectType.AddResourceCostReduction:
                case PerkEffectType.AddPrestigePoints:
                case PerkEffectType.AddPrestigeBonus:
                case PerkEffectType.GetExplorerTimeReduction:
                    consoleMessage = $"<color=grey>Perk '{perkDef.displayName}' alındı (Pasif Bonus veya Etki Yok).</color>";
                    break;

                default:
                    Debug.LogWarning($"HandlePerkReward: Bilinmeyen veya işlenmeyen PerkEffectType '{perkDef.effectType}' ({perkDef.perkID}).");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{perkDef?.perkID ?? "Bilinmeyen Perk"}] HandlePerkReward hatası ({perkDef?.effectType}): {ex.Message}\n{ex.StackTrace}");
            consoleMessage = $"<color=red>HATA: Perk efekti uygulanırken sorun oluştu ({perkDef?.displayName})!</color>";
        }

        if (!string.IsNullOrEmpty(consoleMessage) && gameConsoleExists)
        {
            GameConsole.Instance.AddMessage(consoleMessage);
        }
        else if (!string.IsNullOrEmpty(consoleMessage))
        {
            string cleanMessage = System.Text.RegularExpressions.Regex.Replace(consoleMessage, "<.*?>", string.Empty);
            Debug.Log($"[PerkManager] GameConsole yok, log: {cleanMessage}");
        }
    }

    /// <summary>
    /// Belirli bir PerkEffectType'a sahip tüm perk'lerin toplam etkisini (stack'lerle çarpılmış) hesaplar.
    /// NOT: effectValue'nun Percentage veya Multiplier olup olmadığını KONTROL ETMEZ, sadece toplar/çarpar.
    /// Bu kontrolü StatCalculator gibi çağıran yer yapmalıdır.
    /// </summary>
    public float GetBonusFromPerks(PerkEffectType typeToQuery)
    {
        float totalValue = 0; // Yüzdeler için toplama, çarpanlar için çarpma gerekebilir? Şimdilik toplama.

        if (perkDatabase == null) { return 0; } // Database olmadan hesaplama yapılamaz

        foreach (var pair in _perkCounts)
        {
            string perkID = pair.Key;
            int count = pair.Value;
            if (count <= 0) continue;

            // Veritabanından bu perk'in tanımını al
            PerkDefinition perkDef = perkDatabase.GetPerkDefinitionByID(perkID);

            if (perkDef != null && perkDef.effectType == typeToQuery)
            {
                // Şimdilik sadece Flat ve Percentage değerleri topladığımızı varsayalım.
                // Multiplier için ayrı bir metot veya farklı bir hesaplama gerekebilir.
                totalValue += perkDef.effectValue * count;
            }
        }
        return totalValue;
    }


    // --- KAYIT SİSTEMİ (IGameDataSaveable Uygulaması) ---

    public PerkSaveData GetSaveData()
    {
        Debug.Log("[PerkManager] Kayıt verisi oluşturuluyor.");
        // Anahtar olarak string (perkID) kullanıldığı için direkt kaydedilebilir.
        return new PerkSaveData { perkCounts = new Dictionary<string, int>(_perkCounts) };
    }

    public void LoadFromData(PerkSaveData data)
    {
        // Anahtar string olduğu için direkt yüklenebilir.
        _perkCounts = data?.perkCounts ?? new Dictionary<string, int>();

        if (data != null) { Debug.Log("[PerkManager] Explorer Perk verisi yüklendi."); }
        else { Debug.Log("[PerkManager] Kaydedilmiş Explorer Perk verisi bulunamadı."); }

        OnPerkUpdated?.Invoke();
    }
}