using UnityEngine;
using UnityEngine.UI;

public class TabManager : MonoBehaviour
{
    [Header("Tab Buttons")]
    public Button homeButton;
    public Button profileButton;
    public Button nexusButton;
    public Button questButton;
    public Button adventureButton;
    public Button townButton;
    public Button bossButton;
    public Button trainingButton;

    [Header("Tab Contents")]
    public GameObject homeContent;
    public GameObject profileContent;
    public GameObject nexusContent;
    public GameObject questContent;
    public GameObject adventureContent;
    public GameObject townContent;
    public GameObject bossContent;
    public GameObject trainingContent;

    void Start()
    {
        // Butonlara tıklama fonksiyonlarını bağla
        homeButton.onClick.AddListener(() => OpenTab(homeContent));
        profileButton.onClick.AddListener(() => OpenTab(profileContent));
        nexusButton.onClick.AddListener(() => OpenTab(nexusContent));
        questButton.onClick.AddListener(() => OpenTab(questContent));
        adventureButton.onClick.AddListener(() => OpenTab(adventureContent));
        townButton.onClick.AddListener(() => OpenTab(townContent));
        bossButton.onClick.AddListener(() => OpenTab(bossContent));
        trainingButton.onClick.AddListener(() => OpenTab(trainingContent));

        // Başlangıçta HOME sekmesini aç
        OpenTab(homeContent);
    }

    void OpenTab(GameObject tabToOpen)
    {
        // Tüm sekmeleri kapat
        homeContent.SetActive(false);
        profileContent.SetActive(false);
        nexusContent.SetActive(false);
        questContent.SetActive(false);
        adventureContent.SetActive(false);
        townContent.SetActive(false);
        bossContent.SetActive(false);
        trainingContent.SetActive(false);

        // Seçili sekmeyi aç
        tabToOpen.SetActive(true);
    }
}