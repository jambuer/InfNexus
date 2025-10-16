using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // Listeler için bu satır gerekli

public class StatUI : MonoBehaviour
{
    [Header("Stat Text References")]
    public TextMeshProUGUI physicalText;
    public TextMeshProUGUI mentalText;
    public TextMeshProUGUI perceptionText;
    public TextMeshProUGUI spiritualText;
    public TextMeshProUGUI luckText;
    public TextMeshProUGUI socialText;

    [Header("Stat Arttırma Butonları")]
    public Button addPhysicalButton;
    public Button addMentalButton;
    public Button addPerceptionButton;
    public Button addSpiritualButton;
    public Button addLuckButton;
    public Button addSocialButton;

    private List<Button> allStatButtons; // Bütün butonları bir listede tutacağız
    
        // YENİ EKLENEN KISIM
    [Header("Miktar Belirleme Butonları")]
    public Button setAmount1Button;
    public Button setAmount10Button;
    public Button setAmount100Button;
    public Button setAmount1000Button;
    public Button setAmountMaxButton;

    private int spendAmount = 1; // Mevcut harcama miktarını tutar
    private bool spendMax = false; // Maksimum harcama modunda olup olmadığını tutar
    // YENİ EKLENEN KISIM BİTTİ
    void Start()
    {
        // StatManager ve LevelManager'daki event'lere abone ol
        if (StatManager.Instance != null)
        {
            StatManager.Instance.OnStatChanged += OnStatUpdated;
        }
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelUp += UpdateAllButtonsInteractable;
        }

        // Butonları listeye ekle
        allStatButtons = new List<Button>
        {
            addPhysicalButton, addMentalButton, addPerceptionButton,
            addSpiritualButton, addLuckButton, addSocialButton
        };

        // Butonların OnClick olaylarını ayarla
        addPhysicalButton.onClick.AddListener(() => SpendPointOnStat("Physical"));
        addMentalButton.onClick.AddListener(() => SpendPointOnStat("Mental"));
        addPerceptionButton.onClick.AddListener(() => SpendPointOnStat("Perception"));
        addSpiritualButton.onClick.AddListener(() => SpendPointOnStat("Spiritual"));
        addLuckButton.onClick.AddListener(() => SpendPointOnStat("Luck"));
        addSocialButton.onClick.AddListener(() => SpendPointOnStat("Social"));

        // YENİ: Miktar butonlarının OnClick olaylarını ayarla
        setAmount1Button.onClick.AddListener(() => SetSpendAmount(1));
        setAmount10Button.onClick.AddListener(() => SetSpendAmount(10));
        setAmount100Button.onClick.AddListener(() => SetSpendAmount(100));
        setAmount1000Button.onClick.AddListener(() => SetSpendAmount(1000));
        setAmountMaxButton.onClick.AddListener(() => SetSpendAmountToMax());


        // Başlangıçta tüm UI'ı ve butonları güncelle
        UpdateAllStats();
        UpdateAllButtonsInteractable();
    }
    
        void SetSpendAmount(int amount)
    {
        spendAmount = amount;
        spendMax = false;
        Debug.Log($"Harcama miktarı ayarlandı: {amount}");
        // Miktar değiştiğinde butonların durumunu tekrar kontrol et
        UpdateAllButtonsInteractable();
    }

    void SetSpendAmountToMax()
    {
        spendMax = true;
        Debug.Log("Harcama miktarı ayarlandı: MAX");
        // Miktar değiştiğinde butonların durumunu tekrar kontrol et
        UpdateAllButtonsInteractable();
    }

    void OnDestroy()
    {
        // Event aboneliklerini iptal et
        if (StatManager.Instance != null)
        {
            StatManager.Instance.OnStatChanged -= OnStatUpdated;
        }
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelUp -= UpdateAllButtonsInteractable;
        }
    }

    // Bir stat puanı harcamak için çağrılan ana fonksiyon
    void SpendPointOnStat(string statName)
    {
        if (LevelManager.Instance == null || StatManager.Instance == null) return;
        
        int amountToSpend;
        if (spendMax)
        {
            amountToSpend = LevelManager.Instance.unspentStatPoints;
        }
        else
        {
            amountToSpend = spendAmount;
        }
        
        // Eğer harcanacak puan 0 ise hiçbir şey yapma
        if (amountToSpend <= 0) return;
        // LevelManager'daki yeni fonksiyonumuzu çağırıyoruz.
        // Bu fonksiyon hem puanı azaltacak hem de event'i tetikleyecek.
        if (LevelManager.Instance.SpendStatPoint(amountToSpend))
        {
            // Eğer puan başarıyla harcandıysa, StatManager'da ilgili statı artır.
            switch (statName)
            {
                case "Physical": StatManager.Instance.AddPhysical(amountToSpend, true); break;
                case "Mental": StatManager.Instance.AddMental(amountToSpend, true); break;
                case "Perception": StatManager.Instance.AddPerception(amountToSpend, true); break;
                case "Spiritual": StatManager.Instance.AddSpiritual(amountToSpend, true); break;
                case "Luck": StatManager.Instance.AddLuck(amountToSpend, true); break;
                case "Social": StatManager.Instance.AddSocial(amountToSpend, true); break;
            }
        }
    }

    // Stat butonlarının tıklanabilirliğini günceller
