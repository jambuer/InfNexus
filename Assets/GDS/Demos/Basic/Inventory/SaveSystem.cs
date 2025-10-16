using System.IO;
using UnityEngine;

namespace GDS.Demos.Basic {

    public static class SaveSystem {
        private static string path => Application.persistentDataPath + "/basic-save.json";

        public static void Save(CharacterDto data) {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            Debug.Log("Saved to: " + path);
        }

        public static CharacterDto Load() {
            Debug.Log("should load from path: " + path);

            if (!File.Exists(path)) {
                Debug.LogWarning("Save file not found.");
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<CharacterDto>(json);
        }
    }
}