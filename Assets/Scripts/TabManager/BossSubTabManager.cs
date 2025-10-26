using UnityEngine;
using UnityEngine.UI;

public class BossSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button regionBossButton;
    public Button areaBossButton;
    public Button continentBossButton;
    public Button worldBossButton;
    public Button nexusBossButton;

    [Header("Sub Tab Contents")]
    public GameObject regionBossContent;
    public GameObject areaBossContent;
    public GameObject continentBossContent;
    public GameObject worldBossContent;
    public GameObject nexusBossContent;

    void Start()
    {
        // Tüm içerikleri kapat
        regionBossContent.SetActive(false);
        areaBossContent.SetActive(false);
        continentBossContent.SetActive(false);
        worldBossContent.SetActive(false);
        nexusBossContent.SetActive(false);

        // Butonları bağla
        regionBossButton.onClick.AddListener(() => OpenSubTab(regionBossContent));
        areaBossButton.onClick.AddListener(() => OpenSubTab(areaBossContent));
        continentBossButton.onClick.AddListener(() => OpenSubTab(continentBossContent));
        worldBossButton.onClick.AddListener(() => OpenSubTab(worldBossContent));
        nexusBossButton.onClick.AddListener(() => OpenSubTab(nexusBossContent));

        // İlk sekmeyi aç
        OpenSubTab(regionBossContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        regionBossContent.SetActive(false);
        areaBossContent.SetActive(false);
        continentBossContent.SetActive(false);
        worldBossContent.SetActive(false);
        nexusBossContent.SetActive(false);

        subTabToOpen.SetActive(true);
    }
}