using UnityEngine;
using UnityEngine.UI;

public class HomeSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button newsButton;
    public Button updatesButton;
    public Button pCodeButton;
    public Button thanksButton;

    [Header("Sub Tab Contents")]
    public GameObject newsContent;
    public GameObject updatesContent;
    public GameObject pCodeContent;
    public GameObject thanksContent;

    void Start()
    {
        // Butonlara tıklama fonksiyonlarını bağla
        newsButton.onClick.AddListener(() => OpenSubTab(newsContent));
        updatesButton.onClick.AddListener(() => OpenSubTab(updatesContent));
        pCodeButton.onClick.AddListener(() => OpenSubTab(pCodeContent));
        thanksButton.onClick.AddListener(() => OpenSubTab(thanksContent));

        // Başlangıçta Haberler sekmesini aç
        OpenSubTab(newsContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        // Tüm alt sekmeleri kapat
        newsContent.SetActive(false);
        updatesContent.SetActive(false);
        pCodeContent.SetActive(false);
        thanksContent.SetActive(false);

        // Seçili alt sekmeyi aç
        subTabToOpen.SetActive(true);
    }
}