using System.Collections.Generic;
using KoKoKrunch.Data;
using KoKoKrunch.Managers;
using KoKoKrunch.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KoKoKrunch.UI
{
    public class AdminUI : MonoBehaviour
    {
        [Header("Existing Prefab")]
        [SerializeField] private GameObject tableRowPrefab;

        // Colors
        private readonly Color bgColor = new Color(0.12f, 0.08f, 0.06f, 1f);
        private readonly Color panelColor = new Color(0.18f, 0.14f, 0.12f, 0.95f);
        private readonly Color activeTabColor = new Color(0.9f, 0.75f, 0.2f, 1f);
        private readonly Color inactiveTabColor = new Color(0.35f, 0.28f, 0.22f, 1f);
        private readonly Color buttonColor = new Color(0.9f, 0.75f, 0.2f, 1f);
        private readonly Color dangerColor = new Color(0.85f, 0.25f, 0.2f, 1f);
        private readonly Color inputBgColor = new Color(0.15f, 0.12f, 0.1f, 1f);
        private readonly Color headerTextColor = new Color(0.9f, 0.75f, 0.2f, 1f);

        // UI references built at runtime
        private Button leaderboardTabButton;
        private Button settingsTabButton;
        private GameObject leaderboardPanel;
        private GameObject settingsPanel;

        // Leaderboard
        private Transform tableContent;
        private TextMeshProUGUI totalEntriesText;
        private TextMeshProUGUI exportStatusText;

        // Settings input fields
        private TMP_InputField strawberryPointsInput;
        private TMP_InputField kokoKrunchPointsInput;
        private TMP_InputField itemFallSpeedInput;
        private TMP_InputField maxFallSpeedInput;
        private TMP_InputField fallSpeedIncreaseInput;
        private TMP_InputField initialSpawnIntervalInput;
        private TMP_InputField minimumSpawnIntervalInput;
        private TMP_InputField spawnAccelerationInput;
        private TMP_InputField gameDurationInput;
        private TMP_InputField maxLivesInput;
        private TMP_InputField playerMoveSpeedInput;
        private TextMeshProUGUI settingsStatusText;

        private void Start()
        {
            BuildEntireUI();
            SwitchTab(true);
            PopulateTable();
            PopulateSettings();
        }

        #region Full UI Builder

        private void BuildEntireUI()
        {
            // Canvas (use existing if on a Canvas, otherwise create one)
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("AdminCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                transform.SetParent(canvasObj.transform, false);
            }

            // Ensure EventSystem exists
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }

            // Root container (full screen)
            var root = CreateRect("Root", canvas.transform);
            StretchFull(root);
            var rootImage = root.gameObject.AddComponent<Image>();
            rootImage.color = bgColor;

            // Vertical layout for root: Title, Tabs, Content, BackButton
            var rootVlg = root.gameObject.AddComponent<VerticalLayoutGroup>();
            rootVlg.spacing = 8f;
            rootVlg.padding = new RectOffset(20, 20, 40, 20);
            rootVlg.childControlWidth = true;
            rootVlg.childControlHeight = true;
            rootVlg.childForceExpandWidth = true;
            rootVlg.childForceExpandHeight = false;

            // Title
            var title = CreateText("AdminTitle", root, "Admin Panel", 28f, headerTextColor, TextAlignmentOptions.Center);
            AddLayout(title.gameObject, -1, 50f);

            // Tab bar
            var tabBar = CreateRect("TabBar", root);
            AddLayout(tabBar.gameObject, -1, 50f);
            var tabHlg = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabHlg.spacing = 4f;
            tabHlg.childControlWidth = true;
            tabHlg.childControlHeight = true;
            tabHlg.childForceExpandWidth = true;
            tabHlg.childForceExpandHeight = true;

            leaderboardTabButton = CreateTabButton("LeaderboardTab", tabBar, "Leaderboard");
            settingsTabButton = CreateTabButton("SettingsTab", tabBar, "Game Settings");

            leaderboardTabButton.onClick.AddListener(() => SwitchTab(true));
            settingsTabButton.onClick.AddListener(() => SwitchTab(false));

            // Content area (takes remaining space)
            var contentArea = CreateRect("ContentArea", root);
            var contentLayout = AddLayout(contentArea.gameObject, -1, -1);
            contentLayout.flexibleHeight = 1f;

            // Leaderboard Panel
            BuildLeaderboardPanel(contentArea);

            // Settings Panel
            BuildSettingsPanel(contentArea);

            // Back button
            var backBtn = CreateActionButton("BackButton", root, "Back", inactiveTabColor);
            AddLayout(backBtn.gameObject, -1, 50f);
            backBtn.GetComponent<Button>().onClick.AddListener(OnBack);
        }

        private void BuildLeaderboardPanel(RectTransform parent)
        {
            var panel = CreateRect("LeaderboardPanel", parent);
            StretchFull(panel);
            leaderboardPanel = panel.gameObject;

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.padding = new RectOffset(0, 0, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Total entries text
            totalEntriesText = CreateText("TotalEntries", panel, "Total Entries: 0", 16f, Color.white, TextAlignmentOptions.MidlineLeft);
            AddLayout(totalEntriesText.gameObject, -1, 30f);

            // Table header
            var headerRow = CreateRect("TableHeader", panel);
            AddLayout(headerRow.gameObject, -1, 32f);
            var headerBg = headerRow.gameObject.AddComponent<Image>();
            headerBg.color = new Color(0.25f, 0.2f, 0.15f, 1f);

            var headerHlg = headerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerHlg.spacing = 5f;
            headerHlg.padding = new RectOffset(10, 10, 0, 0);
            headerHlg.childControlWidth = true;
            headerHlg.childControlHeight = true;
            headerHlg.childForceExpandWidth = false;
            headerHlg.childForceExpandHeight = true;

            CreateHeaderCell(headerRow, "#", 50f);
            CreateHeaderCell(headerRow, "Name", 120f);
            CreateHeaderCell(headerRow, "Score", 70f);
            CreateHeaderCell(headerRow, "Timestamp", 0f, 1f);

            // Scroll view for table
            var scrollRect = CreateScrollView("TableScroll", panel);
            var scrollLayout = AddLayout(scrollRect.gameObject, -1, -1);
            scrollLayout.flexibleHeight = 1f;
            tableContent = scrollRect.content;

            // Button row
            var btnRow = CreateRect("ButtonRow", panel);
            AddLayout(btnRow.gameObject, -1, 45f);
            var btnHlg = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 10f;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            btnHlg.childForceExpandHeight = true;

            var exportBtn = CreateActionButton("ExportCSV", btnRow, "Export CSV", buttonColor);
            var clearBtn = CreateActionButton("ClearData", btnRow, "Clear Data", dangerColor);

            exportBtn.GetComponent<Button>().onClick.AddListener(OnExportCSV);
            clearBtn.GetComponent<Button>().onClick.AddListener(OnClearData);

            // Export status text
            exportStatusText = CreateText("ExportStatus", panel, "", 12f, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.MidlineLeft);
            AddLayout(exportStatusText.gameObject, -1, 35f);
        }

        private void BuildSettingsPanel(RectTransform parent)
        {
            var panel = CreateRect("SettingsPanel", parent);
            StretchFull(panel);
            settingsPanel = panel.gameObject;

            var vlg = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5f;
            vlg.padding = new RectOffset(0, 0, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Scroll view for settings
            var scrollRect = CreateScrollView("SettingsScroll", panel);
            var scrollLayout = AddLayout(scrollRect.gameObject, -1, -1);
            scrollLayout.flexibleHeight = 1f;

            var content = scrollRect.content;

            // Build setting rows
            CreateSectionHeader(content, "Scoring");
            strawberryPointsInput = CreateSettingRow(content, "Strawberry Points", TMP_InputField.ContentType.IntegerNumber);
            kokoKrunchPointsInput = CreateSettingRow(content, "KoKo Krunch Points", TMP_InputField.ContentType.IntegerNumber);

            CreateSectionHeader(content, "Difficulty");
            itemFallSpeedInput = CreateSettingRow(content, "Item Fall Speed", TMP_InputField.ContentType.DecimalNumber);
            maxFallSpeedInput = CreateSettingRow(content, "Max Fall Speed", TMP_InputField.ContentType.DecimalNumber);
            fallSpeedIncreaseInput = CreateSettingRow(content, "Fall Speed Increase", TMP_InputField.ContentType.DecimalNumber);

            CreateSectionHeader(content, "Spawning");
            initialSpawnIntervalInput = CreateSettingRow(content, "Spawn Interval (s)", TMP_InputField.ContentType.DecimalNumber);
            minimumSpawnIntervalInput = CreateSettingRow(content, "Min Spawn Interval (s)", TMP_InputField.ContentType.DecimalNumber);
            spawnAccelerationInput = CreateSettingRow(content, "Spawn Acceleration", TMP_InputField.ContentType.DecimalNumber);

            CreateSectionHeader(content, "Game Rules");
            gameDurationInput = CreateSettingRow(content, "Game Duration (s)", TMP_InputField.ContentType.DecimalNumber);
            maxLivesInput = CreateSettingRow(content, "Max Lives", TMP_InputField.ContentType.IntegerNumber);

            CreateSectionHeader(content, "Player");
            playerMoveSpeedInput = CreateSettingRow(content, "Player Move Speed", TMP_InputField.ContentType.DecimalNumber);

            // Button row
            var btnRow = CreateRect("SettingsBtnRow", panel);
            AddLayout(btnRow.gameObject, -1, 45f);
            var btnHlg = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            btnHlg.spacing = 10f;
            btnHlg.childControlWidth = true;
            btnHlg.childControlHeight = true;
            btnHlg.childForceExpandWidth = true;
            btnHlg.childForceExpandHeight = true;

            var saveBtn = CreateActionButton("SaveSettings", btnRow, "Save Settings", buttonColor);
            var resetBtn = CreateActionButton("ResetDefaults", btnRow, "Reset to Defaults", dangerColor);

            saveBtn.GetComponent<Button>().onClick.AddListener(OnSaveSettings);
            resetBtn.GetComponent<Button>().onClick.AddListener(OnResetDefaults);

            // Status text
            settingsStatusText = CreateText("SettingsStatus", panel, "", 14f, new Color(0.7f, 0.7f, 0.7f), TextAlignmentOptions.Center);
            AddLayout(settingsStatusText.gameObject, -1, 30f);
        }

        #endregion

        #region UI Factory Helpers

        private RectTransform CreateRect(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj.GetComponent<RectTransform>();
        }

        private void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private LayoutElement AddLayout(GameObject obj, float prefWidth = -1f, float prefHeight = -1f)
        {
            var le = obj.GetComponent<LayoutElement>();
            if (le == null) le = obj.AddComponent<LayoutElement>();
            if (prefWidth >= 0) le.preferredWidth = prefWidth;
            if (prefHeight >= 0) le.preferredHeight = prefHeight;
            le.flexibleWidth = 1f;
            return le;
        }

        private TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);

            var tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;

            return tmp;
        }

        private Button CreateTabButton(string name, Transform parent, string label)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            var img = obj.GetComponent<Image>();
            img.color = inactiveTabColor;

            var btn = obj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            var textTmp = CreateText("Label", obj.transform, label, 16f, Color.white, TextAlignmentOptions.Center);
            var textRect = textTmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return btn;
        }

        private RectTransform CreateActionButton(string name, Transform parent, string label, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            var img = obj.GetComponent<Image>();
            img.color = color;

            var btn = obj.GetComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            btn.colors = colors;

            var textTmp = CreateText("Label", obj.transform, label, 14f, Color.white, TextAlignmentOptions.Center);
            var textRect = textTmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return obj.GetComponent<RectTransform>();
        }

        private void CreateHeaderCell(Transform parent, string label, float prefWidth, float flexWidth = 0f)
        {
            var tmp = CreateText($"Header_{label}", parent, $"<b>{label}</b>", 13f, headerTextColor, TextAlignmentOptions.MidlineLeft);
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            if (prefWidth > 0) le.preferredWidth = prefWidth;
            le.flexibleWidth = flexWidth;
        }

        private ScrollRect CreateScrollView(string name, Transform parent)
        {
            // ScrollView root
            var scrollObj = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollObj.transform.SetParent(parent, false);

            var scrollImage = scrollObj.GetComponent<Image>();
            scrollImage.color = panelColor;

            var scrollRect = scrollObj.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            // Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollObj.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(5f, 5f);
            viewportRect.offsetMax = new Vector2(-5f, -5f);

            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            scrollRect.viewport = viewportRect;

            // Content
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentVlg = content.GetComponent<VerticalLayoutGroup>();
            contentVlg.spacing = 2f;
            contentVlg.padding = new RectOffset(5, 5, 5, 5);
            contentVlg.childControlWidth = true;
            contentVlg.childControlHeight = true;
            contentVlg.childForceExpandWidth = true;
            contentVlg.childForceExpandHeight = false;

            var csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRect;

            return scrollRect;
        }

        private void CreateSectionHeader(Transform parent, string title)
        {
            var headerObj = new GameObject($"Header_{title}", typeof(RectTransform), typeof(LayoutElement));
            headerObj.transform.SetParent(parent, false);
            headerObj.GetComponent<LayoutElement>().preferredHeight = 35f;

            var tmp = CreateText("Text", headerObj.transform, $"<b>--- {title} ---</b>", 16f, headerTextColor, TextAlignmentOptions.MidlineLeft);
            var textRect = tmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 0f);
            textRect.offsetMax = Vector2.zero;
        }

        private TMP_InputField CreateSettingRow(Transform parent, string label, TMP_InputField.ContentType contentType)
        {
            // Row
            var rowObj = new GameObject($"Row_{label}", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObj.transform.SetParent(parent, false);
            rowObj.GetComponent<LayoutElement>().preferredHeight = 38f;

            var hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10f;
            hlg.padding = new RectOffset(10, 10, 2, 2);
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Label
            var labelTmp = CreateText("Label", rowObj.transform, label, 14f, Color.white, TextAlignmentOptions.MidlineLeft);
            var labelLe = labelTmp.gameObject.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 200f;
            labelLe.flexibleWidth = 1f;

            // Input field
            var inputObj = new GameObject("InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
            inputObj.transform.SetParent(rowObj.transform, false);

            var inputLe = inputObj.GetComponent<LayoutElement>();
            inputLe.preferredWidth = 130f;
            inputLe.preferredHeight = 34f;
            inputLe.flexibleWidth = 0f;

            inputObj.GetComponent<Image>().color = inputBgColor;

            // Text Area
            var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(inputObj.transform, false);
            var taRect = textArea.GetComponent<RectTransform>();
            taRect.anchorMin = Vector2.zero;
            taRect.anchorMax = Vector2.one;
            taRect.offsetMin = new Vector2(8f, 2f);
            taRect.offsetMax = new Vector2(-8f, -2f);

            // Text
            var inputText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            inputText.transform.SetParent(textArea.transform, false);
            var itRect = inputText.GetComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero;
            itRect.anchorMax = Vector2.one;
            itRect.offsetMin = Vector2.zero;
            itRect.offsetMax = Vector2.zero;

            var inputTmp = inputText.GetComponent<TextMeshProUGUI>();
            inputTmp.fontSize = 14f;
            inputTmp.color = Color.white;
            inputTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Placeholder
            var placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholder.transform.SetParent(textArea.transform, false);
            var phRect = placeholder.GetComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;

            var phTmp = placeholder.GetComponent<TextMeshProUGUI>();
            phTmp.text = "...";
            phTmp.fontSize = 14f;
            phTmp.color = new Color(1f, 1f, 1f, 0.3f);
            phTmp.alignment = TextAlignmentOptions.MidlineLeft;

            // Wire TMP_InputField
            var inputField = inputObj.GetComponent<TMP_InputField>();
            inputField.textComponent = inputTmp;
            inputField.textViewport = taRect;
            inputField.placeholder = phTmp;
            inputField.contentType = contentType;
            inputField.pointSize = 14f;

            return inputField;
        }

        #endregion

        #region Tab Switching

        private void SwitchTab(bool showLeaderboard)
        {
            leaderboardPanel.SetActive(showLeaderboard);
            settingsPanel.SetActive(!showLeaderboard);

            // Update tab visuals by changing the Image color directly
            leaderboardTabButton.GetComponent<Image>().color = showLeaderboard ? activeTabColor : inactiveTabColor;
            settingsTabButton.GetComponent<Image>().color = showLeaderboard ? inactiveTabColor : activeTabColor;
        }

        #endregion

        #region Leaderboard

        private void PopulateTable()
        {
            foreach (Transform child in tableContent)
            {
                Destroy(child.gameObject);
            }

            List<PlayerData> entries = DataManager.Instance.GetAllEntries();

            if (totalEntriesText != null)
                totalEntriesText.text = $"Total Entries: {entries.Count}";

            for (int i = 0; i < entries.Count; i++)
            {
                GameObject row = Instantiate(tableRowPrefab, tableContent);
                var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 4)
                {
                    texts[0].text = $"{i + 1}";
                    texts[1].text = entries[i].playerName;
                    texts[2].text = entries[i].score.ToString();
                    texts[3].text = entries[i].timestamp;
                }
            }
        }

        private void OnExportCSV()
        {
            string path = DataManager.Instance.ExportToCSV();
            if (exportStatusText != null)
                exportStatusText.text = $"Exported to:\n{path}";
        }

        private void OnClearData()
        {
            DataManager.Instance.ClearAllData();
            PopulateTable();
            if (exportStatusText != null)
                exportStatusText.text = "All data cleared.";
        }

        #endregion

        #region Settings

        private void PopulateSettings()
        {
            if (SettingsManager.Instance == null) return;

            var s = SettingsManager.Instance.Settings;

            strawberryPointsInput.text = s.strawberryPoints.ToString();
            kokoKrunchPointsInput.text = s.kokoKrunchPoints.ToString();
            itemFallSpeedInput.text = s.itemFallSpeed.ToString("F2");
            maxFallSpeedInput.text = s.maxFallSpeed.ToString("F2");
            fallSpeedIncreaseInput.text = s.fallSpeedIncrease.ToString("F2");
            initialSpawnIntervalInput.text = s.initialSpawnInterval.ToString("F2");
            minimumSpawnIntervalInput.text = s.minimumSpawnInterval.ToString("F2");
            spawnAccelerationInput.text = s.spawnAcceleration.ToString("F2");
            gameDurationInput.text = s.gameDuration.ToString("F1");
            maxLivesInput.text = s.maxLives.ToString();
            playerMoveSpeedInput.text = s.playerMoveSpeed.ToString("F1");
        }

        private RuntimeGameSettings CollectSettingsFromUI()
        {
            var s = new RuntimeGameSettings();

            int.TryParse(strawberryPointsInput.text, out s.strawberryPoints);
            int.TryParse(kokoKrunchPointsInput.text, out s.kokoKrunchPoints);
            float.TryParse(itemFallSpeedInput.text, out s.itemFallSpeed);
            float.TryParse(maxFallSpeedInput.text, out s.maxFallSpeed);
            float.TryParse(fallSpeedIncreaseInput.text, out s.fallSpeedIncrease);
            float.TryParse(initialSpawnIntervalInput.text, out s.initialSpawnInterval);
            float.TryParse(minimumSpawnIntervalInput.text, out s.minimumSpawnInterval);
            float.TryParse(spawnAccelerationInput.text, out s.spawnAcceleration);
            float.TryParse(gameDurationInput.text, out s.gameDuration);
            int.TryParse(maxLivesInput.text, out s.maxLives);
            float.TryParse(playerMoveSpeedInput.text, out s.playerMoveSpeed);

            return s;
        }

        private void OnSaveSettings()
        {
            var settings = CollectSettingsFromUI();
            SettingsManager.Instance.SaveSettings(settings);
            SettingsManager.Instance.ApplyToConfig();

            if (settingsStatusText != null)
                settingsStatusText.text = "Settings saved!";
        }

        private void OnResetDefaults()
        {
            SettingsManager.Instance.ResetToDefaults();
            PopulateSettings();

            if (settingsStatusText != null)
                settingsStatusText.text = "Reset to defaults.";
        }

        #endregion

        private void OnBack()
        {
            SceneLoader.LoadLanding();
        }
    }
}
