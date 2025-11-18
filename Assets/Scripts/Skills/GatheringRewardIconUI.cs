using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GatheringNodeUI kartında gösterilecek (Item, Gold vb.)
/// tek bir ödül ikonunu ve miktarını yöneten basit UI script'i.
/// </summary>
public class GatheringRewardIconUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TextMeshProUGUI amountText;

    /// <summary>
    /// Bu ikonu, gelen sprite ve metin ile ayarlar.
    /// </summary>
    public void Setup(Sprite icon, string amountString)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        if (amountText != null)
        {
            amountText.text = amountString;
            amountText.gameObject.SetActive(!string.IsNullOrEmpty(amountString));
        }
    }
}