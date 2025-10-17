using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action event'i için bu satır gereklidir

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

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

    // QuestItemUI'ın kaynak değişikliklerini dinleyebilmesi için event
    public event Action OnValuesChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
        currentMana = maxMana;
        
        UpdateAllBars();

        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated += UpdateMaxStats;
        }
    }
    
    void OnDestroy()
    {
        if (StatCalculator.Instance != null)
        {
            StatCalculator.Instance.OnStatsRecalculated -= UpdateMaxStats;
        }
    }

    void Update()
    {
        if (StatCalculator.Instance == null) return;

        float healthRegenRate = (float)StatCalculator.Instance.currentStats.HealthRecovery;
        float energyRegenRate = (float)StatCalculator.Instance.currentStats.EnergyRecovery;
        float manaRegenRate = (float)StatCalculator.Instance.currentStats.ManaRecovery;

        float oldHealth = currentHealth;
        float oldEnergy = currentEnergy;
        float oldMana = currentMana;

        currentHealth = Mathf.Clamp(currentHealth + healthRegenRate * Time.deltaTime, 0, maxHealth);
        currentEnergy = Mathf.Clamp(currentEnergy + energyRegenRate * Time.deltaTime, 0, maxEnergy);
        currentMana = Mathf.Clamp(currentMana + manaRegenRate * Time.deltaTime, 0, maxMana);

        if (oldHealth != currentHealth || oldEnergy != currentEnergy || oldMana != currentMana)
        {
            UpdateAllBars();
            OnValuesChanged?.Invoke(); // Değerler değiştiyse haber ver
        }
    }

    private void UpdateMaxStats()
    {
        if (StatCalculator.Instance == null) return;

        maxHealth = (float)StatCalculator.Instance.currentStats.MaxHealth;
        maxEnergy = (float)StatCalculator.Instance.currentStats.MaxEnergy;
        maxMana = (float)StatCalculator.Instance.currentStats.MaxMana;

        currentHealth = Mathf.Min(currentHealth, maxHealth);
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
        currentMana = Mathf.Min(currentMana, maxMana);
        
        UpdateAllBars();
        OnValuesChanged?.Invoke(); // Max statlar değiştiğinde de haber ver
    }

    public void UpdateAllBars()
    {
        UpdateBar(healthFill, healthText, currentHealth, maxHealth);
        UpdateBar(energyFill, energyText, currentEnergy, maxEnergy);
        UpdateBar(manaFill, manaText, currentMana, maxMana);
    }

    void UpdateBar(Image fillImage, TextMeshProUGUI text, float current, float max)
    {
        if (fillImage == null || text == null) return;
        
        float percentage = (max > 0) ? Mathf.Clamp01(current / max) : 0;
        
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
    
    public void ModifyHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateAllBars();
        OnValuesChanged?.Invoke();
    }

    public void ModifyEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
        UpdateAllBars();
        OnValuesChanged?.Invoke();
    }

    public void ModifyMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        UpdateAllBars();
        OnValuesChanged?.Invoke();
    }
}