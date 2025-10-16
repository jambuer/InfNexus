using System;
using UnityEngine;

/// <summary>
/// Görev tamamlandığında kazanılacak kalıcı stat ödüllerini tanımlar.
/// </summary>
[Serializable]
public class StatReward
{
    /// <summary>
    /// Hangi temel statın ödül olarak verileceğini belirten enum.
    /// </summary>
    public StatType statToReward;

    /// <summary>
    /// Verilecek stat miktarı.
    /// </summary>
    [Range(0.1f, 100f)]
    public double amount = 1;
}

/// <summary>
/// Hangi temel statların ödül olarak verilebileceğini veya gereksinim olarak kullanılabileceğini tanımlar.
/// Bu enum'un StatManager.cs içindeki mantıkla uyumlu olması önemlidir.
/// </summary>
public enum StatType
{
    None,
    Physical,
    Mental,
    Perception,
    Spiritual,
    Luck,
    Social
}
