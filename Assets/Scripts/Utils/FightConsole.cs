using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro için
using System.Collections;
using System.Collections.Generic;
using System.Text;

// GameConsole.cs'in kopyası, Savaş Paneli için uyarlandı
public class FightConsole : MonoBehaviour
{
    // Singleton OLMASIN, FightManager referans verecek

    [Header("UI Bileşenleri")]
    public ScrollRect scrollRect;   // ScrollView (Otomatik aşağı kaydırmak için)
    public TextMeshProUGUI consoleText; // Mesajların yazılacağı TextMeshPro objesi

    [Header("Ayarlar")]
    public int maxMessages = 50;   // Ekranda tutulacak maksimum mesaj sayısı

    // Mesajları hafızada tutan liste
    private List<string> messages = new List<string>();

    private void Start()
    {
        // Başlangıçta konsolu temizle (opsiyonel)
        if(consoleText != null) consoleText.text = "";
    }

    /// <summary>
    /// Savaş konsoluna yeni bir mesaj ekler.
    /// </summary>
    public void AddMessage(string newMessage)
    {
        if (consoleText == null) return; // Text atanmamışsa çık

        // Zaman damgası ekleyebiliriz (opsiyonel)
        // string timestamp = $"[{Time.time:F1}] ";
        // newMessage = timestamp + newMessage;

        // Mesaj listesini yönet
        while (messages.Count >= maxMessages)
        {
            messages.RemoveAt(0); // En eski mesajı sil
        }

        messages.Add(newMessage);

        // Tüm mesajları tek bir metin haline getir
        StringBuilder sb = new StringBuilder();
        foreach (string msg in messages)
        {
            sb.AppendLine(msg); // Her mesajı yeni bir satıra ekle
        }

        consoleText.text = sb.ToString();

        consoleText.ForceMeshUpdate();

        // UI'ın güncellenmesi için 1 frame bekleyip en alta kaydır
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// Konsoldaki tüm mesajları temizler.
    /// </summary>
    public void ClearConsole()
    {
        messages.Clear();
        if (consoleText != null) consoleText.text = "";
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f; // En üste kaydır
    }


    // Konsolu en alta kaydıran yardımcı fonksiyon
    private IEnumerator ScrollToBottom()
    {
        // Layout'un güncellenmesi için bir frame bekle
        yield return null;

        // Scroll çubuğunu en alta (0) çek
        if (scrollRect != null)
        {
            // Bazen anında 0'a gitmeyebilir, küçük bir gecikme ekleyebiliriz
            yield return new WaitForEndOfFrame();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}