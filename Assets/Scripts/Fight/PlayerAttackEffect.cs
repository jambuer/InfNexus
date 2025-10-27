using UnityEngine;
using System; // Serializable için

namespace PlayerFightMechanics // Kodları gruplamak için namespace kullanabiliriz
{
    /// <summary>
    /// Oyuncunun gerçekleştirebileceği tüm özel saldırı efektlerinin türleri.
    /// </summary>
    public enum PlayerAttackEffectType
    {
        // Başlangıç Efektleri
        Tenacity,
        DefenseHit,
        ScreamHit,
        PanicHit,
        BoldHit,
        ErmeHit,

        // Sonradan Açılacaklar
        MutlakDelme, // Absolute Pierce
        DestroyedHit,
        BreakingHit,
        CrushingHit,
        Execution,
        Resonance,
        PerfectHit,
        ManaCut,
        LuckyHit,
        ExtraLuckyHit,
        ChaosHit,
        ShardHit,
        SlashHit,
        ReflexHit,
        BreakingDefense,

        // Normal vuruşu da bir tür olarak ekleyebiliriz (loglama için)
        NormalHit,
        CriticalHit // Normal vuruşun kritik hali
    }

    /// <summary>
    /// Tek bir özel saldırı efektinin verilerini tutan ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAttackEffect_", menuName = "Fight/Player Attack Effect")]
    public class PlayerAttackEffectData : ScriptableObject
    {
        [Tooltip("Bu saldırı efektinin türü (Enum).")]
        public PlayerAttackEffectType effectType;

        [Tooltip("Efektin tetiklenme olasılığı (0 ile 1 arası).")]
        [Range(0f, 1f)]
        public float triggerChance = 0.1f; // %10 varsayılan

        [Tooltip("Efektin UI'da veya loglarda görünecek adı.")]
        public string displayName = "Özel Vuruş";

        [Tooltip("Hasar formülünde kullanılacak temel çarpan (örn: Attack*Multiplier + Additive).")]
        public double damageMultiplier = 1.0; // Varsayılan: Attack kadar vurur

        [Tooltip("Hasar formülünde kullanılacak ek düz hasar (örn: Attack*Multiplier + Additive).")]
        public double damageAdditive = 0;

        [Tooltip("Hasar formülünde ek olarak kullanılacak oyuncu statı (örn: DefenseHit için Defense).")]
        public StatType statToAdd = StatType.None; // StatReward'daki enum'ı kullanabiliriz

        [Tooltip("Eklenecek statın hasara etki çarpanı (örn: Defense*StatMultiplier).")]
        public double statMultiplier = 1.0;

        [Tooltip("Hasar formülünde ek olarak kullanılacak oyuncu kaynağı (örn: BoldHit için Health).")]
        public ResourceType resourceToAdd = ResourceType.None;

        [Tooltip("Eklenecek kaynağın hasara etki çarpanı (örn: Health*ResourceMultiplier).")]
        public double resourceMultiplier = 0.5; // BoldHit için Health/2 idi

        [Tooltip("Bu efekt defansın yüzde kaçını yok sayar (0 ile 1 arası, örn: 0.4 = %40).")]
        [Range(0f, 1f)]
        public float defenseIgnorePercent = 0f;

        [Tooltip("Bu efekt aktif olduğunda düşman defansını yüzde kaç azaltır (0 ile 1 arası, örn: 0.6 = %60). Sadece o vuruş için geçerli.")]
        [Range(0f, 1f)]
        public float temporaryDefenseReductionPercent = 0f; // Breaking Defense için

        // Formülü daha açık hale getirmek için enum eklenebilir (opsiyonel)
        // public DamageFormulaType formulaType;
        // public enum DamageFormulaType { Standard, HalfAttack, ThirdAttack, AddDefense, AddHealthOverTwo ... }

        // --- Diğer Özel Mantıklar İçin Alanlar ---
        [Tooltip("Lucky Hit veya Resonance gibi, bu efekt mevcut tur bittikten sonra yeni bir saldırı turu tetikler mi?")]
        public bool triggersExtraTurn = false;
    
        [Tooltip("Extra Lucky Hit gibi toplam hasarı çarpar mı?")]
        public bool multipliesTotalDamage = false;
        [Tooltip("Perfect Hit gibi tüm önceki hasarları toplar mı?")]
        public bool sumsPreviousHits = false;

    }

    // ResourceManager'daki gibi bir enum tanımlayabiliriz
    public enum ResourceType { None, Health, Energy, Mana }
}