using UnityEngine;
using UnityEngine.UI;

public class UIZoomController : MonoBehaviour
{
    [Tooltip("Oyun başladığında Canvas'ın alacağı başlangıç ölçek faktörü (Örn: 0.7f).")]
    public float initialScale = 1.0f; // Varsayılanı 1.0f olarak tutun, ancak Inspector'da değiştirin
    
    // Inspector'da ayarlanacak değerler
    [Tooltip("Canvas'ın referans alındığı Canvas Scaler bileşeni.")]
    public CanvasScaler canvasScaler; 
    
    [Tooltip("Yakınlaştırma/Uzaklaştırma hızı.")]
    public float zoomSpeed = 0.1f;
    
    [Tooltip("Minimum ölçek faktörü (örn: 0.5f = %50 küçültme).")]
    public float minScale = 0.5f;

    [Tooltip("Maksimum ölçek faktörü (örn: 2.0f = %200 büyütme).")]
    public float maxScale = 2.0f; 
    

    void Start()
    {
        // Oyuna başladığınızda Canvas'ı istediğiniz initialScale değerine ayarlar.
        canvasScaler.scaleFactor = initialScale;
    }

    void Update()
    {
        // Farenin tekerleğini kontrol et (Genellikle zoom için kullanılır)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        // Eğer tekerlek hareket ettiyse
        if (scrollInput != 0f)
        {
            // Canvas Scaler'dan mevcut ölçek faktörünü al
            float currentScale = canvasScaler.scaleFactor;
            
            // Yeni ölçek değerini hesapla (Kaydırma yönüne göre artır veya azalt)
            float newScale = currentScale + scrollInput * zoomSpeed;

            // Ölçeği minimum ve maksimum sınırlar arasında tut
            newScale = Mathf.Clamp(newScale, minScale, maxScale);
            
            // Canvas Scaler'a yeni ölçek faktörünü uygula
            canvasScaler.scaleFactor = newScale;
        }
    }
}