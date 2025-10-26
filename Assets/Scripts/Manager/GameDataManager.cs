using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic; 
using System.Linq; 

/// <summary>
/// Oyunun tamamı için kaydetme, yükleme ve yeniden başlatma işlemlerini yöneten merkezi yönetici.
/// 'IGameDataSaveable' arayüzünü uygulayanları otomatik bulur.
/// </summary>
public class GameDataManager : Singleton<GameDataManager>
{
    public static int MaxSaveSlots = 3;

    // YENİ: Arayüz adı değişti
    private List<object> _saveableManagers;

    protected override void Awake()
    {
        base.Awake();

        // YENİ: Arayüz adı değişti
        _saveableManagers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<object>()
            .Where(m => m.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGameDataSaveable<>)))
            .ToList();
        
        // YENİ: Arayüz adı değişti
        Debug.Log($"[GameDataManager] Sahnede {_saveableManagers.Count} adet kaydedilebilir yönetici (IGameDataSaveable) bulundu.");
    }

    public void SaveGame(int slotNumber)
    {
        Debug.Log($"--- SAVING GAME TO SLOT {slotNumber} ---");

        GameSaveData saveData = new GameSaveData();
        saveData.saveTime = DateTime.Now;

        foreach (var manager in _saveableManagers)
        {
            // YENİ: Tüm arayüz adları değişti
            if (manager is IGameDataSaveable<StatSaveData> statManager)
                saveData.statData = statManager.GetSaveData();

            else if (manager is IGameDataSaveable<LevelSaveData> levelManager)
                saveData.levelData = levelManager.GetSaveData();

            else if (manager is IGameDataSaveable<CurrencySaveData> currencyManager)
                saveData.currencyData = currencyManager.GetSaveData();

            else if (manager is IGameDataSaveable<QuestSaveData> questManager)
                saveData.questData = questManager.GetSaveData();

            else if (manager is IGameDataSaveable<MasterySaveData> masteryManager)
                saveData.masteryData = masteryManager.GetSaveData();

            else if (manager is IGameDataSaveable<ResourceSaveData> resourceManager)
                saveData.resourceData = resourceManager.GetSaveData();

            else if (manager is IGameDataSaveable<PerkSaveData> perkManager)
                saveData.perkData = perkManager.GetSaveData();

            else if (manager is IGameDataSaveable<ExplorerSaveData> explorerManager)
                saveData.explorerData = explorerManager.GetSaveData();

            else if (manager is IGameDataSaveable<ChapterSaveData> chapterManager)
                saveData.chapterData = chapterManager.GetSaveData();
            else if (manager is IGameDataSaveable<TimerSaveData> timerManager)
                saveData.timerData = timerManager.GetSaveData();
    
        }

        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(GetSaveFilePath(slotNumber), json);

        Debug.Log($"--- GAME SAVED TO SLOT {slotNumber} ---");
        GameConsole.Instance?.AddMessage($"<color=lightblue>Oyun {slotNumber}. slota kaydedildi.</color>");
    }

    public void LoadGame(int slotNumber)
    {
        string filePath = GetSaveFilePath(slotNumber);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Load failed: Save file for slot {slotNumber} not found!");
            GameConsole.Instance?.AddMessage($"<color=red>Yükleme başarısız: {slotNumber}. slot boş.</color>");
            return;
        }

        Debug.Log($"--- LOADING GAME FROM SLOT {slotNumber} ---");

        string json = File.ReadAllText(filePath);
        GameSaveData saveData = JsonConvert.DeserializeObject<GameSaveData>(json);
        LoadManagerData<TimerSaveData>(saveData.timerData);

        // Yükleme sırasını koru
        LoadManagerData<StatSaveData>(saveData.statData);
        LoadManagerData<LevelSaveData>(saveData.levelData);
        LoadManagerData<CurrencySaveData>(saveData.currencyData);
        LoadManagerData<QuestSaveData>(saveData.questData);
        LoadManagerData<MasterySaveData>(saveData.masteryData);
        LoadManagerData<PerkSaveData>(saveData.perkData);
        LoadManagerData<ExplorerSaveData>(saveData.explorerData);
        LoadManagerData<ChapterSaveData>(saveData.chapterData);
        
        // Kaynaklar en son
        LoadManagerData<ResourceSaveData>(saveData.resourceData);
        
        Debug.Log("--- GAME LOADED, RELOADING SCENE ---");
        GameConsole.Instance?.AddMessage($"<color=lightblue>Oyun {slotNumber}. slottan yükleniyor...</color>");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // YENİ: Arayüz adı değişti
    private void LoadManagerData<T>(T data)
    {
        if (data == null || data.Equals(default(T))) return;
        
        // YENİ: Arayüz adı değişti
        IGameDataSaveable<T> manager = _saveableManagers
            .OfType<IGameDataSaveable<T>>()
            .FirstOrDefault();
            
        if (manager != null)
        {
            manager.LoadFromData(data);
        }
        else
        {
            // YENİ: Arayüz adı değişti
            Debug.LogWarning($"[GameDataManager] {typeof(T)} tipindeki veriyi yükleyecek bir yönetici (IGameDataSaveable) bulunamadı.");
        }
    }

    public void RestartGame()
    {
        Debug.Log("--- RESTARTING GAME, DELETING ALL SAVE FILES ---");
        for (int i = 1; i <= MaxSaveSlots; i++)
        {
            string filePath = GetSaveFilePath(i);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Deleted save file: {filePath}");
            }
        }

        Debug.Log("--- ALL DATA DELETED, RELOADING SCENE ---");
        GameConsole.Instance?.AddMessage("<color=yellow>Oyun yeniden başlatılıyor...</color>");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private string GetSaveFilePath(int slotNumber)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotNumber}.json");
    }
}