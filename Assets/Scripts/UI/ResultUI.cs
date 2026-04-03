using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class ResultUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI congratsText;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button playAgainButton;

        private void Start()
        {
            int score = GameManager.Instance.CurrentScore;
            bool isWin = GameManager.Instance.IsWin;

            scoreText.text = score.ToString();

            if (congratsText != null)
                congratsText.text = isWin ? "CONGRATULATIONS" : "BETTER LUCK NEXT TIME";

            // Play win or lose BGM (loops until leaving this page)
            if (isWin)
                AudioManager.Instance?.PlayWinBGM();
            else
                AudioManager.Instance?.PlayLoseBGM();

            if (leaderboardButton != null)
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        private void OnLeaderboardClicked()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadLeaderboard();
        }

        private void OnPlayAgainClicked()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadNameInput();
        }
    }
}
