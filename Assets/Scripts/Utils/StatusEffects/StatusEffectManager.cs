using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq; // LINQ kullanacağız (FirstOrDefault için)
using System;

/// <summary>
/// Oyuncu (veya potansiyel olarak düşmanlar) üzerindeki geçici
/// statü etkilerini (Buff/Debuff) yöneten merkezi Singleton.
/// Etkileri uygular, sürelerini takip eder ve süre dolunca geri alır.
/// </summary>
public class StatusEffectManager : Singleton<StatusEffectManager>
{
    // HATA 2 & 3 DÜZELTMESİ:
    // Bu iç sınıf (inner class), 'public' metotlarda (RemoveEffect, GetActiveEffects)
    // parametre ve geri dönüş tipi olarak kullanıldığı için 'public' olmalıdır.
    /// <summary>
    /// Şu anda aktif olan bir etkiyi ve kalan süresini takip eden iç sınıf.
    /// </summary>
    public class ActiveStatusEffect
    {
        public StatusEffectData Data;
        public float TimeRemaining;
        public Coroutine RemovalCoroutine;
        public object Source; // Bu etkiyi kimin uyguladığını takip etmek için (örn: "Potion_Item")

        public ActiveStatusEffect(StatusEffectData data, object source, Coroutine coroutine)
        {
            Data = data;
            TimeRemaining = data.duration;
            Source = source;
            RemovalCoroutine = coroutine;
        }
    }

    // Şu anda oyuncu üzerindeki tüm aktif etkiler
    private readonly List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();

    // UI'ın (veya diğer sistemlerin) güncellenmesi için event
    public event Action OnEffectListChanged;

    /// <summary>
    /// Yeni bir statü etkisini oyuncuya uygular.
    /// </summary>
    /// <param name="effectData">Uygulanacak etkinin ScriptableObject tanımı</param>
    /// <param name="source">Bu etkiyi tetikleyen (örn: bir ItemData veya "Düşman Büyüsü")</param>
    public void ApplyEffect(StatusEffectData effectData, object source = null)
    {
        if (effectData == null || effectData.effectType == StatusEffectType.None) return;

        // 1. Etki yığınlanabilir (stackable) değilse, mevcut olanı kontrol et
        if (!effectData.isStackable)
        {
            ActiveStatusEffect existingEffect = _activeEffects.FirstOrDefault(e => e.Data.effectID == effectData.effectID);
            if (existingEffect != null)
            {
                // Efekt zaten var, süresini yenile ve çık
                existingEffect.TimeRemaining = effectData.duration;
                // Debug.Log($"'{effectData.displayName}' süresi yenilendi.");
                OnEffectListChanged?.Invoke();
                return;
            }
        }

        // HATA 1 DÜZELTMESİ (Mantık Değişikliği):
        // Önce ActiveStatusEffect nesnesini (Coroutine null iken) oluştur.
        ActiveStatusEffect newActiveEffect = new ActiveStatusEffect(effectData, source, null);

        // Coroutine'i başlat ve 'newActiveEffect' nesnesini ona parametre olarak ver.
        Coroutine removalCoroutine = StartCoroutine(HandleEffectDuration(newActiveEffect));

        // Şimdi nesnenin Coroutine referansını güncelle.
        newActiveEffect.RemovalCoroutine = removalCoroutine;

        // Aktif etki listesine ekle
        _activeEffects.Add(newActiveEffect);
        
        // 3. İstatistiksel değişikliği yap
        ApplyEffectInternal(newActiveEffect);

        Debug.Log($"Etki Uygulandı: {newActiveEffect.Data.displayName}");
        OnEffectListChanged?.Invoke();
    }

    /// <summary>
    /// Bir etkinin süresini takip eder ve süre dolduğunda kaldırır.
    /// (HATA 1 DÜZELTMESİ: Parametresi 'StatusEffectData'dan 'ActiveStatusEffect'e değiştirildi)
    /// </summary>
    private IEnumerator HandleEffectDuration(ActiveStatusEffect activeEffect)
    {
        // 'duration' 0 veya daha azsa, etki manuel kaldırılana kadar kalıcıdır.
        if (activeEffect.Data.duration <= 0)
        {
            yield break; // Coroutine'i bitir, bekleme yapma.
        }

        // HATA 1 DÜZELTMESİ:
        // Artık 'activeEffect'i aramamıza gerek yok, zaten parametre olarak aldık.
        // 'this.coroutine' gibi hatalı bir kullanıma da gerek kalmadı.
        
        while (activeEffect.TimeRemaining > 0)
        {
            activeEffect.TimeRemaining -= Time.deltaTime;
            yield return null;
        }

        // Süre doldu, etkiyi kaldır.
        Debug.Log($"Etki Sona Erdi: {activeEffect.Data.displayName}");
        RemoveEffect(activeEffect);
    }

    /// <summary>
    /// Belirli bir etkiyi (süresi dolsa da dolmasa da) oyuncudan kaldırır.
    /// (HATA 2 DÜZELTMESİ: 'ActiveStatusEffect' artık public olduğu için bu metot geçerli)
    /// </summary>
    public void RemoveEffect(ActiveStatusEffect effectToRemove)
    {
        if (effectToRemove == null || !_activeEffects.Contains(effectToRemove)) return;

        // 1. Zamanlayıcıyı durdur (eğer çalışıyorsa)
        if (effectToRemove.RemovalCoroutine != null)
        {
            StopCoroutine(effectToRemove.RemovalCoroutine);
        }

        // 2. İstatistiksel değişikliği geri al
        RemoveEffectInternal(effectToRemove);
        
        // 3. Listeden çıkar
        _activeEffects.Remove(effectToRemove);
        
        OnEffectListChanged?.Invoke();
    }

