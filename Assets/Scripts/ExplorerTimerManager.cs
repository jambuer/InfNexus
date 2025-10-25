using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // ToList() için

/// <summary>
/// Oyun kapalıyken bile ilerleyen Explorer zamanlayıcılarını yönetir.
/// Singleton'dır ve IGameDataSaveable arayüzünü uygular.
/// </summary>
public class ExplorerTimerManager : MonoBehaviour, IGameDataSaveable<TimerSaveData> // IGameDataSaveable eklendi
{
    public static ExplorerTimerManager Instance { get; private set; }

    // Aktif zamanlayıcıları runtime'da tutar (UniqueID -> TimerData)
    private Dictionary<string, TimerData> _activeTimers = new Dictionary<string, TimerData>();
    private List<string> _timersToRemove = new List<string>(); // Update sırasında silinecekleri tutar

    // Son Update zamanını tutar (offline ilerleme için)
    private double _lastUpdateTimeStamp;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _lastUpdateTimeStamp = GetCurrentTimestamp(); // Başlangıç zamanını kaydet
            Debug.Log("[ExplorerTimerManager] Başlatıldı.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime; // Bu frame geçen süre
        _timersToRemove.Clear();

        // Tüm aktif zamanlayıcıları ilerlet
        foreach (var pair in _activeTimers)
        {
            if (pair.Value.Tick(deltaTime)) // Eğer zamanlayıcı bittiyse
            {
                _timersToRemove.Add(pair.Key); // Silinecekler listesine ekle
            }
        }

        // Biten zamanlayıcıları ana listeden kaldır
        foreach (string idToRemove in _timersToRemove)
        {
            _activeTimers.Remove(idToRemove);
            Debug.Log($"[ExplorerTimerManager] Zamanlayıcı tamamlandı ve kaldırıldı: {idToRemove}");
        }

        // Son güncelleme zamanını kaydet
         _lastUpdateTimeStamp = GetCurrentTimestamp();
    }

