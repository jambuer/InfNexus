using UnityEngine;

/// <summary>
/// Verilebilecek genel ödül türlerini tanımlar.
/// </summary>
public enum RewardType
{
    XP,             // Oyuncu XP
    Gold,
    NexusCoin,
    People,
    PremiumCoin,
    Item,           // Eşya
    Stat,           // Kalıcı Stat (Physical vb.)
    Perk,           // Perk (Explorer, Lucky vb.)
    LifeSkillXP,    // Yaşam Becerisi XP
    JobXP           // Meslek XP
}

/// <summary>
/// Herhangi bir eylem (Görev, Toplama, Düşman) sonrası verilecek 
/// ödülleri tanımlamak için kullanılan genel veri yapısı.
/// </summary>
[System.Serializable]
public struct GameReward
{
    [Tooltip("Bu ödülün türü (XP, Altın, Eşya vb.)")]
    public RewardType rewardType;

    [Tooltip("Çoğu ödül türü için miktar (XP miktarı, Altın miktarı, Stat miktarı, SkillXP miktarı vb.)")]
    public float amount;

    [Tooltip("Tür = Item: Eşya ID'si (ItemName) \nTür = Stat: Stat Adı (Physical, Mental vb.) \nTür = Perk: Perk Tag'i (perk_first vb.)")]
    public string stringParameter;
    
    [Tooltip("Tür = Item ise, doğrudan ItemData SO'sunu buraya sürükleyebilirsiniz (stringParameter'a alternatiftir).")]
    public ItemData itemData; // ItemManager.cs'in kullandığı ScriptableObject

    [Tooltip("Tür = LifeSkillXP ise, hangi Life Skill'e XP verileceğini seçin.")]
    public LifeSkill lifeSkill; // SkillEnums.cs'den

    [Tooltip("Tür = JobXP ise, hangi Mesleğe XP verileceğini seçin.")]
    public Job job; // SkillEnums.cs'den
}