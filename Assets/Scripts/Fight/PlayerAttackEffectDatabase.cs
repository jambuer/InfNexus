using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ kullanmak için
using PlayerFightMechanics; // Namespace'i kullanmak için

[CreateAssetMenu(fileName = "PlayerAttackEffectDatabase", menuName = "Fight/Player Attack Effect Database")]
public class PlayerAttackEffectDatabase : ScriptableObject
{
    [Tooltip("Oyunda kullanılacak TÜM PlayerAttackEffectData assetlerini buraya sürükleyin.")]
    public List<PlayerAttackEffectData> allAttackEffects;

    private Dictionary<PlayerAttackEffectType, PlayerAttackEffectData> _effectLookup;

    // Oyuna başlarken veya gerektiğinde lookup dictionary'sini oluşturur
    private void InitializeLookup()
    {
        if (_effectLookup == null)
        {
            _effectLookup = new Dictionary<PlayerAttackEffectType, PlayerAttackEffectData>();
            if (allAttackEffects != null)
            {
                foreach (var effectData in allAttackEffects)
                {
                    if (effectData != null && !_effectLookup.ContainsKey(effectData.effectType))
                    {
                        _effectLookup.Add(effectData.effectType, effectData);
                    }
                    else if(effectData != null)
                    {
                         Debug.LogWarning($"PlayerAttackEffectDatabase: Duplicate effectType '{effectData.effectType}' found. Asset: {effectData.name}");
                    }
                }
            }
             Debug.Log($"PlayerAttackEffectDatabase lookup created with {_effectLookup.Count} entries.");
        }
    }

    /// <summary>
    /// Verilen effectType'a göre PlayerAttackEffectData'yı döndürür.
    /// </summary>
    public PlayerAttackEffectData GetAttackEffectData(PlayerAttackEffectType effectType)
    {
        InitializeLookup(); // Lookup hazır değilse oluştur
        _effectLookup.TryGetValue(effectType, out PlayerAttackEffectData data);
        if(data == null && effectType != PlayerAttackEffectType.NormalHit && effectType != PlayerAttackEffectType.CriticalHit) // Normal/Crit hariç bulunamayanları logla
        {
             Debug.LogWarning($"PlayerAttackEffectDatabase: Effect data for '{effectType}' not found!");
        }
        return data;
    }

    /// <summary>
    /// Veritabanındaki tüm (aktif/öğrenilmiş?) saldırı efektlerinin listesini döndürür.
    /// </summary>
    /// <returns></returns>
    public List<PlayerAttackEffectData> GetAllEffects()
    {
        InitializeLookup();
        // TODO: Sadece oyuncunun kilidini açtığı efektleri döndüren bir mantık eklenebilir.
        // Şimdilik hepsi.
        return allAttackEffects ?? new List<PlayerAttackEffectData>();
    }
}