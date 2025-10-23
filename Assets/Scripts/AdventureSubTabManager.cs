using UnityEngine;
using UnityEngine.UI;

public class AdventureSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]

    public Button explorerButton;
    public Button raidButton;
    public Button raceButton;
    public Button regionButton;
    public Button workButton;
    public Button cavesButton;
    public Button oceanButton;
    public Button mountainsButton;

    [Header("Sub Tab Contents")]

    public GameObject explorerContent;
    public GameObject raidContent;
    public GameObject raceContent;
    public GameObject regionContent;
    public GameObject workContent;
    public GameObject cavesContent;
    public GameObject oceanContent;
    public GameObject mountainsContent;

    void Start()
    {
        // Tüm içerikleri kapat
        explorerContent.SetActive(false);
        raidContent.SetActive(false);
        raceContent.SetActive(false);
        regionContent.SetActive(false);
        workContent.SetActive(false);
        cavesContent.SetActive(false);
        oceanContent.SetActive(false);
        mountainsContent.SetActive(false);

        // Butonları bağla
        explorerButton.onClick.AddListener(() => OpenSubTab(explorerContent));
        raidButton.onClick.AddListener(() => OpenSubTab(raidContent));
        raceButton.onClick.AddListener(() => OpenSubTab(raceContent));
        regionButton.onClick.AddListener(() => OpenSubTab(regionContent));
        workButton.onClick.AddListener(() => OpenSubTab(workContent));
        cavesButton.onClick.AddListener(() => OpenSubTab(cavesContent));
        oceanButton.onClick.AddListener(() => OpenSubTab(oceanContent));
        mountainsButton.onClick.AddListener(() => OpenSubTab(mountainsContent));

        // İlk sekmeyi aç
        OpenSubTab(explorerContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        explorerContent.SetActive(false);
        raidContent.SetActive(false);
        raceContent.SetActive(false);
        regionContent.SetActive(false);
        workContent.SetActive(false);
        cavesContent.SetActive(false);
        oceanContent.SetActive(false);
        mountainsContent.SetActive(false);

        subTabToOpen.SetActive(true);
    }
}