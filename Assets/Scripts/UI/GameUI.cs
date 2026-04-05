using System.Collections;
using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject[] heartIcons;
        [SerializeField] private SkeletonGraphic scoreSkeleton;

        private void Start()
        {
            StartCoroutine(PlayScoreAnimation());
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
        IEnumerator PlayScoreAnimation()
        {
            if (scoreSkeleton == null) yield break;

            yield return new WaitForSeconds(1f);
            scoreSkeleton.AnimationState.GetCurrent(0).TimeScale = 0f;
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
                var heart = heartIcons[i];
                var skeleton = heart.GetComponent<SkeletonGraphic>();

                if (skeleton == null) continue;

                if (i < lives)
                {
                    heart.SetActive(true);
                    skeleton.AnimationState.SetAnimation(0, "Heart_Life", true);
                }
                else
                {
                    var track = skeleton.AnimationState.SetAnimation(0, "Heart_Lost", false);

                    // Capture correct reference
                    track.Complete += (entry) =>
                    {
                        heart.SetActive(false);
                    };
                }
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
