using System;
using UnityEngine;

namespace KoKoKrunch.Data
{
    [Serializable]
    public class RuntimeGameSettings
    {
        [Header("Scoring")]
        public int strawberryPoints = 5;
        public int kokoKrunchPoints = 10;

        [Header("Difficulty")]
        public float itemFallSpeed = 4f;
        public float maxFallSpeed = 8f;
        public float fallSpeedIncrease = 0.1f;

        [Header("Spawning")]
        public float initialSpawnInterval = 1.2f;
        public float minimumSpawnInterval = 0.5f;
        public float spawnAcceleration = 0.02f;

        [Header("Game Rules")]
        public float gameDuration = 30f;
        public int maxLives = 3;

        [Header("Player")]
        public float playerMoveSpeed = 8f;

        public static RuntimeGameSettings FromConfig(GameConfig config)
        {
            return new RuntimeGameSettings
            {
                strawberryPoints = config.strawberryPoints,
                kokoKrunchPoints = config.kokoKrunchPoints,
                itemFallSpeed = config.itemFallSpeed,
                maxFallSpeed = config.maxFallSpeed,
                fallSpeedIncrease = config.fallSpeedIncrease,
                initialSpawnInterval = config.initialSpawnInterval,
                minimumSpawnInterval = config.minimumSpawnInterval,
                spawnAcceleration = config.spawnAcceleration,
                gameDuration = config.gameDuration,
                maxLives = config.maxLives,
                playerMoveSpeed = config.playerMoveSpeed
            };
        }

        public void ApplyTo(GameConfig config)
        {
            config.strawberryPoints = strawberryPoints;
            config.kokoKrunchPoints = kokoKrunchPoints;
            config.itemFallSpeed = itemFallSpeed;
            config.maxFallSpeed = maxFallSpeed;
            config.fallSpeedIncrease = fallSpeedIncrease;
            config.initialSpawnInterval = initialSpawnInterval;
            config.minimumSpawnInterval = minimumSpawnInterval;
            config.spawnAcceleration = spawnAcceleration;
            config.gameDuration = gameDuration;
            config.maxLives = maxLives;
            config.playerMoveSpeed = playerMoveSpeed;
        }
    }
}
