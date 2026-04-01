using System.Collections.Generic;
using KoKoKrunch.Data;
using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class AdminUI : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private Button leaderboardTabButton;
        [SerializeField] private Button settingsTabButton;
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Leaderboard")]
        [SerializeField] private Transform tableContent;
        [SerializeField] private GameObject tableRowPrefab;
        [SerializeField] private TextMeshProUGUI totalEntriesText;
        [SerializeField] private TextMeshProUGUI exportStatusText;
        [SerializeField] private Button exportCSVButton;
        [SerializeField] private Button clearDataButton;

        [Header("Settings - Scoring")]
        [SerializeField] private TMP_InputField strawberryPointsInput;
        [SerializeField] private TMP_InputField kokoKrunchPointsInput;

        [Header("Settings - Difficulty")]
        [SerializeField] private TMP_InputField itemFallSpeedInput;
        [SerializeField] private TMP_InputField maxFallSpeedInput;
        [SerializeField] private TMP_InputField fallSpeedIncreaseInput;

        [Header("Settings - Spawning")]
        [SerializeField] private TMP_InputField initialSpawnIntervalInput;
        [SerializeField] private TMP_InputField minimumSpawnIntervalInput;
        [SerializeField] private TMP_InputField spawnAccelerationInput;

        [Header("Settings - Game Rules")]
        [SerializeField] private TMP_InputField gameDurationInput;
        [SerializeField] private TMP_InputField maxLivesInput;

        [Header("Settings - Player")]
        [SerializeField] private TMP_InputField playerMoveSpeedInput;

        [Header("Settings - Buttons")]
        [SerializeField] private Button saveSettingsButton;
        [SerializeField] private Button resetDefaultsButton;
        [SerializeField] private TextMeshProUGUI settingsStatusText;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        // Tab colors
        private readonly Color activeTabColor = new Color(0.9f, 0.75f, 0.2f, 1f);
        private readonly Color inactiveTabColor = new Color(0.35f, 0.28f, 0.22f, 1f);

        private void Start()
        {
            // Wire button listeners
            leaderboardTabButton.onClick.AddListener(() => SwitchTab(true));
            settingsTabButton.onClick.AddListener(() => SwitchTab(false));

            exportCSVButton.onClick.AddListener(OnExportCSV);
            clearDataButton.onClick.AddListener(OnClearData);

            saveSettingsButton.onClick.AddListener(OnSaveSettings);
            resetDefaultsButton.onClick.AddListener(OnResetDefaults);

            backButton.onClick.AddListener(OnBack);

            // Initialize
            SwitchTab(true);
            PopulateTable();
            PopulateSettings();
        }

        #region Tab Switching

        private void SwitchTab(bool showLeaderboard)
        {
            leaderboardPanel.SetActive(showLeaderboard);
            settingsPanel.SetActive(!showLeaderboard);

            leaderboardTabButton.GetComponent<Image>().color = showLeaderboard ? activeTabColor : inactiveTabColor;
            settingsTabButton.GetComponent<Image>().color = showLeaderboard ? inactiveTabColor : activeTabColor;
        }

        #endregion

        #region Leaderboard

        private void PopulateTable()
        {
            foreach (Transform child in tableContent)
            {
                Destroy(child.gameObject);
            }

            List<PlayerData> entries = DataManager.Instance.GetAllEntries();

            if (totalEntriesText != null)
                totalEntriesText.text = $"Total Entries: {entries.Count}";

            for (int i = 0; i < entries.Count; i++)
            {
                GameObject row = Instantiate(tableRowPrefab, tableContent);
                var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 4)
                {
                    texts[0].text = $"{i + 1}";
                    texts[1].text = entries[i].playerName;
                    texts[2].text = entries[i].score.ToString();
                    texts[3].text = entries[i].timestamp;
                }
            }
        }

        private void OnExportCSV()
        {
            string path = DataManager.Instance.ExportToCSV();
            if (exportStatusText != null)
                exportStatusText.text = $"Exported to:\n{path}";
        }

        private void OnClearData()
        {
            DataManager.Instance.ClearAllData();
            PopulateTable();
            if (exportStatusText != null)
                exportStatusText.text = "All data cleared.";
        }

        #endregion

        #region Settings

        private void PopulateSettings()
        {
            if (SettingsManager.Instance == null) return;

            var s = SettingsManager.Instance.Settings;

            strawberryPointsInput.text = s.strawberryPoints.ToString();
            kokoKrunchPointsInput.text = s.kokoKrunchPoints.ToString();
            itemFallSpeedInput.text = s.itemFallSpeed.ToString("F2");
            maxFallSpeedInput.text = s.maxFallSpeed.ToString("F2");
            fallSpeedIncreaseInput.text = s.fallSpeedIncrease.ToString("F2");
            initialSpawnIntervalInput.text = s.initialSpawnInterval.ToString("F2");
            minimumSpawnIntervalInput.text = s.minimumSpawnInterval.ToString("F2");
            spawnAccelerationInput.text = s.spawnAcceleration.ToString("F2");
            gameDurationInput.text = s.gameDuration.ToString("F1");
            maxLivesInput.text = s.maxLives.ToString();
            playerMoveSpeedInput.text = s.playerMoveSpeed.ToString("F1");
        }

        private RuntimeGameSettings CollectSettingsFromUI()
        {
            var s = new RuntimeGameSettings();

            int.TryParse(strawberryPointsInput.text, out s.strawberryPoints);
            int.TryParse(kokoKrunchPointsInput.text, out s.kokoKrunchPoints);
            float.TryParse(itemFallSpeedInput.text, out s.itemFallSpeed);
            float.TryParse(maxFallSpeedInput.text, out s.maxFallSpeed);
            float.TryParse(fallSpeedIncreaseInput.text, out s.fallSpeedIncrease);
            float.TryParse(initialSpawnIntervalInput.text, out s.initialSpawnInterval);
            float.TryParse(minimumSpawnIntervalInput.text, out s.minimumSpawnInterval);
            float.TryParse(spawnAccelerationInput.text, out s.spawnAcceleration);
            float.TryParse(gameDurationInput.text, out s.gameDuration);
            int.TryParse(maxLivesInput.text, out s.maxLives);
            float.TryParse(playerMoveSpeedInput.text, out s.playerMoveSpeed);

            return s;
        }

        private void OnSaveSettings()
        {
            var settings = CollectSettingsFromUI();
            SettingsManager.Instance.SaveSettings(settings);
            SettingsManager.Instance.ApplyToConfig();

            if (settingsStatusText != null)
                settingsStatusText.text = "Settings saved!";
        }

        private void OnResetDefaults()
        {
            SettingsManager.Instance.ResetToDefaults();
            PopulateSettings();

            if (settingsStatusText != null)
                settingsStatusText.text = "Reset to defaults.";
        }

        #endregion

        private void OnBack()
        {
            SceneLoader.LoadLanding();
        }
    }
}