    // --- ETKİ UYGULAMA VE KALDIRMA İÇ MANTIKLARI ---

    /// <summary>
    /// Etkinin stat'lara olan etkisini uygular (switch-case).
    /// </summary>
    private void ApplyEffectInternal(ActiveStatusEffect effect)
    {
        // Gerekli yöneticileri kontrol et
        if (StatManager.Instance == null || ResourceManager.Instance == null)
        {
            Debug.LogError("StatusEffectManager: Gerekli Yöneticiler (Stat/Resource) bulunamadı!");
            return;
        }

        double value = effect.Data.effectValue;
        string param = effect.Data.effectParameter;

        switch (effect.Data.effectType)
        {
            case StatusEffectType.AddStatBonus:
                // StatManager'daki (isPermanent=false) kullanan Add... metotlarını çağır
                CallStatManagerAdd(param, value, isPermanent: false);
                break;
            case StatusEffectType.AddAllStatsBonus:
                // AddAllStats'ın bonus versiyonu yoksa, hepsini tek tek çağırmalıyız
                CallStatManagerAdd("Physical", value, isPermanent: false);
                CallStatManagerAdd("Mental", value, isPermanent: false);
                CallStatManagerAdd("Perception", value, isPermanent: false);
                CallStatManagerAdd("Spiritual", value, isPermanent: false);
                CallStatManagerAdd("Luck", value, isPermanent: false);
                CallStatManagerAdd("Social", value, isPermanent: false);
                break;
            case StatusEffectType.ModifyMaxHealth:
                ResourceManager.Instance.ModifyMaxHealth((float)value);
                break;
            case StatusEffectType.ModifyMaxEnergy:
                ResourceManager.Instance.ModifyMaxEnergy((float)value);
                break;
            case StatusEffectType.ModifyMaxMana:
                ResourceManager.Instance.ModifyMaxMana((float)value);
                break;
        }
    }

    /// <summary>
    /// Etkinin stat'lara olan etkisini geri alır (switch-case).
    /// </summary>
    private void RemoveEffectInternal(ActiveStatusEffect effect)
    {
        if (StatManager.Instance == null || ResourceManager.Instance == null) return;

        double value = effect.Data.effectValue;
        string param = effect.Data.effectParameter;

        switch (effect.Data.effectType)
        {
            case StatusEffectType.AddStatBonus:
                // StatManager'daki 'Remove...Bonus' metotlarını çağır
                CallStatManagerRemove(param, value);
                break;
            case StatusEffectType.AddAllStatsBonus:
                CallStatManagerRemove("Physical", value);
                CallStatManagerRemove("Mental", value);
                CallStatManagerRemove("Perception", value);
                CallStatManagerRemove("Spiritual", value);
                CallStatManagerRemove("Luck", value);
                CallStatManagerRemove("Social", value);
                break;
            case StatusEffectType.ModifyMaxHealth:
                ResourceManager.Instance.ModifyMaxHealth(-(float)value); // Değeri eksi (-) olarak geri al
                break;
            case StatusEffectType.ModifyMaxEnergy:
                ResourceManager.Instance.ModifyMaxEnergy(-(float)value);
                break;
            case StatusEffectType.ModifyMaxMana:
                ResourceManager.Instance.ModifyMaxMana(-(float)value);
                break;
        }
    }

    // --- YARDIMCI METOTLAR (StatManager ile konuşmak için) ---

    /// <summary>
    /// StatManager'a (isPermanent=false) bonus ekleme komutu gönderir.
    /// </summary>
    private void CallStatManagerAdd(string statName, double amount, bool isPermanent)
    {
        if (StatManager.Instance == null) return;
        // dosyasındaki Add... metotlarını kullanır
        switch (statName.ToLowerInvariant()) 
        {
            case "physical": StatManager.Instance.AddPhysical(amount, isPermanent); break;
            case "mental": StatManager.Instance.AddMental(amount, isPermanent); break;
            case "perception": StatManager.Instance.AddPerception(amount, isPermanent); break;
            case "spiritual": StatManager.Instance.AddSpiritual(amount, isPermanent); break;
            case "luck": StatManager.Instance.AddLuck(amount, isPermanent); break;
            case "social": StatManager.Instance.AddSocial(amount, isPermanent); break;
            default: Debug.LogWarning($"StatusEffectManager: Bilinmeyen stat adı: {statName}"); break;
        }
    }

    /// <summary>
    /// StatManager'a bonus kaldırma komutu gönderir.
    /// </summary>
    private void CallStatManagerRemove(string statName, double amount)
    {
        if (StatManager.Instance == null) return;
        // dosyasındaki Remove...Bonus metotlarını kullanır
        switch (statName.ToLowerInvariant())
        {
            case "physical": StatManager.Instance.RemovePhysicalBonus(amount); break;
            case "mental": StatManager.Instance.RemoveMentalBonus(amount); break;
            case "perception": StatManager.Instance.RemovePerceptionBonus(amount); break;
            case "spiritual": StatManager.Instance.RemoveSpiritualBonus(amount); break;
            case "luck": StatManager.Instance.RemoveLuckBonus(amount); break;
            case "social": StatManager.Instance.RemoveSocialBonus(amount); break;
        }
    }

    /// <summary>
    /// UI'ın mevcut aktif efekt listesini alması için (salt okunur).
    /// (HATA 3 DÜZELTMESİ: 'ActiveStatusEffect' artık public olduğu için bu metot geçerli)
    /// </summary>
    public IEnumerable<ActiveStatusEffect> GetActiveEffects()
    {
        return _activeEffects.AsReadOnly();
    }
}