using System;
using KoKoKrunch.Data;
using UnityEngine;

namespace KoKoKrunch.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig config;

        public GameConfig Config => config;
        public string PlayerName { get; set; }
        public int CurrentScore { get; private set; }
        public int CurrentLives { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool IsGameActive { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnLivesChanged;
        public event Action<float> OnTimeChanged;
        public event Action OnGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfig>();
            }
        }

        public void StartGame()
        {
            // Apply runtime settings over ScriptableObject defaults
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.ApplyToConfig();
            }

            CurrentScore = 0;
            CurrentLives = config.maxLives;
            TimeRemaining = config.gameDuration;
            IsGameActive = true;

            OnScoreChanged?.Invoke(CurrentScore);
            OnLivesChanged?.Invoke(CurrentLives);
            OnTimeChanged?.Invoke(TimeRemaining);
        }

        private void Update()
        {
            if (!IsGameActive) return;

            TimeRemaining -= Time.deltaTime;
            OnTimeChanged?.Invoke(TimeRemaining);

            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                EndGame();
            }
        }

        public void AddScore(int points)
        {
            if (!IsGameActive) return;

            CurrentScore += points;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void LoseLife()
        {
            if (!IsGameActive) return;

            CurrentLives--;
            OnLivesChanged?.Invoke(CurrentLives);

            if (CurrentLives <= 0)
            {
                CurrentLives = 0;
                EndGame();
            }
        }

        private void EndGame()
        {
            IsGameActive = false;
            OnGameOver?.Invoke();

            DataManager.Instance.SavePlayerScore(PlayerName, CurrentScore);
        }
    }
}
