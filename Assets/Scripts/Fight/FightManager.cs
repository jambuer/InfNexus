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

    [Header("Savaş Konsolu")]
    public FightConsole fightConsole; // Ayrı bir FightConsole scripti referansı
    [Header("Düşman Düzeni Prefabları")] // YENİ HEADER
    [Tooltip("Küçük/Normal düşmanlar için kullanılacak düzen prefabı (Üzerinde EnemyDisplayUI script'i olmalı).")]
    public GameObject smallEnemyLayoutPrefab; // YENİ REFERANS
    [Tooltip("Büyük/Boss düşmanlar için kullanılacak düzen prefabı (Üzerinde EnemyDisplayUI script'i olmalı).")]
    public GameObject largeEnemyLayoutPrefab; // YENİ REFERANS

    [Header("Düşman Gösterim Prefab")] // YENİ HEADER
    [Tooltip("Düşman bilgilerini (isim, can barı, görsel vb.) gösteren UI prefab'ı.")]
    public GameObject enemyDisplayPrefab;

    [Header("Efekt Veritabanı")]
    public EnemyDatabase enemyDatabase;
    public PlayerAttackEffectDatabase playerAttackEffectDatabase;

    // --- Savaş Durum Bilgileri ---
    private EnemyData currentEnemyData;
    private RaidDifficultyManager.Difficulty currentDifficulty;
    private EnemyData.CombatStats currentEnemyStats; // Anlık, hesaplanmış düşman statları
    private double currentEnemyHealth;
    private bool isPlayerTurn = true;
    private bool isFightActive = false;
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


        if (exitButton != null) exitButton.onClick.AddListener(EndFight);
        if (attackButton != null) attackButton.onClick.AddListener(PlayerAttack); // Ana saldırı butonu PlayerAttack'ı tetikler
        if (respawnButton != null) respawnButton.onClick.AddListener(RestartFight); // Respawn butonu savaşı yeniden başlatır

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

        ResourceManager.Instance?.StopRecovery();
        fightConsole?.ClearConsole();
        fightConsole?.AddMessage($"Savaş başladı: {currentEnemyData.enemyName} ({currentDifficulty})");

        DetermineFirstTurn();
    }
    

    /// <summary>
    /// Oyuncu ve düşman HitRate'lerini karşılaştırarak ilk saldırı sırasını belirler.
    /// </summary>
    void DetermineFirstTurn()
    {
        ComputedStats playerStats = StatCalculator.Instance.currentStats; // Oyuncunun anlık statları

        if (currentEnemyStats.HitRate > playerStats.HitRate)
        {
            isPlayerTurn = false;
            fightConsole?.AddMessage("Düşman daha hızlı! İlk saldırıyı o yapıyor.");
            StartCoroutine(EnemyTurnCoroutine()); // Düşman sırasını başlat
        }
        else
        {
            isPlayerTurn = true;
            waitingForPlayerAction = true; // Oyuncunun eylemini bekle
            if (attackButton != null) attackButton.interactable = true; // Oyuncu butonunu aktif et
            fightConsole?.AddMessage("Sıra sende! Saldırmak için butona bas.");
            // TODO: Oyuncu yetenek butonlarını aktif et
        }
    }

    /// <summary>
    /// Oyuncunun ana saldırı butonuna basıldığında tetiklenir.
    /// </summary>
    void PlayerAttack()
    {
        if (!isFightActive || !isPlayerTurn || !waitingForPlayerAction) return;

        waitingForPlayerAction = false; // Eylem yapıldı, bekleme bitti
        if (attackButton != null) attackButton.interactable = false; // Butonu tekrar pasif yap
        // TODO: Diğer yetenek butonlarını pasif yap

        StartCoroutine(PlayerTurnCoroutine()); // Oyuncu sırasını başlat
    }

    /// <summary>
    /// Oyuncunun saldırı sırasını yöneten Coroutine.
    /// </summary>
    IEnumerator PlayerTurnCoroutine()
    {
        fightConsole?.AddMessage("--- Oyuncu Sırası ---");
        ComputedStats playerStats = StatCalculator.Instance.currentStats;
        EnemyData.CombatStats enemyStats = currentEnemyStats; // Bu zaten StartFight'ta hesaplanmıştı

        List<string> turnLogs = new List<string>();
        double totalDamageThisTurn = 0;
        bool attackHits = false;

        // 1. Vuruş Şansını Hesapla
        bool guaranteedHit = playerStats.HitRate >= enemyStats.HitRate;
        double hitChance = guaranteedHit ? 1.0 : (playerStats.HitRate / enemyStats.HitRate);

        if (random.NextDouble() <= hitChance) // Vuruş başarılı mı?
        {
            // 2. Kaçınma Şansını Hesapla
            double dodgeMultiplier = 1.0;
            double hitRateDifference = playerStats.HitRate / enemyStats.HitRate; // Düşmanın kaçınmasını oyuncunun isabeti etkiliyor

            if (hitRateDifference <= 3.0) // 3 kattan fazla fark yoksa
            {
                dodgeMultiplier = (1.0 - enemyStats.DodgeChance); // Normal etki
            }
            else // 3 kattan fazlaysa
            {
                // Her kat için %5 azaltma
                int multiplesOver3 = (int)Math.Floor(hitRateDifference) - 3;
                double reducedDodge = enemyStats.DodgeChance * (1.0 - (multiplesOver3 * 0.05));
                reducedDodge = Math.Max(0, reducedDodge); // Negatife düşmesin
                dodgeMultiplier = (1.0 - reducedDodge);
            }

            if (random.NextDouble() <= dodgeMultiplier) // Kaçınamadıysa (Vuruş İsabet Etti)
            {
                attackHits = true;
                bool isCritical = false;
                double baseDamage = 0;

                // 3. Kritik Vuruş Hesapla
                if (random.NextDouble() <= playerStats.CritRate)
                {
                    isCritical = true;
                }

                // 4. Temel Hasarı Hesapla (+/- %16)
                double minDamage = playerStats.TotalAttack * 0.84;
                double maxDamage = playerStats.TotalAttack * 1.16;
                baseDamage = minDamage + (random.NextDouble() * (maxDamage - minDamage));

                if (isCritical)
                {
                    baseDamage *= playerStats.CritDamage;
                    turnLogs.Add($"<color=red>Kritik Vuruş!</color> Temel Hasar: {baseDamage:F0}");
                }
                else
                {
                    turnLogs.Add($"Normal Vuruş. Temel Hasar: {baseDamage:F0}");
                }

                // 5. Özel Efektleri İşle
                List<KeyValuePair<PlayerAttackEffectType, double>> triggeredEffects =
                    new List<KeyValuePair<PlayerAttackEffectType, double>>();

                double temporaryDefenseReduction = 0;

                // Breaking Defense gibi vuruş öncesi etkileri kontrol et
                foreach (var effectData in availablePlayerEffects)
                {
                    if (effectData.temporaryDefenseReductionPercent > 0 && random.NextDouble() <= effectData.triggerChance)
                    {
                        temporaryDefenseReduction = Math.Max(temporaryDefenseReduction, effectData.temporaryDefenseReductionPercent);
                        turnLogs.Add($"<color=orange>{effectData.displayName}</color> aktif! Düşman defansı %{temporaryDefenseReduction * 100:F0} azaldı.");
                    }
                }

                // Normal/Kritik vuruşun hasarını listeye ekle (Savunma sonrası)
                double initialDamageAfterDefense = CalculateDamageAfterDefense(baseDamage, enemyStats.Defense, 0, temporaryDefenseReduction);
                triggeredEffects.Add(new KeyValuePair<PlayerAttackEffectType, double>(
                    isCritical ? PlayerAttackEffectType.CriticalHit : PlayerAttackEffectType.NormalHit,
                    initialDamageAfterDefense
                ));

                // Diğer özel vuruşları kontrol et
                foreach (var effectData in availablePlayerEffects)
                {
                    if (effectData.temporaryDefenseReductionPercent > 0 || effectData.multipliesTotalDamage || effectData.sumsPreviousHits) continue;

                    if (random.NextDouble() <= effectData.triggerChance)
                    {
                        double effectBaseDamage = CalculateEffectBaseDamage(effectData, baseDamage, playerStats);
                        double effectDamageAfterDefense = CalculateDamageAfterDefense(effectBaseDamage, enemyStats.Defense, effectData.defenseIgnorePercent, temporaryDefenseReduction);

                        if (effectDamageAfterDefense > 0)
                        {
                            triggeredEffects.Add(new KeyValuePair<PlayerAttackEffectType, double>(effectData.effectType, effectDamageAfterDefense));
                            turnLogs.Add($"<color=yellow>{effectData.displayName}</color> aktif! +{effectDamageAfterDefense:F0} Hasar.");
                        }

                        if (effectData.extraHits > 0)
                        {
                            for (int i = 0; i < effectData.extraHits; i++)
                            {
                                triggeredEffects.Add(new KeyValuePair<PlayerAttackEffectType, double>(effectData.effectType, effectDamageAfterDefense));
                                turnLogs.Add($"<color=purple>{effectData.displayName} (Ekstra Vuruş)!</color> +{effectDamageAfterDefense:F0} Hasar.");
                            }
                        }
                        if (effectData.triggersNextAttack)
                        {
                            turnLogs.Add($"<color=green>{effectData.displayName}</color> aktif! Ekstra saldırı yapılacak.");
                            // TODO: Flag ayarla
                        }
                    }
                }

                // 6. Özel Toplam/Çarpan Efektlerini İşle
                totalDamageThisTurn = triggeredEffects.Sum(kvp => kvp.Value);

                PlayerAttackEffectData perfectHitData = playerAttackEffectDatabase.GetAttackEffectData(PlayerAttackEffectType.PerfectHit);
                if (perfectHitData != null)
                {
                     double playerLuck = StatManager.Instance != null ? StatManager.Instance.GetTotalLuck() : 0; // StatManager'dan al
                     double playerPerception = StatManager.Instance != null ? StatManager.Instance.GetTotalPerception() : 0; // StatManager'dan al
                     double perfectHitChance = ((playerLuck + playerPerception) / 10000.0) / 100.0; // Doğru değerlerle hesapla

                    if (random.NextDouble() <= perfectHitChance)
                    {
                        double perfectHitBonus = playerStats.TotalAttack * perfectHitData.damageMultiplier;
                        double perfectHitTotal = totalDamageThisTurn + perfectHitBonus;
                        turnLogs.Add($"<color=cyan>{perfectHitData.displayName}</color> aktif! +{perfectHitBonus:F0} Bonus Hasar!");
                        totalDamageThisTurn = perfectHitTotal;
                    }
                }

                PlayerAttackEffectData extraLuckyHitData = playerAttackEffectDatabase.GetAttackEffectData(PlayerAttackEffectType.ExtraLuckyHit);
                if (extraLuckyHitData != null && random.NextDouble() <= extraLuckyHitData.triggerChance)
                {
                    if (extraLuckyHitData.multipliesTotalDamage)
                    {
                        double originalDamage = totalDamageThisTurn;
                        totalDamageThisTurn *= extraLuckyHitData.damageMultiplier;
                        turnLogs.Add($"<color=lime>{extraLuckyHitData.displayName}</color> aktif! Toplam Hasar x{extraLuckyHitData.damageMultiplier:F1} ({totalDamageThisTurn:F0})!");
                    }
                }
            }
            else // Kaçındı
            {
                attackHits = false;
                turnLogs.Add("Düşman saldırıdan <color=grey>kaçındı!</color>");
            }
        }
        else // Iskaladı
        {
            attackHits = false;
            turnLogs.Add("Saldırı <color=grey>iskaladı!</color>");
        }

        // 7. Logları Yazdır ve Hasarı Uygula
        foreach (string log in turnLogs)
        {
            fightConsole?.AddMessage(log);
        }

        if (attackHits && totalDamageThisTurn > 0)
        {
            fightConsole?.AddMessage($"<b>Toplam Hasar: {totalDamageThisTurn:F0}</b>");
            currentEnemyHealth -= totalDamageThisTurn;
            // UpdateEnemyHealthUI(); // Bu satır kaldırıldı, prefab scripti çağrılacak
            enemyDisplayInstance?.UpdateHealth(currentEnemyHealth, currentEnemyStats.MaxHealth);
        }

        yield return new WaitForSeconds(0.5f);

        // 8. Düşman Öldü mü Kontrol Et
        if (currentEnemyHealth <= 0)
        {
            EnemyDefeated();
        }
        else
        {
            // 9. Düşman Sırasını Başlat
            isPlayerTurn = false;
            StartCoroutine(EnemyTurnCoroutine());
        }

        // TODO: Lucky Hit flag'i true ise, bu coroutine'i tekrar başlat.
    }

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
            PlayerDefeated();
        }
        else
        {
            isPlayerTurn = true;
            waitingForPlayerAction = true;
            if (attackButton != null) attackButton.interactable = true;
            fightConsole?.AddMessage("Sıra sende!");
        }
    }


    /// <summary>
    /// Düşman yenildiğinde çağrılır.
    /// </summary>
    void EnemyDefeated()
    {
        isFightActive = false;
        fightConsole?.AddMessage($"<color=green>{currentEnemyData.enemyName} yenildi!</color>");
        if (attackButton != null) attackButton.interactable = false;

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
        if (enemyDisplayInstance != null)
        {
            enemyDisplayInstance.EnableDifficultyDropdown();
        }


        // Respawn Mantığı
        if (respawnButton != null) // Respawn butonu varsa
        {
            if (currentEnemyData.canRespawn)
            {
                respawnButton.gameObject.SetActive(true); // gameobject eklendi
                if (respawnButtonText != null) respawnButtonText.text = "Tekrar Saldır";
                respawnButton.interactable = true;
            }
            else if (currentEnemyData.respawnTime > 0)
            {
                respawnButton.gameObject.SetActive(true); // gameobject eklendi
                respawnButton.interactable = false;
                StartCoroutine(RespawnTimerCoroutine(currentEnemyData.respawnTime)); // Geri sayımı başlat
            }
            else
            {
                respawnButton.gameObject.SetActive(false); // gameobject eklendi
            }
        }



        // Kaynak yenilenmesini tekrar başlat
        ResourceManager.Instance?.StartRecovery(); // ResourceManager'da bu fonksiyonu ekleyeceğiz
    }
    

    /// <summary>
    /// Oyuncu yenildiğinde çağrılır.
    /// </summary>
    void PlayerDefeated()
    {
        isFightActive = false;
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
        StopAllCoroutines();

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
    /// "Tekrar Saldır" butonuna basıldığında savaşı aynı ayarlarla yeniden başlatır.
    /// </summary>
    void RestartFight()
    {
        // Gerekli objelerin var olduğundan emin ol
        if (isFightActive || currentEnemyData == null || enemyDisplayInstance == null)
        {
            Debug.LogWarning("RestartFight çağrıldı ancak savaş aktif veya gerekli veriler (EnemyData/DisplayInstance) eksik.");
            return;
        }

        // ---- DEĞİŞİKLİK BURADA ----
        // UI'daki dropdown'dan o an seçili olan YENİ zorluk seviyesini al
        RaidDifficultyManager.Difficulty newDifficulty = enemyDisplayInstance.GetSelectedDifficulty();

        Debug.Log($"'{currentEnemyData.enemyName}' için savaş yeniden başlatılıyor. ID: {currentEnemyData.enemyID}, YENİ Zorluk: {newDifficulty}");

        if (respawnButton != null) respawnButton.gameObject.SetActive(false);

        // Savaşı eski zorluk (`currentDifficulty`) yerine YENİ zorluk (`newDifficulty`) ile başlat
        StartFight(currentEnemyData.enemyID, newDifficulty);
        // ---- DEĞİŞİKLİK BİTTİ ----
    }
     

    // Respawn için geri sayım Coroutine'i
    IEnumerator RespawnTimerCoroutine(float duration)
    {
        float timer = duration;
        while (timer > 0)
        {
            if (respawnButtonText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(timer)); // Yukarı yuvarla
                respawnButtonText.text = $"Bekle ({timeSpan.Minutes:D2}:{timeSpan.Seconds:D2})";
            }
            yield return null; // Bir sonraki frame'e kadar bekle
            timer -= Time.deltaTime;
        }

        // Süre doldu
        if (respawnButton != null)
        {
            respawnButton.interactable = true;
            if (respawnButtonText != null) respawnButtonText.text = "Tekrar Saldır";
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
}