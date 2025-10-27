using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using PlayerFightMechanics; // PlayerAttackEffectData vs. için
using System.Text; // StringBuilder için
using System; // Math, Random için
using System.Linq;


// Bu script, savaşın genel akışını, sıra yönetimini ve hasar hesaplamalarını yönetecek.
public class FightManager : Singleton<FightManager> // Singleton yaptık
{
    [Header("Savaş Paneli UI Referansları")]
    public GameObject fightPanelObject; // Taslaktaki ana panel
   // public TextMeshProUGUI enemyNameText;
   // public TextMeshProUGUI enemyPrimaryTagText;
    //public TextMeshProUGUI enemySecondaryTagText;
    //public Image enemyImage; // Ortadaki düşman görseli
    //public Image backgroundImage; // Panelin arka planı
    //public Slider enemyHealthSlider;
    //public TextMeshProUGUI enemyHealthText;
    public Button exitButton;
    public Button attackButton; // Oyuncunun ana saldırı butonu
    public Button respawnButton; // Tekrar saldır butonu
    public TextMeshProUGUI respawnButtonText; // Tekrar saldır / Bekle vs.
    [Tooltip("Düşman düzeni prefabının ekleneceği boş container GameObject.")]
    public Transform enemyDisplayContainer;
    [Tooltip("Otomatik saldırıyı aç/kapat butonu.")] // YENİ
    public Button autoAttackButton; // Otomatik saldırı butonu
    [Tooltip("Otomatik saldırı butonunun rengini/görselini değiştirmek için (Opsiyonel).")] // YENİ
    public Image autoAttackButtonImage; 
    public Color autoAttackOnColor = Color.green; // YENİ
    public Color autoAttackOffColor = Color.white; // YENİ

    [Tooltip("Otomatik Respawn'ı aç/kapat butonu.")] // YENİ
    public Button autoRespawnButton;
    [Tooltip("Otomatik Respawn butonunun rengini/görselini değiştirmek için (Opsiyonel).")] // YENİ
    public Image autoRespawnButtonImage;
    public Color autoRespawnOnColor = Color.cyan; // YENİ (Farklı bir renk olsun)

    [Header("Savaş Konsolu")]
    public FightConsole fightConsole; // Ayrı bir FightConsole scripti referansı
    [Header("Düşman Düzeni Prefabları")] // YENİ HEADER
    [Tooltip("Küçük/Normal düşmanlar için kullanılacak düzen prefabı (Üzerinde EnemyDisplayUI script'i olmalı).")]
    public GameObject smallEnemyLayoutPrefab; // YENİ REFERANS
    [Tooltip("Büyük/Boss düşmanlar için kullanılacak düzen prefabı (Üzerinde EnemyDisplayUI script'i olmalı).")]
    public GameObject largeEnemyLayoutPrefab; // YENİ REFERANS

    

    [Header("Efekt Veritabanı")]
    public EnemyDatabase enemyDatabase;
    public PlayerAttackEffectDatabase playerAttackEffectDatabase;
    [Header("Savaş Kontrolcüleri")]
    [Tooltip("Kaynak harcama ve maliyet çarpanı butonlarını yöneten script.")]

    public FightAttackController attackController; // YENİ

    // --- Savaş Durum Bilgileri ---
    private bool isAutoRespawning = false; // YENİ
    private bool isAutoAttacking = false; // YENİ
    private int lastSelectedResourceCost = 1; // YENİ (Otomatik saldırının temel maliyeti)
    private double lastSelectedDamageMultiplier = 1.0; // YENİ (Otomatik saldırının temel hasar çarpanı)
    private EnemyData currentEnemyData;
    private RaidDifficultyManager.Difficulty currentDifficulty;
    private EnemyData.CombatStats currentEnemyStats; // Anlık, hesaplanmış düşman statları
    private double currentEnemyHealth;
    private bool isPlayerTurn = true;
    private bool isFightActive = false;
    private int _extraTurnsTriggeredThisAction = 0; // Oyuncu butona bastığından beri tetiklenen ekstra tur sayısı
    private const int MAX_EXTRA_TURNS_PER_ACTION = 5; // Maksimum zincirleme tur limiti

    private bool waitingForPlayerAction = false; // Oyuncu eylemi bekleniyor mu?
    private System.Random random = new System.Random(); // Rastgele sayılar için

    // --- Oyuncu Özel Vuruşları ---
    // TODO: Oyuncunun kilidini açtığı vuruşları tutacak bir liste veya Dictionary (PerkManager'dan alınabilir)
    private List<PlayerAttackEffectData> availablePlayerEffects = new List<PlayerAttackEffectData>();

    // --- OLUŞTURULAN PREFAB REFERANSI ---
    private FightScreenUI enemyDisplayInstance;

    

    protected override void Awake()
    {
        base.Awake(); // Singleton Awake'i çağır
        if (fightPanelObject != null) fightPanelObject.SetActive(false); // Başlangıçta paneli gizle
        if (respawnButton != null) respawnButton.gameObject.SetActive(false); // Respawn butonunu gizle
    }

    void Start()
    {
        if (enemyDatabase == null) Debug.LogError("FightManager: Enemy Database atanmamış!", this);
        if (playerAttackEffectDatabase == null) Debug.LogError("FightManager: Player Attack Effect Database atanmamış!", this);
        if (smallEnemyLayoutPrefab == null) Debug.LogError("FightManager: Small Enemy Layout Prefab atanmamış!", this);
        if (largeEnemyLayoutPrefab == null) Debug.LogError("FightManager: Large Enemy Layout Prefab atanmamış!", this);
        if (enemyDisplayContainer == null) Debug.LogError("FightManager: Enemy Display Container atanmamış!", this);
        if (attackController == null) Debug.LogError("FightManager: Fight Attack Controller atanmamış!", this); // YENİ
        if (autoAttackButton == null) Debug.LogError("FightManager: Auto Attack Button atanmamış!", this); // YENİ
        if (autoRespawnButton == null) Debug.LogError("FightManager: Auto Respawn Button atanmamış!", this); // YENİ

        if (exitButton != null) exitButton.onClick.AddListener(EndFight);
        // if (attackButton != null) attackButton.onClick.AddListener(PlayerAttack); // Ana saldırı butonu PlayerAttack'ı tetikler
        if (respawnButton != null) respawnButton.onClick.AddListener(() => RestartFight()); // Parametresiz çağır, varsayılan false kullanılır.
        if (autoAttackButton != null) autoAttackButton.onClick.AddListener(ToggleAutoAttack); // YENİ
        if (autoRespawnButton != null) autoRespawnButton.onClick.AddListener(ToggleAutoRespawn); // YENİ
        // TODO: Oyuncunun sahip olduğu efektleri yükle (şimdilik veritabanındakilerin hepsini alıyoruz)
        if (playerAttackEffectDatabase != null)
        {
            availablePlayerEffects = playerAttackEffectDatabase.GetAllEffects()
                                    .Where(e => e.effectType != PlayerAttackEffectType.NormalHit && e.effectType != PlayerAttackEffectType.CriticalHit) // Normal/Crit hariç
                                    .ToList();
        }
    }
    


 
 
