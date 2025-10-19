using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ kütüphanesi ile listelerde arama yapmak kolaylaşır

public class ItemManager : MonoBehaviour
{
    // Singleton yapısı: Bu script'e her yerden kolayca erişmemizi sağlar.
    public static ItemManager Instance;

    public List<ItemData> allItems; // Oyundaki TÜM item'ları buraya sürükleyeceğiz.

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değişse bile bu obje silinmesin
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // İsme göre item referansını döndüren yardımcı fonksiyon
    public ItemData GetItemByName(string name)
    {
        return allItems.FirstOrDefault(item => item.itemName == name);
    }
}