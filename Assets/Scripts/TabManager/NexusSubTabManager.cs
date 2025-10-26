using UnityEngine;
using UnityEngine.UI;

public class NexusSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button universeButton;
    public Button uShopButton;
    public Button rebornButton;
    public Button battlefieldButton;
    public Button alliedButton;

    [Header("Sub Tab Contents")]
    public GameObject universeContent;
    public GameObject uShopContent;
    public GameObject rebornContent;
    public GameObject battlefieldContent;
    public GameObject alliedContent;

    void Start()
    {
        universeButton.onClick.AddListener(() => OpenSubTab(universeContent));
        uShopButton.onClick.AddListener(() => OpenSubTab(uShopContent));
        rebornButton.onClick.AddListener(() => OpenSubTab(rebornContent));
        battlefieldButton.onClick.AddListener(() => OpenSubTab(battlefieldContent));
        alliedButton.onClick.AddListener(() => OpenSubTab(alliedContent));

        OpenSubTab(universeContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        universeContent.SetActive(false);
        uShopContent.SetActive(false);
        rebornContent.SetActive(false);
        battlefieldContent.SetActive(false);
        alliedContent.SetActive(false);

        universeButton.onClick.AddListener(() => OpenSubTab(universeContent));

        subTabToOpen.SetActive(true);
    }
}

        