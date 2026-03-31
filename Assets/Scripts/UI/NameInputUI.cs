using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class NameInputUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI errorText;

        private void Start()
        {
            nextButton.onClick.AddListener(OnNextClicked);
            if (errorText != null) errorText.gameObject.SetActive(false);
        }

        private void OnNextClicked()
        {
            string playerName = nameInputField.text.Trim();

            if (string.IsNullOrEmpty(playerName))
            {
                if (errorText != null)
                {
                    errorText.text = "Please enter your name";
                    errorText.gameObject.SetActive(true);
                }
                return;
            }

            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance.PlayerName = playerName;
            SceneLoader.LoadInstruction();
        }
    }
}
