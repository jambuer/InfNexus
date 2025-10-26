using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ kullanmak için

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Fight/Enemy Database")]
public class EnemyDatabase : ScriptableObject
{
    [Tooltip("Oyundaki TÜM düşman (EnemyData) assetlerini buraya sürükleyin.")]
    public List<EnemyData> allEnemies;

    // Hızlı erişim için Dictionary (ID -> EnemyData)
    private Dictionary<string, EnemyData> _enemyLookup;
    private bool _isInitialized = false; // Lookup oluşturuldu mu?

    // Veritabanı ilk kullanıldığında veya gerektiğinde lookup'ı hazırlar
    private void InitializeLookup()
    {
        // Editörde değilsek ve zaten initialize edildiyse tekrar yapma
        if (_isInitialized && Application.isPlaying) return;

        _enemyLookup = new Dictionary<string, EnemyData>();
        if (allEnemies != null)
        {
            foreach (var enemyData in allEnemies)
            {
                if (enemyData != null && !string.IsNullOrEmpty(enemyData.enemyID))
                {
                    if (!_enemyLookup.ContainsKey(enemyData.enemyID))
                    {
                        _enemyLookup.Add(enemyData.enemyID, enemyData);
                    }
                    else
                    {
                        Debug.LogWarning($"EnemyDatabase: Duplicate enemyID '{enemyData.enemyID}' found! Asset: {enemyData.name}", this);
                    }
                }
                else if (enemyData != null)
                {
                    Debug.LogWarning($"EnemyDatabase: EnemyData asset '{enemyData.name}' has an empty ID!", this);
                }
            }
        }
        _isInitialized = true; // Initialize edildi olarak işaretle
        Debug.Log($"EnemyDatabase lookup created/updated with {_enemyLookup.Count} entries.");
    }

    /// <summary>
    /// Verilen enemyID'ye göre EnemyData'yı döndürür. Bulamazsa null döner.
    /// </summary>
    public EnemyData GetEnemyData(string enemyID)
    {
        // Oyundayken her seferinde lookup'ı yeniden oluşturmaya gerek yok,
        // ama editördeyken asset listesi değişebileceği için kontrol faydalı.
        #if UNITY_EDITOR
        InitializeLookup(); // Editörde her çağrıda güncelleyebiliriz
        #else
        if (!_isInitialized) InitializeLookup(); // Oyun build'inde sadece ilk seferde
        #endif


        if (string.IsNullOrEmpty(enemyID))
        {
             Debug.LogWarning("GetEnemyData called with an empty ID.", this);
             return null;
        }

        _enemyLookup.TryGetValue(enemyID, out EnemyData data);
        if (data == null)
        {
            Debug.LogWarning($"EnemyDatabase: Enemy data for ID '{enemyID}' not found!", this);
        }
        return data;
    }

    // Editörde asset listesi değiştiğinde lookup'ı güncellemek için (isteğe bağlı ama kullanışlı)
    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Inspector'da bir değişiklik olduğunda lookup'ı yeniden kurmaya zorla
         _isInitialized = false;
        // InitializeLookup(); // Veya doğrudan burada çağırabiliriz. Start/Awake'ten önce çalışır.
    }
    #endif
}