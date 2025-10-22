using UnityEngine;
using System.Collections.Generic;
using System;

public class Inventory : MonoBehaviour
{
    // Singleton yapısı
    public static Inventory Instance;

    // Oyuncunun sahip olduğu eşyaları ve miktarlarını tutan sözlük (Dictionary)
    // Key: ItemData (Eşyanın kendisi), Value: int (Miktar)
    public Dictionary<ItemData, int> items = new Dictionary<ItemData, int>();
    
    // UI güncellenmesi gerektiğinde bu event'i tetikleriz
    public System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Envantere eşya ekleme
    public void AddItem(ItemData itemToAdd, int amount = 1)
    {
        // Eğer bu item'dan envanterde zaten varsa, sayısını artır
        if (items.ContainsKey(itemToAdd))
        {
            items[itemToAdd] += amount;
        }
        // Yoksa, envantere yeni bir giriş olarak ekle
        else
        {
            items.Add(itemToAdd, amount);
        }

        Debug.Log($"{amount} x {itemToAdd.itemName} envantere eklendi.");
        OnInventoryChanged_Static?.Invoke(); // Envanterin değiştiğini UI'a bildir

        if (GameConsole.Instance != null)
        {
            GameConsole.Instance.AddMessage($"+{amount} {itemToAdd.itemName} elde edildi.");
        }

    }


    // Envanterden eşya silme
    public void RemoveItem(ItemData itemToRemove, int amount = 1)
    {
        if (items.ContainsKey(itemToRemove))
        {
            items[itemToRemove] -= amount;

            // Eğer item'ın sayısı 0 veya altına düşerse, envanterden tamamen kaldır
            if (items[itemToRemove] <= 0)
            {
                items.Remove(itemToRemove);
            }

            Debug.Log($"{amount} x {itemToRemove.itemName} envanterden silindi.");
            OnInventoryChanged_Static?.Invoke();// Envanterin değiştiğini UI'a bildir
        }
    }
    
    public static event Action OnInventoryChanged_Static;

    // Belirli bir item'dan yeterli miktarda var mı diye kontrol et
    public bool HasItem(ItemData item, int amount = 1)
    {
        if (items.ContainsKey(item) && items[item] >= amount)
        {
            return true;
        }
        return false;
    }
}