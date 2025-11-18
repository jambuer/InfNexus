using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Sıralama için

public class CityQuestPanelUI : MonoBehaviour
{
    private List<CityQuestItemUI> instantiatedQuestItems = new List<CityQuestItemUI>();
    private bool hasSubscribed = false;



    [Header("Referanslar")]
    [Tooltip("Tüm görev verilerini içeren QuestDatabase asset'i")]
    public QuestDatabase questDatabase; // Adım 2'de oluşturduğumuz asset buraya atanacak

    [Tooltip("Görev listesinin UI'da gösterileceği container (ScrollView Content)")]
    public Transform questListContainer;

    [Tooltip("Tek bir görevi temsil eden UI prefab'ı (Üzerinde QuestItemUI script'i olmalı)")]
    public GameObject questItemPrefab;

    // Oluşturulan ve aktif olan QuestItemUI'ları takip etmek için liste
    // ========================================================================================
    // UNITY YAŞAM DÖNGÜSÜ & EVENT YÖNETİMİ
    // ========================================================================================

    void Start()
    {
        Debug.Log("CityQuestPanelUI: Start çalışıyor: {gameObject.name}");
        SubscribeToEvents();
        PopulateCityQuestList();

    }

    void OnEnable()
    {
         Debug.Log($"CityQuestPanelUI OnEnable çalışıyor (tekrar): {gameObject.name}"); // Yeni log
         // Start zaten çalıştıysa ve abonelik yapıldıysa, sadece listeyi yenile
         if (hasSubscribed)
         {
             PopulateCityQuestList();
         }
         // Start henüz çalışmadıysa, Start içinde yapılacak.
    }

    void SubscribeToEvents()
    {
        // Zaten abone olduysak tekrar yapma
        if (hasSubscribed) return;

        if (QuestManager.Instance == null)
        {
            Debug.LogError("!!! CityQuestPanelUI.SubscribeToEvents: QuestManager.Instance hala NULL!", this);
            return; // Abone olunamadı
        }

        Debug.Log($"CityQuestPanelUI.SubscribeToEvents: QuestManager.Instance bulundu: {QuestManager.Instance.gameObject.name}");
        QuestManager.Instance.OnQuestProgress += HandleQuestProgressOrCompletion;
        QuestManager.Instance.OnQuestProgressUpdate += UpdateQuestProgressBar;
        hasSubscribed = true; // Abone olundu olarak işaretle

        // Diğer abonelikler (gerekirse)
        // LevelManager.OnPlayerLeveledUp += PopulateCityQuestList;
    }

   void OnDisable()
    {
        // Event aboneliklerini iptal et (sadece abone olduysak)
        if (hasSubscribed && QuestManager.Instance != null)
        {
            Debug.Log($"CityQuestPanelUI OnDisable: Event abonelikleri kaldırılıyor."); // Yeni log
            QuestManager.Instance.OnQuestProgress -= HandleQuestProgressOrCompletion;
            QuestManager.Instance.OnQuestProgressUpdate -= UpdateQuestProgressBar;

            // Diğer abonelik iptalleri...
            // LevelManager.OnPlayerLeveledUp -= PopulateCityQuestList;
        }
        // hasSubscribed = false; // Tekrar Enable olduğunda yeniden abone olabilmesi için flag'i sıfırlayabiliriz,
                                // ama panel yok edilmiyorsa gerek yok. Eğer panel sık sık açılıp kapanacaksa
                                // abonelikleri OnEnable/OnDisable'da yapmak daha doğru olabilir,
                                // ancak Instance kontrolünü Start'tan sonraya bırakmak gerekir.
                                // Şimdilik Start/OnDisable ile deneyelim.
    }

    // ========================================================================================
    // UI YÖNETİMİ
    // ========================================================================================

    public void PopulateCityQuestList()
    {
        if (!ValidateReferences()) return; // Referanslar eksikse devam etme

        // Mevcut UI elemanlarını temizle
        foreach (Transform child in questListContainer) { Destroy(child.gameObject); }
        instantiatedQuestItems.Clear();

        foreach (QuestData quest in questDatabase.allQuests)
        {
            if (quest.questArea == QuestArea.City)
            {
                InstantiateQuestItem(quest);
            }
        }

        // QuestDatabase'deki tüm görevleri kontrol et
        

        // İsteğe bağlı: Görevleri sıralayabilirsiniz (örn: aktif olanlar üste)
         SortQuestItems();
    }

