using UnityEngine;
using TMPro;

/// <summary>
/// UI butonlarından GameDataManager'daki fonksiyonları çağırmak için yardımcı script.
/// Çoklu kayıt sistemini destekler.
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    [Tooltip("İşlem yapılacak mevcut slot numarası.")]
    public int currentSlot = 1;

    [Tooltip("(Opsiyonel) Mevcut slot numarasını gösteren bir metin.")]
    public TextMeshProUGUI slotDisplayText;

    void Start()
    {
        UpdateSlotDisplay();
    }

    /// <summary>
    /// İşlem yapılacak slot numarasını ayarlar.
    /// Bunu UI'daki slot seçme butonlarına bağlayabilirsiniz.
    /// </summary>
    public void SetCurrentSlot(int slotNumber)
    {
        if (slotNumber > 0 && slotNumber <= GameDataManager.MaxSaveSlots)
        {
            currentSlot = slotNumber;
            UpdateSlotDisplay();
            Debug.Log($"Save/Load slot changed to: {currentSlot}");
        }
    }

    private void UpdateSlotDisplay()
    {
        if (slotDisplayText != null)
        {
            slotDisplayText.text = $"SLOT {currentSlot}";
        }
    }

    /// <summary>
    /// Mevcut seçili slota oyunu kaydeder.
    /// </summary>
    public void OnSaveButton()
    {
        GameDataManager.Instance?.SaveGame(currentSlot);
    }

    /// <summary>
    /// Mevcut seçili slottan oyunu yükler.
    /// </summary>
    public void OnLoadButton()
    {
        GameDataManager.Instance?.LoadGame(currentSlot);
    }

    /// <summary>
    /// Oyunu yeniden başlatır ve TÜM slotlardaki kayıtları siler.
    /// </summary>
    public void OnRestartButton()
    {
        Debug.LogWarning("Restarting game! All data will be lost.");
        GameDataManager.Instance?.RestartGame();
    }
}
