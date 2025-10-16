using System;

/// <summary>
/// Görev otomasyonunun bir yükseltme seviyesini tanımlar.
/// </summary>
[Serializable]
public class AutomationTier
{
    /// <summary>Bu otomasyon seviyesini aktif etmek için gereken PeopleCoin miktarı.</summary>
    public double peopleCoinCost = 0;
    /// <summary>Bu seviyede görevin yeni tamamlanma süresi (saniye cinsinden).</summary>
    public float completionTime = 0;
    /// <summary>Bu otomasyon seviyesinin açıklayıcı metni.</summary>
    public string description = "Reduces completion time.";
}