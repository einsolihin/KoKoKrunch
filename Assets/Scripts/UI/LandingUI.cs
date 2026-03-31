using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class LandingUI : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button hiddenAdminButton;

        private int adminTapCount;
        private float adminTapResetTimer;
        private const int RequiredTaps = 10;
        private const float TapResetTimeout = 3f;

        private void Start()
        {
            AudioManager.Instance?.PlayMenuBGM();
            startButton.onClick.AddListener(OnStartClicked);

            if (hiddenAdminButton != null)
                hiddenAdminButton.onClick.AddListener(OnHiddenAdminTapped);
        }

        private void Update()
        {
            if (adminTapCount > 0)
            {
                adminTapResetTimer -= Time.deltaTime;
                if (adminTapResetTimer <= 0f)
                    adminTapCount = 0;
            }
        }

        private void OnStartClicked()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadNameInput();
        }

        private void OnHiddenAdminTapped()
        {
            adminTapCount++;
            adminTapResetTimer = TapResetTimeout;

            if (adminTapCount >= RequiredTaps)
            {
                adminTapCount = 0;
                SceneLoader.LoadAdmin();
            }
        }
    }
}
