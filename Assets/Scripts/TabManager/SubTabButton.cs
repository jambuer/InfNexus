using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SubTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image buttonImage;
    public TextMeshProUGUI buttonText;
    public Image underlineImage; // YENİ: Alt çizgiyi temsil eden Image

    [Header("Normal State")]
    public Color normalColor = new Color(0.843f, 0.843f, 0.843f, 1f);
    public float normalTextSize = 18f;

    [Header("Hover State")]
    public Color hoverColor = new Color(0.843f, 0.843f, 0.843f, 1f);
    public float hoverTextSize = 22f;
    public Color hoverTextColor = Color.white;

    [Header("Underline Effect")]
    // YENİ: Üzerine gelince çizginin ne kadar büyüyeceğini belirler (örn: x=1.2, y=1, z=1)
    public Vector3 hoverLineScale = new Vector3(1.1f, 1f, 1f); 

    // Renkler
    private Color normalLineColor;
    private Color hoverLineColor;
    private Color activeLineColor;
    
    // Orijinal durumları saklamak için
    private Color originalTextColor;
    private Vector3 originalLineScale;
    private bool _isSelected = false;

    void Start()
    {
        originalTextColor = buttonText.color;
        
        // YENİ: Çizginin orijinal ölçeğini kaydet
        if (underlineImage != null)
        {
            originalLineScale = underlineImage.transform.localScale;
        }

        // YENİ: Hex kodlarından Renkleri ayarla
        ColorUtility.TryParseHtmlString("#969262", out normalLineColor);
        ColorUtility.TryParseHtmlString("#C35F13", out hoverLineColor);
        ColorUtility.TryParseHtmlString("#172A41", out activeLineColor);
        
        // Başlangıç durumunu ayarla
        SetNormalState();
    }

    // Fare üzerine gelince
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected) return; // Zaten seçiliyse hover efekti uygulama
        SetHoverState();
    }

    // Fare ayrılınca
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return; // Zaten seçiliyse normal duruma dönme
        SetNormalState();
    }

    // YENİ: Bu metot, button'un seçili durumunu ayarlar (Başka bir script'ten çağrılmalı)
    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        if (_isSelected)
        {
            SetActiveState();
        }
        else
        {
            SetNormalState();
        }
    }

    // YENİ: Normal (seçili değil, hover değil) durum
    private void SetNormalState()
    {
        buttonImage.color = normalColor;
        buttonText.fontSize = normalTextSize;
        buttonText.color = originalTextColor;

        if (underlineImage != null)
        {
            underlineImage.color = normalLineColor;
            underlineImage.transform.localScale = originalLineScale;
        }
    }

    // YENİ: Hover (fare üzerinde, seçili değil) durum
    private void SetHoverState()
    {
        buttonImage.color = hoverColor;
        buttonText.fontSize = hoverTextSize;
        buttonText.color = hoverTextColor;

        if (underlineImage != null)
        {
            underlineImage.color = hoverLineColor;
            underlineImage.transform.localScale = hoverLineScale;
        }
    }

    // YENİ: Aktif (seçili) durum
    private void SetActiveState()
    {
        // Aktif durumun, hover durumu gibi görünmesini sağlıyoruz
        // (Siz aksini belirtmediğiniz için)
        buttonImage.color = hoverColor; 
        buttonText.fontSize = hoverTextSize;
        buttonText.color = hoverTextColor;

        if (underlineImage != null)
        {
            // Ama çizgi rengi, sizin belirttiğiniz aktif rengi olacak
            underlineImage.color = activeLineColor;
            underlineImage.transform.localScale = hoverLineScale;
        }
    }
}