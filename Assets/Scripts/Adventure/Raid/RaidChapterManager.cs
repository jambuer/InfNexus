using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Text;



// ChapterManager'dan kopyalandı ve Raid içeriği için uyarlandı (Ayrı Bölge Butonları)
public class RaidChapterManager : MonoBehaviour // IGameDataSaveable gerekirse eklenecek
{
    // Raid içeriği için ayrı bir Singleton yapısı (veya doğrudan RaidManager üzerinden erişilebilir)
    public static RaidChapterManager Instance { get; private set; }

    [Header("UI Referansları")]
    [Tooltip("Tek bir düşmanı göstermek için kullanılacak UI prefab'ı (Üzerinde RaidEnemyUI script'i olmalı)")]
    public GameObject enemyUIPrefab;
    // Önceki/Sonraki butonlar kaldırıldı
    // public Button previousButton;
    // public Button nextButton;
    [Tooltip("Her bir raid bölgesine geçişi sağlayacak butonların listesi (Sırasıyla atanmalı!)")]
    public List<Button> chapterButtons; // Her chapter için ayrı buton referansları
    [Tooltip("Seçili olan bölgenin adını gösterecek metin (Opsiyonel)")]
    public TextMeshProUGUI chapterDisplayText; // Bölge Adı ve Numarası için Text (Opsiyonel)

    [Header("Raid Bölge/Chapter İçerikleri")]
    [Tooltip("Her bir bölgenin içeriğini (Panel, Kilit Paneli vb.) tutan liste. Buton sırasıyla eşleşmeli!")]
    public List<RaidChapterContent> chapters;

    

    
    

    private int currentChapterIndex = 0;
    // Kaydedilecek açık chapter'lar (ChapterManager gibi)
    private List<int> _unlockedChapterIndices = new List<int> { 0 }; // İlk chapter her zaman açık

    // Chapter yerine Raid Bölgesi gereksinimleri (ChapterManager ile aynı yapıda)
    [System.Serializable]
    public class RaidChapterRequirement // ChapterRequirement kopyası
    {
        public int requiredLevel = 0;
        public double requiredGold = 0;
        public double requiredNexusCoin = 0;
        // ... Diğer gereksinimler (Health, Mana, Energy, Stats, Quests) buraya eklenebilir ...
        public List<string> requiredCompletedRaidBossIDs; // Örnek: Önceki bölgenin boss'unu kesme gereksinimi
    }

    // ChapterContent yerine Raid Bölgesi içeriği
    [System.Serializable]
    public class RaidChapterContent // ChapterContent kopyası
    {
        public string chapterName; // Bölge adı (örn: "Ork Kampı", "Ejderha Yuvası")
        public GameObject contentPanel; // Bu bölgenin düşmanlarını içeren panel (Grid Layout Group'lu)
        public GameObject lockPanel; // Kilitli paneli
        public TextMeshProUGUI lockRequirementsText; // Kilit gereksinimleri metni
        public RaidChapterRequirement requirements; // Bölgeye özel gereksinimler

        [Tooltip("Bu bölgede bulunan düşmanların EnemyData asset listesi.")]
        public List<EnemyData> enemiesInChapter; 
    
    }

    void Awake()
    {
        // Singleton yapısı
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Raid paneliyle birlikte yönetiliyorsa bu gerekmeyebilir.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Buton listesi ve chapter listesi boyutlarının eşleştiğini kontrol et
        if (chapterButtons == null || chapters == null || chapterButtons.Count != chapters.Count)
        {
            Debug.LogError("RaidChapterManager: Chapter Butonları listesi ile Chapters listesi sayısı eşleşmiyor! Lütfen Inspector'ı kontrol edin.", this);
            return;
        }

        // Her butona kendi indeksine göre listener ata
        for (int i = 0; i < chapterButtons.Count; i++)
        {
            if (chapterButtons[i] != null)
            {
                int index = i; // Lambda expression içinde doğru indeksi yakalamak için geçici değişken
                chapterButtons[i].onClick.AddListener(() => GoToChapter(index));
            }
            else
            {
                 Debug.LogWarning($"RaidChapterManager: Chapter Butonları listesindeki {i}. eleman atanmamış.", this);
            }
        }

        // TODO: Kayıt sistemi entegre edilecekse, LoadChapterUnlocks() burada çağrılmalı.

        UpdateChapterDisplay(); // Başlangıçta ilk chapter'ı göster
    }

