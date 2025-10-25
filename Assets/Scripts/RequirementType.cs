// RequirementType.cs

/// <summary>
/// Adventure sisteminde kullanılabilecek tüm gereksinim türlerini tanımlar.
/// </summary>
public enum RequirementType
{
    None, // Varsayılan, geçersiz
    Level,
    Quest,          // Ana görev sistemindeki görev
    ExplorerQuest,  // Adventure sağ panel görevi
    Item,
    Stat,
    Gold,
    NexusCoin,
    People,
    Health,         // Mevcut can
    Energy,         // Mevcut enerji
    Mana,           // Mevcut mana
    MaxHealth,      // Maksimum can (hem gereksinim hem maliyet olabilir)
    MaxEnergy,      // Maksimum enerji (hem gereksinim hem maliyet olabilir)
    MaxMana,        // Maksimum mana (hem gereksinim hem maliyet olabilir)
    Perk,           // Belirli bir sol panel perk'ine sahip olma
    ExplorerTime    // Özel zamanlayıcı gereksinimi (Qperk8'deki gibi)
    // İhtiyaç duydukça buraya yeni türler ekleyebilirsiniz
}