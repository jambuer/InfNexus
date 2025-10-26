// Scripts/Fight/Data/EnemyEnums.cs

/// <summary>
/// Düşmanların birincil kategorilerini tanımlar.
/// </summary>
public enum EnemyPrimaryTag
{
    None,
    Raid,
    Race,
    Boss,
    Quest, // Görevle ilgili özel düşmanlar
    Nexus,
    WorldBoss,
    Allied, // Dost ama savaşılabilen?
    Battlefield
}

/// <summary>
/// Düşmanların ikincil (elemental, tür vb.) kategorilerini tanımlar.
/// </summary>
public enum EnemySecondaryTag
{
    None,
    Water,
    Fire,
    Light,
    Shadow,
    Earth,
    Wind,
    Lightning,
    Poison,
    Illusion
}