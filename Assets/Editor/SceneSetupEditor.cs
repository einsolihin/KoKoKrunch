using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
        private static string ImagesPath => "Assets/Images";
        private static string FontsPath => "Assets/Fonts";

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

        private static Sprite LoadImageSprite(string filename)
        {
            string path = $"{ImagesPath}/{filename}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
                Debug.LogWarning($"Image not found: {path}");
            return sprite;
        }

        private static TMP_FontAsset LoadFont(string filename)
        {
            string path = $"{FontsPath}/{filename}";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null)
                Debug.LogWarning($"Font not found: {path}");
            return font;
        }

        private static void SetImageSprite(GameObject imgObj, string filename)
        {
            var sprite = LoadImageSprite(filename);
            if (sprite != null)
            {
                var img = imgObj.GetComponent<Image>();
                img.sprite = sprite;
                img.color = Color.white;
                img.preserveAspect = true;
            }
        }

        // ──────────────────────────────────────────────
        // Prefabs
        // ──────────────────────────────────────────────
        private static void CreateItemPrefabs()
        {
            CreateItemPrefab("Strawberry", "strawberry.png", StrawberryRed, KoKoKrunch.Gameplay.ItemType.Strawberry, 0.5f);
            CreateItemPrefab("KokoKrunchPack1", "kokokrunch ori.png", KokoPack1Brown, KoKoKrunch.Gameplay.ItemType.KokoKrunchPack1, 0.6f);
            CreateItemPrefab("KokoKrunchPack2", "kokokrunch strawbery.png", KokoPack2Green, KoKoKrunch.Gameplay.ItemType.KokoKrunchPack2, 0.6f);
        }

        private static GameObject CreateItemPrefab(string name, string imageName, Color fallbackColor, KoKoKrunch.Gameplay.ItemType itemType, float size)
        {
            string path = $"{PrefabsPath}/Items/{name}.prefab";

            GameObject obj = new GameObject(name);
            var sr = obj.AddComponent<SpriteRenderer>();
            var sprite = LoadImageSprite(imageName);
            sr.sprite = sprite != null ? sprite : CreatePlaceholderSprite($"item_{name}", 64, 64, fallbackColor);
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

            // Logo image
            var logoImg = CreateImage(canvas.transform, "LogoImage",
                new Vector2(0, 200), new Vector2(300, 150), White);
            SetImageSprite(logoImg, "logo kokokrunch.png");

            // Catch & Win image
            var catchWinImg = CreateImage(canvas.transform, "CatchWinImage",
                new Vector2(0, 50), new Vector2(300, 100), White);
            SetImageSprite(catchWinImg, "catch n win.png");

            // Start Button (use image asset)
            var startBtn = CreateImage(canvas.transform, "StartButton",
                new Vector2(0, -200), new Vector2(200, 70), White);
            SetImageSprite(startBtn, "start button.png");
            startBtn.AddComponent<Button>().targetGraphic = startBtn.GetComponent<Image>();

            // Mascot / Kokokrunch image
            var mascot = CreateImage(canvas.transform, "MascotImage",
                new Vector2(0, -70), new Vector2(150, 150), White);
            SetImageSprite(mascot, "kokokrunch.png");

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
            so.FindProperty("startButton").objectReferenceValue = startBtn.GetComponentInChildren<Button>();
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

            // NAME label image
            var nameLabel = CreateImage(canvas.transform, "NameLabelImage",
                new Vector2(0, 150), new Vector2(200, 60), White);
            SetImageSprite(nameLabel, "name .png");

            // Name box background image
            var nameBox = CreateImage(canvas.transform, "NameBoxImage",
                new Vector2(0, 50), new Vector2(320, 60), White);
            SetImageSprite(nameBox, "name box.png");

            // Input Field (overlaid on the name box)
            var inputFieldObj = CreateInputField(canvas.transform, "NameInputField",
                new Vector2(0, 50), new Vector2(280, 45));
            inputFieldObj.GetComponent<Image>().color = new Color(1, 1, 1, 0); // transparent bg

            // Auto-open Windows touch keyboard when input field is selected
            inputFieldObj.AddComponent<KoKoKrunch.Utils.WindowsTouchKeyboard>();

            // Error text
            var errorText = CreateTMPChild(canvas.transform, "ErrorText", "",
                300, 30, TextAlignmentOptions.Center, new Color(1, 0.3f, 0.3f), 16,
                FontStyles.Normal, new Vector2(0, -5));

            // Next Button (use image asset)
            var nextBtn = CreateImage(canvas.transform, "NextButton",
                new Vector2(0, -200), new Vector2(200, 70), White);
            SetImageSprite(nextBtn, "next buttton.png");
            nextBtn.AddComponent<Button>().targetGraphic = nextBtn.GetComponent<Image>();

            // Mascot
            var mascot = CreateImage(canvas.transform, "MascotImage",
                new Vector2(100, -100), new Vector2(120, 120), White);
            SetImageSprite(mascot, "kokokrunch.png");

            // Wire up NameInputUI
            var nameUI = canvas.AddComponent<KoKoKrunch.UI.NameInputUI>();
            var so = new SerializedObject(nameUI);
            so.FindProperty("nameInputField").objectReferenceValue = inputFieldObj.GetComponent<TMP_InputField>();
            so.FindProperty("nextButton").objectReferenceValue = nextBtn.GetComponentInChildren<Button>();
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

            // Instruction items with real images
            float yStart = 220;
            float yStep = -100;

            // Strawberry row
            var row1 = CreatePanel(canvas.transform, "Row1",
                new Vector2(0, yStart), new Vector2(350, 90), new Color(1, 1, 1, 0));
            var strawberryIcon = CreateImage(row1.transform, "StrawberryIcon",
                new Vector2(-120, 0), new Vector2(60, 60), White);
            SetImageSprite(strawberryIcon, "strawberry.png");
            var pts5 = CreateImage(row1.transform, "5PointsImage",
                new Vector2(30, 0), new Vector2(120, 50), White);
            SetImageSprite(pts5, "5 points.png");

            // Koko Pack 1 row
            var row2 = CreatePanel(canvas.transform, "Row2",
                new Vector2(0, yStart + yStep), new Vector2(350, 90), new Color(1, 1, 1, 0));
            var pack1Icon = CreateImage(row2.transform, "Pack1Icon",
                new Vector2(-120, 0), new Vector2(60, 60), White);
            SetImageSprite(pack1Icon, "kokokrunch ori.png");
            var pts10a = CreateImage(row2.transform, "10PointsImage1",
                new Vector2(30, 0), new Vector2(120, 50), White);
            SetImageSprite(pts10a, "10 points.png");

            // Koko Pack 2 row
            var row3 = CreatePanel(canvas.transform, "Row3",
                new Vector2(0, yStart + yStep * 2), new Vector2(350, 90), new Color(1, 1, 1, 0));
            var pack2Icon = CreateImage(row3.transform, "Pack2Icon",
                new Vector2(-120, 0), new Vector2(60, 60), White);
            SetImageSprite(pack2Icon, "kokokrunch strawbery.png");
            var pts10b = CreateImage(row3.transform, "10PointsImage2",
                new Vector2(30, 0), new Vector2(120, 50), White);
            SetImageSprite(pts10b, "10 points.png");

            // Rules
            CreateTMPChild(canvas.transform, "RulesText",
                "3 LIVES  |  30 SECONDS\nMissed item = Lose 1 life",
                350, 60, TextAlignmentOptions.Center, White, 16, FontStyles.Italic,
                new Vector2(0, -120));

            // Next Button (use image asset)
            var nextBtn = CreateImage(canvas.transform, "NextButton",
                new Vector2(0, -250), new Vector2(200, 70), White);
            SetImageSprite(nextBtn, "next buttton.png");
            nextBtn.AddComponent<Button>().targetGraphic = nextBtn.GetComponent<Image>();

            // Wire up InstructionUI
            var instUI = canvas.AddComponent<KoKoKrunch.UI.InstructionUI>();
            var so = new SerializedObject(instUI);
            so.FindProperty("nextButton").objectReferenceValue = nextBtn.GetComponentInChildren<Button>();
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
            string[] lifeImages = { "life 1.png", "life 2.png", "life 3.png" };
            for (int i = 0; i < 3; i++)
            {
                var heart = CreateImage(heartsPanel.transform, $"Heart{i}",
                    Vector2.zero, new Vector2(30, 30), HeartRed);
                SetImageSprite(heart, lifeImages[i]);
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
            var bgSprite = LoadImageSprite("background.png");
            bgSR.sprite = bgSprite != null ? bgSprite : CreatePlaceholderSprite("game_bg", 450, 800, BgPink);
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

            // Congratulations image
            var congratsImg = CreateImage(canvas.transform, "CongratsImage",
                new Vector2(0, 250), new Vector2(350, 80), White);
            SetImageSprite(congratsImg, "congratulations.png");

            // Final Score label image
            var finalScoreImg = CreateImage(canvas.transform, "FinalScoreImage",
                new Vector2(0, 150), new Vector2(250, 60), White);
            SetImageSprite(finalScoreImg, "final score.png");

            // Score box background
            var scoreBox = CreateImage(canvas.transform, "ScoreBox",
                new Vector2(0, 50), new Vector2(200, 100), White);
            SetImageSprite(scoreBox, "score box.png");

            // Score text (overlaid on score box)
            var scoreText = CreateTMPChild(canvas.transform, "ScoreText", "00",
                200, 100, TextAlignmentOptions.Center, White, 72, FontStyles.Bold,
                new Vector2(0, 50));

            // Congratulations text (hidden, used for code reference)
            var congratsText = CreateTMPChild(canvas.transform, "CongratsText", "",
                1, 1, TextAlignmentOptions.Center, new Color(1, 1, 1, 0), 1,
                FontStyles.Normal, new Vector2(0, 400));

            // Mascot
            var mascot = CreateImage(canvas.transform, "MascotImage",
                new Vector2(0, -80), new Vector2(150, 150), White);
            SetImageSprite(mascot, "kokokrunch.png");

            // Leaderboard Button
            var lbBtn = CreateButton(canvas.transform, "LeaderboardButton", "LEADERBOARD",
                new Vector2(0, -230), new Vector2(250, 55), ButtonPink, TextDark, 22);

            // Play Again Button (use start button image)
            var playBtn = CreateImage(canvas.transform, "PlayAgainButton",
                new Vector2(0, -310), new Vector2(200, 60), White);
            SetImageSprite(playBtn, "start button.png");
            playBtn.AddComponent<Button>().targetGraphic = playBtn.GetComponent<Image>();

            // Wire up ResultUI
            var resultUI = canvas.AddComponent<KoKoKrunch.UI.ResultUI>();
            var so = new SerializedObject(resultUI);
            so.FindProperty("scoreText").objectReferenceValue = scoreText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("congratsText").objectReferenceValue = congratsText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("leaderboardButton").objectReferenceValue = lbBtn.GetComponent<Button>();
            so.FindProperty("playAgainButton").objectReferenceValue = playBtn.GetComponentInChildren<Button>();
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

            // Leaderboard page background/header image
            var lbHeaderImg = CreateImage(canvas.transform, "LeaderboardHeaderImage",
                new Vector2(0, 330), new Vector2(350, 60), White);
            SetImageSprite(lbHeaderImg, "learderboard page.png");

            // Top player highlight
            var topPanel = CreatePanel(canvas.transform, "TopPlayerPanel",
                new Vector2(0, 240), new Vector2(350, 100), new Color(1, 1, 1, 0.15f));

            // Number 1 image
            var num1Img = CreateImage(topPanel.transform, "Num1Image",
                new Vector2(-140, 0), new Vector2(40, 40), White);
            SetImageSprite(num1Img, "number 1.png");

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

            // Scoreboard logo
            var sbLogo = CreateImage(canvas.transform, "ScoreboardLogo",
                new Vector2(0, 150), new Vector2(200, 50), White);
            SetImageSprite(sbLogo, "scoreboard logo.png");

            // Play Again Button
            var playBtn = CreateImage(canvas.transform, "PlayAgainButton",
                new Vector2(0, -310), new Vector2(200, 60), White);
            SetImageSprite(playBtn, "start button.png");
            playBtn.AddComponent<Button>().targetGraphic = playBtn.GetComponent<Image>();

            // Wire up LeaderboardUI
            var lbUI = canvas.AddComponent<KoKoKrunch.UI.LeaderboardUI>();
            var so = new SerializedObject(lbUI);
            so.FindProperty("topPlayerRankText").objectReferenceValue = topRank.GetComponent<TextMeshProUGUI>();
            so.FindProperty("topPlayerNameText").objectReferenceValue = topName.GetComponent<TextMeshProUGUI>();
            so.FindProperty("topPlayerScoreText").objectReferenceValue = topScore.GetComponent<TextMeshProUGUI>();
            so.FindProperty("leaderboardContent").objectReferenceValue = contentRect;

            var entryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/UI/LeaderboardEntryRow.prefab");
            so.FindProperty("leaderboardEntryPrefab").objectReferenceValue = entryPrefab;
            so.FindProperty("playAgainButton").objectReferenceValue = playBtn.GetComponentInChildren<Button>();
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

            Color adminBg = new Color(0.12f, 0.08f, 0.06f, 1f);
            Color panelBg = new Color(0.18f, 0.14f, 0.12f, 0.95f);
            Color activeTab = new Color(0.9f, 0.75f, 0.2f, 1f);
            Color inactiveTab = new Color(0.35f, 0.28f, 0.22f, 1f);
            Color dangerRed = new Color(0.85f, 0.25f, 0.2f, 1f);
            Color inputBg = new Color(0.15f, 0.12f, 0.1f, 1f);
            Color headerGold = new Color(0.9f, 0.75f, 0.2f, 1f);

            // ── Root container (full screen with VLG) ──
            var root = new GameObject("Root");
            root.transform.SetParent(canvas.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            root.AddComponent<Image>().color = adminBg;

            var rootVlg = root.AddComponent<VerticalLayoutGroup>();
            rootVlg.spacing = 8f;
            rootVlg.padding = new RectOffset(20, 20, 40, 20);
            rootVlg.childControlWidth = true;
            rootVlg.childControlHeight = true;
            rootVlg.childForceExpandWidth = true;
            rootVlg.childForceExpandHeight = false;

            // ── Title ──
            var title = CreateTMPChild(root.transform, "AdminTitle", "Admin Panel",
                0, 50, TextAlignmentOptions.Center, headerGold, 28, FontStyles.Bold);
            title.AddComponent<LayoutElement>().preferredHeight = 50;

            // ── Tab Bar ──
            var tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(root.transform, false);
            tabBar.AddComponent<RectTransform>();
            tabBar.AddComponent<LayoutElement>().preferredHeight = 50;
            var tabHlg = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabHlg.spacing = 4f;
            tabHlg.childControlWidth = true;
            tabHlg.childControlHeight = true;
            tabHlg.childForceExpandWidth = true;
            tabHlg.childForceExpandHeight = true;

            var leaderboardTab = CreateAdminTabButton(tabBar.transform, "LeaderboardTab", "Leaderboard", activeTab);
            var settingsTab = CreateAdminTabButton(tabBar.transform, "SettingsTab", "Game Settings", inactiveTab);

            // ── Content Area ──
            var contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(root.transform, false);
            contentArea.AddComponent<RectTransform>();
            var contentLe = contentArea.AddComponent<LayoutElement>();
            contentLe.flexibleHeight = 1f;

            // ════════════════════════════════════════
            // LEADERBOARD PANEL
            // ════════════════════════════════════════
            var lbPanel = new GameObject("LeaderboardPanel");
            lbPanel.transform.SetParent(contentArea.transform, false);
            var lbRect = lbPanel.AddComponent<RectTransform>();
            lbRect.anchorMin = Vector2.zero;
            lbRect.anchorMax = Vector2.one;
            lbRect.offsetMin = Vector2.zero;
            lbRect.offsetMax = Vector2.zero;

            var lbVlg = lbPanel.AddComponent<VerticalLayoutGroup>();
            lbVlg.spacing = 8f;
            lbVlg.padding = new RectOffset(0, 0, 10, 10);
            lbVlg.childControlWidth = true;
            lbVlg.childControlHeight = true;
            lbVlg.childForceExpandWidth = true;
            lbVlg.childForceExpandHeight = false;

            // Total entries
            var totalText = CreateTMPChild(lbPanel.transform, "TotalEntriesText", "Total Entries: 0",
                0, 30, TextAlignmentOptions.MidlineLeft, White, 16);
            totalText.AddComponent<LayoutElement>().preferredHeight = 30;

            // Table header row
            var headerRow = new GameObject("TableHeader");
            headerRow.transform.SetParent(lbPanel.transform, false);
            headerRow.AddComponent<RectTransform>();
            headerRow.AddComponent<LayoutElement>().preferredHeight = 32;
            headerRow.AddComponent<Image>().color = new Color(0.25f, 0.2f, 0.15f, 1f);
            var headerHlg = headerRow.AddComponent<HorizontalLayoutGroup>();
            headerHlg.spacing = 5;
            headerHlg.padding = new RectOffset(10, 10, 0, 0);
            headerHlg.childControlWidth = true;
            headerHlg.childControlHeight = true;
            headerHlg.childForceExpandWidth = false;
            headerHlg.childForceExpandHeight = true;

            CreateAdminHeaderCell(headerRow.transform, "#", 50, headerGold);
            CreateAdminHeaderCell(headerRow.transform, "Name", 120, headerGold);
            CreateAdminHeaderCell(headerRow.transform, "Score", 70, headerGold);
            CreateAdminHeaderCell(headerRow.transform, "Timestamp", 0, headerGold, 1f);

            // Scroll view for table
            var tableScroll = CreateAdminScrollView(lbPanel.transform, "TableScroll", panelBg);
            var tableScrollLe = tableScroll.gameObject.AddComponent<LayoutElement>();
            tableScrollLe.flexibleHeight = 1f;
            var tableContent = tableScroll.content;

            // Leaderboard button row
            var lbBtnRow = new GameObject("ButtonRow");
            lbBtnRow.transform.SetParent(lbPanel.transform, false);
            lbBtnRow.AddComponent<RectTransform>();
            lbBtnRow.AddComponent<LayoutElement>().preferredHeight = 45;
            var lbBtnHlg = lbBtnRow.AddComponent<HorizontalLayoutGroup>();
            lbBtnHlg.spacing = 10;
            lbBtnHlg.childControlWidth = true;
            lbBtnHlg.childControlHeight = true;
            lbBtnHlg.childForceExpandWidth = true;
            lbBtnHlg.childForceExpandHeight = true;

            var exportBtn = CreateAdminActionButton(lbBtnRow.transform, "ExportCSVButton", "Export CSV", activeTab);
            var clearBtn = CreateAdminActionButton(lbBtnRow.transform, "ClearDataButton", "Clear Data", dangerRed);

            // Export status text
            var exportStatus = CreateTMPChild(lbPanel.transform, "ExportStatusText", "",
                0, 35, TextAlignmentOptions.MidlineLeft, new Color(0.7f, 0.7f, 0.7f), 12);
            exportStatus.AddComponent<LayoutElement>().preferredHeight = 35;

            // ════════════════════════════════════════
            // SETTINGS PANEL
            // ════════════════════════════════════════
            var settingsPanel = new GameObject("SettingsPanel");
            settingsPanel.transform.SetParent(contentArea.transform, false);
            var spRect = settingsPanel.AddComponent<RectTransform>();
            spRect.anchorMin = Vector2.zero;
            spRect.anchorMax = Vector2.one;
            spRect.offsetMin = Vector2.zero;
            spRect.offsetMax = Vector2.zero;

            var spVlg = settingsPanel.AddComponent<VerticalLayoutGroup>();
            spVlg.spacing = 5f;
            spVlg.padding = new RectOffset(0, 0, 10, 10);
            spVlg.childControlWidth = true;
            spVlg.childControlHeight = true;
            spVlg.childForceExpandWidth = true;
            spVlg.childForceExpandHeight = false;

            // Settings scroll view
            var settingsScroll = CreateAdminScrollView(settingsPanel.transform, "SettingsScroll", panelBg);
            var settingsScrollLe = settingsScroll.gameObject.AddComponent<LayoutElement>();
            settingsScrollLe.flexibleHeight = 1f;
            var settingsContent = settingsScroll.content;

            // Setting rows
            CreateAdminSectionHeader(settingsContent.transform, "Scoring", headerGold);
            var strawberryInput = CreateAdminSettingRow(settingsContent.transform, "Strawberry Points", inputBg);
            var kokoInput = CreateAdminSettingRow(settingsContent.transform, "KoKo Krunch Points", inputBg);

            CreateAdminSectionHeader(settingsContent.transform, "Difficulty", headerGold);
            var fallSpeedInput = CreateAdminSettingRow(settingsContent.transform, "Item Fall Speed", inputBg);
            var maxFallInput = CreateAdminSettingRow(settingsContent.transform, "Max Fall Speed", inputBg);
            var fallIncreaseInput = CreateAdminSettingRow(settingsContent.transform, "Fall Speed Increase", inputBg);

            CreateAdminSectionHeader(settingsContent.transform, "Spawning", headerGold);
            var spawnIntervalInput = CreateAdminSettingRow(settingsContent.transform, "Spawn Interval (s)", inputBg);
            var minSpawnInput = CreateAdminSettingRow(settingsContent.transform, "Min Spawn Interval (s)", inputBg);
            var spawnAccelInput = CreateAdminSettingRow(settingsContent.transform, "Spawn Acceleration", inputBg);

            CreateAdminSectionHeader(settingsContent.transform, "Game Rules", headerGold);
            var durationInput = CreateAdminSettingRow(settingsContent.transform, "Game Duration (s)", inputBg);
            var livesInput = CreateAdminSettingRow(settingsContent.transform, "Max Lives", inputBg);

            CreateAdminSectionHeader(settingsContent.transform, "Player", headerGold);
            var moveSpeedInput = CreateAdminSettingRow(settingsContent.transform, "Player Move Speed", inputBg);

            // Settings button row
            var sBtnRow = new GameObject("SettingsBtnRow");
            sBtnRow.transform.SetParent(settingsPanel.transform, false);
            sBtnRow.AddComponent<RectTransform>();
            sBtnRow.AddComponent<LayoutElement>().preferredHeight = 45;
            var sBtnHlg = sBtnRow.AddComponent<HorizontalLayoutGroup>();
            sBtnHlg.spacing = 10;
            sBtnHlg.childControlWidth = true;
            sBtnHlg.childControlHeight = true;
            sBtnHlg.childForceExpandWidth = true;
            sBtnHlg.childForceExpandHeight = true;

            var saveBtn = CreateAdminActionButton(sBtnRow.transform, "SaveSettingsButton", "Save Settings", activeTab);
            var resetBtn = CreateAdminActionButton(sBtnRow.transform, "ResetDefaultsButton", "Reset to Defaults", dangerRed);

            // Settings status text
            var settingsStatus = CreateTMPChild(settingsPanel.transform, "SettingsStatusText", "",
                0, 30, TextAlignmentOptions.Center, new Color(0.7f, 0.7f, 0.7f), 14);
            settingsStatus.AddComponent<LayoutElement>().preferredHeight = 30;

            // Hide settings panel by default
            settingsPanel.SetActive(false);

            // ── Back Button ──
            var backBtn = CreateAdminActionButton(root.transform, "BackButton", "Back", inactiveTab);
            backBtn.gameObject.AddComponent<LayoutElement>().preferredHeight = 50;

            // ════════════════════════════════════════
            // Create table row prefab
            // ════════════════════════════════════════
            CreateAdminTableRowPrefab();

            // ════════════════════════════════════════
            // Wire up AdminUI component
            // ════════════════════════════════════════
            var adminUI = canvas.AddComponent<KoKoKrunch.UI.AdminUI>();
            var so = new SerializedObject(adminUI);

            // Tabs
            so.FindProperty("leaderboardTabButton").objectReferenceValue = leaderboardTab.GetComponent<Button>();
            so.FindProperty("settingsTabButton").objectReferenceValue = settingsTab.GetComponent<Button>();
            so.FindProperty("leaderboardPanel").objectReferenceValue = lbPanel;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;

            // Leaderboard
            so.FindProperty("tableContent").objectReferenceValue = tableContent;
            var rowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsPath}/UI/AdminTableRow.prefab");
            so.FindProperty("tableRowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("totalEntriesText").objectReferenceValue = totalText.GetComponent<TextMeshProUGUI>();
            so.FindProperty("exportStatusText").objectReferenceValue = exportStatus.GetComponent<TextMeshProUGUI>();
            so.FindProperty("exportCSVButton").objectReferenceValue = exportBtn.GetComponent<Button>();
            so.FindProperty("clearDataButton").objectReferenceValue = clearBtn.GetComponent<Button>();

            // Settings inputs
            so.FindProperty("strawberryPointsInput").objectReferenceValue = strawberryInput;
            so.FindProperty("kokoKrunchPointsInput").objectReferenceValue = kokoInput;
            so.FindProperty("itemFallSpeedInput").objectReferenceValue = fallSpeedInput;
            so.FindProperty("maxFallSpeedInput").objectReferenceValue = maxFallInput;
            so.FindProperty("fallSpeedIncreaseInput").objectReferenceValue = fallIncreaseInput;
            so.FindProperty("initialSpawnIntervalInput").objectReferenceValue = spawnIntervalInput;
            so.FindProperty("minimumSpawnIntervalInput").objectReferenceValue = minSpawnInput;
            so.FindProperty("spawnAccelerationInput").objectReferenceValue = spawnAccelInput;
            so.FindProperty("gameDurationInput").objectReferenceValue = durationInput;
            so.FindProperty("maxLivesInput").objectReferenceValue = livesInput;
            so.FindProperty("playerMoveSpeedInput").objectReferenceValue = moveSpeedInput;

            // Settings buttons
            so.FindProperty("saveSettingsButton").objectReferenceValue = saveBtn.GetComponent<Button>();
            so.FindProperty("resetDefaultsButton").objectReferenceValue = resetBtn.GetComponent<Button>();
            so.FindProperty("settingsStatusText").objectReferenceValue = settingsStatus.GetComponent<TextMeshProUGUI>();

            // Navigation
            so.FindProperty("backButton").objectReferenceValue = backBtn.GetComponent<Button>();

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, $"{ScenesPath}/AdminScene.unity");
            Debug.Log("AdminScene created with full tabbed UI");
        }

        // ── Admin Scene Helper Methods ──

        private static GameObject CreateAdminTabButton(Transform parent, string name, string label, Color bgColor)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            obj.AddComponent<Image>().color = bgColor;
            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = obj.GetComponent<Image>();

            var labelObj = CreateTMPChild(obj.transform, "Label", label,
                0, 0, TextAlignmentOptions.Center, White, 16, FontStyles.Bold);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return obj;
        }

        private static GameObject CreateAdminActionButton(Transform parent, string name, string label, Color bgColor)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            obj.AddComponent<Image>().color = bgColor;
            var btn = obj.AddComponent<Button>();
            btn.targetGraphic = obj.GetComponent<Image>();

            var labelObj = CreateTMPChild(obj.transform, "Label", label,
                0, 0, TextAlignmentOptions.Center, White, 14, FontStyles.Bold);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return obj;
        }

        private static void CreateAdminHeaderCell(Transform parent, string label, float prefWidth, Color color, float flexWidth = 0f)
        {
            var obj = CreateTMPChild(parent, $"Header_{label}", $"<b>{label}</b>",
                prefWidth, 0, TextAlignmentOptions.MidlineLeft, color, 13);
            var le = obj.AddComponent<LayoutElement>();
            if (prefWidth > 0) le.preferredWidth = prefWidth;
            le.flexibleWidth = flexWidth;
        }

        private static ScrollRect CreateAdminScrollView(Transform parent, string name, Color bgColor)
        {
            var scrollObj = new GameObject(name);
            scrollObj.transform.SetParent(parent, false);
            scrollObj.AddComponent<RectTransform>();
            scrollObj.AddComponent<Image>().color = bgColor;

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            var viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollObj.transform, false);
            var vpRect = viewport.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = new Vector2(5, 5);
            vpRect.offsetMax = new Vector2(-5, -5);
            viewport.AddComponent<Image>().color = White;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = vpRect;

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2;
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            return scrollRect;
        }

        private static void CreateAdminSectionHeader(Transform parent, string title, Color color)
        {
            var obj = new GameObject($"Header_{title}");
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            obj.AddComponent<LayoutElement>().preferredHeight = 35;

            var tmp = CreateTMPChild(obj.transform, "Text", $"<b>--- {title} ---</b>",
                0, 0, TextAlignmentOptions.MidlineLeft, color, 16);
            var textRect = tmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = Vector2.zero;
        }

        private static TMP_InputField CreateAdminSettingRow(Transform parent, string label, Color inputBgColor)
        {
            var row = new GameObject($"Row_{label}");
            row.transform.SetParent(parent, false);
            row.AddComponent<RectTransform>();
            row.AddComponent<LayoutElement>().preferredHeight = 38;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.padding = new RectOffset(10, 10, 2, 2);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Label
            var labelObj = CreateTMPChild(row.transform, "Label", label,
                200, 0, TextAlignmentOptions.MidlineLeft, White, 14);
            var labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 200;
            labelLe.flexibleWidth = 1;

            // Input field
            var inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(row.transform, false);
            inputObj.AddComponent<RectTransform>();
            inputObj.AddComponent<Image>().color = inputBgColor;

            var inputLe = inputObj.AddComponent<LayoutElement>();
            inputLe.preferredWidth = 130;
            inputLe.preferredHeight = 34;
            inputLe.flexibleWidth = 0;

            // Text Area
            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputObj.transform, false);
            var taRect = textArea.AddComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(8, 2);
            taRect.offsetMax = new Vector2(-8, -2);
            textArea.AddComponent<RectMask2D>();

            // Input text
            var inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            var itRect = inputTextObj.AddComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero;
            itRect.anchorMax = Vector2.one;
            itRect.offsetMin = Vector2.zero;
            itRect.offsetMax = Vector2.zero;

            var inputTmp = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputTmp.fontSize = 14;
            inputTmp.color = White;
            inputTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Placeholder
            var phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(textArea.transform, false);
            var phRect = phObj.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;

            var phTmp = phObj.AddComponent<TextMeshProUGUI>();
            phTmp.text = "...";
            phTmp.fontSize = 14;
            phTmp.color = new Color(1f, 1f, 1f, 0.3f);
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Wire TMP_InputField
            var inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textComponent = inputTmp;
            inputField.textViewport = taRect;
            inputField.placeholder = phTmp;
            inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputField.pointSize = 14;

            return inputField;
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
            Debug.Log("Build Settings updated with all 7 scenes");
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
            es.AddComponent<InputSystemUIInputModule>();
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
            var bgSprite = LoadImageSprite("background.png");
            if (bgSprite != null)
            {
                img.sprite = bgSprite;
                img.color = Color.white;
                img.preserveAspect = false;
            }
            else
            {
                img.color = BgPink;
            }
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
