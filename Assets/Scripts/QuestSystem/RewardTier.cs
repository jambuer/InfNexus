using System;
using UnityEngine; // Range attribute'ı için

/// <summary>
/// Para birimi ödüllerinin (Altın, Nexus Coin vb.) olası dilimlerini ve düşme olasılıklarını tanımlar.
/// </summary>
[Serializable]
public class RewardTier
{
    /// <summary>Bu ödül diliminden verilecek minimum miktar.</summary>
    public double minAmount;
    /// <summary>Bu ödül diliminden verilecek maksimum miktar.</summary>
    public double maxAmount;
    /// <summary>Bu ödül diliminin seçilme olasılığı ağırlığı. Toplam ağırlık içindeki payı oranında seçilir.</summary>
    [Range(0.1f, 100f)] // Minimum 0.1 ağırlık, maksmum 100 ağırlık.
    public float probabilityWeight = 1f;

    // Rastgele bir miktar döndürür
    public double GetRandomAmount()
    {
        return UnityEngine.Random.Range((float)minAmount, (float)maxAmount);
    }
}