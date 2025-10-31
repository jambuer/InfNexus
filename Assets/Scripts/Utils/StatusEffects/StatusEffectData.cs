using UnityEngine;

/// <summary>
/// Bir 'Status Effect' (Buff veya Debuff) tanımını tutan ScriptableObject.
/// Örn: "Güç İksiri" -> +10 Physical, 30 saniye.
/// </summary>
[CreateAssetMenu(fileName = "NewStatusEffect_", menuName = "Stats/Status Effect Data")]
public class StatusEffectData : ScriptableObject
{
    [Tooltip("Efektin benzersiz kimliği (örn: Potion_Strength_T1)")]
    public string effectID;
    
    [Tooltip("UI'da görünecek isim (örn: Güç Artışı)")]
    public string displayName;
    
    [Tooltip("UI'da görünecek açıklama")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("UI'da gösterilecek ikon")]
    public Sprite icon;
    
    [Tooltip("Etkinin süresi (saniye). 0 veya daha azı, manuel kaldırılana kadar kalıcı demektir.")]
    public float duration = 30f;
    
    [Tooltip("Bu bir 'Debuff' mu? (UI'da kırmızı göstermek için)")]
    public bool isDebuff = false;

    [Tooltip("Bu etki üst üste eklenebilir mi? (örn: 2 güç iksiri içmek)")]
    public bool isStackable = false;

    [Header("Efekt Detayları")]
    [Tooltip("Etkinin ne yapacağı (örn: AddStatBonus)")]
    public StatusEffectType effectType = StatusEffectType.None;
    
    [Tooltip("Etkinin sayısal değeri")]
    public double effectValue = 10;
    
    [Tooltip("Etki için ek parametre (örn: 'AddStatBonus' için 'Physical' veya 'Mental')")]
    public string effectParameter = ""; 
}