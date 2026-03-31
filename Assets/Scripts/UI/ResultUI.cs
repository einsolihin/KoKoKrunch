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
            scoreText.text = score.ToString();

            if (congratsText != null)
                congratsText.text = "CONGRATULATIONS";

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
