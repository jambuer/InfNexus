using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Fare olayları için bu kütüphane şart

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon; // Item'ın görseli (Senin "1. kategori sabit görsel" dediğin)
    public Text amountText; // Eşya miktarı

    private ItemData item; // Bu slotun temsil ettiği item

    // Slotu item bilgisiyle doldurur
    public void AddItemToSlot(ItemData newItem, int amount)
    {
        item = newItem;
        icon.sprite = item.icon;
        icon.enabled = true;
        
        // Eğer eşya yığınlanabilirse (stackable) miktarı göster
        if (amount > 1)
        {
            amountText.text = amount.ToString();
            amountText.gameObject.SetActive(true);
        }
        else
        {
            amountText.gameObject.SetActive(false);
        }
    }

    // Slotu temizler (item silindiğinde)
    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        amountText.gameObject.SetActive(false);
    }

    // --- Fare Olayları ---

    // Fare üzerine geldiğinde
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Eğer bu slotta bir item varsa, Tooltip'i göster
        if (item != null)
        {
            ItemTooltip.Instance.ShowTooltip(item);
        }
    }

    // Fare üzerinden çekildiğinde
    public void OnPointerExit(PointerEventData eventData)
    {
        // Tooltip'i gizle
        ItemTooltip.Instance.HideTooltip();
    }
}