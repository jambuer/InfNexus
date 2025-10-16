using System;
using UnityEngine; // Range attribute'ı için

/// <summary>
/// Bir görevi başlatmak veya otomasyonunu açmak için gereken koşulları tanımlar.
/// </summary>
[Serializable]
public class QuestRequirements
{
    [Header("Seviye & Kaynak Gereksinimleri")]
    /// <summary>Görevi başlatmak için gereken minimum karakter seviyesi.</summary>
    public int requiredLevel = 1;
    /// <summary>Görevi başlatmak için gereken minimum mevcut sağlık.</summary>
    public double requiredHealth = 0;
    /// <summary>Görevi başlatmak için gereken minimum mevcut enerji.</summary>
    public double requiredEnergy = 0;
    /// <summary>Görevi başlatmak için gereken minimum mevcut mana.</summary>
    public double requiredMana = 0;
    /// <summary>Görevi başlatmak için gereken minimum altın.</summary>
    public double requiredGold = 0;
    /// <summary>Görevi başlatmak için gereken minimum Nexus Coin.</summary>
    public double requiredNexusCoin = 0;
    /// <summary>Görevi başlatmak için harcanacak temel tamamlama süresi. Bu bir maliyet olarak da düşünülebilir.</summary>
    public float baseCompletionTimeCost = 0f; // Buradaki baseCompletionTimeCost değeri, görev başlatma maliyeti olarak görev süresini ifade eder.

    [Header("Stat Gereksinimleri")]
    /// <summary>Görevi başlatmak için gereken minimum Physical statı.</summary>
    public double requiredPhysical = 0;
    /// <summary>Görevi başlatmak için gereken minimum Mental statı.</summary>
    public double requiredMental = 0;
    /// <summary>Görevi başlatmak için gereken minimum Perception statı.</summary>
    public double requiredPerception = 0;
    /// <summary>Görevi başlatmak için gereken minimum Spiritual statı.</summary>
    public double requiredSpiritual = 0;
    /// <summary>Görevi başlatmak için gereken minimum Luck statı.</summary>
    public double requiredLuck = 0;
    /// <summary>Görevi başlatmak için gereken minimum Social statı.</summary>
    public double requiredSocial = 0;
    // Tüm statlar için benzer satırlar eklenebilir.
}