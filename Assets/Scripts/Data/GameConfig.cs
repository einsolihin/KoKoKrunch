using UnityEngine;

namespace KoKoKrunch.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "KoKo Krunch/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        public int maxLives = 3;
        public float gameDuration = 30f;

        [Header("Scoring")]
        public int strawberryPoints = 5;
        public int kokoKrunchPoints = 10;

        [Header("Spawning")]
        public float initialSpawnInterval = 1.2f;
        public float minimumSpawnInterval = 0.5f;
        public float spawnAcceleration = 0.02f;
        public float itemFallSpeed = 4f;
        public float maxFallSpeed = 8f;
        public float fallSpeedIncrease = 0.1f;

        [Header("Leaderboard")]
        public int maxLeaderboardEntries = 20;

        [Header("Player Movement")]
        public float playerMoveSpeed = 8f;
    }
}
