using System.Collections;
using System.Runtime.CompilerServices;
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
        [SerializeField]
        private float delayNextScene = 0.5f; // Delay before allowing scene change (to prevent accidental clicks)
        private bool canChangeScene = false;


        private void Start()
        {
            StartCoroutine(EnableSceneChangeAfterDelay());
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
        IEnumerator EnableSceneChangeAfterDelay()
        {
            canChangeScene = false;
            yield return new WaitForSeconds(delayNextScene);
            canChangeScene = true;
        }
        private void OnLeaderboardClicked()
        {
            if (!canChangeScene) return;
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadLeaderboard();
        }

        private void OnPlayAgainClicked()
        {
            if (!canChangeScene) return;
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadNameInput();
        }
    }
}
