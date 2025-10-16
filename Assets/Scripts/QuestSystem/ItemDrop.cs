using System;
using UnityEngine; // Range attribute'ı için

/// <summary>
/// Görev tamamlandığında düşebilecek eşyaları ve düşme olasılıklarını tanımlar.
/// </summary>
[Serializable]
public class ItemDrop
{
    /// <summary>Düşecek eşyanın ScriptableObject referansı. (ItemData sınıfı ileride tanımlanacak).</summary>
    public ItemData itemToDrop; // Bu sınıfı daha sonra oluşturacağız.
    /// <summary>Eşyanın düşme olasılığı (0 ile 1 arasında).</summary>
    [Range(0f, 1f)]
    public float dropChance = 0.1f; // %10 düşme şansı
    /// <summary>Düşecek eşyanın minimum miktarı.</summary>
    public int minQuantity = 1;
    /// <summary>Düşecek eşyanın maksimum miktarı.</summary>
    public int maxQuantity = 1;

    /// <summary>Eşyanın düşüp düşmeyeceğini ve düşerse miktarını belirler.</summary>
    public (ItemData item, int quantity) GetDrop()
    {
        if (UnityEngine.Random.value <= dropChance)
        {
            int quantity = UnityEngine.Random.Range(minQuantity, maxQuantity + 1); // Max dahil
            return (itemToDrop, quantity);
        }
        return (null, 0); // Eşya düşmezse
    }
}