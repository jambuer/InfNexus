using UnityEngine;
using System.Collections.Generic; // Listeler için
using System; // Guid ve Math için

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Fight/Enemy Data")]
public class EnemyData : ScriptableObject
{
    // --- CombatStats Struct Tanımı ---
    /// <summary>
    /// Belirli bir zorluk seviyesi için hesaplanmış anlık savaş istatistiklerini tutar.
    /// </summary>
    [Serializable] // Inspector'da görülebilmesi için (opsiyonel)
    public struct CombatStats
    {
        public double MaxHealth;
        public double Attack;
        public double Defense;
        public double CritRate; // 0-1 arası
        public double CritDamage; // 1.0+
        public double DodgeChance; // 0-1 arası
        public double HitRate;
        // İleride buraya başka anlık statlar (örn: elemental dirençler) eklenebilir.
    }
    // --- CombatStats Struct Tanımı Bitti ---


    [Header("Temel Bilgiler")]
    [Tooltip("Düşmanın benzersiz kimliği (Otomatik oluşturulur).")]
    public string enemyID;
    [Tooltip("İşaretliyse, savaş ekranı için 'Large Layout Prefab' kullanılır, değilse 'Small Layout Prefab'.")]
    public bool useLargeLayout = false;
    [Tooltip("Oyun içinde görünecek düşman adı.")]
    public string enemyName = "Yeni Düşman";

    [Tooltip("Düşman öldükten sonra 'Tekrar Saldır' butonu görünsün mü?")]
    public bool canRespawn = true; // Varsayılan olarak tekrar saldırılabilir
    [Tooltip("Eğer 'Can Respawn' kapalıysa, tekrar saldırmak için beklenecek süre (saniye). 0 ise hiç beklenmez.")]
    public float respawnTime = 0f;

    [Header("Etiketler")]
    [Tooltip("Düşmanın ana kategorisi (Raid, Boss vb.).")]
    public EnemyPrimaryTag primaryTag = EnemyPrimaryTag.None;
    [Tooltip("Düşmanın ikincil özelliği (Element, Tür vb.).")]
    public EnemySecondaryTag secondaryTag = EnemySecondaryTag.None;

    [Header("Savaş İstatistikleri (Easy Temel Değerler)")] // Başlık güncellendi
    [Tooltip("Saldırı sırasını belirler. Yüksek olan önce saldırır.")]
    public double hitRate = 50;
    [Tooltip("Düşmanın temel saldırı gücü.")]
    public double attack = 10;
    [Tooltip("Düşmanın temel savunma değeri.")]
    public double defense = 5;
    [Tooltip("Düşmanın kritik vuruş yapma şansı (0 ile 1 arası).")]
    [Range(0f, 1f)]
    public double critRate = 0.05; // %5
    [Tooltip("Düşmanın kritik vuruş hasar çarpanı (1.0 = normal hasar, 1.5 = +%50 hasar).")]
    public double critDamage = 1.5; // +%50
    [Tooltip("Düşmanın maksimum can puanı.")]
    public double maxHealth = 100;
    [Tooltip("Düşmanın oyuncu saldırısından kaçınma şansı (0 ile 1 arası).")]
    [Range(0f, 0.9f)] // %90'dan fazla olmasın belki?
    public double dodgeChance = 0.05; // %5

    [Header("Ödüller")]
    [Tooltip("Düşman yenildiğinde kazanılacak tecrübe puanı.")]
    public double experienceReward = 10;
    [Tooltip("Düşman yenildiğinde düşebilecek altın miktarı aralıkları ve olasılıkları.")]
    public List<RewardTier> goldRewardTiers; // Mevcut RewardTier yapısını kullanıyoruz
    [Tooltip("Düşman yenildiğinde düşebilecek Nexus Coin miktarı aralıkları ve olasılıkları.")]
    public List<RewardTier> nexusCoinRewardTiers; // Mevcut RewardTier yapısını kullanıyoruz
    [Tooltip("Düşman yenildiğinde düşebilecek eşyalar ve düşme koşulları.")]
    public List<EnemyItemDrop> itemDrops; // Yeni oluşturduğumuz EnemyItemDrop yapısını kullanıyoruz

