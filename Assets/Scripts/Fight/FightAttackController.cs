using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // Button metinleri için (opsiyonel)

// Bu script, 6 kaynak harcama butonunu ve 5 maliyet çarpanı butonunu yönetir.
public class FightAttackController : MonoBehaviour
{
    [Header("Kaynak Harcama Butonları")]
    [Tooltip("Sırasıyla 1, 5, 10, 25, 50, 100 harcama butonları.")]
    public List<Button> resourceAmountButtons;

    [Header("Maliyet Çarpanı Butonları")]
    [Tooltip("Sırasıyla 1x, 10x, 100x, 1000x, 10000x maliyet çarpanı butonları.")]
    public List<Button> costMultiplierButtons;

    [Header("Görsel Ayarlar")]
    public Color selectedColor = Color.yellow;
    public Color deselectedColor = Color.white;

    // --- Değer Haritaları (Inspector'dan değiştirilebilir yapılabilir) ---
    private readonly int[] resourceCosts = { 1, 5, 10, 25, 50, 100 };
    private readonly double[] damageMultipliers = { 1.0, 1.5, 1.75, 2.0, 2.5, 3.25 };
    private readonly int[] costMultipliers = { 1, 10, 100, 1000, 10000 };
    // -----------------------------------------------------------------

    private int selectedResourceIndex = 0;
    private int selectedCostMultiplierIndex = 0;

    void Start()
    {
        // 1. Kaynak Harcama Butonlarına Listener Ata
        if (resourceAmountButtons != null && resourceAmountButtons.Count == resourceCosts.Length)
        {
            for (int i = 0; i < resourceAmountButtons.Count; i++)
            {
                int index = i; // Lambda expression için index'i yakala
                resourceAmountButtons[i].onClick.AddListener(() => SelectResourceAmount(index));
            }
        } else { Debug.LogError("FightAttackController: Kaynak harcama butonları (6 adet) tam olarak atanmamış!"); }

        // 2. Maliyet Çarpanı Butonlarına Listener Ata
        if (costMultiplierButtons != null && costMultiplierButtons.Count == costMultipliers.Length)
        {
            for (int i = 0; i < costMultiplierButtons.Count; i++)
            {
                int index = i;
                costMultiplierButtons[i].onClick.AddListener(() => SelectCostMultiplier(index));
            }
        } else { Debug.LogError("FightAttackController: Maliyet çarpanı butonları (5 adet) tam olarak atanmamış!"); }

        // Başlangıç görsellerini ayarla
        SelectResourceAmount(0);
        SelectCostMultiplier(0);
    }

    /// <summary>
    /// Kaynak harcama miktarını seçer (1, 5, 10...) VE SALDIRIYI BAŞLATIR.
    /// </summary>
    public void SelectResourceAmount(int index)
    {
        if (index < 0 || index >= resourceAmountButtons.Count) return;

        selectedResourceIndex = index;
        UpdateVisuals(resourceAmountButtons, index);

        // Seçim yapıldığında FightManager'daki ana saldırı fonksiyonunu tetikle
        if (FightManager.Instance != null)
        {
            // FightManager.Instance.CancelAutoAttack(); // Bu satırı kaldır
            FightManager.Instance.OnResourceAttackPressed(index); // YENİ FONKSİYONU ÇAĞIR
        }
    }

    /// <summary>
    /// Maliyet çarpanını seçer (1x, 10x...)
    /// </summary>
    public void SelectCostMultiplier(int index)
    {
        if (index < 0 || index >= costMultiplierButtons.Count) return;

        selectedCostMultiplierIndex = index;
        UpdateVisuals(costMultiplierButtons, index);
        
        // Maliyet çarpanı değiştiğinde otomatik saldırı iptal OLMAMALI.
        // Otomatik saldırı, o an seçili olan çarpanı kullanmalı.
    }

    /// <summary>
    /// Buton listesindeki seçili elemanın görselini günceller.
    /// </summary>
    private void UpdateVisuals(List<Button> buttonList, int selectedIndex)
    {
        for (int i = 0; i < buttonList.Count; i++)
        {
            if (buttonList[i] != null)
            {
                // Örnek: Seçili olanı renklendir
                var colors = buttonList[i].colors;
                colors.colorMultiplier = (i == selectedIndex) ? 1.0f : 0.7f; // Veya Image rengini değiştir
                buttonList[i].colors = colors;

                // Veya Image component'inin rengini değiştir
                // Image img = buttonList[i].GetComponent<Image>();
                // if (img != null) img.color = (i == selectedIndex) ? selectedColor : deselectedColor;
            }
        }
    }

    // --- FightManager'ın Değerleri Alması İçin Public Fonksiyonlar ---

    public int GetSelectedResourceCost()
    {
        return resourceCosts[selectedResourceIndex];
    }

    public double GetSelectedDamageMultiplier()
    {
        return damageMultipliers[selectedResourceIndex];
    }

    public int GetSelectedCostMultiplier()
    {
        return costMultipliers[selectedCostMultiplierIndex];
    }
}