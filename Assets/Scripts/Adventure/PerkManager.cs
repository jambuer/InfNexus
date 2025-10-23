using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Explorer panelinden ve diğer kaynaklardan elde edilen
/// kalıcı, stackable (biriktirilebilir) "Perk"leri (Ustalıkları) yönetir.
/// Bu, QuestManager'a bağlı olan tiered MasteryManager'dan ayrı bir sistemdir.
/// </summary>
public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance { get; private set; }

    // Oyuncunun sahip olduğu tüm perk'leri ve stack sayılarını tutar
    // Key: "First", Value: 3
    // Key: "Explorer", Value: 2
    private Dictionary<string, int> _perkCounts = new Dictionary<string, int>();

    // Bonusların her yerden kolayca sorgulanabilmesi için event (opsiyonel ama iyi pratik)
    public event System.Action OnPerkUpdated;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPerks(); // Kayıtlı perk'leri yükle
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Bir Perk'in stack sayısını artırır.
    /// </summary>
    /// <param name="perkName">Artırılacak Perk'in adı (örn: "First")</param>
    /// <param name="amount">Eklenecek stack sayısı</param>
    public void AddPerk(string perkName, int amount = 1)
    {
        if (!_perkCounts.ContainsKey(perkName))
        {
            _perkCounts[perkName] = 0;
        }
        _perkCounts[perkName] += amount;
        
        Debug.Log($"Perk eklendi: {perkName}, Yeni Stack: {_perkCounts[perkName]}");
        OnPerkUpdated?.Invoke();
        SavePerks();
        
        // Ödülleri doğrudan burada da dağıtabiliriz
        HandlePerkReward(perkName, amount);
    }

    /// <summary>
    /// Bir Perk'in mevcut stack sayısını döndürür.
    /// </summary>
    public int GetPerkCount(string perkName)
    {
        _perkCounts.TryGetValue(perkName, out int count);
        return count;
    }

    /// <summary>
    /// Perk'in ödülünü anında uygular (şimdilik sadece Stat/StatPuanı)
    /// </summary>
    private void HandlePerkReward(string perkName, int amount)
    {
        switch (perkName)
        {
            case "First": // Örnek 1 ve 9
                StatManager.Instance.AddAllStats(amount); // (1) stack için +1, (2) stack için +2 ekler (eğer amount 1 ve 2 ise)
                GameConsole.Instance.AddMessage($"<color=cyan>Perk Ödülü: +{amount} Tüm Statlar</color>");
                break;
            case "WoodCutter": // Örnek 5
              int pointsToAdd = 5 * amount;
              LevelManager.Instance.AddUnspentStatPoints(pointsToAdd); // LevelManager'ın kendi fonksiyonunu çağır
              GameConsole.Instance.AddMessage($"<color=green>Perk Ödülü: +{pointsToAdd} Stat Puanı</color>");
              break;
            case "Lucky": // Örnek 6
                StatManager.Instance.AddStat("Luck", 5 * amount); // Her "Lucky" alımı için +5 Luck
                GameConsole.Instance.AddMessage($"<color=cyan>Perk Ödülü: +{5 * amount} Luck</color>");
                break;
            // Diğer stat/ödül vermeyen perk'ler (Undecided, Explorer, Empty, Raid, Nexus)
            // burada bir şey yapmaz. Onların bonusları ilgili yerlerde GetPerkCount ile sorgulanır.
        }
    }


    // --- BONUS SORGULAMA FONKSİYONLARI ---
    // Diğer script'ler (StatCalculator, ExplorerManager) bonusları buradan sorgulayacak

    /// <summary>
    /// "Undecided" perk'inden gelen toplam Gold Bonus'u döndürür.
    /// </summary>
    public float GetGoldBonusPercent()
    {
        // Örnek 2: "+%5 Gold Bonus"
        // Her stack %5 veriyorsa:
        return GetPerkCount("Undecided") * 0.05f; // 1 stack = 0.05 (%5), 2 stack = 0.10 (%10)
    }

    /// <summary>
    /// "Explorer" perk'inden gelen toplam ExplorerTime süresi düşüşünü döndürür.
    /// </summary>
    public float GetExplorerTimeReduction()
    {
        // Örnek 3: "-3 dakika"
        // Her stack 3 dakika (180 saniye) veriyorsa:
        return GetPerkCount("Explorer") * 180f; // Saniye cinsinden
    }


    // --- KAYIT & YÜKLEME ---
    // (MasteryManager'dakine benzer basit bir PlayerPrefs kaydı)

    [System.Serializable]
    private class PerkSaveData
    {
        public List<string> perkNames = new List<string>();
        public List<int> perkCounts = new List<int>();
    }

    private void SavePerks()
    {
        PerkSaveData saveData = new PerkSaveData();
        saveData.perkNames = _perkCounts.Keys.ToList();
        saveData.perkCounts = _perkCounts.Values.ToList();
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("ExplorerPerkProgress", json);
    }

    private void LoadPerks()
    {
        if (PlayerPrefs.HasKey("ExplorerPerkProgress"))
        {
            string json = PlayerPrefs.GetString("ExplorerPerkProgress");
            PerkSaveData saveData = JsonUtility.FromJson<PerkSaveData>(json);
            _perkCounts.Clear();
            for (int i = 0; i < saveData.perkNames.Count; i++)
            {
                _perkCounts[saveData.perkNames[i]] = saveData.perkCounts[i];
            }
            Debug.Log("Explorer Perk (Ustalık) verisi yüklendi.");
        }
    }
}