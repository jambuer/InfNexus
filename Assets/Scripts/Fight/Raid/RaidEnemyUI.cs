using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System; // TimeSpan ve Enum.GetNames için bu satır eklendi

// Bu script'i Scripts/Fight/UI/ klasörüne kaydedebilirsin.
public class RaidEnemyUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [Tooltip("Düşmanın görselini gösterecek Image.")]
    public Image enemyImage;
    [Tooltip("Düşmanın adını gösterecek TextMeshPro.")]
    public TextMeshProUGUI enemyNameText;
    [Tooltip("Düşmanın birincil etiketini gösterecek TextMeshPro.")]
    public TextMeshProUGUI primaryTagText;
    [Tooltip("Düşmanın ikincil etiketini gösterecek TextMeshPro.")]
    public TextMeshProUGUI secondaryTagText;
    [Tooltip("Zorluk seviyesini seçmek için Dropdown.")]
    public TMP_Dropdown difficultyDropdown;
    [Tooltip("Savaşı başlatmak için kullanılacak Button.")]
    public Button attackButton;
    [Tooltip("Saldırı butonunun metnini (örn: 'Saldır', 'Tekrar Saldır') gösterecek TextMeshPro.")]
    public TextMeshProUGUI attackButtonText;
    [Tooltip("Düşmanın olası ödüllerini gösterecek TextMeshPro.")]
    public TextMeshProUGUI rewardsText;
    [Tooltip("Düşman yenildikten sonra tekrar saldırı için kalan süreyi gösterecek TextMeshPro.")]
    public TextMeshProUGUI respawnTimerText; // Tekrar Saldır butonu yerine gösterilebilir

    [Header("Renk Ayarları")]
    public Color canAttackColor = Color.green; // Saldırı butonu aktif rengi
    public Color cannotAttackColor = Color.gray; // Saldırı butonu pasif rengi