    /// <summary>
    /// Verilen ID'ye sahip düşmanla savaşı başlatır.
    /// </summary>
    public void StartFight(string enemyID, RaidDifficultyManager.Difficulty difficulty) // Parametre ID olarak değişti
    {                
        if (isFightActive) { Debug.LogWarning("Zaten bir savaş aktif!"); return; }
        CancelAllAutomation(); // YENİ (Fonksiyonu aşağıya ekleyeceğiz)
        lastSelectedResourceCost = 1; // YENİ
        lastSelectedDamageMultiplier = 1.0; // YENİ
        _extraTurnsTriggeredThisAction = 0; // YENİ: Savaş başladığında sayacı sıfırla
        if (string.IsNullOrEmpty(enemyID)) { Debug.LogError("StartFight: enemyID null veya boş!"); return; }
        if (enemyDatabase == null) { Debug.LogError("StartFight: Enemy Database atanmamış!"); return; }

        // Veritabanından EnemyData'yı al
        currentEnemyData = enemyDatabase.GetEnemyData(enemyID);
        if (currentEnemyData == null) { Debug.LogError($"StartFight: ID'si '{enemyID}' olan EnemyData bulunamadı!"); return; }

        // Doğru layout prefabını seç
        GameObject prefabToInstantiate = currentEnemyData.useLargeLayout ? largeEnemyLayoutPrefab : smallEnemyLayoutPrefab;
        if (prefabToInstantiate == null) { Debug.LogError($"StartFight: Düşman '{currentEnemyData.enemyName}' için uygun layout prefabı atanmamış (useLargeLayout={currentEnemyData.useLargeLayout})!"); return; }
        if (enemyDisplayContainer == null) { Debug.LogError("StartFight: Enemy Display Container atanmamış!"); return; }

        currentDifficulty = difficulty;
        currentEnemyStats = currentEnemyData.GetStatsForDifficulty(difficulty);
        currentEnemyHealth = currentEnemyStats.MaxHealth;
        isFightActive = true;
        waitingForPlayerAction = false;

        Debug.Log($"Savaş Başlatıldı: {currentEnemyData.enemyName} ({currentDifficulty}) - Can: {currentEnemyHealth}");

        // --- UI Güncelleme ---
        if (fightPanelObject != null) fightPanelObject.SetActive(true);

        // Önceki düşman display'ini temizle
        if (enemyDisplayInstance != null) Destroy(enemyDisplayInstance.gameObject);
        foreach (Transform child in enemyDisplayContainer) Destroy(child.gameObject); // Container içini de temizle (garanti)


        // Yeni düşman display prefabını oluştur ve kur
        GameObject enemyDisplayGO = Instantiate(prefabToInstantiate, enemyDisplayContainer); // Belirlenen Container'a ekle
        enemyDisplayInstance = enemyDisplayGO.GetComponent<FightScreenUI>();

        if (enemyDisplayInstance != null)
        {
            enemyDisplayInstance.Setup(currentEnemyData, currentEnemyStats); // Prefabın scriptine verileri gönder
        }
        else
        {
            Debug.LogError($"StartFight: Seçilen Enemy Layout Prefab ({prefabToInstantiate.name}) üzerinde EnemyDisplayUI script'i bulunamadı!", prefabToInstantiate);
            EndFight();
            return;
        }

        if (respawnButton != null) respawnButton.gameObject.SetActive(false);
        if (attackButton != null) attackButton.interactable = false;
        
        lastSelectedResourceCost = 1; // YENİ
        lastSelectedDamageMultiplier = 1.0; // YENİ

        ResourceManager.Instance?.StopRecovery();
        fightConsole?.ClearConsole();
        fightConsole?.AddMessage($"Savaş başladı: {currentEnemyData.enemyName} ({currentDifficulty})");

        DetermineFirstTurn();
    }
    

    /// <summary>
    /// Oyuncu ve düşman HitRate'lerini karşılaştırarak ilk saldırı sırasını belirler.
    /// </summary>
    /// <summary>
    /// Oyuncu ve düşman HitRate'lerini karşılaştırarak ilk saldırı sırasını belirler
    /// VE gerekirse otomatik saldırıyı tetikler.
    /// </summary>
    void DetermineFirstTurn()
    {
        ComputedStats playerStats = StatCalculator.Instance.currentStats;

        if (currentEnemyStats.HitRate > playerStats.HitRate)
        {
            // --- Düşman Başlıyor ---
            isPlayerTurn = false;
            waitingForPlayerAction = false; // Oyuncu eylemi beklenmiyor
            SetPlayerActionInteractable(false);
            // if (attackButton != null) attackButton.interactable = false; // attackButton yok
            fightConsole?.AddMessage("Düşman daha hızlı! İlk saldırıyı o yapıyor.");
            StartCoroutine(EnemyTurnCoroutine()); // Düşman sırasını başlat
        }
        else
        {
            // --- Oyuncu Başlıyor ---
            isPlayerTurn = true;

            // Otomatik saldırı AÇIK mı?
            if (isAutoAttacking)
            {
                waitingForPlayerAction = false; // Eylem beklenmiyor
                SetPlayerActionInteractable(false);
                // if (attackButton != null) attackButton.interactable = false; // attackButton yok
                fightConsole?.AddMessage("Sıra sende! Otomatik saldırı başlıyor...");
                StartAutoAttackTurn(); // Otomatik saldırıyı HEMEN başlat
            }
            else // Otomatik saldırı KAPALI
            {

                waitingForPlayerAction = true; // Manuel eylem bekleniyor
                SetPlayerActionInteractable(true);
                // if (attackButton != null) attackButton.interactable = true; // attackButton yok
                fightConsole?.AddMessage("Sıra sende! Saldırmak için (1, 5, 10...) butonuna bas.");
            }
        }
    }

    /// <summary>
    /// FightAttackController'daki 6 kaynak butonundan birine basıldığında çağrılır.
    /// </summary>
    public void OnResourceAttackPressed(int resourceIndex)
    {
        // Sadece sırası bizdeyse ve manuel eylem bekleniyorsa çalışır
        if (!isFightActive || !isPlayerTurn || !waitingForPlayerAction) return;
        if (attackController == null) return; // Kontrolcü yoksa çık

        // Eylem yapıldı, bekleme bitti
        waitingForPlayerAction = false;
        SetPlayerActionInteractable(false);

        // Gerekli değerleri attackController'dan al
        lastSelectedResourceCost = attackController.GetSelectedResourceCost();
        lastSelectedDamageMultiplier = attackController.GetSelectedDamageMultiplier();
        int costMultiplier = attackController.GetSelectedCostMultiplier();
        int totalCost = lastSelectedResourceCost * costMultiplier;

        // Kaynak kontrolü
        bool isOnlyEnergy = (lastSelectedResourceCost == 1);
        if (!HasEnoughResources(totalCost, isOnlyEnergy))
        {
            // Yeterli kaynak yoksa sırayı geri ver
            waitingForPlayerAction = true;
            SetPlayerActionInteractable(true);
            return;
        }

        // YENİ: Ana aksiyon Coroutine'ini başlat
        StartCoroutine(ExecutePlayerActionSequence(totalCost, lastSelectedDamageMultiplier, costMultiplier));
    }

    /// <summary>
    /// Otomatik saldırı için oyuncu sırasını başlatır.
    /// </summary>
    private void StartAutoAttackTurn()
    {
        if (!isFightActive || !isPlayerTurn || !isAutoAttacking)
        {
            CancelAllAutomation();
            return;
        }
        if (attackController == null)
        {
            Debug.LogError("AutoAttack: Fight Attack Controller atanmamış!");
            CancelAllAutomation();
            return;
        }

        // Gerekli değerleri al (En son manuel seçim + o anki çarpan)
        double damageMultiplier = lastSelectedDamageMultiplier;
        int resourceCost = lastSelectedResourceCost;
        int costMultiplier = attackController.GetSelectedCostMultiplier(); // O ANKİ ÇARPANI AL
        int totalCost = resourceCost * costMultiplier;

        // Kaynak kontrolü
        bool isOnlyEnergy = (resourceCost == 1);
        if (!HasEnoughResources(totalCost, isOnlyEnergy))
        {
            fightConsole?.AddMessage($"Otomatik Saldırı için yeterli kaynak yok!"); // Mesajı HasEnoughResources yazdırıyor zaten
            CancelAllAutomation();
            waitingForPlayerAction = true;
            SetPlayerActionInteractable(true);
            // if (attackButton != null) attackButton.interactable = true; // attackButton yok artık
            return;
        }

        // YENİ: Ana aksiyon Coroutine'ini başlat
        fightConsole?.AddMessage("Otomatik Saldırı...");
        SetPlayerActionInteractable(false);
        StartCoroutine(ExecutePlayerActionSequence(totalCost, damageMultiplier, costMultiplier));
    }

