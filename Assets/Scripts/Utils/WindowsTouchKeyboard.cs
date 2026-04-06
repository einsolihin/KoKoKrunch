using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

namespace KoKoKrunch.Utils
{
    /// <summary>
    /// Attach to any GameObject with a TMP_InputField.
    /// Opens the Windows touch keyboard (TabTip) when the field is selected,
    /// and closes it when deselected.
    /// Works on Windows touch panel builds. No effect in Editor or non-Windows platforms.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class WindowsTouchKeyboard : MonoBehaviour, ISelectHandler
    {
        private TMP_InputField inputField;

        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            OpenKeyboard();
        }

        private void OnDisable()
        {
            StartCoroutine(DelayedClose());
        }

        private IEnumerator DelayedClose()
        {
            yield return new WaitForSeconds(0.3f);

            if (!inputField.isFocused)
                CloseKeyboard();
        }

        public static void OpenKeyboard()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                string tabTipPath = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";
                if (System.IO.File.Exists(tabTipPath))
                {
                    Process.Start(tabTipPath);
                }
                else
                {
                    // Fallback to on-screen keyboard
                    Process.Start("osk.exe");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[TouchKeyboard] Could not open keyboard: {e.Message}");
            }
#endif
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
        }
    }
}
