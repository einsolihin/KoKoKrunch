using UnityEngine;

namespace KoKoKrunch.Utils
{
    public class ScreenSetup : MonoBehaviour
    {
        [SerializeField] private int targetWidth = 450;
        [SerializeField] private int targetHeight = 800;

        private void Awake()
        {
            Screen.SetResolution(targetWidth, targetHeight, false);
        }
    }
}