    // ScriptableObject ilk oluşturulduğunda veya Unity'de seçildiğinde çalışır
    private void OnEnable()
    {
        // Eğer ID boşsa, yeni bir benzersiz ID ata
        if (string.IsNullOrEmpty(enemyID))
        {
            enemyID = Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            // Değişikliği kaydetmek için Editor tooling kullan (Oyun build'inde çalışmaz)
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }

    /// <summary>
    /// Belirtilen zorluk seviyesine göre düşmanın savaş istatistiklerini hesaplar ve döndürür.
    /// Asset'teki değerler 'Easy' zorluğu olarak kabul edilir.
    /// </summary>
    /// <param name="difficulty">Hesaplama yapılacak zorluk seviyesi.</param>
    /// <returns>Hesaplanmış CombatStats yapısı.</returns>
    public CombatStats GetStatsForDifficulty(RaidDifficultyManager.Difficulty difficulty)
    {
        CombatStats stats = new CombatStats();

        // 1. Temel Çarpanı Hesapla
        double multiplier = 1.0; // Easy için temel çarpan
        switch (difficulty)
        {
            case RaidDifficultyManager.Difficulty.Normal:
                multiplier = 1.30;
                break;
            case RaidDifficultyManager.Difficulty.Hard:
                multiplier = 1.30 * 2.0; // Normal'in 2 katı
                break;
            case RaidDifficultyManager.Difficulty.VeryHard:
                multiplier = (1.30 * 2.0) * 3.0; // Hard'ın 3 katı
                break;
            case RaidDifficultyManager.Difficulty.Nightmare:
                multiplier = ((1.30 * 2.0) * 3.0) * 5.0; // Very Hard'ın 5 katı
                break;
            case RaidDifficultyManager.Difficulty.Easy: // Easy için çarpan 1.0
            default:
                multiplier = 1.0;
                break;
        }

        // 2. Statları Çarpanla Hesapla
        // Not: Math.Round kullanarak veya doğrudan double bırakarak küsuratları yönetebilirsin.
        stats.MaxHealth = Math.Round(this.maxHealth * multiplier);
        stats.Attack = Math.Round(this.attack * multiplier);
        stats.Defense = Math.Round(this.defense * multiplier);
        stats.HitRate = Math.Round(this.hitRate * multiplier); // HitRate de zorlukla artmalı mı? Tasarıma bağlı.

        // Şans bazlı statlar için dikkatli olmalıyız (genellikle %100'ü geçmemeli)
        // CritRate ve DodgeChance zorlukla artmalı mı, yoksa sabit mi kalmalı?
        // Şimdilik zorlukla arttığını varsayalım ama üst limit koyalım.
        stats.CritRate = Math.Min(0.95, this.critRate * multiplier); // Örnek: %95 limit
        stats.DodgeChance = Math.Min(0.90, this.dodgeChance * multiplier); // Örnek: %90 limit

        // CritDamage genellikle çarpan olarak artar, üst limit olmayabilir.
        stats.CritDamage = this.critDamage * multiplier; // Veya daha yavaş artması istenebilir: 1.0 + (this.critDamage - 1.0) * multiplier

        // Önemli: İstatistiklerin negatif olmamasını sağla (özellikle multiplier < 1 durumlarında)
        stats.MaxHealth = Math.Max(1, stats.MaxHealth); // En az 1 can
        stats.Attack = Math.Max(0, stats.Attack);
        stats.Defense = Math.Max(0, stats.Defense);
        stats.HitRate = Math.Max(0, stats.HitRate);
        stats.CritRate = Math.Max(0, stats.CritRate);
        stats.CritDamage = Math.Max(1, stats.CritDamage); // Kritik hasar en az 1x olmalı
        stats.DodgeChance = Math.Max(0, stats.DodgeChance);


        return stats;
    }
}