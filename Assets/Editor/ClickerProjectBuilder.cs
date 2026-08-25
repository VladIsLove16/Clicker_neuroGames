using System;
using System.IO;
using Clicker.Game;
using Clicker.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Clicker.Editor
{
    /// <summary>
    /// Idempotent project composer. It creates all scene objects and assigns every runtime reference.
    /// </summary>
    public static class ClickerProjectBuilder
    {
        private const string GeneratedRoot = "Assets/Clicker";
        private const string LegacyConfigPath = GeneratedRoot + "/Config/DefaultClickerGameConfig.asset";
        private const string CanvasTargetPath = GeneratedRoot + "/Prefabs/CanvasClickTarget.prefab";
        private const string WorldTargetPath = GeneratedRoot + "/Prefabs/WorldClickTarget.prefab";
        private const string WorldMaterialPath = GeneratedRoot + "/Materials/WorldTarget.mat";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string CanvasGameScenePath = "Assets/Scenes/Game.unity";
        private const string WorldGameScenePath = "Assets/Scenes/Game3D.unity";
        private const string TmpFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private static readonly Color Background = new(0.018f, 0.035f, 0.075f, 1f);
        private static readonly Color Panel = new(0.055f, 0.09f, 0.17f, 0.96f);
        private static readonly Color Cyan = new(0.18f, 0.92f, 0.80f, 1f);
        private static readonly Color Gold = new(1f, 0.72f, 0.18f, 1f);
        private static readonly Color TextPrimary = new(0.93f, 0.97f, 1f, 1f);
        private static readonly Color TextSecondary = new(0.56f, 0.68f, 0.82f, 1f);

        [MenuItem("Tools/Clicker/Build Project")]
        public static void Build()
        {
            EnsureFolder(GeneratedRoot);
            EnsureFolder(GeneratedRoot + "/Prefabs");
            EnsureFolder(GeneratedRoot + "/Materials");
            EnsureTmpResources();

            AssetDatabase.DeleteAsset(LegacyConfigPath);
            AssetDatabase.DeleteAsset(GeneratedRoot + "/Config");

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
            if (font == null)
            {
                throw new InvalidOperationException("TextMesh Pro essential resources could not be imported.");
            }

            GameObject canvasTarget = CreateCanvasTargetPrefab(font);
            GameObject worldTarget = CreateWorldTargetPrefab();

            BuildMainMenuScene(font);
            BuildGameScene(font, canvasTarget, null, false, CanvasGameScenePath);
            BuildGameScene(font, null, worldTarget, true, WorldGameScenePath);
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Clicker project successfully generated: scenes, prefabs, and direct references are ready.");
        }

        private static void EnsureTmpResources()
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath) != null)
            {
                return;
            }

            string packageRoot = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly).resolvedPath;
            string packagePath = Path.Combine(packageRoot, "Package Resources", "TMP Essential Resources.unitypackage");
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static GameObject CreateCanvasTargetPrefab(TMP_FontAsset font)
        {
            GameObject root = new(
                "CanvasClickTarget",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline),
                typeof(CanvasClickTargetView));

            try
            {
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(180f, 180f);

                Image image = root.GetComponent<Image>();
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                image.preserveAspect = true;
                image.color = new Color(0.10f, 0.16f, 0.29f, 1f);

                Outline outline = root.GetComponent<Outline>();
                outline.effectColor = new Color(0.35f, 0.55f, 0.85f, 0.45f);
                outline.effectDistance = new Vector2(5f, -5f);

                TextMeshProUGUI label = CreateText(
                    "Number",
                    root.transform,
                    font,
                    "1",
                    58f,
                    FontStyles.Bold,
                    TextAlignmentOptions.Center,
                    TextPrimary);
                Stretch(label.rectTransform, 18f, 18f, 18f, 18f);

                CanvasClickTargetView target = root.GetComponent<CanvasClickTargetView>();
                target.targetGraphic = image;
                target.transition = Selectable.Transition.None;
                SetReference(target, "surface", image);
                SetReference(target, "numberLabel", label);

                PrefabUtility.SaveAsPrefabAsset(root, CanvasTargetPath);
                AssetDatabase.ImportAsset(CanvasTargetPath, ImportAssetOptions.ForceSynchronousImport);
                return AssetDatabase.LoadAssetAtPath<GameObject>(CanvasTargetPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateWorldTargetPrefab()
        {
            Material material = CreateWorldMaterial();
            GameObject root = new("WorldClickTarget", typeof(SphereCollider), typeof(WorldClickTargetView));

            try
            {
                SphereCollider collider = root.GetComponent<SphereCollider>();
                collider.radius = 0.58f;

                GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Visual";
                visual.transform.SetParent(root.transform, false);
                Object.DestroyImmediate(visual.GetComponent<Collider>());
                Renderer renderer = visual.GetComponent<Renderer>();
                renderer.sharedMaterial = material;

                GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                core.name = "Core";
                core.transform.SetParent(visual.transform, false);
                core.transform.localScale = Vector3.one * 0.58f;
                Object.DestroyImmediate(core.GetComponent<Collider>());
                core.GetComponent<Renderer>().sharedMaterial = material;

                WorldClickTargetView target = root.GetComponent<WorldClickTargetView>();
                target.transition = Selectable.Transition.None;
                SetReference(target, "targetRenderer", renderer);
                SetReference(target, "visual", visual.transform);

                PrefabUtility.SaveAsPrefabAsset(root, WorldTargetPath);
                AssetDatabase.ImportAsset(WorldTargetPath, ImportAssetOptions.ForceSynchronousImport);
                return AssetDatabase.LoadAssetAtPath<GameObject>(WorldTargetPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Material CreateWorldMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(WorldMaterialPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                throw new InvalidOperationException("No suitable lit shader is available.");
            }

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, WorldMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.enableInstancing = true;
            material.SetColor("_BaseColor", new Color(0.08f, 0.22f, 0.42f, 1f));
            material.SetColor("_Color", new Color(0.08f, 0.22f, 0.42f, 1f));
            material.SetFloat("_Smoothness", 0.72f);
            material.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildMainMenuScene(TMP_FontAsset font)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera(false);
            CreateEventSystem();

            Canvas canvas = CreateCanvas();
            CreateImage("Background", canvas.transform, Background, true);
            RectTransform safeArea = CreateSafeArea(canvas.transform);

            Image card = CreateImage("MenuCard", safeArea, Panel, false);
            RectTransform cardRect = card.rectTransform;
            Center(cardRect, new Vector2(760f, 680f));
            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(56, 56, 48, 48);
            layout.spacing = 22f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = CreateText(
                "Title", card.transform, font, "CLICK ORDER", 76f, FontStyles.Bold,
                TextAlignmentOptions.Center, TextPrimary);
            AddLayout(title.gameObject, 145f);

            TextMeshProUGUI subtitle = CreateText(
                "Subtitle", card.transform, font,
                "Hit the highlighted target. Wrong target: -1.0s",
                27f, FontStyles.Normal, TextAlignmentOptions.Center, TextSecondary);
            AddLayout(subtitle.gameObject, 75f);

            CoreButton canvasButton = CreateButton(card.transform, font, "START  /  CANVAS", Cyan, Background);
            AddLayout(canvasButton.gameObject, 104f);
            CoreButton worldButton = CreateButton(card.transform, font, "START  /  3D", Gold, Background);
            AddLayout(worldButton.gameObject, 104f);

            TextMeshProUGUI hint = CreateText(
                "Hint", card.transform, font,
                "Mouse  |  Touch  |  D-pad / Stick + Submit",
                22f, FontStyles.Normal, TextAlignmentOptions.Center, TextSecondary);
            AddLayout(hint.gameObject, 62f);

            Navigation canvasNavigation = canvasButton.navigation;
            canvasNavigation.mode = Navigation.Mode.Explicit;
            canvasNavigation.selectOnDown = worldButton;
            canvasButton.navigation = canvasNavigation;

            Navigation worldNavigation = worldButton.navigation;
            worldNavigation.mode = Navigation.Mode.Explicit;
            worldNavigation.selectOnUp = canvasButton;
            worldButton.navigation = worldNavigation;

            MainMenu menu = new GameObject("MainMenuController", typeof(MainMenu)).GetComponent<MainMenu>();
            SetReference(menu, "canvasModeButton", canvasButton);
            SetReference(menu, "worldModeButton", worldButton);
            SetString(menu, "canvasGameSceneName", "Game");
            SetString(menu, "worldGameSceneName", "Game3D");

            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void BuildGameScene(
            TMP_FontAsset font,
            GameObject canvasTargetPrefab,
            GameObject worldTargetPrefab,
            bool useWorldTargets,
            string scenePath)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera(useWorldTargets);
            if (useWorldTargets)
            {
                camera.gameObject.AddComponent<PhysicsRaycaster>();
                CreateWorldLight();
            }

            CreateEventSystem();
            Canvas canvas = CreateCanvas();
            if (!useWorldTargets)
            {
                CreateImage("Background", canvas.transform, Background, true).raycastTarget = false;
            }

            RectTransform safeArea = CreateSafeArea(canvas.transform);
            TargetBoard targetBoard = useWorldTargets
                ? CreateWorldBoard(camera, worldTargetPrefab)
                : CreateCanvasBoard(safeArea, canvasTargetPrefab);

            GameHudView hud = CreateHud(safeArea, font, useWorldTargets ? "3D TARGETS" : "CANVAS TARGETS");
            ResultScreen result = CreateResultScreen(safeArea, font);

            GameManager manager = new GameObject("GameManager", typeof(GameManager)).GetComponent<GameManager>();
            SetFloat(manager, "roundDurationSeconds", 30f);
            SetInt(manager, "targetCount", 9);
            SetFloat(manager, "wrongTargetPenaltySeconds", 1f);
            SetReference(manager, "targetBoard", targetBoard);
            SetReference(manager, "hud", hud);
            SetReference(manager, "resultScreen", result);
            SetString(manager, "mainMenuSceneName", "MainMenu");

            EditorSceneManager.SaveScene(scene, scenePath);
        }

        private static CanvasTargetBoard CreateCanvasBoard(RectTransform safeArea, GameObject targetPrefab)
        {
            GameObject boardObject = new("CanvasTargetBoard", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ResponsiveGridLayout), typeof(CanvasTargetBoard));
            boardObject.transform.SetParent(safeArea, false);
            RectTransform rect = boardObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.10f, 0.10f);
            rect.anchorMax = new Vector2(0.90f, 0.79f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CanvasTargetBoard board = boardObject.GetComponent<CanvasTargetBoard>();
            SetReference(board, "targetRoot", rect);
            SetReference(board, "targetPrefab", targetPrefab);
            SetInt(board, "columns", 3);
            return board;
        }

        private static WorldTargetBoard CreateWorldBoard(Camera camera, GameObject targetPrefab)
        {
            GameObject boardObject = new("WorldTargetBoard", typeof(WorldTargetBoard));
            GameObject root = new("Targets");
            root.transform.SetParent(boardObject.transform, false);

            WorldTargetBoard board = boardObject.GetComponent<WorldTargetBoard>();
            SetReference(board, "worldCamera", camera);
            SetReference(board, "targetRoot", root.transform);
            SetReference(board, "targetPrefab", targetPrefab);
            SetInt(board, "columns", 3);
            return board;
        }

        private static GameHudView CreateHud(RectTransform safeArea, TMP_FontAsset font, string modeName)
        {
            GameObject root = new("HUD", typeof(RectTransform), typeof(CanvasGroup), typeof(GameHudView));
            root.transform.SetParent(safeArea, false);
            Stretch((RectTransform)root.transform, 0f, 0f, 0f, 0f);

            Image bar = CreateImage("TopBar", root.transform, Panel, false);
            RectTransform barRect = bar.rectTransform;
            barRect.anchorMin = new Vector2(0.04f, 0.84f);
            barRect.anchorMax = new Vector2(0.96f, 0.965f);
            barRect.offsetMin = Vector2.zero;
            barRect.offsetMax = Vector2.zero;
            bar.raycastTarget = false;

            TextMeshProUGUI score = CreateText(
                "Score", bar.transform, font, "SCORE  0", 35f, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, TextPrimary);
            SetAnchored(score.rectTransform, new Vector2(0.04f, 0f), new Vector2(0.34f, 1f), Vector2.zero, Vector2.zero);

            TextMeshProUGUI timer = CreateText(
                "Timer", bar.transform, font, "30.0", 55f, FontStyles.Bold,
                TextAlignmentOptions.Center, Cyan);
            SetAnchored(timer.rectTransform, new Vector2(0.36f, 0f), new Vector2(0.64f, 1f), Vector2.zero, Vector2.zero);

            TextMeshProUGUI mode = CreateText(
                "Mode", bar.transform, font, modeName, 25f, FontStyles.Bold,
                TextAlignmentOptions.MidlineRight, TextSecondary);
            SetAnchored(mode.rectTransform, new Vector2(0.66f, 0f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);

            TextMeshProUGUI penalty = CreateText(
                "Penalty", root.transform, font, "-1.0s", 45f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Color(1f, 0.32f, 0.30f, 1f));
            penalty.rectTransform.anchorMin = new Vector2(0.5f, 0.76f);
            penalty.rectTransform.anchorMax = new Vector2(0.5f, 0.76f);
            penalty.rectTransform.sizeDelta = new Vector2(300f, 80f);
            penalty.rectTransform.anchoredPosition = Vector2.zero;

            GameHudView hud = root.GetComponent<GameHudView>();
            SetReference(hud, "root", root.GetComponent<CanvasGroup>());
            SetReference(hud, "timerText", timer);
            SetReference(hud, "scoreText", score);
            SetReference(hud, "penaltyText", penalty);
            return hud;
        }

        private static ResultScreen CreateResultScreen(RectTransform safeArea, TMP_FontAsset font)
        {
            Image overlay = CreateImage("ResultScreen", safeArea, new Color(0.01f, 0.02f, 0.05f, 0.88f), true);
            ResultScreen result = overlay.gameObject.AddComponent<ResultScreen>();

            Image card = CreateImage("ResultCard", overlay.transform, Panel, false);
            Center(card.rectTransform, new Vector2(660f, 650f));
            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(58, 58, 44, 44);
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI complete = CreateText(
                "RoundComplete", card.transform, font, "TIME!", 55f, FontStyles.Bold,
                TextAlignmentOptions.Center, Gold);
            AddLayout(complete.gameObject, 100f);

            TextMeshProUGUI caption = CreateText(
                "ScoreCaption", card.transform, font, "YOUR SCORE", 25f, FontStyles.Bold,
                TextAlignmentOptions.Center, TextSecondary);
            AddLayout(caption.gameObject, 50f);

            TextMeshProUGUI score = CreateText(
                "FinalScore", card.transform, font, "0", 110f, FontStyles.Bold,
                TextAlignmentOptions.Center, TextPrimary);
            AddLayout(score.gameObject, 145f);

            CoreButton restart = CreateButton(card.transform, font, "RESTART", Cyan, Background);
            AddLayout(restart.gameObject, 98f);
            CoreButton mainMenu = CreateButton(card.transform, font, "MAIN MENU", new Color(0.16f, 0.24f, 0.38f, 1f), TextPrimary);
            AddLayout(mainMenu.gameObject, 98f);

            Navigation restartNavigation = restart.navigation;
            restartNavigation.mode = Navigation.Mode.Explicit;
            restartNavigation.selectOnDown = mainMenu;
            restart.navigation = restartNavigation;
            Navigation menuNavigation = mainMenu.navigation;
            menuNavigation.mode = Navigation.Mode.Explicit;
            menuNavigation.selectOnUp = restart;
            mainMenu.navigation = menuNavigation;

            SetReference(result, "root", overlay.gameObject);
            SetReference(result, "scoreText", score);
            SetReference(result, "restartButton", restart);
            SetReference(result, "mainMenuButton", mainMenu);
            return result;
        }

        private static Camera CreateCamera(bool worldMode)
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.fieldOfView = 48f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.transform.rotation = Quaternion.identity;
            if (!worldMode)
            {
                camera.cullingMask = 0;
            }

            return camera;
        }

        private static void CreateWorldLight()
        {
            GameObject lightObject = new("Key Light", typeof(Light));
            Light light = lightObject.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.78f, 0.9f, 1f, 1f);
            light.intensity = 1.7f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(38f, -32f, 0f);

            GameObject rimObject = new("Rim Light", typeof(Light));
            Light rim = rimObject.GetComponent<Light>();
            rim.type = LightType.Directional;
            rim.color = new Color(0.25f, 0.65f, 1f, 1f);
            rim.intensity = 0.85f;
            rimObject.transform.rotation = Quaternion.Euler(-25f, 150f, 0f);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static RectTransform CreateSafeArea(Transform parent)
        {
            GameObject safeArea = new("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeArea.transform.SetParent(parent, false);
            RectTransform rect = safeArea.GetComponent<RectTransform>();
            Stretch(rect, 0f, 0f, 0f, 0f);
            return rect;
        }

        private static void CreateEventSystem()
        {
            GameObject eventSystem = new("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.GetComponent<EventSystem>().sendNavigationEvents = true;
        }

        private static CoreButton CreateButton(Transform parent, TMP_FontAsset font, string text, Color background, Color foreground)
        {
            GameObject root = new("Button_" + text.Replace(" ", string.Empty), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CoreButton));
            root.transform.SetParent(parent, false);
            Image image = root.GetComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = background;

            CoreButton button = root.GetComponent<CoreButton>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = Color.Lerp(background, Color.white, 0.18f);
            colors.selectedColor = Color.Lerp(background, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(background, Color.black, 0.15f);
            colors.disabledColor = new Color(background.r, background.g, background.b, 0.4f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            TextMeshProUGUI label = CreateText(
                "Label", root.transform, font, text, 31f, FontStyles.Bold,
                TextAlignmentOptions.Center, foreground);
            Stretch(label.rectTransform, 20f, 12f, 20f, 12f);
            return button;
        }

        private static Image CreateImage(string name, Transform parent, Color color, bool stretch)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(parent, false);
            Image image = root.GetComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            image.type = Image.Type.Sliced;
            image.color = color;
            if (stretch)
            {
                Stretch(image.rectTransform, 0f, 0f, 0f, 0f);
            }

            return image;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            TMP_FontAsset font,
            string text,
            float fontSize,
            FontStyles style,
            TextAlignmentOptions alignment,
            Color color)
        {
            GameObject root = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            root.transform.SetParent(parent, false);
            TextMeshProUGUI label = root.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = color;
            label.enableAutoSizing = true;
            label.fontSizeMin = Mathf.Max(12f, fontSize * 0.55f);
            label.fontSizeMax = fontSize;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            return label;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(CanvasGameScenePath, true),
                new EditorBuildSettingsScene(WorldGameScenePath, true)
            };
        }

        private static void AddLayout(GameObject target, float preferredHeight)
        {
            LayoutElement element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
            element.preferredHeight = preferredHeight;
            element.minHeight = preferredHeight;
            element.flexibleHeight = 0f;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent))
            {
                throw new InvalidOperationException($"Cannot create asset folder '{path}'.");
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void Center(RectTransform rect, Vector2 size)
        {
            rect.anchorMin = Vector2.one * 0.5f;
            rect.anchorMax = Vector2.one * 0.5f;
            rect.pivot = Vector2.one * 0.5f;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void SetAnchored(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void SetReference(Object target, string fieldName, Object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), $"Cannot assign null to {target.GetType().Name}.{fieldName}.");
            }

            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(fieldName)
                ?? throw new MissingFieldException(target.GetType().Name, fieldName);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);

            if (property.objectReferenceValue == null)
            {
                throw new InvalidOperationException(
                    $"Unity rejected reference '{value.name}' for {target.GetType().Name}.{fieldName}.");
            }
        }

        private static void SetString(Object target, string fieldName, string value)
        {
            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(fieldName)
                ?? throw new MissingFieldException(target.GetType().Name, fieldName);
            property.stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string fieldName, int value)
        {
            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(fieldName)
                ?? throw new MissingFieldException(target.GetType().Name, fieldName);
            property.intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string fieldName, float value)
        {
            SerializedObject serialized = new(target);
            SerializedProperty property = serialized.FindProperty(fieldName)
                ?? throw new MissingFieldException(target.GetType().Name, fieldName);
            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }
    }
}
