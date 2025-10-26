using UnityEngine;
using System; // Serializable için

/// <summary>
/// Bir düşmandan düşebilecek tek bir eşyayı ve düşme koşullarını tanımlar.
/// </summary>
[Serializable]
public class EnemyItemDrop
{
    [Tooltip("Düşecek eşyanın ItemData asset'i.")]
    public ItemData itemToDrop;

    [Tooltip("Oyuncunun Drop Rate'i bu değerden düşükse düşme şansı azalır.")]
    public double dropRateThreshold = 1000; // Örnek değer

    [Tooltip("Oyuncunun Drop Rate'i eşik değerin altındaysa, normal düşme şansının uygulanacak kat çarpanı (örn: 0.2 = 5 kat daha az şans).")]
    [Range(0.01f, 1f)]
    public float chanceMultiplierBelowThreshold = 0.2f; // Varsayılan olarak 5 kat az şans

    [Tooltip("Oyuncunun Drop Rate'i eşik değere eşit veya üzerindeyse temel düşme şansı (0 ile 1 arası). Bu şans oyuncunun genel Drop Rate bonuslarıyla artabilir.")]
    [Range(0f, 1f)]
    public float baseDropChance = 0.1f; // %10 temel şans

    [Tooltip("Bu eşyanın bu düşmandan düşebileceği maksimum adet. 0 ise sınırsız düşebilir.")]
    public int maxDrops = 0; // 0 = Sınırsız

    [Tooltip("Eğer işaretliyse ve düşme başarılı olursa, düşecek eşya miktarı oyuncunun Drop Rate'inin eşik değere oranının tam katı kadar olur (örn: DR 3400, Eşik 1000 ise 3 adet düşer). İşaretli değilse her zaman 1 adet düşer.")]
    public bool quantityScalesWithDropRate = false;

    // TODO: İleride, düşmanın zorluk seviyesine göre düşme şansını veya miktarını etkileyen çarpanlar eklenebilir.
    // public float easyDropMultiplier = 1.0f;
    // public float nightmareDropMultiplier = 0.5f;
}