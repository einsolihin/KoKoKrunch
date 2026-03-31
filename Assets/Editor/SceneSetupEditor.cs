using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;
using System.Collections.Generic;

namespace KoKoKrunch.Editor
{
    public class SceneSetupEditor : EditorWindow
    {
        // Brand colors
        private static readonly Color BgPink = new Color(0.886f, 0.306f, 0.455f, 1f);       // #E24E74
        private static readonly Color BgDarkPink = new Color(0.780f, 0.220f, 0.373f, 1f);    // #C7385F
        private static readonly Color White = Color.white;
        private static readonly Color HeartRed = new Color(0.9f, 0.15f, 0.25f, 1f);
        private static readonly Color ButtonPink = new Color(0.95f, 0.75f, 0.80f, 1f);
        private static readonly Color TextDark = new Color(0.3f, 0.1f, 0.15f, 1f);
        private static readonly Color Gold = new Color(1f, 0.84f, 0f, 1f);
        private static readonly Color StrawberryRed = new Color(0.9f, 0.1f, 0.2f, 1f);
        private static readonly Color KokoPack1Brown = new Color(0.6f, 0.4f, 0.1f, 1f);
        private static readonly Color KokoPack2Green = new Color(0.2f, 0.6f, 0.2f, 1f);
        private static readonly Color CatcherYellow = new Color(1f, 0.85f, 0.3f, 1f);

        private static string ScenesPath => "Assets/Scenes/Game";
        private static string PrefabsPath => "Assets/Prefabs";