void UpdateAllButtonsInteractable()
    {
        if (LevelManager.Instance == null) return;

        int pointsAvailable = LevelManager.Instance.unspentStatPoints;
        
        // Stat arttırma (+) butonlarının durumu
        bool canSpendAny = pointsAvailable > 0;
        foreach (Button btn in allStatButtons)
        {
            if (btn != null) btn.gameObject.SetActive(canSpendAny);
        }

        // YENİ: Miktar butonlarının durumu
        // Yeterli puan yoksa miktar butonlarını pasif yap
        if (setAmount1Button != null) setAmount1Button.interactable = pointsAvailable >= 1;
        if (setAmount10Button != null) setAmount10Button.interactable = pointsAvailable >= 10;
        if (setAmount100Button != null) setAmount100Button.interactable = pointsAvailable >= 100;
        if (setAmount1000Button != null) setAmount1000Button.interactable = pointsAvailable >= 1000;
        if (setAmountMaxButton != null) setAmountMaxButton.interactable = pointsAvailable > 0;
    }
    // Bir stat değiştiğinde UI metnini güncellemek için
    void OnStatUpdated(string statName, double newValue)
    {
        if (statName == "All")
        {
            UpdateAllStats();
        }
        else
        {
            UpdateSingleStat(statName);
        }
        // Stat harcandıktan sonra butonların durumunu tekrar kontrol et
        UpdateAllButtonsInteractable();
    }
    
    // ----- Buradan sonrası StatUI.cs'in eski kodları, değişiklik yok -----

    public void UpdateAllStats()
    {
        if (StatManager.Instance == null) return;
        UpdateStatText(physicalText, StatManager.Instance.physical, StatManager.Instance.physicalBonus);
        UpdateStatText(mentalText, StatManager.Instance.mental, StatManager.Instance.mentalBonus);
        UpdateStatText(perceptionText, StatManager.Instance.perception, StatManager.Instance.perceptionBonus);
        UpdateStatText(spiritualText, StatManager.Instance.spiritual, StatManager.Instance.spiritualBonus);
        UpdateStatText(luckText, StatManager.Instance.luck, StatManager.Instance.luckBonus);
        UpdateStatText(socialText, StatManager.Instance.social, StatManager.Instance.socialBonus);
    }
    
    void UpdateSingleStat(string statName)
    {
        if (StatManager.Instance == null) return;
        switch (statName)
        {
            case "Physical": UpdateStatText(physicalText, StatManager.Instance.physical, StatManager.Instance.physicalBonus); break;
            case "Mental": UpdateStatText(mentalText, StatManager.Instance.mental, StatManager.Instance.mentalBonus); break;
            case "Perception": UpdateStatText(perceptionText, StatManager.Instance.perception, StatManager.Instance.perceptionBonus); break;
            case "Spiritual": UpdateStatText(spiritualText, StatManager.Instance.spiritual, StatManager.Instance.spiritualBonus); break;
            case "Luck": UpdateStatText(luckText, StatManager.Instance.luck, StatManager.Instance.luckBonus); break;
            case "Social": UpdateStatText(socialText, StatManager.Instance.social, StatManager.Instance.socialBonus); break;
        }
    }
    
    void UpdateStatText(TextMeshProUGUI textComponent, double baseValue, double bonusValue)
    {
        if (textComponent == null) return;
        double total = baseValue + bonusValue;
        if (bonusValue > 0)
        {
            string baseStr = FormatNumber(baseValue);
            string bonusStr = FormatNumber(bonusValue);
            textComponent.text = $"{baseStr} <color=#00FF00>(+{bonusStr})</color>";
        }
        else
        {
            textComponent.text = FormatNumber(total);
        }
    }
    
    string FormatNumber(double value)
    {
        if (value < 1000) return value.ToString("F0");
        if (value < 1000000) return (value / 1000).ToString("F1") + "K";
        if (value < 1000000000) return (value / 1000000).ToString("F1") + "M";
        if (value < 1000000000000) return (value / 1000000000).ToString("F1") + "B";
        if (value < 1000000000000000) return (value / 1000000000000).ToString("F1") + "T";
        return value.ToString("E2");
    }
}