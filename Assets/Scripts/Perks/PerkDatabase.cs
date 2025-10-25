using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Projedeki tüm PerkDefinition asset'lerini merkezi bir yerde tutar.
/// Perk bilgilerini ID'ye göre sorgulamak için kullanılır.
/// </summary>
[CreateAssetMenu(fileName = "PerkDatabase", menuName = "Adventure/Perk Database")]
public class PerkDatabase : ScriptableObject
{
    [Tooltip("Projedeki TÜM Perk Definition (.asset) dosyalarını buraya sürükleyin.")]
    public List<PerkDefinition> allPerkDefinitions; // Inspector'dan doldurulacak liste

    private Dictionary<string, PerkDefinition> _perkLookupByID;

    private void EnsureLookupReady()
    {
        // Daha önce oluşturulmadıysa veya liste değişmişse (Editörde) yeniden oluştur
        if (_perkLookupByID == null || (_perkLookupByID.Count != (allPerkDefinitions?.Count ?? 0)))
        {
            _perkLookupByID = new Dictionary<string, PerkDefinition>();
            if (allPerkDefinitions != null)
            {
                foreach (PerkDefinition perkDef in allPerkDefinitions)
                {
                    if (perkDef != null && !string.IsNullOrEmpty(perkDef.perkID))
                    {
                        if (!_perkLookupByID.ContainsKey(perkDef.perkID))
                        {
                            _perkLookupByID.Add(perkDef.perkID, perkDef);
                        }
                        else
                        {
                            Debug.LogWarning($"PerkDatabase: Duplicate perkID '{perkDef.perkID}' found in perk asset '{perkDef.name}'. Only the first one will be used in lookup.");
                        }
                    }
                    else
                    {
                         Debug.LogWarning($"PerkDatabase: Perk asset '{perkDef?.name ?? "NULL ASSET"}' has missing data (perkID) and cannot be added to lookup.");
                    }
                }
            }
             // Editörde değilsek log yazdırabiliriz
             #if !UNITY_EDITOR
             Debug.Log($"PerkDatabase lookup created with {_perkLookupByID.Count} entries.");
             #endif
        }
    }

    /// <summary>
    /// Verilen perkID'ye göre PerkDefinition'ı bulur.
    /// </summary>
    /// <param name="perkID">Aranan perk'in benzersiz kimliği.</param>
    /// <returns>Bulunan PerkDefinition veya bulunamazsa null.</returns>
    public PerkDefinition GetPerkDefinitionByID(string perkID)
    {
        EnsureLookupReady(); // Dictionary'nin hazır olduğundan emin ol

        if (string.IsNullOrEmpty(perkID)) return null;

        _perkLookupByID.TryGetValue(perkID, out PerkDefinition perkDef);
        #if UNITY_EDITOR // Editörde uyarı verelim
        if(perkDef == null && Application.isPlaying) Debug.LogWarning($"PerkDatabase: Perk definition with ID '{perkID}' not found in database.");
        #endif
        return perkDef;
    }
}