        [MenuItem("KoKo Krunch/Setup All Scenes and Prefabs")]
        public static void SetupAll()
        {
            if (!EditorUtility.DisplayDialog("KoKo Krunch Setup",
                "This will create all 6 scenes, prefabs, and GameConfig.\nExisting files with the same names will be overwritten.\n\nProceed?",
                "Yes, Set Up Everything", "Cancel"))
                return;

            EnsureDirectories();
            CreateGameConfig();
            CreateItemPrefabs();
            CreatePlayerPrefab();
            CreateLeaderboardEntryPrefab();

            CreateLandingScene();
            CreateNameInputScene();
            CreateInstructionScene();
            CreateGameScene();
            CreateResultScene();
            CreateLeaderboardScene();
            CreateAdminScene();

            AddScenesToBuildSettings();

            EditorUtility.DisplayDialog("Setup Complete",
                "All scenes, prefabs, and config have been created!\n\n" +
                "Next steps:\n" +
                "1. Open LandingScene\n" +
                "2. Create a persistent GameObject and add GameManager, DataManager, AudioManager\n" +
                "3. Assign the GameConfig asset to GameManager\n" +
                "4. Hit Play to test!",
                "Got it!");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = {
                "Assets/Scenes/Game",
                "Assets/Prefabs/Items",
                "Assets/Prefabs/Player",
                "Assets/Prefabs/UI",
                "Assets/Sprites/Placeholder",
                "Assets/Scripts/Data"
            };

            foreach (var dir in dirs)
            {
                if (!AssetDatabase.IsValidFolder(dir))
                {
                    string parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                    string folderName = Path.GetFileName(dir);
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        // ──────────────────────────────────────────────
        // GameConfig ScriptableObject
        // ──────────────────────────────────────────────
        private static void CreateGameConfig()
        {
            // Place in Resources so PersistentManagersBootstrap can load it at runtime
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
                AssetDatabase.CreateFolder("Assets", "Resources");

            string path = "Assets/Resources/GameConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null) return;

            var config = ScriptableObject.CreateInstance<KoKoKrunch.Data.GameConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            Debug.Log("GameConfig created at " + path);
        }

        // ──────────────────────────────────────────────
        // Placeholder Sprites
        // ──────────────────────────────────────────────
        private static Sprite CreatePlaceholderSprite(string name, int w, int h, Color color)
        {
            string path = $"Assets/Sprites/Placeholder/{name}.png";
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null)
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);

            Texture2D tex = new Texture2D(w, h);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();

            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ──────────────────────────────────────────────
        // Prefabs
        // ──────────────────────────────────────────────
        private static void CreateItemPrefabs()
        {
            CreateItemPrefab("Strawberry", StrawberryRed, KoKoKrunch.Gameplay.ItemType.Strawberry, 0.5f);
            CreateItemPrefab("KokoKrunchPack1", KokoPack1Brown, KoKoKrunch.Gameplay.ItemType.KokoKrunchPack1, 0.6f);
            CreateItemPrefab("KokoKrunchPack2", KokoPack2Green, KoKoKrunch.Gameplay.ItemType.KokoKrunchPack2, 0.6f);
        }

        private static GameObject CreateItemPrefab(string name, Color color, KoKoKrunch.Gameplay.ItemType itemType, float size)
        {
            string path = $"{PrefabsPath}/Items/{name}.prefab";

            GameObject obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite($"item_{name}", 64, 64, color);
            obj.transform.localScale = Vector3.one * size;

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            var fi = obj.AddComponent<KoKoKrunch.Gameplay.FallingItem>();
            // Set itemType via SerializedObject
            var so = new SerializedObject(fi);
            so.FindProperty("itemType").enumValueIndex = (int)itemType;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            Debug.Log($"Prefab created: {path}");
            return prefab;
        }

        private static void CreatePlayerPrefab()
        {
            string path = $"{PrefabsPath}/Player/Catcher.prefab";

            GameObject obj = new GameObject("Catcher");
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreatePlaceholderSprite("catcher", 128, 64, CatcherYellow);
            sr.sortingOrder = 5;
            obj.transform.localScale = new Vector3(1.2f, 0.6f, 1f);

            var col = obj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 0.5f);

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;

            obj.AddComponent<KoKoKrunch.Gameplay.PlayerCatcher>();

            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            Debug.Log($"Prefab created: {path}");
        }

        private static void CreateLeaderboardEntryPrefab()
        {
            string path = $"{PrefabsPath}/UI/LeaderboardEntryRow.prefab";

            GameObject row = new GameObject("LeaderboardEntryRow");
            var rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(400, 30);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 10;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Rank
            CreateTMPChild(row.transform, "RankText", "#1", 50, 30, TextAlignmentOptions.Left, White, 14);
            // Name
            CreateTMPChild(row.transform, "NameText", "PLAYER", 200, 30, TextAlignmentOptions.Left, White, 14);
            // Score
            CreateTMPChild(row.transform, "ScoreText", "0 POINTS", 140, 30, TextAlignmentOptions.Right, White, 14);

            row.AddComponent<KoKoKrunch.UI.LeaderboardEntry>();

            PrefabUtility.SaveAsPrefabAsset(row, path);
            Object.DestroyImmediate(row);
            Debug.Log($"Prefab created: {path}");
        }

        // ──────────────────────────────────────────────
        // SCENE BUILDERS
        // ──────────────────────────────────────────────

        #region Landing Scene
        private static void CreateLandingScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            GameObject canvas = CreateCanvas();
            CreateBackground(canvas.transform);

            // Logo area
            var logoPanel = CreatePanel(canvas.transform, "LogoPanel",
                new Vector2(0, 150), new Vector2(350, 200), new Color(1, 1, 1, 0));
            CreateTMPChild(logoPanel.transform, "TitleText", "KOKO KRUNCH\nSTRAWBERRY",
                350, 80, TextAlignmentOptions.Center, White, 36, FontStyles.Bold);
            CreateTMPChild(logoPanel.transform, "SubtitleText", "CATCH & WIN",
                350, 60, TextAlignmentOptions.Center, Gold, 42, FontStyles.Bold);

            // Start Button
            var startBtn = CreateButton(canvas.transform, "StartButton", "START!",
                new Vector2(0, -200), new Vector2(200, 60), ButtonPink, TextDark, 28);

            // Mascot placeholder
            var mascot = CreateImage(canvas.transform, "MascotImage",
                new Vector2(0, -50), new Vector2(120, 120), CatcherYellow);

            // Hidden admin button (top-right, invisible — tap 10 times to access admin)
            var hiddenBtn = new GameObject("HiddenAdminButton");
            hiddenBtn.transform.SetParent(canvas.transform, false);
            var hiddenRect = hiddenBtn.AddComponent<RectTransform>();
            hiddenRect.anchorMin = new Vector2(1, 1);
            hiddenRect.anchorMax = new Vector2(1, 1);
            hiddenRect.pivot = new Vector2(1, 1);
            hiddenRect.anchoredPosition = Vector2.zero;
            hiddenRect.sizeDelta = new Vector2(80, 80);

            var hiddenImg = hiddenBtn.AddComponent<Image>();
            hiddenImg.color = new Color(0, 0, 0, 0); // fully transparent

            var hiddenButton = hiddenBtn.AddComponent<Button>();
            hiddenButton.transition = Selectable.Transition.None;

            // Wire up LandingUI
            var landingUI = canvas.AddComponent<KoKoKrunch.UI.LandingUI>();
            var so = new SerializedObject(landingUI);
            so.FindProperty("startButton").objectReferenceValue = startBtn.GetComponent<Button>();
            so.FindProperty("hiddenAdminButton").objectReferenceValue = hiddenButton;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Add ScreenSetup
            canvas.AddComponent<KoKoKrunch.Utils.ScreenSetup>();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/LandingScene.unity");
            Debug.Log("LandingScene created");
        }
        #endregion

