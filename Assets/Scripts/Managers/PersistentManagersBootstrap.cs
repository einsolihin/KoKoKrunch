using UnityEngine;

namespace KoKoKrunch.Managers
{
    public class PersistentManagersBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (GameManager.Instance != null) return;

            // Try loading the ManagersPrefab from Resources (editable in Inspector)
            var prefab = Resources.Load<GameObject>("ManagersPrefab");
            if (prefab != null)
            {
                var managers = Instantiate(prefab);
                managers.name = "--- Managers ---";
                DontDestroyOnLoad(managers);

                // Ensure GameConfig is assigned to GameManager
                var gm = managers.GetComponent<GameManager>();
                if (gm != null && gm.Config == null)
                {
                    var config = Resources.Load<KoKoKrunch.Data.GameConfig>("GameConfig");
                    if (config != null)
                    {
                        var field = typeof(GameManager).GetField("config",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        field?.SetValue(gm, config);
                    }
                }
            }
            else
            {
                // Fallback: create managers from code (no audio clips will be assigned)
                Debug.LogWarning("[Bootstrap] ManagersPrefab not found in Resources. " +
                    "Run 'KoKo Krunch > Setup All Scenes and Prefabs' to create it.");
                CreateManagersFromCode();
            }

            Debug.Log("[Bootstrap] Persistent Managers created automatically.");
        }

        private static void CreateManagersFromCode()
        {
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

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var amType = typeof(AudioManager);
            amType.GetField("bgmSource", flags)?.SetValue(audioManager, bgmSource);
            amType.GetField("sfxSource", flags)?.SetValue(audioManager, sfxSource);

            // SettingsManager
            if (SettingsManager.Instance == null)
                managers.AddComponent<SettingsManager>();

            // ScreensaverManager
            managers.AddComponent<ScreensaverManager>();

            // AdminExitManager
            managers.AddComponent<AdminExitManager>();
        }
    }
}