    private void InstantiateQuestItem(QuestData quest)
    {
        GameObject questItemGO = Instantiate(questItemPrefab, questListContainer);
       CityQuestItemUI cityQuestItemUI = questItemGO.GetComponent<CityQuestItemUI>();

        if (cityQuestItemUI != null)
        {
            cityQuestItemUI.Setup(quest); // CityQuestItemUI'ı görev verisiyle doldur
            instantiatedQuestItems.Add(cityQuestItemUI);

            // Başlangıç ilerleme durumunu ayarla
            float initialProgress = 0f;
            if (QuestManager.Instance != null && QuestManager.Instance.IsQuestActive(quest.questID))
            {
                // Aktif görevler için anlık progress'i almak idealdir, ancak QuestManager'da
                // bunun için doğrudan bir fonksiyon yoksa, 0 ile başlayıp event ile güncellenmesini bekleyebiliriz.
                // VEYA QuestManager'a GetCurrentProgress(questID) gibi bir fonksiyon eklenebilir.
                 initialProgress = 0f; // Şimdilik 0 varsayalım
                 // questItemUI.UpdateProgress(QuestManager.Instance.GetCurrentProgress(quest.questID)); // Eğer fonksiyon varsa
            }
             else if (QuestManager.Instance != null && QuestManager.Instance.GetCompletionCount(quest.questID) > 0 && quest.completionLimit > 0 && QuestManager.Instance.GetCompletionCount(quest.questID) >= quest.completionLimit)
             {
                 // Tamamlanmış ve limitine ulaşmış görevler için progress'i 1 yapabiliriz.
                 initialProgress = 1f;
             }

            cityQuestItemUI.UpdateProgress(initialProgress);
        }
        else
        {
            Debug.LogError($"CityQuestPanelUI: QuestItemPrefab ({questItemPrefab.name}) üzerinde CityQuestItemUI script'i bulunamadı!", this);
            Destroy(questItemGO);
        }
    }

    // ========================================================================================
    // EVENT HANDLER METOTLARI
    // ========================================================================================

    private void HandleQuestProgressOrCompletion(QuestData quest, int completionCount)
    {
        // Sadece City görevleriyle ilgileniyorsak UI'ı güncelle
        if (quest.questArea == QuestArea.City)
        {
            // Optimize Yöntem: Sadece ilgili item'ı bul ve güncelle
            CityQuestItemUI itemToUpdate = instantiatedQuestItems.Find(item => item.GetQuestID() == quest.questID);
            if (itemToUpdate != null)
            {
                 // QuestItemUI'da tamamlama durumunu güncelleyen bir fonksiyon olduğunu varsayalım
                 // Veya Setup'ı tekrar çağırabiliriz.
                 itemToUpdate.Setup(quest); // Setup içindeki mantık güncellemeyi de yapmalı (buton durumu vb.)
               //itemToUpdate.UpdateProgress(0f); // Tamamlandıktan sonra progress sıfırlanır
            }
            else
            {
                 // Eğer görev önceden listede yoktuysa (belki yeni açıldı), listeyi yeniden doldurmak gerekebilir
                 // Veya InstantiateQuestItem(quest) çağırılabilir (sıralama bozulabilir)
                 PopulateCityQuestList(); // En basit ama az optimize yöntem
            }
        }
    }

    private void UpdateQuestProgressBar(string questID, float progress)
    {
        // DEĞİŞİKLİK 4: Listede doğru tipi ara ve fonksiyonu çağır
        CityQuestItemUI itemToUpdate = instantiatedQuestItems.Find(item => item.GetQuestID() == questID);
        if (itemToUpdate != null)
        {
            itemToUpdate.UpdateProgress(progress);
        }
    }

    // ========================================================================================
    // YARDIMCI METOTLAR
    // ========================================================================================

    /// <summary>
    /// Inspector'da atanması gereken referansların kontrolünü yapar.
    /// </summary>
    private bool ValidateReferences()
    {
        bool isValid = true;
        if (questDatabase == null)
        {
            Debug.LogError("CityQuestPanelUI: QuestDatabase atanmamış!", this);
            isValid = false;
        }
        if (questListContainer == null)
        {
            Debug.LogError("CityQuestPanelUI: Quest List Container atanmamış!", this);
            isValid = false;
        }
        if (questItemPrefab == null)
        {
            Debug.LogError("CityQuestPanelUI: Quest Item Prefab atanmamış!", this);
            isValid = false;
        }
        else if (questItemPrefab.GetComponent<CityQuestItemUI>() == null)
        {
             Debug.LogError($"CityQuestPanelUI: QuestItemPrefab ({questItemPrefab.name}) üzerinde CityQuestItemUI script'i bulunmuyor!", this);
             isValid = false;
        }
        return isValid;
    }

    /// <summary>
    /// (Opsiyonel) Bir görevin genel gereksinimlerinin karşılanıp karşılanmadığını kontrol eder.
    /// </summary>
    private bool CheckGenericRequirements(List<Requirement> requirements)
    {
        // [YENİ] Artık bu işi panel değil, merkezi GameValidator yapıyor.
        return GameValidator.Instance.AreRequirementsMet(requirements);
    }


     /// <summary>
     /// (Opsiyonel) Görev listesini istenen kritere göre sıralar.
     /// </summary>
     private void SortQuestItems()
    {
        instantiatedQuestItems = instantiatedQuestItems.OrderBy(item =>
        {
            // Düzeltme: item.GetComponent<QuestItemUI>().currentQuestData yerine property'e eriş
            QuestData questData = item.CurrentQuestData;
            if (questData == null) return int.MaxValue; // Null ise sona at

            string id = questData.questID;
            bool isActive = QuestManager.Instance.IsQuestActive(id);
            int completions = QuestManager.Instance.GetCompletionCount(id);
            int limit = questData.completionLimit; // Doğrudan property'den al
            bool isCompletedAndLimited = limit > 0 && completions >= limit;

            if (isActive) return 0;
            if (!isCompletedAndLimited) return 1;
            return 2;
        }).ToList();

        for (int i = 0; i < instantiatedQuestItems.Count; i++)
        {
            instantiatedQuestItems[i].transform.SetSiblingIndex(i);
        }
    }
}