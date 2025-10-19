using UnityEngine;

public class TownYanPanel : MonoBehaviour
{
    // Inspector'dan yandan açılacak olan paneli buraya sürükle
    public GameObject townQuestPanel;

    void Start()
    {
        // Oyun başladığında panelin kapalı olduğundan emin ol
        if (townQuestPanel != null)
        {
            townQuestPanel.SetActive(false);
        }
    }

    // Bu fonksiyonu, panelin açılmasını tetikleyecek olan Buton'un
    // OnClick() olayına atayacağız.
    public void ToggleTownPanel()
    {
        if (townQuestPanel != null)
        {
            // Panelin mevcut durumunun tersini yap (açıksa kapat, kapalıysa aç)
            bool isActive = townQuestPanel.activeSelf;
            townQuestPanel.SetActive(!isActive);
        }
    }
}