    /// <summary>
    /// Tek bir oyuncu saldırı turunun sonucunu tutar.
    /// </summary>
    private class TurnResult
    {
        public double DamageDealt = 0;              // Bu turda hesaplanan toplam hasar (çarpanlar hariç)
        public List<string> Logs = new List<string>(); // Bu turdaki loglar
        public bool ExtraTurnTriggered = false;     // Bu turda Lucky/Resonance tetiklendi mi?
        public bool ExtraLuckyHitTriggered = false; // Bu turda Extra Lucky Hit tetiklendi mi?
    }



    /// <summary>
    /// Tek bir oyuncu saldırı turunu (ilk veya ekstra) hesaplar ve sonucunu döndürür.
    /// Kaynak HARCAMAZ, hasar UYGULAMAZ, yeni tur BAŞLATMAZ.
    /// </summary>
    /// <param name="isCriticalTurn">Bu turun kritik vuruş olup olmadığı (önceki turdan belirlenir mi?). Şimdilik her tur kendi kritiğini hesaplasın.</param>
    /// <returns>Hesaplanan hasar, loglar ve tetiklenen bayrakları içeren TurnResult.</returns>
    private TurnResult ExecuteSinglePlayerTurn() // IEnumerator DEĞİL, TurnResult döndürüyor
    {
        TurnResult result = new TurnResult(); // Sonucu tutacak nesne
        ComputedStats playerStats = StatCalculator.Instance.currentStats;
        EnemyData.CombatStats enemyStats = currentEnemyStats;

        bool attackHits = false;
        // Not: totalDamageThisTurn artık result.DamageDealt olacak.
        // Not: turnLogs artık result.Logs olacak.
        // Not: extraTurnTriggered artık result.ExtraTurnTriggered olacak.

        // 1. Vuruş Şansı
        bool guaranteedHit = playerStats.HitRate >= enemyStats.HitRate;
        double hitChance = guaranteedHit ? 1.0 : (playerStats.HitRate / enemyStats.HitRate);

        if (random.NextDouble() <= hitChance) // Vuruş başarılı mı?
        {
            // 2. Kaçınma Şansı
            double dodgeMultiplier = 1.0;
            double hitRateDifference = playerStats.HitRate / enemyStats.HitRate;
            if (hitRateDifference <= 3.0) { dodgeMultiplier = (1.0 - enemyStats.DodgeChance); }
            else
            {
                int multiplesOver3 = (int)Math.Floor(hitRateDifference) - 3;
                double reducedDodge = enemyStats.DodgeChance * (1.0 - (multiplesOver3 * 0.05));
                dodgeMultiplier = (1.0 - Math.Max(0, reducedDodge));
            }

            if (random.NextDouble() <= dodgeMultiplier) // İsabet Etti
            {
                attackHits = true;
                bool isCritical = false;
                double baseDamage = 0;

                // 3. Kritik Vuruş
                if (random.NextDouble() <= playerStats.CritRate) isCritical = true;

                // 4. Temel Hasar
                double minDamage = playerStats.TotalAttack * 0.84;
                double maxDamage = playerStats.TotalAttack * 1.16;
                baseDamage = minDamage + (random.NextDouble() * (maxDamage - minDamage));

                if (isCritical) { baseDamage *= playerStats.CritDamage; }
                result.Logs.Add($"{(isCritical ? "<color=red>Kritik Vuruş!</color>" : "Normal Vuruş.")} Temel Hasar: {NumberFormatter.FormatNumber(baseDamage)}");

                // 5. Özel Efektler
                List<KeyValuePair<PlayerAttackEffectType, double>> triggeredEffects = new List<KeyValuePair<PlayerAttackEffectType, double>>();
                double temporaryDefenseReduction = 0;

                // Vuruş öncesi efektler (Breaking Defense)
                foreach (var effectData in availablePlayerEffects)
                {
                    if (effectData.temporaryDefenseReductionPercent > 0 && random.NextDouble() <= effectData.triggerChance)
                    {
                        temporaryDefenseReduction = Math.Max(temporaryDefenseReduction, effectData.temporaryDefenseReductionPercent);
                        result.Logs.Add($"<color=orange>{effectData.displayName}</color> aktif! Düşman defansı %{temporaryDefenseReduction * 100:F0} azaldı.");
                    }
                }

                // Normal/Kritik vuruş hasarı (savunma sonrası)
                double initialDamageAfterDefense = CalculateDamageAfterDefense(baseDamage, enemyStats.Defense, 0, temporaryDefenseReduction);
                triggeredEffects.Add(new KeyValuePair<PlayerAttackEffectType, double>(
                    isCritical ? PlayerAttackEffectType.CriticalHit : PlayerAttackEffectType.NormalHit,
                    initialDamageAfterDefense));

                // Diğer özel hasar vuruşları
                foreach (var effectData in availablePlayerEffects)
                {
                    // ŞİMDİLİK TÜM ÖZEL ETKİLERİ ATLA (Vuruş öncesi, Toplam Çarpan/Toplama, Ekstra Tur)
                    if (effectData.temporaryDefenseReductionPercent > 0 ||
                        effectData.multipliesTotalDamage ||
                        effectData.sumsPreviousHits ||
                        effectData.triggersExtraTurn)
                    {
                        continue;
                    }

                    if (random.NextDouble() <= effectData.triggerChance)
                    {
                        double effectBaseDamage = CalculateEffectBaseDamage(effectData, baseDamage, playerStats);
                        double effectDamageAfterDefense = CalculateDamageAfterDefense(effectBaseDamage, enemyStats.Defense, effectData.defenseIgnorePercent, temporaryDefenseReduction);

                        if (effectDamageAfterDefense > 0)
                        {
                            triggeredEffects.Add(new KeyValuePair<PlayerAttackEffectType, double>(effectData.effectType, effectDamageAfterDefense));
                            result.Logs.Add($"<color=yellow>{effectData.displayName}</color> aktif! +{NumberFormatter.FormatNumber(effectDamageAfterDefense)} Hasar.");
                        }
                    }
                }

                // Ekstra Tur Tetikleyen Efektleri Kontrol Et (Lucky Hit, Resonance)
                foreach (var effectData in availablePlayerEffects)
                {
                    if (effectData.triggersExtraTurn && random.NextDouble() <= effectData.triggerChance)
                    {
                        result.ExtraTurnTriggered = true; // Bayrağı ayarla
                        result.Logs.Add($"<color=green>{effectData.displayName}</color> aktif! Ekstra saldırı yapılacak.");
                    }
                }

                // 6. Özel Toplam/Çarpan Efektlerini İŞLEME (Hasarı biriktir)

                // Şu ana kadar tetiklenen TÜM efektlerin hasarlarını topla
                result.DamageDealt = triggeredEffects.Sum(kvp => kvp.Value); // Hasarı sonuca yaz

                // Perfect Hit kontrolü (Sadece hasarı ekler)
                PlayerAttackEffectData perfectHitData = playerAttackEffectDatabase.GetAttackEffectData(PlayerAttackEffectType.PerfectHit);
                if (perfectHitData != null)
                {
                    double playerLuck = StatManager.Instance != null ? StatManager.Instance.GetTotalLuck() : 0;
                    double playerPerception = StatManager.Instance != null ? StatManager.Instance.GetTotalPerception() : 0;
                    double perfectHitChance = ((playerLuck + playerPerception) / 10000.0) / 100.0;
                    if (random.NextDouble() <= perfectHitChance)
                    {
                        double perfectHitBonus = playerStats.TotalAttack * perfectHitData.damageMultiplier;
                        result.DamageDealt += perfectHitBonus; // Hasarı sonuca ekle
                        result.Logs.Add($"<color=cyan>{perfectHitData.displayName}</color> aktif! +{NumberFormatter.FormatNumber(perfectHitBonus)} Bonus Hasar!");
                    }
                }

                // Extra Lucky Hit kontrolü (Sadece bayrağı ayarlar, ÇARPMAZ)
                PlayerAttackEffectData extraLuckyHitData = playerAttackEffectDatabase.GetAttackEffectData(PlayerAttackEffectType.ExtraLuckyHit);
                if (extraLuckyHitData != null && random.NextDouble() <= extraLuckyHitData.triggerChance && extraLuckyHitData.multipliesTotalDamage)
                {
                    result.ExtraLuckyHitTriggered = true; // Bayrağı ayarla
                    result.Logs.Add($"<color=lime>{extraLuckyHitData.displayName}</color> aktif! (Nihai hasar çarpılacak)");
                }

            } // -> İsabet Etti Bitti
            else // Kaçındı
            {
                attackHits = false;
                result.Logs.Add("Düşman saldırıdan <color=grey>kaçındı!</color>");
            }
        } // -> Vuruş Başarılı Bitti
        else // Iskaladı
        {
            attackHits = false;
            result.Logs.Add("Saldırı <color=grey>iskaladı!</color>");
        }

        // Eğer isabet etmediyse hasarı sıfırla
        if (!attackHits)
        {
            result.DamageDealt = 0;
        }

        // Hesaplanan sonucu döndür
        return result;

    } // -> ExecuteSinglePlayerTurn Bitti

    