        #region Name Input Scene
        private static void CreateNameInputScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            GameObject canvas = CreateCanvas();
            CreateBackground(canvas.transform);

            // NAME label
            CreateTMPChild(canvas.transform, "NameLabel", "NAME",
                300, 60, TextAlignmentOptions.Center, White, 40, FontStyles.Bold,
                new Vector2(0, 100));

            // Input Field
            var inputFieldObj = CreateInputField(canvas.transform, "NameInputField",
                new Vector2(0, 20), new Vector2(300, 50));

            // Auto-open Windows touch keyboard when input field is selected
            inputFieldObj.AddComponent<KoKoKrunch.Utils.WindowsTouchKeyboard>();

            // Error text
            var errorText = CreateTMPChild(canvas.transform, "ErrorText", "",
                300, 30, TextAlignmentOptions.Center, new Color(1, 0.3f, 0.3f), 16,
                FontStyles.Normal, new Vector2(0, -20));

            // Next Button
            var nextBtn = CreateButton(canvas.transform, "NextButton", "NEXT",
                new Vector2(0, -200), new Vector2(200, 60), ButtonPink, TextDark, 28);

            // Mascot
            CreateImage(canvas.transform, "MascotImage",
                new Vector2(100, -100), new Vector2(100, 100), CatcherYellow);

            // Wire up NameInputUI
            var nameUI = canvas.AddComponent<KoKoKrunch.UI.NameInputUI>();
            var so = new SerializedObject(nameUI);
            so.FindProperty("nameInputField").objectReferenceValue = inputFieldObj.GetComponent<TMP_InputField>();
            so.FindProperty("nextButton").objectReferenceValue = nextBtn.GetComponent<Button>();
            so.FindProperty("errorText").objectReferenceValue = errorText.GetComponent<TextMeshProUGUI>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/NameInputScene.unity");
            Debug.Log("NameInputScene created");
        }
        #endregion

        #region Instruction Scene
        private static void CreateInstructionScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            GameObject canvas = CreateCanvas();
            CreateBackground(canvas.transform);

            // Title
            CreateTMPChild(canvas.transform, "InstructionTitle", "HOW TO PLAY",
                350, 50, TextAlignmentOptions.Center, White, 32, FontStyles.Bold,
                new Vector2(0, 280));

            // Instruction items
            float yStart = 180;
            float yStep = -90;

            // Strawberry
            var row1 = CreatePanel(canvas.transform, "Row1",
                new Vector2(0, yStart), new Vector2(350, 80), new Color(1, 1, 1, 0));
            CreateImage(row1.transform, "StrawberryIcon",
                new Vector2(-120, 0), new Vector2(50, 50), StrawberryRed);
            CreateTMPChild(row1.transform, "StrawberryLabel", "STRAWBERRY\n+5 POINTS",
                200, 60, TextAlignmentOptions.Left, White, 18, FontStyles.Normal,
                new Vector2(30, 0));

            // Koko Pack 1
            var row2 = CreatePanel(canvas.transform, "Row2",
                new Vector2(0, yStart + yStep), new Vector2(350, 80), new Color(1, 1, 1, 0));
            CreateImage(row2.transform, "Pack1Icon",
                new Vector2(-120, 0), new Vector2(50, 50), KokoPack1Brown);
            CreateTMPChild(row2.transform, "Pack1Label", "KOKO KRUNCH PACK\n+10 POINTS",
                200, 60, TextAlignmentOptions.Left, White, 18, FontStyles.Normal,
                new Vector2(30, 0));

