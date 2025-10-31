using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // TimeSpan için eklendi
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;


// Bu script, FightManager tarafından Instantiate edilecek DÜZEN prefablarının üzerinde olacak.
public class FightScreenUI : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyPrimaryTagText;
    public TextMeshProUGUI enemySecondaryTagText;
    public Image enemyImage;
    public Image backgroundImage; // Arka plan görseli
    public Slider enemyHealthSlider;
    public TextMeshProUGUI enemyHealthText;
    public TMP_Dropdown difficultyDropdown; // Zorluk seçimi
    public TextMeshProUGUI rewardsText; // Ödül gösterimi
    public TextMeshProUGUI respawnTimerText; // Tekrar doğma süresi

    private EnemyData _enemyData;
    private RaidDifficultyManager.Difficulty _selectedDifficulty; // Bu UI'ın yönettiği zorluk

    /// <summary>
    /// Düşman bilgilerini ve başlangıç statlarını UI'a yükler.
    /// FightManager tarafından çağrılır.
    /// </summary>
    public void Setup(EnemyData enemyData, EnemyData.CombatStats initialStats)
    {
        _enemyData = enemyData;

        if (_enemyData == null)
        {
            Debug.LogError("FightScreenUI.Setup: EnemyData null!", this);
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // Metinleri Doldur
        if (enemyNameText != null) enemyNameText.text = _enemyData.enemyName;
        if (enemyPrimaryTagText != null) enemyPrimaryTagText.text = _enemyData.primaryTag.ToString();
        if (enemySecondaryTagText != null) enemySecondaryTagText.text = _enemyData.secondaryTag.ToString();

        if (enemyImage != null)
        {
            if (_enemyData.enemySprite != null)
            {
                enemyImage.sprite = _enemyData.enemySprite; // Doğrudan atama
                enemyImage.color = Color.white; // Görünür olması için rengi beyaza ayarla
            }
            else
            {
                enemyImage.sprite = null; // Sprite yoksa boşalt
                enemyImage.color = Color.magenta; // Hata/placeholder rengi
                Debug.LogWarning($"Düşman '{_enemyData.enemyName}' için 'enemySprite' atanmamış.");
            }
        }

        if (backgroundImage != null)
        {
            if (_enemyData.backgroundSprite != null)
            {
                backgroundImage.sprite = _enemyData.backgroundSprite; // Doğrudan atama
                backgroundImage.color = Color.white;
            }
            else
            {
                backgroundImage.sprite = null; // Sprite yoksa boşalt
                backgroundImage.color = Color.grey; // Varsayılan/placeholder
                Debug.LogWarning($"Düşman '{_enemyData.enemyName}' için 'backgroundSprite' atanmamış.");
            }
        }


        // Ödül Metni
        if (rewardsText != null) rewardsText.text = FormatRewardsText(_enemyData);

        // Zorluk Dropdown
        if (difficultyDropdown != null)
        {
            PopulateDifficultyDropdown();
            // FightManager'dan gelen zorluğu ayarla (eğer farklıysa)
            RaidDifficultyManager.Difficulty fightDifficulty = FightManager.Instance.GetCurrentDifficulty(); // FightManager'a bu fonksiyonu ekleyeceğiz
            difficultyDropdown.SetValueWithoutNotify((int)fightDifficulty);
            _selectedDifficulty = fightDifficulty;
            difficultyDropdown.onValueChanged.RemoveAllListeners(); // Öncekileri temizle (önemli)
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChangedInFight); // Listener'ı ekle

            difficultyDropdown.interactable = false;// Savaş sırasında değiştirilemesin
        }

        // Respawn Timer
        if (respawnTimerText != null) respawnTimerText.gameObject.SetActive(false);


        // Can Barını Başlat
        UpdateHealth(initialStats.MaxHealth, initialStats.MaxHealth);
    }
    void OnDifficultyChangedInFight(int index)
    {
         _selectedDifficulty = (RaidDifficultyManager.Difficulty)index;
         // Uyarıyı kaldırabilir veya normal log yapabiliriz:
         Debug.Log($"Düşman için yeni zorluk seçildi: {_selectedDifficulty}");
    }

    /// <summary>
    /// Zorluk seçimi dropdown'ını tıklanabilir hale getirir.
    /// (Genellikle düşman yenildikten sonra çağrılır)
    /// </summary>
    public void EnableDifficultyDropdown()
    {
        if (difficultyDropdown != null)
        {
            difficultyDropdown.interactable = true;
            Debug.Log("Zorluk Dropdown'ı aktif edildi.");
        }
    }


    /// <summary>
    /// Düşmanın can barını ve metnini günceller. FightManager tarafından çağrılır.
    /// </summary>
    public void UpdateHealth(double currentHealth, double maxHealth)
    {
         if (enemyHealthSlider != null)
        {
            // Slider logaritmik vb. değilse, devasa sayılarla düzgün çalışmaz.
            // Şimdilik doğrusal (linear) bırakıyoruz.
            if (maxHealth > 0)
            {
                 // Slider'a hala tam değerleri veriyoruz (görsel temsil)
                 enemyHealthSlider.maxValue = (float)maxHealth;
                 enemyHealthSlider.value = (float)Math.Max(0, currentHealth);
            } else {
                 enemyHealthSlider.maxValue = 1;
                 enemyHealthSlider.value = 0;
            }
        }
        if (enemyHealthText != null)
        {
            // Metni formatlıyoruz
            string currentHealthFormatted = NumberFormatter.FormatNumber(Math.Max(0, currentHealth)); // FORMATLAMA VAR MI?
            string maxHealthFormatted = NumberFormatter.FormatNumber(maxHealth); // FORMATLAMA VAR MI?
            
            enemyHealthText.text = $"{currentHealthFormatted} / {maxHealthFormatted}"; // FORMATLANDI
        }
    }

     // --- Helper Fonksiyonlar (RaidEnemyUI'dan kopyalandı) ---

     void PopulateDifficultyDropdown()
    {
        if(difficultyDropdown == null) return;
        difficultyDropdown.ClearOptions();
        List<string> difficultyNames = System.Enum.GetNames(typeof(RaidDifficultyManager.Difficulty)).ToList();
        difficultyDropdown.AddOptions(difficultyNames);
    }

     string FormatRewardsText(EnemyData enemyData)
    {
        if (enemyData == null) return "Ödüller: Bilgi Yok";
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>Olası Ödüller:</b>");
        if (enemyData.experienceReward > 0) sb.AppendLine($"- {enemyData.experienceReward:F0} XP");
        if (enemyData.goldRewardTiers != null && enemyData.goldRewardTiers.Count > 0) { /*...*/ } // İçerik aynı
        if (enemyData.nexusCoinRewardTiers != null && enemyData.nexusCoinRewardTiers.Count > 0) { /*...*/ } // İçerik aynı
        if (enemyData.itemDrops != null && enemyData.itemDrops.Count > 0) { /*...*/ } // İçerik aynı
        if (sb.Length <= "<b>Olası Ödüller:</b>\n".Length) sb.AppendLine("- Yok");
        return sb.ToString().TrimEnd();
    }

     // Respawn timer gösterme/güncelleme (FightManager yerine bu script de yapabilir)
     public void ShowRespawnTimer(float duration)
     {
         if (respawnTimerText != null)
         {
             respawnTimerText.gameObject.SetActive(true);
             StartCoroutine(RespawnTimerCoroutine(duration));
         }
     }

    IEnumerator RespawnTimerCoroutine(float duration)
    {
        float timer = duration;
        while (timer > 0)
        {
            if (respawnTimerText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(timer));
                respawnTimerText.text = $"Doğma Süresi: {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            yield return null;
            timer -= Time.deltaTime;
        }
        if (respawnTimerText != null) respawnTimerText.gameObject.SetActive(false);
        // Süre dolduğunda FightManager'daki respawn butonunu aktif etmesi için bir event tetiklenebilir.
        // FightManager.Instance?.OnRespawnTimerEnd(); // Örneğin
    }


    // FightManager'a mevcut seçili zorluğu sorması için bir yol
    public RaidDifficultyManager.Difficulty GetSelectedDifficulty()
    {
        return _selectedDifficulty;
    }

    

   
     
}