    /// <summary>
    /// Verilen özel efekt verisine göre temel hasarı hesaplar (Stat/Resource eklemeleriyle).
    /// </summary>
    double CalculateEffectBaseDamage(PlayerAttackEffectData effectData, double baseAttackDamage, ComputedStats playerStats)
    {
        double effectDamage = baseAttackDamage * effectData.damageMultiplier + effectData.damageAdditive;

        // Stat Ekleme
        if (effectData.statToAdd != StatType.None && StatManager.Instance != null) // Instance kontrolü eklendi
        {
            double statValue = StatManager.Instance.GetTotalStat(effectData.statToAdd.ToString()); // Doğru enum kullanımı
            effectDamage += statValue * effectData.statMultiplier;
        }

        // Kaynak Ekleme
        if (effectData.resourceToAdd != ResourceType.None && ResourceManager.Instance != null) // Instance kontrolü eklendi
        {
            float resourceValue = 0;
            switch (effectData.resourceToAdd)
            {
                case ResourceType.Health: resourceValue = ResourceManager.Instance.currentHealth; break;
                case ResourceType.Energy: resourceValue = ResourceManager.Instance.currentEnergy; break;
                case ResourceType.Mana: resourceValue = ResourceManager.Instance.currentMana; break;
            }
            effectDamage += resourceValue * effectData.resourceMultiplier;
        }

        return effectDamage;
    }


    /// <summary>
    /// Hasarı, düşman defansını, defans yok saymayı ve geçici defans azaltmayı hesaba katarak son hasarı hesaplar.
    /// </summary>
    double CalculateDamageAfterDefense(double incomingDamage, double enemyDefense, float defenseIgnorePercent, double temporaryDefenseReductionPercent)
    {
        // Önce geçici azaltmayı uygula
        double effectiveDefense = enemyDefense * (1.0 - temporaryDefenseReductionPercent);
        // Sonra defans yok saymayı uygula
        double defenseToApply = effectiveDefense * (1.0 - defenseIgnorePercent);
        // Savunma formülünü uygula
        double finalDamage = incomingDamage - (defenseToApply * 2.16);
        // Hasarın minimum 1 olmasını sağla (veya 0, tasarıma bağlı)
        return Math.Max(1, finalDamage);
    }


    /// <summary>
    /// Düşmanın saldırı sırasını yöneten Coroutine.
    /// </summary>
    IEnumerator EnemyTurnCoroutine()
    {
        // ... (İçerik aynı kalır, sadece en sonda oyuncu canını güncellerken ResourceManager kullanılır) ...
        fightConsole?.AddMessage("--- Düşman Sırası ---");
        yield return new WaitForSeconds(0.8f);

        ComputedStats playerStats = StatCalculator.Instance.currentStats;
        EnemyData.CombatStats enemyStats = currentEnemyStats;
        double damageDealt = 0;

        bool guaranteedHit = enemyStats.HitRate >= playerStats.HitRate;
        double hitChance = guaranteedHit ? 1.0 : (enemyStats.HitRate / playerStats.HitRate);

        if (random.NextDouble() <= hitChance)
        {
            bool isCritical = random.NextDouble() <= enemyStats.CritRate;
            double minDamage = enemyStats.Attack * 0.9;
            double maxDamage = enemyStats.Attack * 1.1;
            double baseDamage = minDamage + (random.NextDouble() * (maxDamage - minDamage));

            if (isCritical)
            {
                baseDamage *= enemyStats.CritDamage;
                fightConsole?.AddMessage($"Düşman <color=orange>kritik vurdu!</color> Temel Hasar: {baseDamage:F0}");
            }
            else
            {
                fightConsole?.AddMessage($"Düşman saldırdı. Temel Hasar: {baseDamage:F0}");
            }

            damageDealt = CalculateDamageAfterDefense(baseDamage, playerStats.TotalDefense, 0, 0);

            fightConsole?.AddMessage($"Alınan Hasar: <color=red>{damageDealt:F0}</color>");
            ResourceManager.Instance?.ModifyHealth(-(float)damageDealt);
        }
        else
        {
            fightConsole?.AddMessage("Düşman <color=grey>iskaladı!</color>");
        }

        yield return new WaitForSeconds(0.5f);

        if (ResourceManager.Instance != null && ResourceManager.Instance.currentHealth <= 0)
        {
            PlayerDefeated(); // Otomatik saldırıyı da iptal eder
        }
        else
        {
            // ---- YENİ OTOMATİK SALDIRI KONTROLÜ ----
            isPlayerTurn = true; // Sıra oyuncuya geçti

            if (isAutoAttacking)
            {
                // Otomatik saldırı açıksa, bekleme, hemen saldır
                waitingForPlayerAction = false;
                SetPlayerActionInteractable(false);
                if (attackButton != null) attackButton.interactable = false;
                StartAutoAttackTurn(); // Yeni fonksiyonu çağır
            }

            else
            {
                // Otomatik saldırı kapalıysa, oyuncunun butonlara basmasını bekle
                waitingForPlayerAction = true;
                SetPlayerActionInteractable(true);
                if (attackButton != null) attackButton.interactable = true;
                fightConsole?.AddMessage("Sıra sende!");
            }

        }
    }
    
