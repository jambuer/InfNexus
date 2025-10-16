using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Belirli bir eylem (görev, zanaat vb.) için Ustalık yolunu ve seviyelerini tanımlayan ScriptableObject.
/// </summary>
[CreateAssetMenu(fileName = "NewMasteryPath", menuName = "Mastery System/Mastery Data")]
public class MasteryData : ScriptableObject
{
    /// <summary>
    /// Bu ustalığın benzersiz kimliği. QuestData'daki masteryID ile eşleşmelidir.
    /// </summary>
    [Tooltip("Bu ustalığın benzersiz kimliği. QuestData'daki masteryID ile eşleşmelidir.")]
    public string masteryID;

    /// <summary>
    /// Ustalığın arayüzde görünecek adı (örn: "Demircilik Ustalığı").
    /// </summary>
    [Tooltip("Ustalığın arayüzde görünecek adı (örn: 'Demircilik Ustalığı').")]
    public string displayName;

    /// <summary>
    /// Bu ustalık yolundaki tüm seviyeleri ve ödülleri içeren liste.
    /// </summary>
    public List<MasteryTier> masteryTiers;
}

/// <summary>
/// Bir ustalık seviyesinin gereksinimlerini ve ödülünü tanımlar.
/// </summary>
[System.Serializable]
public class MasteryTier
{
    /// <summary>
    /// Bu seviyeye ulaşmak için ilgili eylemin kaç kez tamamlanması gerektiği.
    /// </summary>
    [Tooltip("Bu seviyeye ulaşmak için ilgili eylemin kaç kez tamamlanması gerektiği.")]
    public int completionRequirement;

    /// <summary>
    /// Bu seviyeye ulaşıldığında verilecek ödülün türü.
    /// </summary>
    [Tooltip("Bu seviyeye ulaşıldığında verilecek ödülün türü.")]
    public MasteryRewardType rewardType;

    /// <summary>
    /// Verilecek ödülün sayısal değeri. Yüzdesel ise 0.1 = %10, düz ise doğrudan değer.
    /// </summary>
    [Tooltip("Verilecek ödülün sayısal değeri. Yüzdesel ise 0.1 = %10, düz ise doğrudan değer.")]
    public float rewardValue;

    /// <summary>
    /// Bu seviyenin arayüzde görünecek açıklaması (örn: "Demircilik süresini %5 azaltır").
    /// </summary>
    [Tooltip("Bu seviyenin arayüzde görünecek açıklaması.")]
    [TextArea]
    public string description;
}

/// <summary>
/// Ustalık seviyelerinde verilebilecek ödül türlerini tanımlar.
/// </summary>
public enum MasteryRewardType
{
    /// <summary>İlgili eylemin kaynak maliyetini (örn: enerji) yüzde olarak azaltır.</summary>
    ReduceActionCostPercent,
    /// <summary>İlgili eylemin getirisini (örn: altın, üretilen eşya) düz birim olarak artırır.</summary>
    IncreaseYieldFlat,
    /// <summary>İlgili eylemin tamamlanma süresini yüzde olarak azaltır.</summary>
    ReduceActionTimePercent,
    /// <summary>Belirtilen bir temel statı kalıcı olarak artırır.</summary>
    IncreaseSpecificStat,
    /// <summary>Yeni bir zanaat tarifi veya görev açar (bu, oyunun başka bir yerinde event olarak dinlenebilir).</summary>
    UnlockNewRecipe
}