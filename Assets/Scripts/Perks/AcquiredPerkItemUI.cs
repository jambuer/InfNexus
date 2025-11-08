using UnityEngine;
using TMPro; // TextMeshPro kullanıyorsanız bu satır gerekli

/// <summary>
/// ScrollView'daki her bir kazanılmış perk satırını temsil eden UI script'i.
/// Bu script'i, içinde iki Text objesi olan bir Prefab'a ekleyin.
/// </summary>
public class AcquiredPerkItemUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI perkNameText; // Inspector'dan "Display Name" text'ini sürükleyin

    [SerializeField]
    private TextMeshProUGUI perkDescriptionText; // Inspector'dan "Description" text'ini sürükleyin

    /// <summary>
    /// Bu UI elemanını (satırı) verilen perk verisi ile doldurur.
    /// </summary>
    /// <param name="perkDef">Gösterilecek perk'in PerkDefinition verisi</param>
    /// <param name="stackCount">Oyuncunun bu perk'ten kaç tane biriktirdiği (PerkManager'dan gelir)</param>
    public void Setup(PerkDefinition perkDef, int stackCount)
    {
        if (perkDef == null)
        {
            Debug.LogError("[AcquiredPerkItemUI] Setup fonksiyonuna null perkDef geldi.");
            return;
        }

        // 1. Görünen Ad (Display Name)
        // PerkManager'daki log'lardan "displayName" alanınız olduğunu biliyorum.
        if (perkNameText != null)
        {
            perkNameText.text = perkDef.displayName;
        }

        // 2. Açıklama ve Seviye
        if (perkDescriptionText != null)
        {
            // PerkDefinition'da "description" adında bir alanınız olduğunu varsayıyorum.
            // Eğer alanın adı farklıysa (örn: "perkDescription"), burayı güncelleyin.
            string description = perkDef.description; // <-- Gerekirse bu alan adını (description) güncelleyin.
            
            perkDescriptionText.text = $"{description} (Seviye: {stackCount})";
        }
    }
}