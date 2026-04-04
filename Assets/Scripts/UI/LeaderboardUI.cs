using System.Collections;
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
        [Header("Top 1 (Center)")]
        [SerializeField] private TextMeshProUGUI top1RankText;
        [SerializeField] private TextMeshProUGUI top1NameScoreText;
        [SerializeField] private TextMeshProUGUI top1NameText;
        [SerializeField] private TextMeshProUGUI top1ScoreText;

        [Header("Top 2 (Left)")]
        [SerializeField] private TextMeshProUGUI top2RankText;
        [SerializeField] private TextMeshProUGUI top2NameScoreText;
        [SerializeField] private TextMeshProUGUI top2NameText;
        [SerializeField] private TextMeshProUGUI top2ScoreText;

        [Header("Top 3 (Right)")]
        [SerializeField] private TextMeshProUGUI top3RankText;
        [SerializeField] private TextMeshProUGUI top3NameScoreText;
        [SerializeField] private TextMeshProUGUI top3NameText;
        [SerializeField] private TextMeshProUGUI top3ScoreText;

        [Header("Leaderboard List (#4+)")]
        [SerializeField] private Transform leaderboardContent;
        [SerializeField] private GameObject leaderboardEntryPrefab;

        [Header("Buttons")]
        [SerializeField] private Button playAgainButton;

        [SerializeField]
        private float delayNextScene = 0.5f; // Delay before allowing scene change (to prevent accidental clicks)
        private bool canChangeScene = false;

        private void Start()
        {
            StartCoroutine(EnableSceneChangeAfterDelay());
            AudioManager.Instance?.PlayMenuBGM();
            PopulateLeaderboard();

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        private void PopulateLeaderboard()
        {
            List<PlayerData> entries = DataManager.Instance.GetLeaderboard();
            int maxLeaderboardEntries = GameManager.Instance.Config.maxLeaderboardEntries;

            // Top 1
            SetTopPlayer(top1RankText, top1NameText, top1ScoreText, top1NameScoreText, entries, 0);

            // Top 2
            SetTopPlayer(top2RankText, top2NameText, top2ScoreText, top2NameScoreText, entries, 1);

            // Top 3
            SetTopPlayer(top3RankText, top3NameText, top3ScoreText, top3NameScoreText, entries, 2);

            // Clear existing list entries
            foreach (Transform child in leaderboardContent)
            {
                Destroy(child.gameObject);
            }

            // Populate #4 onwards
            for (int i = 3; i < maxLeaderboardEntries; i++)
            {
                GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
                var entry = entryObj.GetComponent<LeaderboardEntry>();

                if (entries.Count <=i)
                {
                    if (entry != null)
                    {
                        entry.SetData(i + 1, "EMPTY", 0);
                    }
                    else
                    {
                        // Fallback: find texts by component order
                        var texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                        if (texts.Length >= 3)
                        {
                            texts[0].text = $"#{i + 1}";
                            texts[1].text = "EMPTY";
                            texts[2].text = "0 POINTS";
                        }
                    }
                }
                else{
                    if (entry != null)
                    {
                        entry.SetData(i + 1, entries[i].playerName, entries[i].score);
                    }
                    else
                    {
                        // Fallback: find texts by component order
                        var texts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
                        if (texts.Length >= 3)
                        {
                            texts[0].text = $"#{i + 1}";
                            texts[1].text = entries[i].playerName;
                            texts[2].text = $"{entries[i].score} POINTS";
                        }
                    }
                }
            }
        }

        private void SetTopPlayer(TextMeshProUGUI rankText, TextMeshProUGUI nameText,
            TextMeshProUGUI scoreText, TextMeshProUGUI nameScoreText, List<PlayerData> entries, int index)
        {
            if (index < entries.Count)
            {
                if (rankText != null) rankText.text = $"#{index + 1}";
                if (nameText != null) nameText.text = entries[index].playerName;
                if (scoreText != null) scoreText.text = $"{entries[index].score} POINTS";
                if (nameScoreText != null) nameScoreText.text = $"{entries[index].playerName} {entries[index].score} POINTS";

            }
            else
            {
                if (rankText != null) rankText.text = $"#{index + 1}";
                if (nameText != null) nameText.text = "XXXX";
                if (scoreText != null) scoreText.text = "0 POINTS";
                if (nameScoreText != null) nameScoreText.text = "XXXX 0 POINTS";
            }
        }

        IEnumerator EnableSceneChangeAfterDelay()
        {
            canChangeScene = false;
            yield return new WaitForSeconds(delayNextScene);
            canChangeScene = true;
        }

        private void OnPlayAgainClicked()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadLanding();
        }
    }
}
