using UnityEngine;

/// <summary>
/// Generic Singleton base class. Ensures only one instance of a MonoBehaviour exists.
/// Usage: public class MyManager : Singleton<MyManager> { ... }
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // Scene'de var olan bir instance'ı ara
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        // Yoksa yeni bir GameObject oluşturup ona ekle
                        GameObject singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";

                        // Scene değiştiğinde yok olmamasını sağla (İsteğe bağlı, Manager'lar için genellikle istenir)
                        DontDestroyOnLoad(singletonObject);

                        Debug.Log($"[Singleton] An instance of {typeof(T)} is needed in the scene, so '{singletonObject.name}' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        // Scene'de zaten varsa logla (Birden fazla olmamalı)
                        T[] instances = FindObjectsByType<T>(FindObjectsSortMode.None);
                        if (instances.Length > 1)
                        {
                            Debug.LogError($"[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it. Found {instances.Length} instances of {typeof(T)}.");
                            // İsteğe bağlı: Diğer instanceları yok et
                            // for (int i = 1; i < instances.Length; i++) Destroy(instances[i].gameObject);
                        }
                        // Debug.Log($"[Singleton] Using instance already created: {_instance.gameObject.name}");
                    }
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// Awake metodu genellikle Singleton instance'ını ayarlar.
    /// Eğer alt sınıfta Awake override edilecekse, base.Awake() çağrılmalıdır.
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            // Scene değiştiğinde yok olmamasını sağla (Eğer GameObject sahnede manuel olarak oluşturulduysa)
            // DontDestroyOnLoad(gameObject); // Eğer Manager'larınız zaten root objelerse bu satıra gerek olmayabilir veya çift DontDestroyOnLoad hatası verebilir.
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already exists. Destroying duplicate: {gameObject.name}");
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed,
    /// it will create a buggy ghost object that will stay on the Editor scene
    /// even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
           // _applicationIsQuitting = true; // Sadece uygulama kapanırken değil, sahne değişirken de Destroy çağrılabilir, bu yüzden bu satır sorun yaratabilir.
        }
    }

     protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true; // Uygulama kapanırken işaretle
    }
}