private string _enemyID; // Savaş başlatmak için sadece ID'yi saklıyoruz
    private EnemyData _currentData; // UI'ı doldurmak için gelen datayı saklıyoruz
    private RaidDifficultyManager.Difficulty _selectedDifficulty = RaidDifficultyManager.Difficulty.Easy;
    private bool _isOnRespawnCooldown = false;
    private float _remainingRespawnTime = 0f;

    void Awake()
    {
        if (difficultyDropdown != null)
        {
            PopulateDifficultyDropdown();
            difficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
        }
        else
        {
            Debug.LogError($"RaidEnemyUI ({gameObject.name}): Difficulty Dropdown atanmamış!", this);
        }

        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonClicked);
            if (attackButtonText == null)
            {
                attackButtonText = attackButton.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        else
        {
            Debug.LogError($"RaidEnemyUI ({gameObject.name}): Attack Button atanmamış!", this);
        }

        if (respawnTimerText != null)
        {
            respawnTimerText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Bu UI elemanını belirtilen düşman verisiyle kurar.
    /// </summary>
    public void Setup(EnemyData data)
    {
        _currentData = data; // Gelen datayı class seviyesindeki değişkene ata

        if (_currentData == null)
        {
            Debug.LogError($"RaidEnemyUI.Setup: EnemyData null!", this);
            gameObject.SetActive(false);
            return;
        }

        _enemyID = _currentData.enemyID; // ID'yi de al

        gameObject.SetActive(true);

        // UI Elemanlarını Doldur (Artık _currentData kullanılır)
        if (enemyNameText != null) enemyNameText.text = _currentData.enemyName;
        if (primaryTagText != null) primaryTagText.text = _currentData.primaryTag.ToString();
        if (secondaryTagText != null) secondaryTagText.text = _currentData.secondaryTag.ToString();
        if (enemyImage != null) enemyImage.color = Color.gray; // Placeholder

        // Ödül Metnini Formatla (Artık parametresiz çağrılır)
        if (rewardsText != null) rewardsText.text = FormatRewardsText(); // CS1501 HATASI DÜZELTİLDİ

        // Zorluk Dropdown
        if (difficultyDropdown != null)
        {
            difficultyDropdown.SetValueWithoutNotify((int)RaidDifficultyManager.Difficulty.Easy);
            _selectedDifficulty = RaidDifficultyManager.Difficulty.Easy;
        }

        // Buton Durumu
        UpdateAttackButtonState(true);
        _isOnRespawnCooldown = false;
        _remainingRespawnTime = 0f;
        if (respawnTimerText != null) respawnTimerText.gameObject.SetActive(false);
    }

    void PopulateDifficultyDropdown()
    {
        if(difficultyDropdown == null) return;
        difficultyDropdown.ClearOptions();
        List<string> difficultyNames = System.Enum.GetNames(typeof(RaidDifficultyManager.Difficulty)).ToList();
        difficultyDropdown.AddOptions(difficultyNames);
    }

    void OnDifficultyChanged(int index)
    {
        _selectedDifficulty = (RaidDifficultyManager.Difficulty)index;
        Debug.Log($"Düşman {_currentData?.enemyName} için zorluk değiştirildi: {_selectedDifficulty}");
    }

    void OnAttackButtonClicked()
    {
        if (string.IsNullOrEmpty(_enemyID))
        {
            Debug.LogError("RaidEnemyUI: Saldırılacak düşman ID'si boş!", this);
            return;
        }

        if (_isOnRespawnCooldown)
        {
            Debug.Log($"Düşman {_currentData?.enemyName} henüz tekrar saldırmak için hazır değil.");
            return;
        }

        Debug.Log($"{_currentData?.enemyName} düşmanına ({_enemyID}) {_selectedDifficulty} zorluğunda saldırılıyor...");
        FightManager.Instance.StartFight(_enemyID, _selectedDifficulty);
    }

    /// <summary>
    /// Saldırı butonunun metnini ve tıklanabilirliğini ayarlar.
    /// </summary>
    void UpdateAttackButtonState(bool canAttack)
    {
        if (attackButton == null || _currentData == null) return; // _currentData kontrolü

        attackButton.interactable = canAttack;

        var colors = attackButton.colors;
        colors.disabledColor = cannotAttackColor;
        attackButton.colors = colors;
        Image btnImage = attackButton.GetComponent<Image>();
        if (btnImage != null) btnImage.color = canAttack ? canAttackColor : cannotAttackColor;

        if (attackButtonText != null)
        {
            bool previouslyDefeated = false; // TODO: Kayıt sisteminden alınacak

            if (!canAttack && _isOnRespawnCooldown)
            {
                attackButtonText.text = "Bekleniyor...";
                attackButton.gameObject.SetActive(false);
                if (respawnTimerText != null) respawnTimerText.gameObject.SetActive(true);
            }
            // CS0103 HATALARI DÜZELTİLDİ (Artık _currentData kullanılır)
            else if (previouslyDefeated && _currentData.canRespawn) 
            {
                attackButtonText.text = "Tekrar Saldır";
                attackButton.gameObject.SetActive(true);
                if (respawnTimerText != null) respawnTimerText.gameObject.SetActive(false);
            }
            else
            {
                attackButtonText.text = "Saldır";
                attackButton.gameObject.SetActive(true);
                if (respawnTimerText != null) respawnTimerText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Düşmanın ödül metnini formatlar. (Parametresiz hale getirildi)
    /// </summary>
    string FormatRewardsText() // CS1501 HATASI DÜZELTİLDİ (Tanım parametresiz)
    {
        // CS0103 HATALARI DÜZELTİLDİ (Artık _currentData kullanılır)
        if (_currentData == null) return "Ödüller: Bilgi Yok";

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>Olası Ödüller:</b>");

        if (_currentData.experienceReward > 0) sb.AppendLine($"- {_currentData.experienceReward:F0} XP");

        if (_currentData.goldRewardTiers != null && _currentData.goldRewardTiers.Count > 0)
        {
            double minGold = _currentData.goldRewardTiers.Min(t => t.minAmount);
            double maxGold = _currentData.goldRewardTiers.Max(t => t.maxAmount);
            if (minGold >= maxGold) sb.AppendLine($"- {minGold:F0} Altın");
            else sb.AppendLine($"- {minGold:F0}-{maxGold:F0} Altın");
        }

        if (_currentData.nexusCoinRewardTiers != null && _currentData.nexusCoinRewardTiers.Count > 0)
        {
            double minNexus = _currentData.nexusCoinRewardTiers.Min(t => t.minAmount);
            double maxNexus = _currentData.nexusCoinRewardTiers.Max(t => t.maxAmount);
            if (minNexus >= maxNexus) sb.AppendLine($"- {minNexus:F0} Nexus Coin");
            else sb.AppendLine($"- {minNexus:F0}-{maxNexus:F0} Nexus Coin");
        }

        if (_currentData.itemDrops != null && _currentData.itemDrops.Count > 0)
        {
            foreach (var drop in _currentData.itemDrops)
            {
                if (drop.itemToDrop != null)
                {
                    sb.AppendLine($"- {drop.itemToDrop.itemName} (%{(drop.baseDropChance * 100):F1})");
                }
            }
        }

        if (sb.Length <= "<b>Olası Ödüller:</b>\n".Length)
        {
            sb.AppendLine("- Yok");
        }

        return sb.ToString().TrimEnd();
    }

    void Update()
    {
        if (_isOnRespawnCooldown)
        {
            _remainingRespawnTime -= Time.deltaTime;
            if (respawnTimerText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0, _remainingRespawnTime));
                respawnTimerText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }

            if (_remainingRespawnTime <= 0)
            {
                _isOnRespawnCooldown = false;
                _remainingRespawnTime = 0;
                UpdateAttackButtonState(true);
                if (respawnTimerText != null) respawnTimerText.gameObject.SetActive(false);
            }
        }
    }

    public void OnEnemyDefeated()
    {
        // CS0103 HATALARI DÜZELTİLDİ (Artık _currentData kullanılır)
        if (_currentData == null) return; 

        if (_currentData.canRespawn)
        {
            UpdateAttackButtonState(true);
            Debug.Log($"{_currentData.enemyName} yenildi. Tekrar saldırılabilir.");
        }
        else if (_currentData.respawnTime > 0)
        {
            _isOnRespawnCooldown = true;
            _remainingRespawnTime = _currentData.respawnTime;
            UpdateAttackButtonState(false);
            Debug.Log($"{_currentData.enemyName} yenildi. {_currentData.respawnTime} saniye sonra tekrar saldırılabilir.");
        }
        else
        {
            UpdateAttackButtonState(false);
            if (attackButtonText != null) attackButtonText.text = "Yenildi";
            Debug.Log($"{_currentData.enemyName} yenildi ve tekrar saldıralamaz.");
        }
    }
}