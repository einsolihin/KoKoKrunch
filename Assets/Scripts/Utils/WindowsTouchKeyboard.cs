using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace KoKoKrunch.Utils
{
    /// <summary>
    /// Attach to any GameObject with a TMP_InputField.
    /// Opens the Windows touch keyboard (TabTip) when the field is selected or tapped.
    /// If both native keyboards fail, falls back to a Unity on-screen keyboard prefab.
    /// Closes it only when the GameObject is disabled (scene transition / popup hidden).
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class WindowsTouchKeyboard : MonoBehaviour, ISelectHandler, IPointerClickHandler
    {
        private static GameObject keyboardInstance;
        private static Canvas keyboardCanvas;
        private static WindowsTouchKeyboard activeInstance;

        private TMP_InputField inputField;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            activeInstance = this;
            OpenKeyboard();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            activeInstance = this;
            OpenKeyboard();
        }

        private void OnDisable()
        {
            CloseKeyboard();
        }

        public static void OpenKeyboard()
        {
            bool nativeOpened = false;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            nativeOpened = TryOpenNativeKeyboard();
#endif

            if (!nativeOpened)
            {
                OpenUnityKeyboard();
            }
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private static bool TryOpenNativeKeyboard()
        {
            try
            {
                string tabTipPath = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";
                if (System.IO.File.Exists(tabTipPath))
                {
                    Process.Start(tabTipPath);
                    return true;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[TouchKeyboard] TabTip failed: {e.Message}");
            }

            try
            {
                Process.Start("osk.exe");
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[TouchKeyboard] osk.exe failed: {e.Message}");
            }

            return false;
        }
#endif

        private static void OpenUnityKeyboard()
        {
            if (activeInstance == null) return;

            // Already open — just reassign the input field
            if (keyboardInstance != null)
            {
                var keyboard = keyboardInstance.GetComponent<KeyboardScript>();
                if (keyboard != null)
                    keyboard.TextField = activeInstance.inputField;
                keyboardInstance.SetActive(true);
                return;
            }

            // Load prefab from Resources
            var prefab = Resources.Load<GameObject>("OnScreenKeyboard");
            if (prefab == null)
            {
                UnityEngine.Debug.LogWarning("[TouchKeyboard] OnScreenKeyboard prefab not found in Resources/");
                return;
            }

            // Create a dedicated Canvas with highest sorting order
            var canvasGo = new GameObject("OnScreenKeyboardCanvas");
            Object.DontDestroyOnLoad(canvasGo);
            keyboardCanvas = canvasGo.AddComponent<Canvas>();
            keyboardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            keyboardCanvas.sortingOrder = 9999;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(450f, 800f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Instantiate keyboard as child of the overlay canvas
            keyboardInstance = Object.Instantiate(prefab, canvasGo.transform);
            keyboardInstance.name = "OnScreenKeyboard";

            // Position at the bottom of the screen
            var rt = keyboardInstance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = Vector2.zero;
            }

            // Assign input field and show English layout
            var keyboardScript = keyboardInstance.GetComponent<KeyboardScript>();
            if (keyboardScript != null)
            {
                keyboardScript.TextField = activeInstance.inputField;
                keyboardScript.ShowLayout(keyboardScript.EngLayoutSml);
            }
        }

        public static void CloseKeyboard()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                foreach (var proc in Process.GetProcessesByName("TabTip"))
                    proc.Kill();
                foreach (var proc in Process.GetProcessesByName("osk"))
                    proc.Kill();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[TouchKeyboard] Could not close keyboard: {e.Message}");
            }
#endif

            // Also close Unity keyboard if open
            if (keyboardInstance != null)
                keyboardInstance.SetActive(false);
        }

        /// <summary>
        /// Fully destroys the Unity on-screen keyboard instance and its canvas.
        /// </summary>
        public static void DestroyUnityKeyboard()
        {
            if (keyboardCanvas != null)
            {
                Object.Destroy(keyboardCanvas.gameObject);
                keyboardCanvas = null;
                keyboardInstance = null;
            }
        }
    }
}
