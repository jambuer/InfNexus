using System.Collections.Generic;
using System; // Action için


/// <summary>
/// Aktif bir zamanlayıcının bilgilerini tutar (kaydedilmez, sadece runtime'da kullanılır).
/// </summary>
public class TimerData
{
    public string UniqueID;         // Zamanlayıcının benzersiz kimliği (örn: Perk ID, Quest ID)
    public float RemainingTime;    // Kalan süre (saniye)
    public Action OnComplete;       // Süre dolduğunda çağrılacak metot
    public Action<float> OnUpdate; // Süre güncellendiğinde çağrılacak metot (UI için)

    // Zamanlayıcıyı başlatmak için constructor
    public TimerData(string id, float duration, Action onCompleteCallback, Action<float> onUpdateCallback = null)
    {
        UniqueID = id;
        RemainingTime = duration;
        OnComplete = onCompleteCallback;
        OnUpdate = onUpdateCallback;
    }

    /// <summary>
    /// Zamanlayıcıyı bir miktar ilerletir. Süre dolduysa true döner.
    /// </summary>
    /// <param name="deltaTime">Geçen süre (saniye).</param>
    /// <returns>Süre dolduysa true.</returns>
    public bool Tick(float deltaTime)
    {
        if (RemainingTime <= 0) return true; // Zaten bitmişse

        RemainingTime -= deltaTime;
        OnUpdate?.Invoke(RemainingTime); // UI'ı güncelle

        if (RemainingTime <= 0)
        {
            RemainingTime = 0; // Negatife düşmesin
            OnComplete?.Invoke(); // Tamamlama metodunu çağır
            return true; // Bittiğini bildir
        }
        return false; // Henüz bitmedi
    }
}

/// <summary>
/// Kayıt dosyasına yazılacak zamanlayıcı bilgisini tutar.
/// </summary>
[System.Serializable] // Kaydedilebilmesi için gerekli
public struct SavedTimerData
{
    public string uniqueID;
    public float remainingTime;
    public double endTimeStamp; // Oyundan çıkıldığında bitiş zamanını kaydetmek için (opsiyonel ama daha doğru)
}

/// <summary>
/// Tüm aktif zamanlayıcıların kaydedilecek verilerini içeren sınıf.
/// </summary>
[System.Serializable]
public class TimerSaveData // Bu sınıf GameSaveData.cs içine eklenecek
{
    public List<SavedTimerData> activeTimers = new List<SavedTimerData>();
}