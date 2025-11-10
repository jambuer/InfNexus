using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// "Kuzey", "Güney" gibi bir bölgeyi temsil eden butonun script'i.
/// </summary>
[RequireComponent(typeof(Button))]
public class GatheringRegionButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI regionNameText; // Buton prefabındaki Text objesi

    private GatheringRegion _myRegion;
    private GatheringUIManager _uiManager;
    private Button _button;

    /// <summary>
    /// Bu butonu ilgili bölge verisi ile kurar.
    /// </summary>
    public void Setup(GatheringRegion regionData, GatheringUIManager manager)
    {
        _myRegion = regionData;
        _uiManager = manager;

        if (regionNameText != null)
        {
            regionNameText.text = _myRegion.regionName;
        }

        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Butona tıklandığında UI Yöneticisine haber verir.
    /// </summary>
    private void OnClick()
    {
        if (_uiManager != null && _myRegion != null)
        {
            _uiManager.OnRegionSelected(_myRegion);
        }
    }

    void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}