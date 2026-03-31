using System.Collections.Generic;
using KoKoKrunch.Data;
using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class LeaderboardUI : MonoBehaviour
    {
        [Header("Top Player")]
        [SerializeField] private TextMeshProUGUI topPlayerRankText;
        [SerializeField] private TextMeshProUGUI topPlayerNameText;
        [SerializeField] private TextMeshProUGUI topPlayerScoreText;

        [Header("Leaderboard List")]
        [SerializeField] private Transform leaderboardContent;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;

        private void Start()
        {
            PopulateLeaderboard();

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        private void PopulateLeaderboard()
        {
            List<PlayerData> entries = DataManager.Instance.GetLeaderboard();

            if (entries.Count > 0)
            {
                var top = entries[0];
                if (topPlayerRankText != null) topPlayerRankText.text = "#1";
                if (topPlayerNameText != null) topPlayerNameText.text = top.playerName;
                if (topPlayerScoreText != null) topPlayerScoreText.text = $"{top.score} POINTS";
            }

            foreach (Transform child in leaderboardContent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                var texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 3)
                {
                    texts[0].text = $"#{i + 1}";
                    texts[1].text = entries[i].playerName;
                    texts[2].text = $"{entries[i].score} POINTS";
                }
            }
        }

        private void OnPlayAgainClicked()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadLanding();
        }
    }
}
