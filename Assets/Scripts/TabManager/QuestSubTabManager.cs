using UnityEngine;
using UnityEngine.UI;

public class QuestSubTabManager : MonoBehaviour
{
    [Header("Sub Tab Buttons")]
    public Button questsButton;
    public Button completedQuestsButton;
    public Button alliedQuestButton;
    public Button specialButton;
    public Button challengeButton;

    [Header("Sub Tab Contents")]
    public GameObject questsContent;
    public GameObject completedQuestsContent;
    public GameObject alliedQuestContent;
    public GameObject specialContent;
    public GameObject challengeContent;

    void Start()
    {
        // Önce tüm içerikleri kapat
        questsContent.SetActive(false);
        completedQuestsContent.SetActive(false);
        alliedQuestContent.SetActive(false);
        specialContent.SetActive(false);
        challengeContent.SetActive(false);

        // Butonları bağla
        questsButton.onClick.AddListener(() => OpenSubTab(questsContent));
        completedQuestsButton.onClick.AddListener(() => OpenSubTab(completedQuestsContent));
        alliedQuestButton.onClick.AddListener(() => OpenSubTab(alliedQuestContent));
        specialButton.onClick.AddListener(() => OpenSubTab(specialContent));
        challengeButton.onClick.AddListener(() => OpenSubTab(challengeContent));

        // İlk sekmeyi aç
        OpenSubTab(questsContent);
    }

    void OpenSubTab(GameObject subTabToOpen)
    {
        questsContent.SetActive(false);
        completedQuestsContent.SetActive(false);
        alliedQuestContent.SetActive(false);
        specialContent.SetActive(false);
        challengeContent.SetActive(false);

        subTabToOpen.SetActive(true);
    }
}