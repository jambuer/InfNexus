using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    void Awake()
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

    [Header("Health")]
    public Image healthFill;
    public TextMeshProUGUI healthText;
    public float currentHealth = 100f;
    public float maxHealth = 100f;

    [Header("Energy")]
    public Image energyFill;
    public TextMeshProUGUI energyText;
    public float currentEnergy = 100f;
    public float maxEnergy = 100f;

    [Header("Mana")]
    public Image manaFill;
    public TextMeshProUGUI manaText;
    public float currentMana = 100f;
    public float maxMana = 100f;

    void Start()
    {
        // Başlangıç değerlerini ayarla
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentMana = maxMana;
        
        UpdateAllBars();

        // StatCalculator'dan gelen anonsları dinlemeye başla
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated += UpdateMaxStats;
        }
    }
    
    void OnDestroy()
    {
        // Obje yok olduğunda anons dinlemeyi bırak
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated -= UpdateMaxStats;
        }
    }

    // --- YENİ EKLENEN KISIM: SÜREKLİ YENİLENME ---
    void Update()
    {
        // Eğer StatCalculator hazır değilse hiçbir şey yapma
        if (StatCalculator.Instance == null) return;

        // StatCalculator'dan saniye başına yenilenme miktarlarını al
        float healthRegenRate = (float)StatCalculator.Instance.currentStats.HealthRecovery;
        float energyRegenRate = (float)StatCalculator.Instance.currentStats.EnergyRecovery;
        float manaRegenRate = (float)StatCalculator.Instance.currentStats.ManaRecovery;

        // Her frame'de (milisaniye) ne kadar yenileneceğini hesapla (Time.deltaTime kullanarak)
        // Mevcut değere ekle ve maksimum değeri geçmediğinden emin ol (Mathf.Clamp ile)
        currentHealth = Mathf.Clamp(currentHealth + healthRegenRate * Time.deltaTime, 0, maxHealth);
        currentEnergy = Mathf.Clamp(currentEnergy + energyRegenRate * Time.deltaTime, 0, maxEnergy);
        currentMana = Mathf.Clamp(currentMana + manaRegenRate * Time.deltaTime, 0, maxMana);

        // Barları her frame'de güncelleyerek akıcı bir dolum efekti sağla
        UpdateAllBars();
    }
    // --- YENİLENME KISMI BİTTİ ---

    private void UpdateMaxStats()
    {
        if (StatCalculator.Instance == null) return;

        // StatCalculator'dan hesaplanmış yeni maksimum değerleri al
        maxHealth = (float)StatCalculator.Instance.currentStats.MaxHealth;
        maxEnergy = (float)StatCalculator.Instance.currentStats.MaxEnergy;
        maxMana = (float)StatCalculator.Instance.currentStats.MaxMana;

        // Mevcut değerin, yeni maksimum değerden büyük olmamasını sağla (AMA DOLDURMA)
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        currentMana = Mathf.Min(currentMana, maxMana);
        
        UpdateAllBars();
    }

    public void UpdateAllBars()
    {
        UpdateBar(healthFill, healthText, currentHealth, maxHealth);
        UpdateBar(energyFill, energyText, currentEnergy, maxEnergy);
        UpdateBar(manaFill, manaText, currentMana, maxMana);
    }

    void UpdateBar(Image fillImage, TextMeshProUGUI text, float current, float max)
    {
        // Bu fonksiyon aynı kalıyor, değişiklik yok
        if (fillImage == null || text == null) return;
        float percentage = Mathf.Clamp01(current / max);
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        RectTransform parentRect = fillRect.parent.GetComponent<RectTransform>();
        if (parentRect == null) return;
        float parentWidth = parentRect.rect.width;
        float originalLeft = 10f;
        float originalRight = 12f;
        float rightOffset = -((parentWidth - originalLeft - originalRight) * (1f - percentage) + originalRight);
        fillRect.offsetMin = new Vector2(originalLeft, fillRect.offsetMin.y);
        fillRect.offsetMax = new Vector2(rightOffset, fillRect.offsetMax.y);
        text.text = Mathf.RoundToInt(current) + " / " + Mathf.RoundToInt(max);
    }
    
    // Modify fonksiyonları aynı kalıyor, değişiklik yok
    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    public void ModifyEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
    }

    public void ModifyMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
    }
}