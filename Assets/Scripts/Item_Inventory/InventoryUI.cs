using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel; 
    public Transform slotContainer; 
    public GameObject slotPrefab; 

    [Header("Envanter Ayarları")]
    // Başlangıçta kaç tane boş slotun görünmesini istediğinizi buradan ayarlayın
    public int initialSlotCount = 20; 

    List<InventorySlot> slots = new List<InventorySlot>();

    void Start()
    {
        // Envanter her değiştiğinde (AddItem, RemoveItem) UpdateUI fonksiyonunu çalıştır
        Inventory.Instance.OnInventoryChanged += UpdateUI;
        Inventory.OnInventoryChanged_Static += UpdateUI;

        // Başlangıçta sabit boş slotları oluştur
        InitializeFixedSlots(initialSlotCount);

        // İlk yüklemede UI'ı güncelle
        UpdateUI();
    }

    // Başlangıçta sadece boş slotları oluşturur. Bu slotlar HER ZAMAN aktif kalacaktır.
    void InitializeFixedSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, slotContainer);
            InventorySlot newSlot = newSlotObj.GetComponent<InventorySlot>();
            
            // Slotu boş olarak temizle ve görünür yap.
            newSlot.ClearSlot(); 
            newSlotObj.SetActive(true); 
            
            slots.Add(newSlot);
        }
    }

    // Envanter panelini açıp kapatan bir fonksiyon
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
        // 1. Adım: Mevcut itemları slotlara yerleştir
        int slotIndex = 0;
        
        // Dictionary'yi döngüye al
        foreach (var itemEntry in Inventory.Instance.items)
        {
            // EĞER mevcut slot sayımız, yerleştirmemiz gereken item sayısından az ise, YENİ SLOT OLUŞTUR
            if (slotIndex >= slots.Count)
            {
                // Dinamik olarak yeni slot oluştur (InitialSlotCount aşıldı)
                GameObject newSlotObj = Instantiate(slotPrefab, slotContainer);
                slots.Add(newSlotObj.GetComponent<InventorySlot>());
            }
            
            // Slotu item bilgisiyle doldur ve görünür yap.
            slots[slotIndex].AddItemToSlot(itemEntry.Key, itemEntry.Value);
            slots[slotIndex].gameObject.SetActive(true); 
            
            slotIndex++;
        }

        // 2. Adım: Kalan slotları temizle.
        // Bu slotlar ya başlangıçta oluşturulan boş slotlar olacak ya da bir item vardı ama silindi.
        for (int j = slotIndex; j < slots.Count; j++)
        {
            slots[j].ClearSlot();
            // Bu slotları GİZLEMİYORUZ, aksine BOŞ slot olarak GÖSTERİYORUZ.
            slots[j].gameObject.SetActive(true); 
        }
    }
}