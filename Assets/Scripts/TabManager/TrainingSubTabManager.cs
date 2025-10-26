using UnityEngine;
using UnityEngine.UI;

public class TrainingSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button physicalButton;
    public Button mentalButton;
    public Button spiritualButton;
    public Button citizenButton;
    public Button hereditaryButton;
    public Button mysticButton;

    [Header("Sub Tab Contents")]
    public GameObject physicalContent;
    public GameObject mentalContent;
    public GameObject spiritualContent;
    public GameObject citizenContent;
    public GameObject hereditaryContent;
    public GameObject mysticContent;

    void Start()
    {
        // Tüm içerikleri kapat
        physicalContent.SetActive(false);
        mentalContent.SetActive(false);
        spiritualContent.SetActive(false);
        citizenContent.SetActive(false);
        hereditaryContent.SetActive(false);
        mysticContent.SetActive(false);

        // Butonları bağla
        physicalButton.onClick.AddListener(() => OpenSubTab(physicalContent));
        mentalButton.onClick.AddListener(() => OpenSubTab(mentalContent));
        spiritualButton.onClick.AddListener(() => OpenSubTab(spiritualContent));
        citizenButton.onClick.AddListener(() => OpenSubTab(citizenContent));
        hereditaryButton.onClick.AddListener(() => OpenSubTab(hereditaryContent));
        mysticButton.onClick.AddListener(() => OpenSubTab(mysticContent));

        // İlk sekmeyi aç
        OpenSubTab(physicalContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        physicalContent.SetActive(false);
        mentalContent.SetActive(false);
        spiritualContent.SetActive(false);
        citizenContent.SetActive(false);
        hereditaryContent.SetActive(false);
        mysticContent.SetActive(false);

        subTabToOpen.SetActive(true);
    }
}