/// <summary>
/// Bir sınıfın GameDataManager tarafından kaydedilip yüklenebilmesi
/// için uyması gereken "sözleşmeyi" (arayüz) tanımlar.
/// T, o sınıfa ait kaydedilebilir veri tipidir (örn: StatSaveData).
/// </summary>
public interface ISaveable<T>
{
    /// <summary>
    /// Kayıt dosyasından gelen verileri bu yöneticiye yükler.
    /// </summary>
    void LoadFromData(T data);
    
    /// <summary>
    /// Kayıt dosyasına yazılacak güncel verileri oluşturur ve döndürür.
    /// </summary>
    T GetSaveData();
}