using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera mainCamera; // Kameramıza referans tutacak değişken

    // İstenilen Orthographic Size değeri (Inspector'dan ayarlayabilirsin)
    public float targetOrthographicSize = 10f; 

    void Start()
    {
        // Bu script'in bağlı olduğu GameObject üzerindeki Camera bileşenini bul
        mainCamera = GetComponent<Camera>();

        // Eğer kamera bulunduysa ve Orthographic ise boyutunu ayarla
        if (mainCamera != null && mainCamera.orthographic)
        {
            mainCamera.orthographicSize = targetOrthographicSize;
            Debug.Log("Kamera Orthographic Size ayarlandı: " + targetOrthographicSize);
        }
        else
        {
            Debug.LogError("Bu GameObject üzerinde Orthographic bir Camera bileşeni bulunamadı!");
        }
    }

    // İstersen oyun sırasında boyutu değiştirecek fonksiyonlar da ekleyebilirsin
    public void SetOrthographicSize(float newSize)
    {
        if (mainCamera != null && mainCamera.orthographic)
        {
            mainCamera.orthographicSize = newSize;
        }
    }
}