    /// <summary>
    /// Belirtilen indeksteki chapter'a gider.
    /// </summary>
    public void GoToChapter(int index)
    {
        // Geçerli bir indeks mi ve mevcut chapter'dan farklı mı?
        if (index >= 0 && index < chapters.Count && index != currentChapterIndex)
        {
            currentChapterIndex = index;
            UpdateChapterDisplay();
        }
        // Eğer tıklanan chapter kilitliyse bir uyarı verilebilir (opsiyonel)
        else if (!_unlockedChapterIndices.Contains(index))
        {
            Debug.Log($"Raid Bölgesi '{chapters[index].chapterName}' henüz kilitli.");
            // İsteğe bağlı: GameConsole.Instance.AddMessage(...) ile oyuncuya bildirim
        }
    }

    /// <summary>
    /// Aktif raid bölgesini gösterir, kilit durumunu kontrol eder ve UI'ı günceller.
    /// </summary>
    void UpdateChapterDisplay()
    {
        if (chapters == null || chapters.Count == 0) return; // Chapter yoksa çık

        // Chapter index'ini sınırlar içinde tut
        currentChapterIndex = Mathf.Clamp(currentChapterIndex, 0, chapters.Count - 1);

        // Opsiyonel: Seçili bölge adını Text'e yazdır
        if (chapterDisplayText != null)
             chapterDisplayText.text = $"{chapters[currentChapterIndex].chapterName ?? $"Bölge {currentChapterIndex + 1}"}";

        // Butonların görsellerini güncelle (seçili/kilitli durumuna göre)
        UpdateButtonVisuals();

        // Panelleri yönet
        for (int i = 0; i < chapters.Count; i++)
        {
            bool isActiveChapter = i == currentChapterIndex;
            RaidChapterContent current = chapters[i]; // Kolay erişim için

            // Kilit kontrolünü yap (Kilidi açmayı dener veya kilit panelini gösterir)
            CheckChapterLock(current, i);

            // İçerik panelini sadece aktif ve KİLİDİ AÇIK chapter için göster
            bool shouldShowContent = isActiveChapter && _unlockedChapterIndices.Contains(i);
            if (current.contentPanel != null)
            {
                current.contentPanel.SetActive(shouldShowContent);

                // Eğer bu panel gösterilecekse ve içinde düşmanlar varsa, düşman UI'larını oluştur/güncelle
                if (shouldShowContent && current.enemiesInChapter != null)
                {
                    PopulateEnemyList(current.contentPanel.transform, current.enemiesInChapter); // Yeni fonksiyonu çağır
                }
                else if (!shouldShowContent) // Eğer panel gizlenecekse içini temizleyebiliriz (opsiyonel)
                {
                    ClearEnemyList(current.contentPanel.transform);
                }
            }

            // Kilit panelinin görünürlüğü CheckChapterLock içinde ayarlanıyor.
        }
    }

