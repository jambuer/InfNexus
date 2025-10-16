using UnityEngine;
using UnityEngine.UI;

public class ProfileSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button profileButton;
    public Button inventoryButton;
    public Button skillTreeButton;
    public Button dnaButton;
    public Button soulButton;

    [Header("Sub Tab Contents")]
    public GameObject profileContent;
    public GameObject inventoryContent;
    public GameObject skillTreeContent;
    public GameObject dnaContent;
    public GameObject soulContent;

    void Start()
    {
        // Butonlara tıklama fonksiyonlarını bağla
        profileButton.onClick.AddListener(() => OpenSubTab(profileContent));
        inventoryButton.onClick.AddListener(() => OpenSubTab(inventoryContent));
        skillTreeButton.onClick.AddListener(() => OpenSubTab(skillTreeContent));
        dnaButton.onClick.AddListener(() => OpenSubTab(dnaContent));
        soulButton.onClick.AddListener(() => OpenSubTab(soulContent));

        // Başlangıçta Profil sekmesini aç
        OpenSubTab(profileContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        // Tüm alt sekmeleri kapat
        profileContent.SetActive(false);
        inventoryContent.SetActive(false);
        skillTreeContent.SetActive(false);
        dnaContent.SetActive(false);
        soulContent.SetActive(false);

        // Seçili alt sekmeyi aç
        subTabToOpen.SetActive(true);
    }
}