    /// <summary>
    /// Yeni bir zamanlayıcı başlatır veya mevcut olanı günceller (opsiyonel).
    /// </summary>
    /// <param name="uniqueID">Zamanlayıcının benzersiz kimliği.</param>
    /// <param name="duration">Toplam süre (saniye).</param>
    /// <param name="onCompleteCallback">Süre dolduğunda çağrılacak metot.</param>
    /// <param name="onUpdateCallback">Süre güncellendiğinde (kalan süre ile) çağrılacak metot.</param>
    public void StartTimer(string uniqueID, float duration, Action onCompleteCallback, Action<float> onUpdateCallback = null)
    {
        if (duration <= 0) // Süre 0 veya negatifse hemen tamamla
        {
             Debug.Log($"[ExplorerTimerManager] Zamanlayıcı ({uniqueID}) süresi <= 0, hemen tamamlanıyor.");
            onCompleteCallback?.Invoke();
            return;
        }

        if (_activeTimers.ContainsKey(uniqueID))
        {
            // TODO: Zaten çalışan bir zamanlayıcı varsa ne yapılacağına karar ver
            // Üzerine yaz? Hata ver? Şimdilik üzerine yazalım.
            Debug.LogWarning($"[ExplorerTimerManager] Zamanlayıcı ({uniqueID}) zaten çalışıyordu, üzerine yazılıyor.");
            _activeTimers[uniqueID] = new TimerData(uniqueID, duration, onCompleteCallback, onUpdateCallback);
        }
        else
        {
            _activeTimers.Add(uniqueID, new TimerData(uniqueID, duration, onCompleteCallback, onUpdateCallback));
             Debug.Log($"[ExplorerTimerManager] Zamanlayıcı başlatıldı: {uniqueID}, Süre: {duration}s");
        }

        // Başlatıldığı anda UI'ı ilk değerle güncelle
        _activeTimers[uniqueID].OnUpdate?.Invoke(duration);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip zamanlayıcıyı iptal eder ve kaldırır.
    /// </summary>
    public void CancelTimer(string uniqueID)
    {
        if (_activeTimers.ContainsKey(uniqueID))
        {
            // UI'ı sıfırla (eğer OnUpdate atanmışsa)
            _activeTimers[uniqueID].OnUpdate?.Invoke(0f);

            _activeTimers.Remove(uniqueID);
            Debug.Log($"[ExplorerTimerManager] Zamanlayıcı iptal edildi: {uniqueID}");
        }
    }

    /// <summary>
    /// Belirtilen ID'ye sahip zamanlayıcı aktif mi?
    /// </summary>
    public bool IsTimerActive(string uniqueID)
    {
        return _activeTimers.ContainsKey(uniqueID);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip zamanlayıcının kalan süresini döndürür. Yoksa 0 döner.
    /// </summary>
    public float GetRemainingTime(string uniqueID)
    {
         if (_activeTimers.TryGetValue(uniqueID, out TimerData timer))
         {
             return timer.RemainingTime;
         }
         return 0f;
    }


    // --- KAYIT SİSTEMİ ---

    public TimerSaveData GetSaveData()
    {
        TimerSaveData saveData = new TimerSaveData();
        double currentTime = GetCurrentTimestamp();
        foreach (var pair in _activeTimers)
        {
            saveData.activeTimers.Add(new SavedTimerData
            {
                uniqueID = pair.Key,
                remainingTime = pair.Value.RemainingTime // Anlık kalan süre
                // endTimeStamp = currentTime + pair.Value.RemainingTime // Bitiş zamanı damgası
            });
        }
        Debug.Log($"[ExplorerTimerManager] Kaydedilecek {_activeTimers.Count} zamanlayıcı verisi oluşturuldu.");
        return saveData;
    }

    public void LoadFromData(TimerSaveData data)
    {
        _activeTimers.Clear(); // Önce mevcutları temizle
        if (data == null || data.activeTimers == null)
        {
             Debug.Log("[ExplorerTimerManager] Yüklenecek zamanlayıcı verisi bulunamadı.");
            _lastUpdateTimeStamp = GetCurrentTimestamp(); // Yükleme zamanını kaydet
             return;
        }

        double loadTime = GetCurrentTimestamp();
         // Offline süreyi hesaplamak için kayıtlı son güncelleme zamanına ihtiyacımız var.
         // Bunu GameSaveData'ya ekleyebiliriz veya basitçe Application.persistentDataPath'deki
         // dosyanın son yazılma zamanını kullanabiliriz (daha az doğru).
         // Şimdilik, basitlik adına offline ilerlemeyi TAM OLARAK hesaplamıyoruz,
         // sadece kalan süreleri yüklüyoruz. Daha doğru offline hesaplama için yapı genişletilebilir.

        // TODO: Daha doğru offline ilerleme hesaplaması için son save/quit zamanını al.
        // double lastSaveTime = GetTimestampFromSaveFile() ?? loadTime;
         // float offlineTime = (float)(loadTime - lastSaveTime);
         float offlineTime = 0; // Şimdilik 0


        Debug.Log($"[ExplorerTimerManager] {data.activeTimers.Count} zamanlayıcı verisi yükleniyor. Offline Süre (Hesaplanmadı): {offlineTime}s");

        foreach (SavedTimerData savedTimer in data.activeTimers)
        {
            // Offline süreyi hesaba kat
            float newRemainingTime = savedTimer.remainingTime - offlineTime;

            // ExplorerManager gibi ilgili yerlerden OnComplete ve OnUpdate callback'lerini almamız lazım.
            // Bu kısım biraz karmaşık. Şimdilik callback'leri null olarak başlatacağız.
            // ExplorerManager Start'ta veya Load sonrası timer'ları tekrar kontrol edip
            // UI update callback'lerini yeniden atayabilir.
            Action onCompleteCallback = GetCompletionCallback(savedTimer.uniqueID); // Yardımcı metot
            Action<float> onUpdateCallback = GetUpdateCallback(savedTimer.uniqueID); // Yardımcı metot


            if (newRemainingTime <= 0)
            {
                // Eğer offline sürede tamamlandıysa, tamamlama işlemini hemen yap
                Debug.Log($"[ExplorerTimerManager] Zamanlayıcı ({savedTimer.uniqueID}) offline sürede tamamlandı.");
                onCompleteCallback?.Invoke();
            }
            else
            {
                // Henüz bitmediyse, yeni kalan süreyle listeye ekle
                _activeTimers.Add(savedTimer.uniqueID, new TimerData(savedTimer.uniqueID, newRemainingTime, onCompleteCallback, onUpdateCallback));
                // Yükleme sonrası UI'ı ilk değerle güncelle
                onUpdateCallback?.Invoke(newRemainingTime);
            }
        }
         _lastUpdateTimeStamp = loadTime; // Yükleme zamanını kaydet
    }

    // --- Yardımcı Metotlar ---

    // Unix zaman damgasını alır (double olarak saniye cinsinden)
    private double GetCurrentTimestamp()
    {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    // TODO: uniqueID'ye göre doğru OnComplete metodunu döndüren mantığı ekleyin
    // Bu, ExplorerManager'a sorgu yaparak veya ID yapısını kullanarak yapılabilir.
    private Action GetCompletionCallback(string uniqueID)
    {
        // Örnek: Eğer ID "Perk_" ile başlıyorsa ExplorerManager.CompletePerk'i çağır
        if (uniqueID.StartsWith("Qperk") && ExplorerManager.Instance != null) // Qperk asset isimlerine göre varsayım
        {
             // ExplorerManager'daki perk listesinden ID'ye uyan PerkData'yı bulmamız lazım
             ExplorerPerkData perkData = ExplorerManager.Instance.leftPanelPerks.Find(p => p.name + "_Timer" == uniqueID);
             if (perkData != null)
             {
                 return () => ExplorerManager.Instance.CompletePerkAfterTimer(perkData); // Yeni bir metot gerekebilir
             }
        }
        // Örnek: Eğer ID "EXPLORER_QUEST_" ile başlıyorsa ExplorerManager.CompleteExplorerQuest'i çağır
        else if (uniqueID.StartsWith("EXPLORER_QUEST_") && ExplorerManager.Instance != null)
        {
             ExplorerQuestData questData = ExplorerManager.Instance.rightPanelQuests.Find(q => q.questID == uniqueID);
             if (questData != null)
             {
                 return () => ExplorerManager.Instance.CompleteExplorerQuestAfterTimer(questData); // Yeni bir metot gerekebilir
             }
        }
         Debug.LogWarning($"[ExplorerTimerManager] Zamanlayıcı ({uniqueID}) için tamamlama callback'i bulunamadı.");
        return null; // Callback bulunamazsa null döndür
    }

     // TODO: uniqueID'ye göre doğru OnUpdate metodunu döndüren mantığı ekleyin
    private Action<float> GetUpdateCallback(string uniqueID)
    {
         // ExplorerManager'a kalan süreyi bildiren bir metot çağırabiliriz.
        if (ExplorerManager.Instance != null)
        {
             return (remainingTime) => ExplorerManager.Instance.UpdateUITimer(uniqueID, remainingTime); // ExplorerManager'da bu metot olmalı
        }
        return null;
    }

}