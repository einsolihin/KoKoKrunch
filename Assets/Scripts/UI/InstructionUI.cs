using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class InstructionUI : MonoBehaviour
    {
        [SerializeField] private Button nextButton;

        private void Start()
        {
            nextButton.onClick.AddListener(OnNextClicked);
        }

        private void OnNextClicked()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            SceneLoader.LoadGame();
        }
    }
}
