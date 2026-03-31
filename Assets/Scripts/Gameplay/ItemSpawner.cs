using KoKoKrunch.Managers;
using UnityEngine;

namespace KoKoKrunch.Gameplay
{
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Item Prefabs")]
        [SerializeField] private GameObject strawberryPrefab;
        [SerializeField] private GameObject kokoKrunchPack1Prefab;
        [SerializeField] private GameObject kokoKrunchPack2Prefab;

        [Header("Spawn Area")]
        [SerializeField] private float spawnYPosition = 6f;
        [SerializeField] private float spawnXMin = -2.5f;
        [SerializeField] private float spawnXMax = 2.5f;
        [SerializeField] private float bottomBoundary = -6f;

        private float spawnTimer;
        private float currentSpawnInterval;
        private float currentFallSpeed;

        private void OnEnable()
        {
            ResetSpawner();
        }

        private void ResetSpawner()
        {
            var config = GameManager.Instance.Config;
            currentSpawnInterval = config.initialSpawnInterval;
            currentFallSpeed = config.itemFallSpeed;
            spawnTimer = 0f;
        }

        private void Update()
        {
            if (!GameManager.Instance.IsGameActive) return;

            spawnTimer += Time.deltaTime;

            if (spawnTimer >= currentSpawnInterval)
            {
                SpawnItem();
                spawnTimer = 0f;

                var config = GameManager.Instance.Config;
                currentSpawnInterval = Mathf.Max(config.minimumSpawnInterval,
                    currentSpawnInterval - config.spawnAcceleration);
                currentFallSpeed = Mathf.Min(config.maxFallSpeed,
                    currentFallSpeed + config.fallSpeedIncrease);
            }
        }

        private void SpawnItem()
        {
            float randomX = Random.Range(spawnXMin, spawnXMax);
            Vector3 spawnPosition = new Vector3(randomX, spawnYPosition, 0f);

            GameObject prefab = GetRandomPrefab();
            if (prefab == null) return;

            GameObject item = Instantiate(prefab, spawnPosition, Quaternion.identity);
            var fallingItem = item.GetComponent<FallingItem>();
            if (fallingItem != null)
            {
                fallingItem.Initialize(currentFallSpeed, bottomBoundary);
            }
        }

        private GameObject GetRandomPrefab()
        {
            float roll = Random.value;

            if (roll < 0.4f)
                return strawberryPrefab;
            else if (roll < 0.7f)
                return kokoKrunchPack1Prefab;
            else
                return kokoKrunchPack2Prefab;
        }
    }
}
