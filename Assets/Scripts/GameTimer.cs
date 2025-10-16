using UnityEngine;
using TMPro; // TextMeshPro kullanmak için bu satır zorunlu
using System; // TimeSpan yapısını kullanmak için bu satır zorunlu

public class GameTimer : MonoBehaviour
{
    [Header("UI Referansı")]
    public TextMeshProUGUI timerText;

    private float elapsedTime = 0f;

    void Update()
    {
        // Her frame'de geçen süreyi toplama ekle
        elapsedTime += Time.deltaTime;

        // Toplam saniyeyi, okunması kolay bir formata (saat, dakika, saniye) dönüştür
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);

        // Metni formatla (Örnek: 25:33:48). Toplam saat, dakika ve saniyeyi gösterir.
        string formattedTime = string.Format("{0:00}:{1:00}:{2:00}",
                                            Math.Floor(timeSpan.TotalHours), // 24 saati aşarsa 25, 26 diye devam eder
                                            timeSpan.Minutes,
                                            timeSpan.Seconds);

        // Metni UI elemanına yazdır
        if (timerText != null)
        {
            timerText.text = formattedTime;
        }
    }
}