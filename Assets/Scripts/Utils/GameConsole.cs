using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class GameConsole : MonoBehaviour
{
    // Singleton (Her yerden erişim için)
    public static GameConsole Instance;

    [Header("UI Bileşenleri")]
    public ScrollRect scrollRect;   // ScrollView'un kendisi (Otomatik aşağı kaydırmak için)
    public Text consoleText;        // Mesajların yazılacağı ana Text objesi

    [Header("Ayarlar")]
    public int maxMessages = 100;   // Ekranda tutulacak maksimum mesaj sayısı

    // Mesajları hafızada tutan liste
    private List<string> messages = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Başlangıçta konsolu temizle
        consoleText.text = "";
    }

    // Dışarıdan çağrılacak ana fonksiyon
    public void AddMessage(string newMessage)
    {
        // Mesaj listesini yönet
        if (messages.Count >= maxMessages)
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

        // UI'ın güncellenmesi için 1 frame bekleyip en alta kaydır
        StartCoroutine(ScrollToBottom());
    }

    // Konsolu en alta kaydıran yardımcı fonksiyon
    private IEnumerator ScrollToBottom()
    {
        // Layout'un güncellenmesi için bir frame bekle
        yield return null; 
        
        // Scroll çubuğunu en alta (0) çek
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}