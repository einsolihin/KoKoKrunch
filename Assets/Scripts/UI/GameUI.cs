using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image[] heartIcons;

        private void Start()
        {
            AudioManager.Instance?.PlayGameBGM();
            GameManager.Instance.StartGame();

            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnTimeChanged += UpdateTimer;
            GameManager.Instance.OnGameOver += OnGameOver;

            UpdateScore(0);
            UpdateLives(GameManager.Instance.Config.maxLives);
            UpdateTimer(GameManager.Instance.Config.gameDuration);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnTimeChanged -= UpdateTimer;
            GameManager.Instance.OnGameOver -= OnGameOver;
        }

        private void UpdateScore(int score)
        {
            scoreText.text = score.ToString("D2");
        }

        private void UpdateTimer(float time)
        {
            int seconds = Mathf.CeilToInt(time);
            timerText.text = seconds.ToString("D2");
        }

        private void UpdateLives(int lives)
        {
            for (int i = 0; i < heartIcons.Length; i++)
            {
                heartIcons[i].gameObject.SetActive(i < lives);
            }
        }

        private void OnGameOver()
        {
            AudioManager.Instance?.StopBGM();

            Invoke(nameof(GoToResult), 1.5f);
        }

        private void GoToResult()
        {
            SceneLoader.LoadResult();
        }
    }
}
