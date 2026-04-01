using KoKoKrunch.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KoKoKrunch.Managers
{
    public class ScreensaverManager : MonoBehaviour
    {
        public static ScreensaverManager Instance { get; private set; }

        private const float InactivityTimeout = 40f;
        private float inactivityTimer;
        private bool screensaverActive;

        private Canvas screensaverCanvas;
        private GameObject screensaverOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            CreateScreensaverOverlay();
            ResetTimer();
        }

        private void Update()
        {
            if (screensaverActive)
            {
                if (HasAnyInput())
                {
                    DismissScreensaver();
                }
                return;
            }

            // Don't run screensaver during gameplay or admin
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == SceneLoader.GameScene || currentScene == SceneLoader.AdminScene)
            {
                ResetTimer();
                return;
            }

            if (HasAnyInput())
            {
                ResetTimer();
            }
            else
            {
                inactivityTimer -= Time.unscaledDeltaTime;
                if (inactivityTimer <= 0f)
                {
                    ActivateScreensaver();
                }
            }
        }

        private bool HasAnyInput()
        {
            // Touch
            if (Input.touchCount > 0) return true;

            // Mouse click
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) return true;

            // Any key
            if (Input.anyKeyDown) return true;

            // Mouse movement
            if (Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
                return true;

            return false;
        }

        public void ResetTimer()
        {
            inactivityTimer = InactivityTimeout;
        }

        private void ActivateScreensaver()
        {
            screensaverActive = true;
            screensaverOverlay.SetActive(true);
        }

        private void DismissScreensaver()
        {
            screensaverActive = false;
            screensaverOverlay.SetActive(false);
            ResetTimer();
            SceneLoader.LoadLanding();
        }

        private void CreateScreensaverOverlay()
        {
            // Create a persistent canvas for the screensaver
            var canvasObj = new GameObject("ScreensaverCanvas");
            canvasObj.transform.SetParent(transform, false);

            screensaverCanvas = canvasObj.AddComponent<Canvas>();
            screensaverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            screensaverCanvas.sortingOrder = 999; // On top of everything

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Fullscreen dark overlay
            screensaverOverlay = new GameObject("ScreensaverOverlay");
            screensaverOverlay.transform.SetParent(canvasObj.transform, false);

            var rect = screensaverOverlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = screensaverOverlay.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.95f);

            // Optional: "Touch to continue" text
            var textObj = new GameObject("ScreensaverText");
            textObj.transform.SetParent(screensaverOverlay.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = "Touch to continue";
            tmp.fontSize = 28f;
            tmp.color = new Color(1f, 1f, 1f, 0.5f);
            tmp.alignment = TMPro.TextAlignmentOptions.Center;

            screensaverOverlay.SetActive(false);
        }
    }
}
