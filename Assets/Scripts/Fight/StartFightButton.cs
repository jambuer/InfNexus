using UnityEngine;
using UnityEngine.UI; // Button component'i için gerekli

// Bu script, bir butona eklendiğinde Inspector'dan belirtilen
// Enemy ID ile FightManager'ı tetikler.
[RequireComponent(typeof(Button))] // Bu scriptin eklendiği objede Button olmasını zorunlu kılar
public class StartFightButton : MonoBehaviour
{
    [Header("Savaş Ayarları")]
    [Tooltip("Saldırılacak düşmanın EnemyDatabase'deki benzersiz ID'si.")]
    public string enemyIDToAttack; // Inspector'dan girilecek ID

    [Tooltip("Savaşın hangi zorlukta başlayacağı.")]
    public RaidDifficultyManager.Difficulty fightDifficulty = RaidDifficultyManager.Difficulty.Normal; // Varsayılan zorluk

    private Button _button;

    void Start()
    {
        _button = GetComponent<Button>();
        // Butonun onClick olayına StartFightOnClick fonksiyonunu otomatik ekle
        _button.onClick.AddListener(StartFightOnClick);
    }

    // Butona tıklandığında çağrılacak fonksiyon
    public void StartFightOnClick()
    {
        // ID'nin boş olup olmadığını kontrol et
        if (string.IsNullOrEmpty(enemyIDToAttack))
        {
            Debug.LogError($"StartFightButton ({gameObject.name}): Enemy ID To Attack alanı boş bırakılmış!", this);
            return;
        }

        // FightManager'ın var olup olmadığını kontrol et ve savaşı başlat
        if (FightManager.Instance != null)
        {
            Debug.Log($"StartFightButton: '{enemyIDToAttack}' ID'li düşmana {fightDifficulty} zorluğunda saldırı başlatılıyor...");
            FightManager.Instance.StartFight(enemyIDToAttack, fightDifficulty);
        }
        else
        {
            Debug.LogError("StartFightButton: FightManager bulunamadı!", this);
        }
    }

    // Listener'ı temizlemek için (obje yok edildiğinde vb.)
    void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(StartFightOnClick);
        }
    }
}