using UnityEngine;

namespace KoKoKrunch.Managers
{
    public class PersistentManagersBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (GameManager.Instance != null) return;

            GameObject managers = new GameObject("--- Managers ---");
            DontDestroyOnLoad(managers);

            // GameManager + GameConfig
            var gm = managers.AddComponent<GameManager>();
            var config = Resources.Load<KoKoKrunch.Data.GameConfig>("GameConfig");
            if (config != null)
            {
                var so = typeof(GameManager).GetField("config",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                so?.SetValue(gm, config);
            }
            else
            {
                Debug.LogWarning("[Bootstrap] GameConfig not found in Resources folder. Using defaults.");
            }

            // DataManager
            managers.AddComponent<DataManager>();

            // AudioManager with audio sources
            var audioManager = managers.AddComponent<AudioManager>();

            var bgmSource = managers.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.volume = 0.5f;

            var sfxSource = managers.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = 1f;

            // Assign audio sources via reflection
            var amType = typeof(AudioManager);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            amType.GetField("bgmSource", flags)?.SetValue(audioManager, bgmSource);
            amType.GetField("sfxSource", flags)?.SetValue(audioManager, sfxSource);

            // SettingsManager
            if (SettingsManager.Instance == null)
                managers.AddComponent<SettingsManager>();

            // ScreensaverManager
            managers.AddComponent<ScreensaverManager>();

            // AdminExitManager
            managers.AddComponent<AdminExitManager>();

            Debug.Log("[Bootstrap] Persistent Managers created automatically.");
        }
    }
}