    /// <summary>
    /// Düşman yenildiğinde çağrılır.
    /// </summary>
    void EnemyDefeated()
    {

        //CancelAllAutomation();
        isFightActive = false;
        waitingForPlayerAction = false;
        SetPlayerActionInteractable(false);
        fightConsole?.AddMessage($"<color=green>{currentEnemyData.enemyName} yenildi!</color>");
        //if (attackButton != null) attackButton.interactable = false;

        // Ödülleri Dağıt
        // XP
        double finalExp = currentEnemyData.experienceReward; // TODO: Zorluk çarpanı eklenebilir
        LevelManager.Instance?.AddXP(finalExp);
        fightConsole?.AddMessage($"<color=green>+{finalExp:F0} XP</color> kazanıldı.");

        // Altın
        if (currentEnemyData.goldRewardTiers != null && currentEnemyData.goldRewardTiers.Count > 0)
        {
            double baseGold = GetWeightedReward(currentEnemyData.goldRewardTiers);
            double finalGold = baseGold; // TODO: Zorluk ve oyuncu bonusları eklenebilir
            if (finalGold > 0) CurrencyManager.Instance?.AddGold(finalGold);
            fightConsole?.AddMessage($"<color=yellow>+{finalGold:F0} Altın</color> kazanıldı.");
        }
        // Nexus Coin
        if (currentEnemyData.nexusCoinRewardTiers != null && currentEnemyData.nexusCoinRewardTiers.Count > 0)
        {
            double baseNexusCoin = GetWeightedReward(currentEnemyData.nexusCoinRewardTiers);
            double finalNexusCoin = baseNexusCoin; // TODO: Zorluk ve oyuncu bonusları eklenebilir
            if (finalNexusCoin > 0) CurrencyManager.Instance?.AddNexusCoin(finalNexusCoin);
            fightConsole?.AddMessage($"<color=yellow>+{finalNexusCoin:F0} Nexus Coin</color> kazanıldı.");
        }


        // Eşyalar
        if (currentEnemyData.itemDrops != null && Inventory.Instance != null && StatCalculator.Instance != null) // StatCalculator kontrolü eklendi
        {
            ComputedStats playerStats = StatCalculator.Instance.currentStats;
            foreach (var dropInfo in currentEnemyData.itemDrops)
            {
                if (dropInfo.itemToDrop == null) continue;

                float actualDropChance = dropInfo.baseDropChance;
                // Eşik kontrolü
                if (playerStats.DropRate < dropInfo.dropRateThreshold)
                {
                    actualDropChance *= dropInfo.chanceMultiplierBelowThreshold;
                }
                // Oyuncu bonusunu ekle (doğrudan eklemek yerine çarpan olarak daha mantıklı olabilir)
                actualDropChance *= (1.0f + (float)playerStats.DropRate); // Veya sadece playerStats.DropRate eklenebilir? Tasarıma bağlı. Şimdilik çarpıyoruz.
                actualDropChance = Mathf.Clamp01(actualDropChance); // %100'ü geçmesin

                if (random.NextDouble() <= actualDropChance)
                {
                    int quantity = 1;
                    if (dropInfo.quantityScalesWithDropRate && dropInfo.dropRateThreshold > 0)
                    {
                        quantity = (int)Math.Floor(playerStats.DropRate / dropInfo.dropRateThreshold);
                        quantity = Math.Max(1, quantity); // En az 1 düşsün
                    }

                    // TODO: Maksimum düşme sayısını (maxDrops) kontrol et (kayıt sistemi gerektirir)

                    Inventory.Instance.AddItem(dropInfo.itemToDrop, quantity);
                    fightConsole?.AddMessage($"<color=orange>+{quantity} {dropInfo.itemToDrop.itemName}</color> düştü!");
                }
            }
        }

        // Düşman yenildi, zorluk dropdown'ını tekrar aç
        enemyDisplayInstance?.EnableDifficultyDropdown();

        //respawn mantığı
        bool canEnemyRespawn = currentEnemyData != null && (currentEnemyData.canRespawn || currentEnemyData.respawnTime > 0);



        // Eğer Otomatik Respawn AÇIKSA ve düşman tekrar canlanabiliyorsa
        if (isAutoRespawning && canEnemyRespawn)
        {
            fightConsole?.AddMessage("Otomatik Respawn aktif, düşman yeniden canlandırılacak...");
            if (respawnButton != null) respawnButton.gameObject.SetActive(false); // Manuel butonu gizle

            if (currentEnemyData.canRespawn) // Anında canlanma
            {
                StartCoroutine(RestartFightAfterDelay(0.5f, isAutoAttacking)); // Kısa bir gecikmeyle yeniden başlat
            }
            else // Süreyle canlanma
            {
                // Zamanlayıcıyı başlat, bittiğinde OTOMATİK yeniden başlatsın
                StartCoroutine(RespawnTimerCoroutine(currentEnemyData.respawnTime, true, isAutoAttacking)); // isAutoAttacking parametresi eklendi
            }
        }
        // Eğer Otomatik Respawn KAPALIYSA, manuel respawn butonunu göster (eğer canlanabiliyorsa)
        else if (respawnButton != null && canEnemyRespawn)
        {
            respawnButton.gameObject.SetActive(true);
            respawnButton.interactable = currentEnemyData.canRespawn; // Süre varsa başta tıklanamaz

            if (currentEnemyData.canRespawn)
            {
                if (respawnButtonText != null) respawnButtonText.text = "Tekrar Saldır";
            }
            else // Süreyle canlanma
            {
                // Zamanlayıcıyı başlat, bittiğinde SADECE butonu aktif etsin
                StartCoroutine(RespawnTimerCoroutine(currentEnemyData.respawnTime, false, false)); // false parametresi eklendi
            }
        }
        else // Düşman tekrar canlanamıyorsa
        {
            if (respawnButton != null) respawnButton.gameObject.SetActive(false);
            fightConsole?.AddMessage("Bu düşman tekrar canlanamaz.");
            CancelAllAutomation();
        }

        // Kaynak yenilenmesini tekrar başlat
        ResourceManager.Instance?.StartRecovery(); // ResourceManager'da bu fonksiyonu ekleyeceğiz
    }
    

    /// <summary>
    /// Oyuncu yenildiğinde çağrılır.
    /// </summary>
    void PlayerDefeated()
        {
        CancelAllAutomation();
        isFightActive = false;
        SetPlayerActionInteractable(false);
        fightConsole?.AddMessage("<color=red>Yenildin!</color>");
        if (attackButton != null) attackButton.interactable = false;
        if (respawnButton != null) respawnButton.gameObject.SetActive(false); // gameobject eklendi

        ResourceManager.Instance?.StartRecovery();
        // TODO: Oyuncuya seçenek sun
        // Şimdilik sadece savaşı bitiriyoruz (EndFight çağrılabilir veya başka bir panel açılabilir)
        // EndFight(); // Örneğin 2 saniye sonra otomatik kapat
         StartCoroutine(EndFightAfterDelay(2.0f));
    }


    

    /// <summary>
    /// Savaştan çıkış butonuna basıldığında veya savaş bittiğinde çağrılır.
    /// </summary>
    public void EndFight()
    {
        if (!isFightActive && (fightPanelObject == null || !fightPanelObject.activeSelf)) return;

        Debug.Log("Savaş Paneli Kapatılıyor.");
        isFightActive = false;
        waitingForPlayerAction = false;
        SetPlayerActionInteractable(false);
        StopAllCoroutines();

        CancelAllAutomation();
        fightConsole?.ClearConsole();


        ResourceManager.Instance?.StartRecovery();

        if (fightPanelObject != null) fightPanelObject.SetActive(false);

        // OLUŞTURULAN DÜŞMAN DÜZENİ PREFABINI YOK ET
        if (enemyDisplayInstance != null)
        {
            Destroy(enemyDisplayInstance.gameObject);
            enemyDisplayInstance = null; // Referansı temizle
        }
    }

