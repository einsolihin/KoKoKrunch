using UnityEngine;

namespace KoKoKrunch.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("BGM Clips")]
        [SerializeField] private AudioClip menuBGM;
        [SerializeField] private AudioClip gameBGM;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip catchSFX;
        [SerializeField] private AudioClip missSFX;
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip gameOverSFX;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayMenuBGM()
        {
            PlayBGM(menuBGM);
        }

        public void PlayGameBGM()
        {
            PlayBGM(gameBGM);
        }

        public void StopBGM()
        {
            if (bgmSource != null)
                bgmSource.Stop();
        }

        public void PlayCatchSFX() => PlaySFX(catchSFX);
        public void PlayMissSFX() => PlaySFX(missSFX);
        public void PlayButtonClickSFX() => PlaySFX(buttonClickSFX);
        public void PlayGameOverSFX() => PlaySFX(gameOverSFX);

        private void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;

            sfxSource.PlayOneShot(clip);
        }
    }
}
