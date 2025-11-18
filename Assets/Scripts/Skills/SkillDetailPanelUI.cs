using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gathering ekranının sağ panelinde, seçilen bir eylemin ilişkili olduğu
/// Yaşam Becerisinin (örn: Odunculuk) detaylarını gösteren UI paneli.
/// </summary>
public class SkillDetailPanelUI : MonoBehaviour
{
    // Bu paneli, GatheringNodeUI'dan kolayca erişilebilir yapmak için Singleton yapıyoruz.
    public static SkillDetailPanelUI Instance { get; private set; }

    [Header("UI Referansları")]
    [SerializeField]
    private GameObject panelContainer; // Panelin tamamını (göstermek/gizlemek için)
    [SerializeField]
    private TextMeshProUGUI skillNameText;
    [SerializeField]
    private TextMeshProUGUI skillDescriptionText;
    [SerializeField]
    private TextMeshProUGUI skillLevelText;
    [SerializeField]
    private Slider skillXPSlider;
    [SerializeField]
    private TextMeshProUGUI skillXPText; // (örn: "150 / 400 XP")
    [SerializeField]
    private TextMeshProUGUI skillBonusText;


    // [YENİ] Bonusları listelemek için (Adım 9'da doldurulabilir)
    // [SerializeField]
    // private TextMeshProUGUI skillBonusText; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Başlangıçta paneli gizle
        HidePanel();
    }

    /// <summary>
    /// Paneli gizler (veya varsayılan duruma getirir).
    /// </summary>
    public void HidePanel()
    {
        if (panelContainer != null)
            panelContainer.SetActive(false);
    }

    /// <summary>
    /// Belirli bir Yaşam Becerisinin (LifeSkill) bilgilerini alıp paneli doldurur.
    /// </summary>
    /// <summary>
    /// Belirli bir Yaşam Becerisinin (LifeSkill) bilgilerini alıp paneli doldurur.
    /// [DEĞİŞTİ] Artık bonus açıklamalarını da gösterir.
    /// </summary>
    public void ShowSkill(LifeSkill skillToShow)
    {
        if (skillToShow == LifeSkill.None)
        {
            HidePanel();
            return;
        }

        var manager = LifeJobsSkillsManager.Instance;
        if (manager == null)
        {
            Debug.LogError("[SkillDetailPanelUI] LifeJobsSkillsManager bulunamadı!");
            return;
        }

        LifeSkillData data = manager.GetSkillData(skillToShow);
        int level = manager.GetSkillLevel(skillToShow);
        double currentXP = manager.GetSkillXP(skillToShow);
        double nextLevelXP = manager.GetXPForNextSkillLevel(skillToShow);
        float progress = manager.GetSkillXPProgress(skillToShow);

        // [YENİ] Adım 12: Bonus açıklamasını al
        string bonusDescription = manager.GetSkillBonusDescription(skillToShow);

        // 1. İsim ve Açıklama (ScriptableObject'tan)
        if (data != null)
        {
            if (skillNameText != null) skillNameText.text = data.displayName;
            if (skillDescriptionText != null) skillDescriptionText.text = data.description;
        }
        else
        {
            if (skillNameText != null) skillNameText.text = skillToShow.ToString();
            if (skillDescriptionText != null) skillDescriptionText.text = "Beceri verisi bulunamadı.";
        }

        // 2. Seviye
        if (skillLevelText != null)
            skillLevelText.text = $"Seviye {level}";

        // 3. XP Barı ve Metni
        if (skillXPSlider != null)
            skillXPSlider.value = progress;

        if (skillXPText != null)
        {
            if (nextLevelXP == double.MaxValue) // Maks seviye
                skillXPText.text = "MAKS SEVİYE";
            else
                skillXPText.text = $"{NumberFormatter.FormatNumber(currentXP)} / {NumberFormatter.FormatNumber(nextLevelXP)} XP";
        }

        // 4. [YENİ] Bonus Metni
        if (skillBonusText != null)
        {
            skillBonusText.text = bonusDescription;
        }

        // 5. Paneli Göster
        if (panelContainer != null)
            panelContainer.SetActive(true);
    }

}