using KoKoKrunch.Managers;
using UnityEngine;

namespace KoKoKrunch.Gameplay
{
    public enum ItemType
    {
        Strawberry,
        KokoKrunchPack1,
        KokoKrunchPack2
    }

    public class FallingItem : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;

        private float fallSpeed;
        private float bottomBoundary;

        public void Initialize(float speed, float boundary)
        {
            fallSpeed = speed;
            bottomBoundary = boundary;
        }

        private void Update()
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            if (transform.position.y < bottomBoundary)
            {
                OnMissed();
            }
        }

        public int GetPoints()
        {
            var config = GameManager.Instance.Config;
            return itemType == ItemType.Strawberry ? config.strawberryPoints : config.kokoKrunchPoints;
        }

        public void OnCaught()
        {
            GameManager.Instance.AddScore(GetPoints());
            AudioManager.Instance?.PlayCatchSFX();
            Destroy(gameObject);
        }

        private void OnMissed()
        {
            GameManager.Instance.LoseLife();
            AudioManager.Instance?.PlayMissSFX();
            Destroy(gameObject);
        }
    }
}
