using UnityEngine;
using UnityEngine.SceneManagement;

namespace KoKoKrunch.Utils
{
    public static class SceneLoader
    {
        public const string LandingScene = "LandingScene";
        public const string NameInputScene = "NameInputScene";
        public const string InstructionScene = "InstructionScene";
        public const string GameScene = "GameScene";
        public const string ResultScene = "ResultScene";
        public const string LeaderboardScene = "LeaderboardScene";
        public const string AdminScene = "AdminScene";

        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadLanding() => LoadScene(LandingScene);
        public static void LoadNameInput() => LoadScene(NameInputScene);
        public static void LoadInstruction() => LoadScene(InstructionScene);
        public static void LoadGame() => LoadScene(GameScene);
        public static void LoadResult() => LoadScene(ResultScene);
        public static void LoadLeaderboard() => LoadScene(LeaderboardScene);
        public static void LoadAdmin() => LoadScene(AdminScene);
    }
}