    /// <summary>
    /// Butonların görsellerini (seçili, kilitli, normal) ayarlar.
    /// </summary>
    void UpdateButtonVisuals()
    {
        if (chapterButtons == null) return;

        for (int i = 0; i < chapterButtons.Count; i++)
        {
            if (chapterButtons[i] == null) continue;

            bool isUnlocked = _unlockedChapterIndices.Contains(i);
            bool isSelected = i == currentChapterIndex;

            // Butonun tıklanabilirliğini ayarla (Sadece kilidi açık olanlara tıklanabilsin)
            chapterButtons[i].interactable = isUnlocked;

            // TODO: Butonun görselini (rengini, boyutunu, sprite'ını vb.)
            // isSelected ve isUnlocked durumlarına göre değiştir.
            // Örnek: Seçili ve açık olanı parlak, açık ama seçili olmayanı normal, kilitli olanı soluk/gri yap.
            var colors = chapterButtons[i].colors;
            if (isSelected && isUnlocked) {
                colors.colorMultiplier = 1.0f; // Seçili ve açık: Parlak
            } else if (isUnlocked) {
                colors.colorMultiplier = 0.7f; // Açık ama seçili değil: Normal/Hafif soluk
            } else {
                colors.colorMultiplier = 0.4f; // Kilitli: Soluk/Gri
            }
            chapterButtons[i].colors = colors;

            // Kilitli butonun metnine "(Kilitli)" ekleyebiliriz (opsiyonel)
            // TextMeshProUGUI buttonText = chapterButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            // if (buttonText != null)
            // {
            //    buttonText.text = chapters[i].chapterName + (isUnlocked ? "" : " (Kilitli)");
            // }
        }
    }


