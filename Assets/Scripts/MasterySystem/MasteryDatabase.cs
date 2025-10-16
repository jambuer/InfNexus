using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ sorguları için (örneğin .FirstOrDefault())

/// <summary>
/// Tüm MasteryData ScriptableObject'larını bir araya toplayan bir veritabanı.
/// MasteryManager tarafından ustalık verilerine erişim sağlamak için kullanılır.
/// </summary>
[CreateAssetMenu(fileName = "MasteryDatabase", menuName = "Mastery System/Mastery Database")]
public class MasteryDatabase : ScriptableObject
{
    /// <summary>
    /// Projedeki tüm ustalık yollarının listesi.
    /// Bu listeyi Unity Editor'dan manuel olarak doldurun.
    /// </summary>
    [Tooltip("Projedeki tüm ustalık yollarının listesi. Unity Editor'dan sürükleyip bırakarak doldurun.")]
    public List<MasteryData> allMasteryPaths;

    /// <summary>
    /// Belirtilen masteryID'ye sahip MasteryData'yı bulur ve döndürür.
    /// </summary>
    /// <param name="masteryID">Aranacak ustalık yolunun benzersiz kimliği.</param>
    /// <returns>Bulunan MasteryData nesnesi, bulunamazsa null.</returns>
    public MasteryData GetMasteryData(string masteryID)
    {
        if (string.IsNullOrEmpty(masteryID))
        {
            Debug.LogWarning("GetMasteryData: Boş bir masteryID ile arama yapıldı.");
            return null;
        }

        // LINQ kullanarak listede arama yapıyoruz. Performans kritikse daha optimize bir Dictionary kullanılabilir.
        // Ancak ustalık yolu sayısı çok fazla (binlerce) olmadıkça bu yöntem yeterlidir.
        return allMasteryPaths.FirstOrDefault(mastery => mastery.masteryID == masteryID);
    }
}