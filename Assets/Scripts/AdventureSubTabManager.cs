using UnityEngine;
using UnityEngine.UI;

public class AdventureSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button guildButton;
    public Button regionButton;
    public Button raceButton;
    public Button workButton;
    public Button explorerButton;
    public Button cavesButton;
    public Button oceanButton;
    public Button mountainsButton;

    [Header("Sub Tab Contents")]
    public GameObject guildContent;
    public GameObject regionContent;
    public GameObject raceContent;
    public GameObject workContent;
    public GameObject explorerContent;
    public GameObject cavesContent;
    public GameObject oceanContent;
    public GameObject mountainsContent;

    void Start()
    {
        // Tüm içerikleri kapat
        guildContent.SetActive(false);
        regionContent.SetActive(false);
        raceContent.SetActive(false);
        workContent.SetActive(false);
        explorerContent.SetActive(false);
        cavesContent.SetActive(false);
        oceanContent.SetActive(false);
        mountainsContent.SetActive(false);

        // Butonları bağla
        guildButton.onClick.AddListener(() => OpenSubTab(guildContent));
        regionButton.onClick.AddListener(() => OpenSubTab(regionContent));
        raceButton.onClick.AddListener(() => OpenSubTab(raceContent));
        workButton.onClick.AddListener(() => OpenSubTab(workContent));
        explorerButton.onClick.AddListener(() => OpenSubTab(explorerContent));
        cavesButton.onClick.AddListener(() => OpenSubTab(cavesContent));
        oceanButton.onClick.AddListener(() => OpenSubTab(oceanContent));
        mountainsButton.onClick.AddListener(() => OpenSubTab(mountainsContent));

        // İlk sekmeyi aç
        OpenSubTab(guildContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        guildContent.SetActive(false);
        regionContent.SetActive(false);
        raceContent.SetActive(false);
        workContent.SetActive(false);
        explorerContent.SetActive(false);
        cavesContent.SetActive(false);
        oceanContent.SetActive(false);
        mountainsContent.SetActive(false);

        subTabToOpen.SetActive(true);
    }
}