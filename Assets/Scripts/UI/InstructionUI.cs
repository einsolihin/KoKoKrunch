using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KoKoKrunch.UI
{
    public class InstructionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI strawberryPointText;
        [SerializeField] private TextMeshProUGUI kokokrunch1PointText;
        [SerializeField] private TextMeshProUGUI kokokrunch2PointText;
        [SerializeField] private Button nextButton;

        private void Start()
        {
            nextButton.onClick.AddListener(OnNextClicked);
            strawberryPointText.text = $"+{GameManager.Instance.Config.strawberryPoints} points";
            kokokrunch1PointText.text = $"+{GameManager.Instance.Config.kokoKrunchPoints} points";
            kokokrunch2PointText.text = $"+{GameManager.Instance.Config.kokoKrunchPoints} points";
        }

        private void OnNextClicked()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadGame();
        }
    }
}
