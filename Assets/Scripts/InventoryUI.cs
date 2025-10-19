using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel; // Tüm envanterin ana paneli
    public Transform slotContainer; // Slot prefab'larının ekleneceği yer (örn: bir Grid Layout Group)
    public GameObject slotPrefab; // InventorySlot.cs script'ini içeren prefab

    // Oluşturulan slotları takip etmek için bir liste
    List<InventorySlot> slots = new List<InventorySlot>();

    void Start()
    {
        // Envanter her değiştiğinde (AddItem, RemoveItem) UpdateUI fonksiyonunu çalıştır
        Inventory.Instance.OnInventoryChanged += UpdateUI;

        // Başlangıçta envanter panelini gizle (isteğe bağlı)
        inventoryPanel.SetActive(false);
    }

    // Envanter panelini açıp kapatan bir fonksiyon (bir butona bağlanabilir)
    public void ToggleInventory()
    {
        bool isActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isActive);

        // Panel açıldığında UI'ı güncelle
        if (!isActive)
        {
            UpdateUI();
        }
    }

    // Envanter verisine bakarak UI'ı yeniden çizer
    void UpdateUI()
    {
        // Mevcut envanterdeki item sayısını al
        int itemCount = Inventory.Instance.items.Count;

        // 1. Adım: Yeterli sayıda slot olduğundan emin ol
        while (slots.Count < itemCount)
        {
            // Eksikse yeni slot oluştur
            GameObject newSlotObj = Instantiate(slotPrefab, slotContainer);
            slots.Add(newSlotObj.GetComponent<InventorySlot>());
        }

        // 2. Adım: Slotları doldur
        int i = 0;
        foreach (var itemEntry in Inventory.Instance.items)
        {
            // itemEntry.Key = EnvanterItemData (Odun)
            // itemEntry.Value = int (Miktar, örn: 10)
            slots[i].AddItemToSlot(itemEntry.Key, itemEntry.Value);
            slots[i].gameObject.SetActive(true); // Slotu görünür yap
            i++;
        }

        // 3. Adım: Fazla (boş) slotları gizle
        for (int j = i; j < slots.Count; j++)
        {
            slots[j].ClearSlot();
            slots[j].gameObject.SetActive(false); // Veya ClearSlot() içinde gizle
        }
    }
}