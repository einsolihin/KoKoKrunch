using System.Collections.Generic;
using System.IO;
using System.Linq;
using KoKoKrunch.Data;
using UnityEngine;

namespace KoKoKrunch.Managers
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private const string LeaderboardFileName = "leaderboard.json";
        private LeaderboardData leaderboardData;

        private string FilePath => Path.Combine(Application.persistentDataPath, LeaderboardFileName);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadLeaderboard();
        }

        public void SavePlayerScore(string playerName, int score)
        {
            var entry = new PlayerData(playerName, score);
            leaderboardData.entries.Add(entry);
            SaveLeaderboard();
        }

        public List<PlayerData> GetLeaderboard()
        {
            return leaderboardData.entries
                .OrderByDescending(e => e.score)
                .Take(GameManager.Instance.Config.maxLeaderboardEntries)
                .ToList();
        }

        public List<PlayerData> GetAllEntries()
        {
            return leaderboardData.entries
                .OrderByDescending(e => e.score)
                .ToList();
        }

        public void ClearAllData()
        {
            leaderboardData.entries.Clear();
            SaveLeaderboard();
            Debug.Log("[DataManager] All data cleared.");
        }

        public string ExportToCSV()
        {
            var csv = "Rank,Player Name,Score,Timestamp\n";
            var entries = GetAllEntries();
            for (int i = 0; i < entries.Count; i++)
            {
                csv += $"{i + 1},{entries[i].playerName},{entries[i].score},{entries[i].timestamp}\n";
            }

            var csvPath = Path.Combine(Application.persistentDataPath, "leaderboard_export.csv");
            File.WriteAllText(csvPath, csv);
            Debug.Log($"CSV exported to: {csvPath}");

            return csvPath;
        }

        private void LoadLeaderboard()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                leaderboardData = JsonUtility.FromJson<LeaderboardData>(json);
            }
            else
            {
                leaderboardData = new LeaderboardData();
            }
        }

        private void SaveLeaderboard()
        {
            var json = JsonUtility.ToJson(leaderboardData, true);
            File.WriteAllText(FilePath, json);
        }
    }
}
