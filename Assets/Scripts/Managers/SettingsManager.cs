using System.IO;
using KoKoKrunch.Data;
using UnityEngine;

namespace KoKoKrunch.Managers
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        private const string SettingsFileName = "game_settings.json";
        private RuntimeGameSettings currentSettings;

        private string FilePath => Path.Combine(Application.persistentDataPath, SettingsFileName);

        public RuntimeGameSettings Settings => currentSettings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
        }

        public void LoadSettings()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                currentSettings = JsonUtility.FromJson<RuntimeGameSettings>(json);
                Debug.Log("[SettingsManager] Loaded runtime settings from JSON.");
            }
            else
            {
                ResetToDefaults();
            }
        }

        public void SaveSettings()
        {
            var json = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(FilePath, json);
            Debug.Log($"[SettingsManager] Settings saved to: {FilePath}");
        }

        public void SaveSettings(RuntimeGameSettings settings)
        {
            currentSettings = settings;
            SaveSettings();
        }

        public void ResetToDefaults()
        {
            var config = GameManager.Instance != null ? GameManager.Instance.Config : Resources.Load<GameConfig>("GameConfig");

            if (config != null)
            {
                currentSettings = RuntimeGameSettings.FromConfig(config);
            }
            else
            {
                currentSettings = new RuntimeGameSettings();
            }

            SaveSettings();
            Debug.Log("[SettingsManager] Settings reset to defaults.");
        }

        public void ApplyToConfig()
        {
            if (GameManager.Instance != null && currentSettings != null)
            {
                currentSettings.ApplyTo(GameManager.Instance.Config);
                Debug.Log("[SettingsManager] Runtime settings applied to GameConfig.");
            }
        }
    }
}