    /// <summary>
    /// "Tekrar Saldır" butonuna basıldığında veya otomatik olarak savaşı aynı ayarlarla yeniden başlatır.
    /// </summary>
    /// <param name="startAutoAttackImmediately">Eğer true ise ve Otomatik Saldırı modu açıksa, ilk turu otomatik başlatır.</param>
    /// <summary>
    /// "Tekrar Saldır" butonuna basıldığında veya otomatik olarak savaşı yeniden başlatır.
    /// </summary>
    /// <param name="startAutoAttackImmediately">Eğer true ise, savaş başlar başlamaz otomatik saldırıyı tetikler.</param>
    void RestartFight(bool startAutoAttackImmediately = false)
     {
         // Gerekli kontroller
         if (isFightActive || currentEnemyData == null)
         {
             Debug.LogWarning($"RestartFight çağrıldı ama koşullar uygun değil (isFightActive={isFightActive}, currentEnemyData={(currentEnemyData==null?"null":"OK")})");
             return;
         }
          // Manuel restart için (startAutoAttackImmediately=false) display örneği olmalı
         if (!startAutoAttackImmediately && enemyDisplayInstance == null)
         {
             Debug.LogWarning("Manuel RestartFight çağrıldı ama enemyDisplayInstance null.");
             return;
         }


         // Zorluğu al (Manuel ise UI'dan, otomatik ise mevcut olandan)
         RaidDifficultyManager.Difficulty newDifficulty = currentDifficulty;
         if(!startAutoAttackImmediately && enemyDisplayInstance != null)
         {
            newDifficulty = enemyDisplayInstance.GetSelectedDifficulty();
         }


         Debug.Log($"'{currentEnemyData.enemyName}' için savaş yeniden başlatılıyor. ID: {currentEnemyData.enemyID}, Zorluk: {newDifficulty}, OtoBaşlat={startAutoAttackImmediately}");

         // Manuel respawn butonunu gizle (zaten gizli olabilir)
         if (respawnButton != null) respawnButton.gameObject.SetActive(false);

         // --- YENİ MANTIK ---
         // Önce StartFight'ı çağırıp savaşın başlamasını beklemiyoruz.
         // Önce savaşın durumunu hazırlayıp, SONRA DetermineFirstTurn'ü çağıracağız
         // ve DetermineFirstTurn otomatik saldırı durumunu yönetecek.

         // Savaş durumunu hazırla
         currentDifficulty = newDifficulty; // Zorluğu güncelle
         currentEnemyStats = currentEnemyData.GetStatsForDifficulty(currentDifficulty); // Statları hesapla
         currentEnemyHealth = currentEnemyStats.MaxHealth; // Canı doldur
         isFightActive = true; // SAVAŞI AKTİF ET
         waitingForPlayerAction = false; // Başlangıçta eylem bekleme
         _extraTurnsTriggeredThisAction = 0; // Ekstra tur sayacını sıfırla

         // UI'ı güncelle
         if (fightPanelObject != null && !fightPanelObject.activeSelf) fightPanelObject.SetActive(true); // Panel kapalıysa aç
         // Mevcut display instance'ını YENİDEN KUR (Destroy/Instantiate yerine)
         if(enemyDisplayInstance != null)
         {
            enemyDisplayInstance.Setup(currentEnemyData, currentEnemyStats); // UI'ı yeni statlarla güncelle
            // Zorluk dropdown'ı Setup içinde kilitleniyor zaten
         } else {
             // Bu durum olmamalı ama olursa logla
             Debug.LogError("RestartFight: enemyDisplayInstance is null during setup!");
         }

         ResourceManager.Instance?.StopRecovery(); // Kurtarmayı durdur
         fightConsole?.ClearConsole(); // Konsolu temizle
         fightConsole?.AddMessage($"Savaş yeniden başladı: {currentEnemyData.enemyName} ({currentDifficulty})");

         // --- Otomatik Saldırı Kontrolü ---
         // DetermineFirstTurn'ü çağırmadan önce, eğer otomatik başlatma isteniyorsa
         // ve Otomatik Saldırı modu AÇIKSA, isAutoAttacking'i true yapalım.
         // DetermineFirstTurn bu bayrağa göre hareket edecek.
         // startAutoAttackImmediately parametresi aslında isAutoAttacking ile aynı işi görüyor olmalı.
         // Eğer Auto Respawn + Auto Attack açıksa, startAutoAttackImmediately true gelir.
         // Eğer sadece Auto Respawn açıksa, startAutoAttackImmediately false gelir.
         // Bu yüzden parametreye GEREK YOK gibi görünüyor, direkt isAutoAttacking'e bakalım.

         // İlk Saldıranı Belirle (Bu fonksiyon isAutoAttacking'e göre otomatik saldırıyı başlatır)
         DetermineFirstTurn();

     }


    // Respawn için geri sayım Coroutine'i
    // Respawn için geri sayım Coroutine'i
    // YENİ: bool autoRestart parametresi eklendi
    IEnumerator RespawnTimerCoroutine(float duration, bool autoRestart, bool shouldStartAutoAttack)
    {
        float timer = duration;
        // Manuel buton görünür olmalı (ama tıklanamaz) ki oyuncu süreyi görsün
        if (!autoRestart && respawnButton != null) respawnButton.gameObject.SetActive(true);

        while (timer > 0)
        {
            // Zamanlayıcı metnini GÜNCELLE (hem manuel hem otomatik için)
            string timerText = "";
             TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(timer));
             timerText = $"Bekle ({timeSpan.Minutes:D2}:{timeSpan.Seconds:D2})";
             if (!autoRestart && respawnButtonText != null) respawnButtonText.text = timerText;

            yield return null;
            timer -= Time.deltaTime;

             // Otomatik Respawn iptal edilirse Coroutine'i durdur
             if (autoRestart && !isAutoRespawning)
             {
                 // ... (Manuel butonu gösterme mantığı aynı kalır) ...
                  fightConsole?.AddMessage("Otomatik Respawn iptal edildi (zamanlayıcı durdu).");
                  if (respawnButton != null && currentEnemyData != null)
                  {
                      respawnButton.gameObject.SetActive(true);
                      respawnButton.interactable = false; // Hala tıklanamaz
                  }
                 yield break;
            }
        }

