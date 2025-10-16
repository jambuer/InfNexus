using UnityEngine;
using UnityEngine.UI;

public class TownSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button cityButton;
    public Button gatheringButton;
    public Button craftingButton;
    public Button emporiumButton;
    public Button peopleButton;
    public Button areasButton;
    public Button defenseButton;

    [Header("Sub Tab Contents")]
    public GameObject cityContent;
    public GameObject gatheringContent;
    public GameObject craftingContent;
    public GameObject emporiumContent;
    public GameObject peopleContent;
    public GameObject areasContent;
    public GameObject defenseContent;

    void Start()
    {
        // Tüm içerikleri kapat
        cityContent.SetActive(false);
        gatheringContent.SetActive(false);
        craftingContent.SetActive(false);
        emporiumContent.SetActive(false);
        peopleContent.SetActive(false);
        areasContent.SetActive(false);
        defenseContent.SetActive(false);

        // Butonları bağla
        cityButton.onClick.AddListener(() => OpenSubTab(cityContent));
        gatheringButton.onClick.AddListener(() => OpenSubTab(gatheringContent));
        craftingButton.onClick.AddListener(() => OpenSubTab(craftingContent));
        emporiumButton.onClick.AddListener(() => OpenSubTab(emporiumContent));
        peopleButton.onClick.AddListener(() => OpenSubTab(peopleContent));
        areasButton.onClick.AddListener(() => OpenSubTab(areasContent));
        defenseButton.onClick.AddListener(() => OpenSubTab(defenseContent));

        // İlk sekmeyi aç
        OpenSubTab(cityContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        cityContent.SetActive(false);
        gatheringContent.SetActive(false);
        craftingContent.SetActive(false);
        emporiumContent.SetActive(false);
        peopleContent.SetActive(false);
        areasContent.SetActive(false);
        defenseContent.SetActive(false);

        subTabToOpen.SetActive(true);
    }
}