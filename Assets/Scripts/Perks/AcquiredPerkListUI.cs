using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;


/// <summary>
/// PerkManager'da kazanılmış olan perk'leri bir ScrollView Content'i içinde listeleyen ana UI script'i.
/// PerkManager'daki mevcut PerkDatabase'i kullanır.
/// </summary>
public class AcquiredPerkListUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField]
    private Transform contentParent; // ScrollView'ın "Content" objesini buraya sürükleyin

    [SerializeField]
    private GameObject perkItemPrefab; // Bir önceki adımda oluşturduğunuz "AcquiredPerkItemUI" script'ine sahip prefab

    private bool _isInitialized = false;

    void OnEnable()
    {
        if (PerkManager.Instance == null)
        {
            Debug.LogError("[AcquiredPerkListUI] PerkManager bulunamadı!", this);
            return;
        }

        // PerkManager'daki veritabanının yüklendiğinden emin ol
        if (PerkManager.Instance.perkDatabase == null)
        {
            Debug.LogError("[AcquiredPerkListUI] PerkManager'daki PerkDatabase referansı atanmamış!", this);
            return;
        }

        // PerkManager'ı dinlemeye başla (event adı dosyanızdakine göre düzeltildi)
        PerkManager.Instance.OnPerkUpdated += RefreshUI;
        _isInitialized = true;
        
        // UI'ı hemen tazele
        RefreshUI();
    }

    void OnDisable()
    {
        // Dinlemeyi bırak
        if (PerkManager.Instance != null && _isInitialized)
        {
            PerkManager.Instance.OnPerkUpdated -= RefreshUI;
        }
    }

    /// <summary>
    /// Perk listesi değiştiğinde (OnPerkUpdated eventi tetiklendiğinde) UI'ı yeniden çizer.
    /// </summary>
    public void RefreshUI()
    {
        if (PerkManager.Instance == null || contentParent == null || perkItemPrefab == null)
        {
            Debug.LogWarning("[AcquiredPerkListUI] Referanslar eksik, UI yenilenemiyor.");
            return;
        }

        // 1. Mevcut listeyi temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. PerkManager'daki mevcut veritabanını al
        PerkDatabase perkDB = PerkManager.Instance.perkDatabase;
        if (perkDB == null)
        {
            Debug.LogError("[AcquiredPerkListUI] PerkDatabase bulunamadı!", this);
            return;
        }

        // 3. PerkManager'dan kazanılmış tüm perk'leri al (Yeni eklediğimiz fonksiyon ile)
        Dictionary<string, int> acquiredPerks = PerkManager.Instance.GetPerkCounts();
        
        if (acquiredPerks.Count == 0)
        {
            // Gösterilecek perk yoksa çık
            return;
        }

        // 4. Her bir kazanılmış perk için UI objesi oluştur
        foreach (var perkEntry in acquiredPerks)
        {
            string perkID = perkEntry.Key;
            int perkStackCount = perkEntry.Value;

            if (perkStackCount <= 0) continue; // 0 seviye perk'leri gösterme

            // 5. Veritabanından bu ID'ye ait PerkDefinition verisini bul
            // (PerkManager'ın kullandığı GetBonusFromPerks'e bakarak PerkDatabase'de GetPerkDefinitionByID diye bir fonksiyon olduğunu varsayıyorum)
            PerkDefinition perkDef = perkDB.GetPerkDefinitionByID(perkID); // <-- PerkDatabase'inizdeki fonksiyonun adı farklıysa burayı güncelleyin

            if (perkDef != null)
            {
                // 6. Prefab'ı Content içine oluştur
                GameObject perkInstance = Instantiate(perkItemPrefab, contentParent);

                // 7. Prefab'ın script'ine verileri gönder
                AcquiredPerkItemUI itemScript = perkInstance.GetComponent<AcquiredPerkItemUI>();
                if (itemScript != null)
                {
                    itemScript.Setup(perkDef, perkStackCount);
                }
                else
                {
                    Debug.LogError($"[AcquiredPerkListUI] 'perkItemPrefab' üzerinde 'AcquiredPerkItemUI' script'i bulunamadı!", perkItemPrefab);
                }
            }
            else
            {
                Debug.LogWarning($"[AcquiredPerkListUI] '{perkID}' ID'sine sahip perk, veritabanında bulunamadı. UI'a eklenemiyor.");
            }
        }
    }
}