        // Süre doldu
        if (autoRestart && isAutoRespawning) // Otomatik mod aktifse savaşı yeniden başlat
        {
            fightConsole?.AddMessage("Süre doldu, Otomatik Respawn savaşı yeniden başlatıyor...");
            Debug.Log(">>> Timer finished, calling RestartFight (Auto)...");
            RestartFight();
        }
        else if (respawnButton != null) // Manuel moddaysa butonu aktif et
        {
            respawnButton.interactable = true;
            if (respawnButtonText != null) respawnButtonText.text = "Tekrar Saldır";
            fightConsole?.AddMessage("Düşman tekrar canlandı. Saldırmak için butona bas.");
        }
    }

    // Belirli bir süre sonra savaşı yeniden başlatır (Otomatik Respawn için)
 IEnumerator RestartFightAfterDelay(float delay, bool shouldStartAutoAttack)
 {
     yield return new WaitForSeconds(delay);
     // Yeniden başlatmadan önce Otomatik Respawn hala açık mı diye kontrol et
     if (isAutoRespawning && currentEnemyData != null)
        {
        Debug.Log(">>> Delay finished, calling RestartFight (Auto)...");
         RestartFight();
     }
     else if (currentEnemyData != null) // Otomatik mod kapandıysa manuel butonu göster
     {
          if (respawnButton != null && (currentEnemyData.canRespawn || currentEnemyData.respawnTime > 0))
          {
               respawnButton.gameObject.SetActive(true);
               respawnButton.interactable = true; // Anında canlanma olduğu için tıklanabilir
               if(respawnButtonText != null) respawnButtonText.text = "Tekrar Saldır";
          }
     }
 }



    // Oyuncu yenildiğinde belirli bir süre sonra savaşı bitirir
    IEnumerator EndFightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndFight();
    }

    /// <summary>
    /// Mevcut savaşın zorluk seviyesini döndürür.
    /// </summary>
    public RaidDifficultyManager.Difficulty GetCurrentDifficulty()
    {
        return currentDifficulty;
    }



    // RewardTier listesinden ağırlıklı rastgele ödül seçen yardımcı fonksiyon (QuestManager'dan kopyalandı)
    private double GetWeightedReward(List<RewardTier> tiers)
    {
        if (tiers == null || tiers.Count == 0) return 0;
        float totalWeight = tiers.Sum(t => t.probabilityWeight);
        if (totalWeight <= 0) return tiers.LastOrDefault()?.GetRandomAmount() ?? 0;
        float randomPoint = UnityEngine.Random.Range(0, totalWeight);
        foreach (var tier in tiers)
        {
            if (randomPoint < tier.probabilityWeight) return tier.GetRandomAmount();
            randomPoint -= tier.probabilityWeight;
        }
        return tiers.Last().GetRandomAmount(); // Hata durumunda sonuncuyu ver
    }




    
    /// <summary>
    /// Otomatik Saldırı butonuna basıldığında çağrılır. Sadece bayrağı değiştirir.
    /// </summary>
    public void ToggleAutoAttack()
    {
        isAutoAttacking = !isAutoAttacking; // Durumu tersine çevir
        UpdateAutoAttackButtonVisual(); // Görseli güncelle

        if (isAutoAttacking)
        {
            fightConsole?.AddMessage("Otomatik Saldırı AÇIK.");
            // Eğer Otomatik Saldırı AÇILDIYSA ve SIRA BİZDEYSE ve bekliyorsak,
            // oyuncunun 1, 5, 10... butonlarından birine basarak ilk saldırıyı BAŞLATMASINI bekleriz.
            // Saldırıyı burada otomatik başlatmıyoruz.
        }
        else
        {
            fightConsole?.AddMessage("Otomatik Saldırı KAPALI.");
            // Eğer Otomatik Saldırı KAPANDIYSA ve sıra bizdeyse ve saldırı beklenmiyorsa,
            // manuel girişi tekrar bekleme moduna geçirelim (güvenlik önlemi).
             if (isFightActive && isPlayerTurn && !waitingForPlayerAction)
             {
                  waitingForPlayerAction = true;
             }
        }
    }




    /// <summary>
    /// Otomatik saldırı butonunun görselini günceller.
    /// </summary>
    private void UpdateAutoAttackButtonVisual()
    {
        if (autoAttackButtonImage != null)
        {
            autoAttackButtonImage.color = isAutoAttacking ? autoAttackOnColor : autoAttackOffColor;
        }
        // Veya butonun metnini değiştir:
        // TextMeshProUGUI autoButtonText = autoAttackButton.GetComponentInChildren<TextMeshProUGUI>();
        // if(autoButtonText != null) autoButtonText.text = isAutoAttacking ? "Oto: AÇIK" : "Oto: KAPALI";
    }

    /// <summary>
    /// Gerekli kaynakların yeterli olup olmadığını kontrol eder ve konsola mesaj yazar.
    /// </summary>
    /// <param name="totalCost">Toplam maliyet (örn: 500)</param>
    /// <param name="isOnlyEnergy">Sadece Enerji mi harcıyor (Cost = 1 durumu)</param>
    /// <returns>Yeterli kaynak varsa true</returns>
    private bool HasEnoughResources(int totalCost, bool isOnlyEnergy)
    {
        if (ResourceManager.Instance == null)
        {
            Debug.LogError("HasEnoughResources: ResourceManager bulunamadı!");
            return false;
        }

        if (isOnlyEnergy) // Sadece 1 maliyetli saldırı (Sadece Enerji)
        {
            if (ResourceManager.Instance.currentEnergy < totalCost)
            {
                fightConsole?.AddMessage($"Yeterli Enerji yok! ({totalCost} gerekli, {ResourceManager.Instance.currentEnergy:F0} mevcut)");
                return false;
            }
        }
        else // %80 Enerji, %20 Mana
        {
            float energyCost = totalCost * 0.8f;
            float manaCost = totalCost * 0.2f;

            if (ResourceManager.Instance.currentEnergy < energyCost)
            {
                fightConsole?.AddMessage($"Yeterli Enerji yok! ({energyCost:F0} gerekli, {ResourceManager.Instance.currentEnergy:F0} mevcut)");
                return false;
            }
            if (ResourceManager.Instance.currentMana < manaCost)
            {
                fightConsole?.AddMessage($"Yeterli Mana yok! ({manaCost:F0} gerekli, {ResourceManager.Instance.currentMana:F0} mevcut)");
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Gerekli kaynakları harcar.
    /// </summary>
    /// <param name="totalCost">Toplam maliyet</param>
    /// <param name="isOnlyEnergy">Sadece Enerji mi harcıyor (Cost = 1 durumu)</param>
    private void SpendResources(int totalCost, bool isOnlyEnergy)
    {
        if (ResourceManager.Instance == null) return;

        if (isOnlyEnergy)
        {
            ResourceManager.Instance.ModifyEnergy(-totalCost);
            fightConsole?.AddMessage($"<color=orange>{totalCost} Enerji harcandı.</color>");
        }
        else
        {
            float energyCost = totalCost * 0.8f;
            float manaCost = totalCost * 0.2f;
            ResourceManager.Instance.ModifyEnergy(-energyCost);
            ResourceManager.Instance.ModifyMana(-manaCost);
            fightConsole?.AddMessage($"<color=orange>{energyCost:F0} Enerji</color> ve <color=blue>{manaCost:F0} Mana</color> harcandı.");
        }
    }

    /// <summary>
    /// Oyuncunun bir saldırı aksiyonunu (ilk vuruş + tüm ekstra turlar) yöneten ana Coroutine.
    /// </summary>
    private IEnumerator ExecutePlayerActionSequence(int initialTotalCost, double initialDamageMultiplier, double initialCostMultiplier)
    {
        // Aksiyon için toplamları ve bayrakları tutacak değişkenler
        double totalAccumulatedDamage = 0;
        List<string> allLogs = new List<string>();
        bool anyExtraLuckyHitTriggered = false;
        _extraTurnsTriggeredThisAction = 0; // Bu aksiyon için sayacı sıfırla

        // 1. İlk Turu Gerçekleştir
        fightConsole?.AddMessage("--- Oyuncu Aksiyonu Başlıyor ---");

        // Kaynakları SADECE ilk tur için harca
        bool isOnlyEnergy = (lastSelectedResourceCost == 1);
        SpendResources(initialTotalCost, isOnlyEnergy);

        // İlk turu hesapla
        TurnResult currentTurnResult = ExecuteSinglePlayerTurn();

        // Sonuçları biriktir
        totalAccumulatedDamage += currentTurnResult.DamageDealt;
        allLogs.AddRange(currentTurnResult.Logs);
        if (currentTurnResult.ExtraLuckyHitTriggered) anyExtraLuckyHitTriggered = true;

        // Kısa bekleme (ilk vuruşun logları görünsün)
        yield return new WaitForSeconds(0.1f);


        // 2. Ekstra Turları Döngüyle Gerçekleştir (Limit dahilinde)
        while (currentTurnResult.ExtraTurnTriggered && _extraTurnsTriggeredThisAction < MAX_EXTRA_TURNS_PER_ACTION)
        {
            _extraTurnsTriggeredThisAction++; // Sayacı artır
            fightConsole?.AddMessage($"<color=cyan>--- Ekstra Saldırı Turu ({_extraTurnsTriggeredThisAction}/{MAX_EXTRA_TURNS_PER_ACTION}) ---</color>");
            yield return new WaitForSeconds(0.1f); // Görsel bekleme

            // YENİ turu hesapla (Kaynak harcamadan)
            currentTurnResult = ExecuteSinglePlayerTurn(); // ExecuteSinglePlayerTurn kaynak harcamıyor

            // Sonuçları biriktir
            totalAccumulatedDamage += currentTurnResult.DamageDealt;
            allLogs.AddRange(currentTurnResult.Logs);
            if (currentTurnResult.ExtraLuckyHitTriggered) anyExtraLuckyHitTriggered = true; // Herhangi bir turda tetiklenmesi yeterli

            // Kısa bekleme
            yield return new WaitForSeconds(0.1f);

            // Eğer bu ekstra turda tekrar ekstra tur tetiklenmediyse VEYA limit dolduysa döngüden çık
            if (!currentTurnResult.ExtraTurnTriggered || _extraTurnsTriggeredThisAction >= MAX_EXTRA_TURNS_PER_ACTION)
            {
                if (currentTurnResult.ExtraTurnTriggered && _extraTurnsTriggeredThisAction >= MAX_EXTRA_TURNS_PER_ACTION)
                {
                    fightConsole?.AddMessage("<color=grey>Ekstra saldırı limiti doldu.</color>");
                }
                break; // Döngüyü sonlandır
            }
        } // -> while Bitti


        // 3. Nihai Hasarı Hesapla ve Uygula

        // Kaynak ve Maliyet çarpanlarını uygula (biriken hasara)
        double finalDamageBeforeExtraLucky = totalAccumulatedDamage * initialDamageMultiplier * initialCostMultiplier;

        // Extra Lucky Hit tetiklendiyse nihai hasarı çarp
        if (anyExtraLuckyHitTriggered)
        {
            PlayerAttackEffectData extraLuckyHitData = playerAttackEffectDatabase.GetAttackEffectData(PlayerAttackEffectType.ExtraLuckyHit);
            if (extraLuckyHitData != null && extraLuckyHitData.multipliesTotalDamage)
            {
                finalDamageBeforeExtraLucky *= extraLuckyHitData.damageMultiplier;
            }
        }

        // 4. Tüm Logları Yazdır
        fightConsole?.AddMessage("--- Aksiyon Sonucu ---");
        foreach (string log in allLogs)
        {
            fightConsole?.AddMessage(log); // Biriken tüm logları göster
        }

        // 5. Nihai Hasar Logunu Yazdır ve Uygula
        if (totalAccumulatedDamage > 0) // Eğer herhangi bir turda hasar verildiyse
        {
            fightConsole?.AddMessage($"Kaynak Bonusu (x{initialDamageMultiplier:F2}) + Maliyet Çarpanı (x{initialCostMultiplier})");
            if (anyExtraLuckyHitTriggered) fightConsole?.AddMessage($"<color=lime>Extra Lucky Hit Bonusu Aktif!</color>");
            fightConsole?.AddMessage($"<b>%%% Nihai Toplam Hasar: {NumberFormatter.FormatNumber(finalDamageBeforeExtraLucky)} %%%</b>");

            currentEnemyHealth -= finalDamageBeforeExtraLucky; // Hasarı TEK SEFERDE uygula
            enemyDisplayInstance?.UpdateHealth(currentEnemyHealth, currentEnemyStats.MaxHealth); // UI'ı güncelle
        }
        else // Hiçbir turda hasar verilemediyse (Iskalama/Kaçınma)
        {
            if (allLogs.Count > 0 && allLogs.All(l => l.Contains("kaçındı") || l.Contains("iskaladı")))
                fightConsole?.AddMessage("<b>Tüm saldırılar iskaladı veya kaçınıldı!</b>");
            else
                fightConsole?.AddMessage("<b>Toplam Hasar: 0</b> (Düşman defansı hasarı engelledi)");
        }


        // 6. Sonrası İçin Bekleme
        yield return new WaitForSeconds(0.5f);

        // 7. Düşman Öldü mü Kontrol Et
        if (currentEnemyHealth <= 0)
        {
            EnemyDefeated(); // Düşman öldü
        }
        else
        {
            // 8. Düşman Sırasını Başlat
            isPlayerTurn = false;
            StartCoroutine(EnemyTurnCoroutine());
        }

    } // -> ExecutePlayerActionSequence Bitti
    
    /// <summary>
    /// Otomatik Respawn butonuna basıldığında çağrılır. Sadece bayrağı değiştirir.
    /// </summary>
    public void ToggleAutoRespawn()
    {
        isAutoRespawning = !isAutoRespawning; // Durumu tersine çevir
        UpdateAutoRespawnButtonVisual(); // Görseli güncelle

        if (isAutoRespawning)
        {
            fightConsole?.AddMessage("Otomatik Respawn AÇIK.");
            // --- BU KONTROL BLOĞUNU SİLİYORUZ ---
            /*
            // Eğer Otomatik Respawn AÇILDIYSA ve SAVAŞ BİTMİŞSE (Respawn butonu görünürse)
            // ve düşman tekrar canlanabiliyorsa, hemen yeniden başlatmayı dene.
            if (!isFightActive && respawnButton != null && respawnButton.gameObject.activeSelf && currentEnemyData != null && (currentEnemyData.canRespawn || currentEnemyData.respawnTime > 0))
            {
                 if(respawnButton.interactable) // Süre dolmuşsa veya anında ise
                 {
                     fightConsole?.AddMessage("Otomatik Respawn aktif, düşman yeniden canlandırılıyor...");
                     RestartFight(isAutoAttacking); // Auto Attack da açıksa true gönder
                 }
                 else // Zamanlayıcı çalışıyorsa
                 {
                      fightConsole?.AddMessage("Otomatik Respawn aktif, düşmanın canlanma süresi bekleniyor...");
                 }
            }
            */
            // --- SİLME BİTTİ ---
            // Otomatik respawn'ın devreye girmesi için EnemyDefeated fonksiyonunun çalışması beklenecek.
        }
        else
        {
            fightConsole?.AddMessage("Otomatik Respawn KAPALI.");
            // Zamanlayıcıyı manuel moda çevirme mantığı RespawnTimerCoroutine içinde zaten var.
        }
    }

    /// <summary>
    /// Otomatik Respawn butonunun görselini günceller.
    /// </summary>
    private void UpdateAutoRespawnButtonVisual()
    {
        if (autoRespawnButtonImage != null)
        {
            autoRespawnButtonImage.color = isAutoRespawning ? autoRespawnOnColor : autoAttackOffColor; // Kapalı rengi diğeriyle aynı
        }
    }

    /// <summary>
    /// TÜM otomasyon modlarını (Otomatik Saldırı ve Otomatik Respawn) kapatır.
    /// </summary>
    public void CancelAllAutomation()
    {
        bool wasAutomating = isAutoAttacking || isAutoRespawning;
        if (isAutoAttacking)
        {
            isAutoAttacking = false;
            UpdateAutoAttackButtonVisual();
        }
        if (isAutoRespawning)
        {
            isAutoRespawning = false;
            UpdateAutoRespawnButtonVisual();
        }

        // Eğer herhangi bir otomasyon açıktıysa log mesajı yazdır
        // (CancelAutoAttack zaten yazdırıyordu, buraya taşıdık)
        if (wasAutomating)
        {
            fightConsole?.AddMessage("Otomatik modlar iptal edildi.");
        }
    }

    /// <summary>
    /// Oyuncu aksiyon butonlarının (kaynak harcama butonları vb.) tıklanabilirliğini ayarlar.
    /// </summary>
    private void SetPlayerActionInteractable(bool interactable)
    {
        if (attackController != null && attackController.resourceAmountButtons != null)
        {
            foreach (var button in attackController.resourceAmountButtons)
            {
                if (button != null)
                {
                    // Kaynak kontrolü yapmadan, sadece genel durumu ayarla
                    button.interactable = interactable;
                }
            }
        }
        // TODO: Yetenek butonları eklendiğinde onları da yönet.
        // Debug.Log($"Oyuncu Aksiyon Butonları Interactable: {interactable}"); // İstersen bu log kalabilir.
    }


    

}