            // Koko Pack 2
            var row3 = CreatePanel(canvas.transform, "Row3",
                new Vector2(0, yStart + yStep * 2), new Vector2(350, 80), new Color(1, 1, 1, 0));
            CreateImage(row3.transform, "Pack2Icon",
                new Vector2(-120, 0), new Vector2(50, 50), KokoPack2Green);
            CreateTMPChild(row3.transform, "Pack2Label", "KOKO KRUNCH PACK\n+10 POINTS",
                200, 60, TextAlignmentOptions.Left, White, 18, FontStyles.Normal,
                new Vector2(30, 0));

            // Rules
            CreateTMPChild(canvas.transform, "RulesText",
                "3 LIVES  |  30 SECONDS\nMissed item = Lose 1 life",
                350, 60, TextAlignmentOptions.Center, White, 16, FontStyles.Italic,
                new Vector2(0, -100));

            // Next Button
            var nextBtn = CreateButton(canvas.transform, "NextButton", "NEXT",
                new Vector2(0, -250), new Vector2(200, 60), ButtonPink, TextDark, 28);

            // Wire up InstructionUI
            var instUI = canvas.AddComponent<KoKoKrunch.UI.InstructionUI>();
            var so = new SerializedObject(instUI);
            so.FindProperty("nextButton").objectReferenceValue = nextBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/InstructionScene.unity");
            Debug.Log("InstructionScene created");
        }
        #endregion

        #region Game Scene
        private static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cam = CreateCamera();
            cam.GetComponent<Camera>().backgroundColor = BgPink;

            CreateEventSystem();
            GameObject canvas = CreateCanvas();

            // HUD - Top bar
            var hudPanel = CreatePanel(canvas.transform, "HUDPanel",
                new Vector2(0, 0), new Vector2(0, 80), new Color(0, 0, 0, 0));
            var hudRect = hudPanel.GetComponent<RectTransform>();
            hudRect.anchorMin = new Vector2(0, 1);
            hudRect.anchorMax = new Vector2(1, 1);
            hudRect.pivot = new Vector2(0.5f, 1);
            hudRect.anchoredPosition = Vector2.zero;
            hudRect.sizeDelta = new Vector2(0, 80);

            // Hearts container
            var heartsPanel = CreatePanel(hudPanel.transform, "HeartsPanel",
                new Vector2(0, 0), new Vector2(120, 40), new Color(0, 0, 0, 0));
            var heartsRect = heartsPanel.GetComponent<RectTransform>();
            heartsRect.anchorMin = new Vector2(0, 0.5f);
            heartsRect.anchorMax = new Vector2(0, 0.5f);
            heartsRect.pivot = new Vector2(0, 0.5f);
            heartsRect.anchoredPosition = new Vector2(15, 0);

            var hlg = heartsPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            Image[] heartIcons = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var heart = CreateImage(heartsPanel.transform, $"Heart{i}",
                    Vector2.zero, new Vector2(30, 30), HeartRed);
                heartIcons[i] = heart.GetComponent<Image>();
                var le = heart.AddComponent<LayoutElement>();
                le.preferredWidth = 30;
                le.preferredHeight = 30;
            }

            // Score text (top right area)
            var scoreLabel = CreateTMPChild(hudPanel.transform, "ScoreLabel", "SCORE",
                80, 20, TextAlignmentOptions.Right, White, 12, FontStyles.Normal);
            var scoreLabelRect = scoreLabel.GetComponent<RectTransform>();
            scoreLabelRect.anchorMin = new Vector2(1, 0.5f);
            scoreLabelRect.anchorMax = new Vector2(1, 0.5f);
            scoreLabelRect.pivot = new Vector2(1, 0.5f);
            scoreLabelRect.anchoredPosition = new Vector2(-15, 15);

            var scoreText = CreateTMPChild(hudPanel.transform, "ScoreText", "00",
                80, 35, TextAlignmentOptions.Right, White, 28, FontStyles.Bold);
            var scoreRect = scoreText.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(1, 0.5f);
            scoreRect.anchorMax = new Vector2(1, 0.5f);
            scoreRect.pivot = new Vector2(1, 0.5f);
            scoreRect.anchoredPosition = new Vector2(-15, -12);

            // Timer text (top center-right)
            var timerLabel = CreateTMPChild(hudPanel.transform, "TimerLabel", "TIME",
                80, 20, TextAlignmentOptions.Center, White, 12, FontStyles.Normal);
            var timerLabelRect = timerLabel.GetComponent<RectTransform>();
            timerLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
            timerLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
            timerLabelRect.anchoredPosition = new Vector2(0, 15);

            var timerText = CreateTMPChild(hudPanel.transform, "TimerText", "30",
                80, 35, TextAlignmentOptions.Center, White, 28, FontStyles.Bold);
            var timerRect = timerText.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 0.5f);
            timerRect.anchorMax = new Vector2(0.5f, 0.5f);
            timerRect.anchoredPosition = new Vector2(0, -12);

            // Wire up GameUI
            var gameUI = canvas.AddComponent<KoKoKrunch.UI.GameUI>();
            var so = new SerializedObject(gameUI);
            so.FindProperty("scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("timerText").objectReferenceValue = timerText.GetComponent<TextMeshProUGUI>();

            var heartsProp = so.FindProperty("heartIcons");
            heartsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
                heartsProp.GetArrayElementAtIndex(i).objectReferenceValue = heartIcons[i];

            so.ApplyModifiedPropertiesWithoutUndo();

            // Game world objects — Catcher
            var catcherPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Player/Catcher.prefab");
            if (catcherPrefab != null)
            {
                var catcher = (GameObject)PrefabUtility.InstantiatePrefab(catcherPrefab);
                catcher.transform.position = new Vector3(0, -4f, 0);
            }

            // Item Spawner
            GameObject spawnerObj = new GameObject("ItemSpawner");
            var spawner = spawnerObj.AddComponent<KoKoKrunch.Gameplay.ItemSpawner>();
            var spawnerSO = new SerializedObject(spawner);

            var strawberryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Items/Strawberry.prefab");
            var pack1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Items/KokoKrunchPack1.prefab");
            var pack2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/Items/KokoKrunchPack2.prefab");

            spawnerSO.FindProperty("strawberryPrefab").objectReferenceValue = strawberryPrefab;
            spawnerSO.FindProperty("kokoKrunchPack1Prefab").objectReferenceValue = pack1Prefab;
            spawnerSO.FindProperty("kokoKrunchPack2Prefab").objectReferenceValue = pack2Prefab;
            spawnerSO.ApplyModifiedPropertiesWithoutUndo();

            // Background sprite for game area
            GameObject bgObj = new GameObject("GameBackground");
            var bgSR = bgObj.AddComponent<SpriteRenderer>();
            bgSR.sprite = CreatePlaceholderSprite("game_bg", 450, 800, BgPink);
            bgSR.sortingOrder = -10;
            bgObj.transform.localScale = new Vector3(5f, 8f, 1f);

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/GameScene.unity");
            Debug.Log("GameScene created");
        }
        #endregion

        #region Result Scene
        private static void CreateResultScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            GameObject canvas = CreateCanvas();
            CreateBackground(canvas.transform);

            // Congratulations text
            var congratsText = CreateTMPChild(canvas.transform, "CongratsText", "CONGRATULATIONS",
                400, 60, TextAlignmentOptions.Center, White, 32, FontStyles.Bold,
                new Vector2(0, 150));

            // Score
            var scoreText = CreateTMPChild(canvas.transform, "ScoreText", "00",
                300, 120, TextAlignmentOptions.Center, White, 96, FontStyles.Bold,
                new Vector2(0, 30));

            // Mascot
            CreateImage(canvas.transform, "MascotImage",
                new Vector2(0, -100), new Vector2(120, 120), CatcherYellow);

            // Leaderboard Button
            var lbBtn = CreateButton(canvas.transform, "LeaderboardButton", "LEADERBOARD",
                new Vector2(0, -230), new Vector2(250, 55), ButtonPink, TextDark, 22);

            // Play Again Button
            var playBtn = CreateButton(canvas.transform, "PlayAgainButton", "PLAY AGAIN",
                new Vector2(0, -300), new Vector2(250, 55), White, TextDark, 22);

            // Wire up ResultUI
            var resultUI = canvas.AddComponent<KoKoKrunch.UI.ResultUI>();
            var so = new SerializedObject(resultUI);
            so.FindProperty("scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("congratsText").objectReferenceValue = congratsText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("leaderboardButton").objectReferenceValue = lbBtn.GetComponent<Button>();
            so.FindProperty("playAgainButton").objectReferenceValue = playBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/ResultScene.unity");
            Debug.Log("ResultScene created");
        }
        #endregion

        #region Leaderboard Scene
        private static void CreateLeaderboardScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            GameObject canvas = CreateCanvas();
            CreateBackground(canvas.transform);

            // Title / Logo area
            CreateTMPChild(canvas.transform, "LeaderboardTitle", "LEADERBOARD",
                350, 50, TextAlignmentOptions.Center, White, 28, FontStyles.Bold,
                new Vector2(0, 330));

            // Top player highlight
            var topPanel = CreatePanel(canvas.transform, "TopPlayerPanel",
                new Vector2(0, 240), new Vector2(350, 100), new Color(1, 1, 1, 0.15f));

            var topRank = CreateTMPChild(topPanel.transform, "TopRankText", "#1",
                50, 40, TextAlignmentOptions.Center, Gold, 28, FontStyles.Bold,
                new Vector2(-130, 0));
            var topName = CreateTMPChild(topPanel.transform, "TopNameText", "---",
                180, 40, TextAlignmentOptions.Left, White, 24, FontStyles.Bold,
                new Vector2(0, 10));
            var topScore = CreateTMPChild(topPanel.transform, "TopScoreText", "0 POINTS",
                180, 30, TextAlignmentOptions.Left, Gold, 18, FontStyles.Normal,
                new Vector2(0, -20));

            // Scrollable leaderboard list
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(canvas.transform, false);
            var scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchoredPosition = new Vector2(0, -50);
            scrollRect.sizeDelta = new Vector2(400, 400);

            var scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;

            var scrollImage = scrollObj.AddComponent<Image>();
            scrollImage.color = new Color(0, 0, 0, 0.1f);

            var mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // Content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 600);

            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 5;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollView.content = contentRect;

            // Buttons
            var playBtn = CreateButton(canvas.transform, "PlayAgainButton", "PLAY AGAIN",
                new Vector2(0, -310), new Vector2(250, 50), ButtonPink, TextDark, 22);

            // Wire up LeaderboardUI
            var lbUI = canvas.AddComponent<KoKoKrunch.UI.LeaderboardUI>();
            var so = new SerializedObject(lbUI);
            so.FindProperty("topPlayerRankText").objectReferenceValue = topRank.GetComponent<TextMeshProUGUI>();
            so.FindProperty("topPlayerNameText").objectReferenceValue = topName.GetComponent<TextMeshProUGUI>();
            so.FindProperty("topPlayerScoreText").objectReferenceValue = topScore.GetComponent<TextMeshProUGUI>();
            so.FindProperty("leaderboardContent").objectReferenceValue = contentRect;

            var entryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/UI/LeaderboardEntryRow.prefab");
            so.FindProperty("leaderboardEntryPrefab").objectReferenceValue = entryPrefab;
            so.FindProperty("playAgainButton").objectReferenceValue = playBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/LeaderboardScene.unity");
            Debug.Log("LeaderboardScene created");
        }
        #endregion

        #region Admin Scene
        private static void CreateAdminScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateEventSystem();
            GameObject canvas = CreateCanvas();

            // Dark background for admin feel
            var bg = new GameObject("Background");
            bg.transform.SetParent(canvas.transform, false);
            var bgRect = bg.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.1f, 0.12f, 1f);

            // Title
            CreateTMPChild(canvas.transform, "AdminTitle", "ADMIN PANEL",
                350, 40, TextAlignmentOptions.Center, White, 26, FontStyles.Bold,
                new Vector2(0, 360));

            // Total entries info
            var totalText = CreateTMPChild(canvas.transform, "TotalEntriesText", "Total Entries: 0",
                350, 25, TextAlignmentOptions.Left, new Color(0.7f, 0.7f, 0.7f), 14, FontStyles.Normal,
                new Vector2(0, 325));

            // Table header
            var headerRow = CreatePanel(canvas.transform, "TableHeader",
                new Vector2(0, 295), new Vector2(410, 30), new Color(0.3f, 0.2f, 0.25f, 1f));
            var headerHLG = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerHLG.spacing = 5;
            headerHLG.padding = new RectOffset(10, 10, 0, 0);
            headerHLG.childAlignment = TextAnchor.MiddleLeft;
            headerHLG.childForceExpandHeight = false;

            CreateTableHeaderCell(headerRow.transform, "#", 40);
            CreateTableHeaderCell(headerRow.transform, "NAME", 120);
            CreateTableHeaderCell(headerRow.transform, "SCORE", 70);
            CreateTableHeaderCell(headerRow.transform, "TIMESTAMP", 150);

            // Scrollable table body
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(canvas.transform, false);
            var scrollRectTransform = scrollObj.AddComponent<RectTransform>();
            scrollRectTransform.anchoredPosition = new Vector2(0, 20);
            scrollRectTransform.sizeDelta = new Vector2(410, 500);

            var scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;

            var scrollImage = scrollObj.AddComponent<Image>();
            scrollImage.color = new Color(0.2f, 0.15f, 0.17f, 1f);

            var mask = scrollObj.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            // Content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0);

            var vlg = contentObj.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.spacing = 2;
            vlg.padding = new RectOffset(0, 0, 5, 5);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = contentObj.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollView.content = contentRect;

            // Export status text
            var exportStatus = CreateTMPChild(canvas.transform, "ExportStatusText", "",
                400, 40, TextAlignmentOptions.Center, new Color(0.5f, 1f, 0.5f), 12, FontStyles.Italic,
                new Vector2(0, -270));

            // Buttons
            var exportBtn = CreateButton(canvas.transform, "ExportCSVButton", "EXPORT TO CSV",
                new Vector2(0, -310), new Vector2(250, 50), new Color(0.2f, 0.7f, 0.3f), White, 20);
            var clearBtn = CreateButton(canvas.transform, "ClearDataButton", "CLEAR ALL DATA",
                new Vector2(0, -370), new Vector2(250, 50), new Color(0.8f, 0.2f, 0.2f), White, 20);
            var backBtn = CreateButton(canvas.transform, "BackButton", "BACK",
                new Vector2(0, -420), new Vector2(250, 45), new Color(0.4f, 0.4f, 0.4f), White, 18);

            // Create admin table row prefab
            CreateAdminTableRowPrefab();

            // Wire up AdminUI
            var adminUI = canvas.AddComponent<KoKoKrunch.UI.AdminUI>();
            var so = new SerializedObject(adminUI);
            so.FindProperty("tableContent").objectReferenceValue = contentRect;

            var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/UI/AdminTableRow.prefab");
            so.FindProperty("tableRowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("totalEntriesText").objectReferenceValue = totalText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("exportStatusText").objectReferenceValue = exportStatus.GetComponent<TextMeshProUGUI>();
            so.FindProperty("exportCSVButton").objectReferenceValue = exportBtn.GetComponent<Button>();
            so.FindProperty("clearDataButton").objectReferenceValue = clearBtn.GetComponent<Button>();
            so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/AdminScene.unity");
            Debug.Log("AdminScene created");
        }

        private static void CreateTableHeaderCell(Transform parent, string text, float width)
        {
            var cell = CreateTMPChild(parent, $"Header_{text}", text,
                width, 30, TextAlignmentOptions.Left, new Color(0.9f, 0.8f, 0.85f), 12, FontStyles.Bold);
            var le = cell.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = 30;
        }

        private static void CreateAdminTableRowPrefab()
        {
            string path = $"{PrefabsPath}/UI/AdminTableRow.prefab";

            GameObject row = new GameObject("AdminTableRow");
            var rowRect = row.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(410, 28);

            var rowImg = row.AddComponent<Image>();
            rowImg.color = new Color(0.25f, 0.18f, 0.2f, 0.8f);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.spacing = 5;
            hlg.padding = new RectOffset(10, 10, 0, 0);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // #
            var numCell = CreateTMPChild(row.transform, "NumText", "1", 40, 28,
                TextAlignmentOptions.Left, White, 12);
            numCell.AddComponent<LayoutElement>().preferredWidth = 40;

            // Name
            var nameCell = CreateTMPChild(row.transform, "NameText", "Player", 120, 28,
                TextAlignmentOptions.Left, White, 12);
            nameCell.AddComponent<LayoutElement>().preferredWidth = 120;

            // Score
            var scoreCell = CreateTMPChild(row.transform, "ScoreText", "0", 70, 28,
                TextAlignmentOptions.Left, White, 12);
            scoreCell.AddComponent<LayoutElement>().preferredWidth = 70;

            // Timestamp
            var tsCell = CreateTMPChild(row.transform, "TimestampText", "---", 150, 28,
                TextAlignmentOptions.Left, new Color(0.7f, 0.7f, 0.7f), 11);
            tsCell.AddComponent<LayoutElement>().preferredWidth = 150;

            PrefabUtility.SaveAsPrefabAsset(row, path);
            Object.DestroyImmediate(row);
            Debug.Log($"Prefab created: {path}");
        }
        #endregion

        // ──────────────────────────────────────────────
        // Build Settings
        // ──────────────────────────────────────────────
        private static void AddScenesToBuildSettings()
        {
            string[] sceneNames = {
                "LandingScene", "NameInputScene", "InstructionScene",
                "GameScene", "ResultScene", "LeaderboardScene", "AdminScene"
            };

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var name in sceneNames)
            {
                string path = $"{ScenesPath}/{name}.unity";
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Build Settings updated with all 6 scenes");
        }

        // ──────────────────────────────────────────────
        // UI HELPER METHODS
        // ──────────────────────────────────────────────

        private static GameObject CreateCamera()
        {
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            var camera = cam.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.backgroundColor = BgPink;
            camera.clearFlags = CameraClearFlags.SolidColor;
            return cam;
        }

        private static void CreateEventSystem()
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(450, 800);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            return canvasObj;
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            var rect = bg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            var img = bg.AddComponent<Image>();
            img.color = BgPink;
        }

        private static GameObject CreatePanel(Transform parent, string name,
            Vector2 position, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = panel.AddComponent<Image>();
            img.color = color;

            return panel;
        }

        private static GameObject CreateImage(Transform parent, string name,
            Vector2 position, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = obj.AddComponent<Image>();
            img.color = color;

            return obj;
        }

        private static GameObject CreateTMPChild(Transform parent, string name, string text,
            float width, float height, TextAlignmentOptions alignment, Color color,
            float fontSize, FontStyles style = FontStyles.Normal, Vector2? position = null)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, height);
            if (position.HasValue) rect.anchoredPosition = position.Value;

            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = alignment;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.enableAutoSizing = false;

            return obj;
        }

        private static GameObject CreateButton(Transform parent, string name, string label,
            Vector2 position, Vector2 size, Color bgColor, Color textColor, float fontSize)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            // Button label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(btnObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;

            var tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;

            return btnObj;
        }

        private static GameObject CreateInputField(Transform parent, string name,
            Vector2 position, Vector2 size)
        {
            GameObject ifObj = new GameObject(name);
            ifObj.transform.SetParent(parent, false);
            var rect = ifObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var bgImage = ifObj.AddComponent<Image>();
            bgImage.color = White;

            // Text Area
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(ifObj.transform, false);
            var taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(10, 5);
            taRect.offsetMax = new Vector2(-10, -5);

            textArea.AddComponent<RectMask2D>();

            // Placeholder
            var placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            var phRect = placeholder.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.sizeDelta = Vector2.zero;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;

            var phTMP = placeholder.AddComponent<TextMeshProUGUI>();
            phTMP.text = "Enter your name...";
            phTMP.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            phTMP.fontSize = 20;
            phTMP.fontStyle = FontStyles.Italic;

            // Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(textArea.transform, false);
            var txtRect = textObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            var txtTMP = textObj.AddComponent<TextMeshProUGUI>();
            txtTMP.color = TextDark;
            txtTMP.fontSize = 22;

            // TMP_InputField component
            var inputField = ifObj.AddComponent<TMP_InputField>();
            inputField.textViewport = taRect;
            inputField.textComponent = txtTMP;
            inputField.placeholder = phTMP;
            inputField.characterLimit = 20;

            return ifObj;
        }
    }
}
