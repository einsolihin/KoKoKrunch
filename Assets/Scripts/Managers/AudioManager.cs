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
        [SerializeField] private AudioClip winBGM;
        [SerializeField] private AudioClip loseBGM;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip catchCorrectSFX;
        [SerializeField] private AudioClip catchWrongSFX;
        [SerializeField] private AudioClip buttonClickSFX;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // ── BGM ──

        public void PlayMenuBGM()
        {
            PlayBGM(menuBGM);
        }

        public void PlayGameBGM()
        {
            PlayBGM(gameBGM);
        }

        public void PlayWinBGM()
        {
            PlayBGM(winBGM, false);

        }

        public void PlayLoseBGM()
        {
            PlayBGM(loseBGM, false);
        }

        public void StopBGM()
        {
            if (bgmSource != null)
                bgmSource.Stop();
        }

        // ── SFX ──

        public void PlayCatchCorrectSFX() => PlaySFX(catchCorrectSFX);
        public void PlayCatchWrongSFX() => PlaySFX(catchWrongSFX);
        public void PlayButtonClickSFX() => PlaySFX(buttonClickSFX);

        // ── Internal ──

        private void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (bgmSource == null || clip == null) return;

            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.Play();
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;

            sfxSource.PlayOneShot(clip);
        }
    }
}
