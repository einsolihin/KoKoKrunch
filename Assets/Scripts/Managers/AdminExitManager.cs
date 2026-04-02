using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace KoKoKrunch.Managers
{
    public class AdminExitManager : MonoBehaviour
    {
        public static AdminExitManager Instance { get; private set; }

        private const string ExitPassword = "kokokrushexit";
        private const int RequiredTaps = 5;
        private const float TapResetTimeout = 3f;
        private const float CornerSize = 0.15f; // 15% of screen from top-right

        private int tapCount;
        private float tapResetTimer;

        // Password popup UI
        private Canvas popupCanvas;
        private GameObject popupOverlay;
        private TMP_InputField passwordInput;
        private TextMeshProUGUI errorText;

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

        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Start()
        {
            CreatePasswordPopup();
        }

        private void Update()
        {
            // Don't detect corner taps while popup is active
            if (popupOverlay != null && popupOverlay.activeSelf) return;

            // Reset tap timer
            if (tapCount > 0)
            {
                tapResetTimer -= Time.unscaledDeltaTime;
                if (tapResetTimer <= 0f)
                    tapCount = 0;
            }

            // Detect taps/clicks in top-right corner (new Input System)
            Vector2 pos = Vector2.zero;
            bool tapped = false;

            if (Touch.activeTouches.Count > 0 && Touch.activeTouches[0].phase == TouchPhase.Began)
            {
                pos = Touch.activeTouches[0].screenPosition;
                tapped = true;
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                pos = Mouse.current.position.ReadValue();
                tapped = true;
            }

            if (tapped && IsInTopRightCorner(pos))
            {
                tapCount++;
                tapResetTimer = TapResetTimeout;

                if (tapCount >= RequiredTaps)
                {
                    tapCount = 0;
                    ShowPasswordPopup();
                }
            }
        }

        private bool IsInTopRightCorner(Vector2 screenPos)
        {
            float screenW = Screen.width;
            float screenH = Screen.height;

            return screenPos.x > screenW * (1f - CornerSize) && screenPos.y > screenH * (1f - CornerSize);
        }

        private void ShowPasswordPopup()
        {
            popupOverlay.SetActive(true);
            passwordInput.text = "";
            if (errorText != null)
                errorText.text = "";
            passwordInput.ActivateInputField();
        }

        private void HidePasswordPopup()
        {
            popupOverlay.SetActive(false);
        }

        private void OnSubmitPassword()
        {
            if (passwordInput.text == ExitPassword)
            {
                Debug.Log("[AdminExit] Correct password. Exiting application.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            else
            {
                if (errorText != null)
                    errorText.text = "Incorrect password";
                passwordInput.text = "";
                passwordInput.ActivateInputField();
            }
        }

        private void CreatePasswordPopup()
        {
            var canvasObj = new GameObject("AdminExitCanvas");
            canvasObj.transform.SetParent(transform, false);

            popupCanvas = canvasObj.AddComponent<Canvas>();
            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popupCanvas.sortingOrder = 998; // Below screensaver, above everything else

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Fullscreen dim overlay
            popupOverlay = new GameObject("PopupOverlay");
            popupOverlay.transform.SetParent(canvasObj.transform, false);

            var overlayRect = popupOverlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var overlayImg = popupOverlay.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.85f);

            // Center panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(popupOverlay.transform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500, 350);

            var panelImg = panel.AddComponent<Image>();
            panelImg.color = new Color(0.18f, 0.14f, 0.12f, 1f);

            // Title
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(0, 40);

            var titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "Admin Exit";
            titleTmp.fontSize = 24f;
            titleTmp.color = new Color(0.9f, 0.75f, 0.2f, 1f);
            titleTmp.alignment = TextAlignmentOptions.Center;

            // Password input field
            var inputObj = new GameObject("PasswordInput");
            inputObj.transform.SetParent(panel.transform, false);

            var inputRect = inputObj.AddComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = new Vector2(0, 20);
            inputRect.sizeDelta = new Vector2(400, 50);

            var inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.15f, 0.12f, 0.1f, 1f);

            // Text Area
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            var taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(10, 5);
            taRect.offsetMax = new Vector2(-10, -5);
            textArea.AddComponent<RectMask2D>();

            // Input Text
            var inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            var itRect = inputTextObj.AddComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero;
            itRect.anchorMax = Vector2.one;
            itRect.offsetMin = Vector2.zero;
            itRect.offsetMax = Vector2.zero;

            var inputTmp = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputTmp.fontSize = 18f;
            inputTmp.color = Color.white;
            inputTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Placeholder
            var phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(textArea.transform, false);
            var phRect = phObj.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;

            var phTmp = phObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = "Enter password...";
            phTmp.fontSize = 18f;
            phTmp.color = new Color(1f, 1f, 1f, 0.3f);
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Wire TMP_InputField
            passwordInput = inputObj.AddComponent<TMP_InputField>();
            passwordInput.textComponent = inputTmp;
            passwordInput.textViewport = taRect;
            passwordInput.placeholder = phTmp;
            passwordInput.contentType = TMP_InputField.ContentType.Password;
            passwordInput.pointSize = 18f;

            // Error text
            var errorObj = new GameObject("ErrorText");
            errorObj.transform.SetParent(panel.transform, false);
            var errorRect = errorObj.AddComponent<RectTransform>();
            errorRect.anchorMin = new Vector2(0.5f, 0.5f);
            errorRect.anchorMax = new Vector2(0.5f, 0.5f);
            errorRect.pivot = new Vector2(0.5f, 0.5f);
            errorRect.anchoredPosition = new Vector2(0, -25);
            errorRect.sizeDelta = new Vector2(400, 30);

            errorText = errorObj.AddComponent<TextMeshProUGUI>();
            errorText.fontSize = 14f;
            errorText.color = new Color(0.9f, 0.3f, 0.3f, 1f);
            errorText.alignment = TextAlignmentOptions.Center;

            // Button row
            var btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(panel.transform, false);
            var btnRowRect = btnRow.AddComponent<RectTransform>();
            btnRowRect.anchorMin = new Vector2(0.5f, 0);
            btnRowRect.anchorMax = new Vector2(0.5f, 0);
            btnRowRect.pivot = new Vector2(0.5f, 0);
            btnRowRect.anchoredPosition = new Vector2(0, 30);
            btnRowRect.sizeDelta = new Vector2(400, 50);

            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            // Submit button
            var submitBtn = CreatePopupButton(btnRow.transform, "Submit", new Color(0.9f, 0.75f, 0.2f, 1f));
            submitBtn.onClick.AddListener(OnSubmitPassword);

            // Cancel button
            var cancelBtn = CreatePopupButton(btnRow.transform, "Cancel", new Color(0.4f, 0.35f, 0.3f, 1f));
            cancelBtn.onClick.AddListener(HidePasswordPopup);

            popupOverlay.SetActive(false);
        }

        private Button CreatePopupButton(Transform parent, string label, Color color)
        {
            var obj = new GameObject(label + "Button");
            obj.transform.SetParent(parent, false);

            var img = obj.AddComponent<Image>();
            img.color = color;

            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;

            var textObj = new GameObject("Label");
            textObj.transform.SetParent(obj.transform, false);

            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return btn;
        }
    }
}