    /// <summary>
    /// Belirtilen chapter'ın kilidini kontrol eder ve gerekiyorsa açar.
    /// </summary>
    void CheckChapterLock(RaidChapterContent chapter, int chapterIndex)
    {
        bool isUnlocked = _unlockedChapterIndices.Contains(chapterIndex);

        if (isUnlocked)
        {
            if (chapter.lockPanel != null)
                chapter.lockPanel.SetActive(false); // Kilit panelini gizle
            return; // Zaten açıksa başka işlem yapma
        }

        // Kilitli ve henüz açılmamışsa, gereksinimleri kontrol et
        if (AreRequirementsMet(chapter.requirements))
        {
            // Gereksinimler karşılandıysa kilidi aç
            _unlockedChapterIndices.Add(chapterIndex);
            // TODO: Kayıt sistemi varsa SaveChapterUnlocks() çağrılmalı.

            if (chapter.lockPanel != null)
                chapter.lockPanel.SetActive(false); // Paneli gizle

            Debug.Log($"Raid Bölgesi '{chapter.chapterName}' kilidi açıldı!");
            UpdateButtonVisuals(); // Kilit açılınca buton görsellerini güncelle
        }
        else
        {
            // Gereksinimler karşılanmadıysa kilitli paneli göster ve metni güncelle
            // Sadece AKTİF chapter için kilit panelini gösteriyoruz.
            if (chapterIndex == currentChapterIndex && chapter.lockPanel != null)
            {
                chapter.lockPanel.SetActive(true);
                if (chapter.lockRequirementsText != null)
                    chapter.lockRequirementsText.text = GetRequirementsText(chapter.requirements);
            }
            else if (chapter.lockPanel != null) // Aktif değilse kilit panelini gizle
            {
                chapter.lockPanel.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Bir Raid Bölgesinin gereksinimlerinin karşılanıp karşılanmadığını kontrol eder.
    /// </summary>
    bool AreRequirementsMet(RaidChapterRequirement req)
    {
        if (req == null) return true; // Gereksinim yoksa true

        // Örnek kontroller (ChapterManager'dan uyarlandı)
        if (LevelManager.Instance != null && LevelManager.Instance.currentLevel < req.requiredLevel) return false;
        if (CurrencyManager.Instance != null && CurrencyManager.Instance.gold < req.requiredGold) return false;
        // ... Diğer stat, kaynak kontrolleri ...

        // Örnek Boss tamamlama kontrolü (RaidManager gibi bir yerden alınacak)
        // if (req.requiredCompletedRaidBossIDs != null && RaidManager.Instance != null)
        // {
        //     foreach (string bossID in req.requiredCompletedRaidBossIDs)
        //     {
        //         if (!RaidManager.Instance.IsBossCompleted(bossID)) return false;
        //     }
        // }

        return true; // Tüm kontrollerden geçtiyse true
    }

    /// <summary>
    /// Raid Bölgesi gereksinim metnini UI için oluşturur.
    /// </summary>
    string GetRequirementsText(RaidChapterRequirement req)
    {
        if (req == null) return "Kilidi Açık";

        StringBuilder sb = new StringBuilder("Bölge Kilitli! Gereksinimler:\n\n");
        string metColorHex = ColorUtility.ToHtmlStringRGB(Color.green); // Renkleri tanımla
        string notMetColorHex = ColorUtility.ToHtmlStringRGB(Color.red);

        // Seviye
        if (req.requiredLevel > 0)
        {
            bool met = LevelManager.Instance != null && LevelManager.Instance.currentLevel >= req.requiredLevel;
            sb.AppendLine($"<color=#{(met ? metColorHex : notMetColorHex)}>- Seviye: {req.requiredLevel} (Mevcut: {LevelManager.Instance?.currentLevel ?? 0})</color>");
        }
        // Altın
        if (req.requiredGold > 0)
        {
            bool met = CurrencyManager.Instance != null && CurrencyManager.Instance.gold >= req.requiredGold;
            sb.AppendLine($"<color=#{(met ? metColorHex : notMetColorHex)}>- Altın: {req.requiredGold:F0} (Mevcut: {CurrencyManager.Instance?.gold ?? 0:F0})</color>");
        }
        // ... Diğer stat, kaynak metinleri (renk eklenmiş haliyle)...

        // Örnek Boss gereksinim metni
        // if (req.requiredCompletedRaidBossIDs != null && req.requiredCompletedRaidBossIDs.Count > 0)
        // {
        //     sb.AppendLine("- Önceki Boss'ları Tamamla:");
        //     foreach (string bossID in req.requiredCompletedRaidBossIDs)
        //     {
        //         bool completed = RaidManager.Instance?.IsBossCompleted(bossID) ?? false;
        //         sb.AppendLine($"  <color=#{(completed ? metColorHex : notMetColorHex)}>• {bossID}</color>"); // Boss ID yerine Boss Adı gösterilebilir
        //     }
        // }

        return sb.ToString();
    }
    
    /// <summary>
    /// Belirtilen content transform'unun içini verilen düşman listesiyle doldurur.
    /// </summary>
    /// <param name="contentParent">Enemy UI prefablarının ekleneceği Transform (Grid Layout Group içeren).</param>
    /// <param name="enemies">Gösterilecek EnemyData listesi.</param>
    void PopulateEnemyList(Transform contentParent, List<EnemyData> enemies)
    {
        if (enemyUIPrefab == null)
        {
            Debug.LogError("RaidChapterManager: Enemy UI Prefab atanmamış!", this);
            return;
        }
        if (contentParent == null) return;

        // Önce mevcut düşmanları temizle
        ClearEnemyList(contentParent);

        // Yeni düşmanları ekle
        foreach (EnemyData enemyData in enemies)
        {
            if (enemyData == null) continue; // Boş veri varsa atla

            GameObject enemyGO = Instantiate(enemyUIPrefab, contentParent);
            RaidEnemyUI enemyUI = enemyGO.GetComponent<RaidEnemyUI>();
            if (enemyUI != null)
            {
                enemyUI.Setup(enemyData);
            }
            else
            {
                Debug.LogError($"RaidChapterManager: Enemy UI Prefab ({enemyUIPrefab.name}) üzerinde RaidEnemyUI script'i bulunamadı!", enemyUIPrefab);
                Destroy(enemyGO); // Hatalı objeyi yok et
            }
        }
    }

    /// <summary>
    /// Belirtilen content transform'unun içindeki tüm çocuk objeleri (düşman UI'ları) siler.
    /// </summary>
    void ClearEnemyList(Transform contentParent)
    {
         if (contentParent == null) return;
        // İçindeki tüm çocukları yok et
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    // TODO: Kayıt/Yükleme fonksiyonları (GetSaveData, LoadFromData) ChapterManager'daki gibi eklenebilir.
    // Şimdilik sadece _unlockedChapterIndices listesi kullanılıyor.
}