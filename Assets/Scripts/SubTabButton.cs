using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SubTabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image buttonImage;
    public TextMeshProUGUI buttonText;

    [Header("Normal State")]
    public Color normalColor = new Color(0.843f, 0.843f, 0.843f, 1f);
    public float normalTextSize = 18f;

    [Header("Hover State")]
    public Color hoverColor = new Color(0.843f, 0.843f, 0.843f, 1f);
    public float hoverTextSize = 22f;
    public Color hoverTextColor = Color.white;

    private Color originalTextColor;

    void Start()
    {
        originalTextColor = buttonText.color;
        
        // Başlangıç durumunu ayarla
        buttonImage.color = normalColor;
        buttonText.fontSize = normalTextSize;
    }

    // Fare üzerine gelince
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = hoverColor;
        buttonText.fontSize = hoverTextSize;
        buttonText.color = hoverTextColor;
    }

    // Fare ayrılınca
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = normalColor;
        buttonText.fontSize = normalTextSize;
        buttonText.color = originalTextColor;
    }
}