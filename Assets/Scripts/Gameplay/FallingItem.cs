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

        private static readonly Color StrawberryTrailColor = new Color(1f, 1f, 0.6f);
        private static readonly Color KokoPack1TrailColor = new Color(1f, 0.9f, 0.4f);
        private static readonly Color KokoPack2TrailColor = new Color(1f, 1f, 0.8f);

        public void Initialize(float speed, float boundary)
        {
            fallSpeed = speed;
            bottomBoundary = boundary;
            // SetupTrail();
        }

        private void SetupTrail()
        {
            var existing = GetComponentInChildren<TrailRenderer>();
            if (existing != null) return;

            var trailObj = new GameObject("Trail");
            trailObj.transform.SetParent(transform, false);

            var trail = trailObj.AddComponent<TrailRenderer>();
            trail.time = 0.7f;

            bool isStrawberry = itemType == ItemType.Strawberry;
            trail.widthCurve = isStrawberry
                ? new AnimationCurve(new Keyframe(0f, 4f), new Keyframe(4f, 0f))
                : new AnimationCurve(new Keyframe(0f, 6f), new Keyframe(6f, 0f));

            trail.numCornerVertices = 4;
            trail.numCapVertices = 4;
            trail.sortingOrder = 1;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            // Use the item's own SpriteRenderer material (guaranteed URP-compatible)
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                trail.material = sr.sharedMaterial;

            Color trailColor = itemType switch
            {
                ItemType.Strawberry => StrawberryTrailColor,
                ItemType.KokoKrunchPack1 => KokoPack1TrailColor,
                ItemType.KokoKrunchPack2 => KokoPack2TrailColor,
                _ => Color.white
            };

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(trailColor, 0f), new GradientColorKey(trailColor, 1f) },
                new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            trail.colorGradient = gradient;
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
            AudioManager.Instance?.PlayCatchCorrectSFX();
            Destroy(gameObject);
        }

        private void OnMissed()
        {
            GameManager.Instance.LoseLife();
            AudioManager.Instance?.PlayCatchWrongSFX();
            Destroy(gameObject);
        }
    }
}
