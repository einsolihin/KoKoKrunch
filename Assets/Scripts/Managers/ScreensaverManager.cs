using KoKoKrunch.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace KoKoKrunch.Managers
{
    public class ScreensaverManager : MonoBehaviour
    {
        public static ScreensaverManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float inactivityTimeout = 40f;

        [Header("Video")]
        [SerializeField] private VideoClip advertisementClip;

        private float inactivityTimer;
        private bool screensaverActive;

        private Canvas screensaverCanvas;
        private GameObject screensaverOverlay;
        private VideoPlayer videoPlayer;
        private RenderTexture renderTexture;
        private Vector2 lastMousePosition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnhancedTouchSupport.Enable();
        }

        private void Start()
        {
            CreateScreensaverOverlay();
            ResetTimer();
        }

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();

            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
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
            // Touch (new Input System)
            if (Touch.activeTouches.Count > 0) return true;

            // Mouse click (new Input System)
            if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
                return true;

            // Any key (new Input System)
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;

            // Mouse movement (new Input System)
            if (Mouse.current != null)
            {
                Vector2 currentMousePos = Mouse.current.position.ReadValue();
                if (Vector2.Distance(currentMousePos, lastMousePosition) > 1f)
                {
                    lastMousePosition = currentMousePos;
                    return true;
                }
            }

            return false;
        }

        public void ResetTimer()
        {
            inactivityTimer = inactivityTimeout;
        }

        private void ActivateScreensaver()
        {
            screensaverActive = true;
            screensaverOverlay.SetActive(true);

            // Stop any BGM and play video
            AudioManager.Instance?.StopBGM();

            if (videoPlayer != null && advertisementClip != null)
            {
                videoPlayer.clip = advertisementClip;
                videoPlayer.isLooping = true;
                videoPlayer.Play();
            }
        }

        private void DismissScreensaver()
        {
            screensaverActive = false;
            screensaverOverlay.SetActive(false);

            if (videoPlayer != null)
                videoPlayer.Stop();

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
            screensaverCanvas.sortingOrder = 999;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Fullscreen overlay container
            screensaverOverlay = new GameObject("ScreensaverOverlay");
            screensaverOverlay.transform.SetParent(canvasObj.transform, false);

            var rect = screensaverOverlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // Black background behind video
            var bgImg = screensaverOverlay.AddComponent<Image>();
            bgImg.color = Color.black;

            // RenderTexture for video output
            renderTexture = new RenderTexture(1080, 1920, 0);
            renderTexture.Create();

            // VideoPlayer on this manager GameObject
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.isLooping = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

            // RawImage to display the video
            var videoObj = new GameObject("VideoDisplay");
            videoObj.transform.SetParent(screensaverOverlay.transform, false);

            var videoRect = videoObj.AddComponent<RectTransform>();
            videoRect.anchorMin = Vector2.zero;
            videoRect.anchorMax = Vector2.one;
            videoRect.offsetMin = Vector2.zero;
            videoRect.offsetMax = Vector2.zero;

            var rawImage = videoObj.AddComponent<RawImage>();
            rawImage.texture = renderTexture;

            screensaverOverlay.SetActive(false);
        }
    }
}
