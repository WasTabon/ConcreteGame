using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LevelSelectionUIGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Level Selection UI")]
    public static void ShowWindow()
    {
        GetWindow<LevelSelectionUIGenerator>("Level Selection Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Selection UI Generator", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generate Level Selection UI"))
        {
            GenerateLevelSelectionUI();
        }
    }

    private static void GenerateLevelSelectionUI()
    {
        GameObject canvasGO = new GameObject("LevelSelectionCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(2340, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject backgroundGO = new GameObject("Background");
        backgroundGO.transform.SetParent(canvasGO.transform, false);
        Image bgImage = backgroundGO.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.2f);
        RectTransform bgRect = backgroundGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "Выбор уровня";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -100);
        titleRect.sizeDelta = new Vector2(800, 100);

        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(canvasGO.transform, false);
        ScrollRect scrollRect = scrollViewGO.AddComponent<ScrollRect>();
        Image scrollBg = scrollViewGO.AddComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0.3f);
        
        RectTransform scrollRectTransform = scrollViewGO.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.1f, 0.1f);
        scrollRectTransform.anchorMax = new Vector2(0.9f, 0.85f);
        scrollRectTransform.sizeDelta = Vector2.zero;

        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewGO.transform, false);
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;

        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(300, 200);
        gridLayout.spacing = new Vector2(30, 30);
        gridLayout.padding = new RectOffset(50, 50, 50, 50);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 6;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        
        ContentSizeFitter contentSizeFitter = content.AddComponent<ContentSizeFitter>();
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(0, 1);
        contentRect.pivot = new Vector2(0, 1);

        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30;

        for (int i = 1; i <= 30; i++)
        {
            CreateLevelButton(content.transform, i);
        }

        GameObject loadingPanelGO = new GameObject("LoadingPanel");
        loadingPanelGO.transform.SetParent(canvasGO.transform, false);
        Image loadingBg = loadingPanelGO.AddComponent<Image>();
        loadingBg.color = new Color(0, 0, 0, 0.9f);
        RectTransform loadingRect = loadingPanelGO.GetComponent<RectTransform>();
        loadingRect.anchorMin = Vector2.zero;
        loadingRect.anchorMax = Vector2.one;
        loadingRect.sizeDelta = Vector2.zero;

        GameObject loadingTextGO = new GameObject("LoadingText");
        loadingTextGO.transform.SetParent(loadingPanelGO.transform, false);
        TextMeshProUGUI loadingText = loadingTextGO.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Загрузка...";
        loadingText.fontSize = 64;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = Color.white;
        RectTransform loadingTextRect = loadingTextGO.GetComponent<RectTransform>();
        loadingTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        loadingTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        loadingTextRect.sizeDelta = new Vector2(400, 100);

        loadingPanelGO.SetActive(false);

        GameObject controllerGO = new GameObject("LevelSelectionController");
        controllerGO.transform.SetParent(canvasGO.transform, false);
        LevelSelectionController controller = controllerGO.AddComponent<LevelSelectionController>();
        controller.loadingPanel = loadingPanelGO;

        Button[] allButtons = content.GetComponentsInChildren<Button>();
        controller.levelButtons = allButtons;

        EditorUtility.SetDirty(canvasGO);
        
        Debug.Log("Level Selection UI Generated Successfully!");
    }

    private static void CreateLevelButton(Transform parent, int levelNumber)
    {
        GameObject buttonGO = new GameObject($"LevelButton_{levelNumber}");
        buttonGO.transform.SetParent(parent, false);
        
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.5f, 0.7f);
        
        Button button = buttonGO.AddComponent<Button>();
        
        GameObject levelNumberGO = new GameObject("LevelNumber");
        levelNumberGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI levelText = levelNumberGO.AddComponent<TextMeshProUGUI>();
        levelText.text = levelNumber.ToString();
        levelText.fontSize = 48;
        levelText.fontStyle = FontStyles.Bold;
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.color = Color.white;
        RectTransform levelTextRect = levelNumberGO.GetComponent<RectTransform>();
        levelTextRect.anchorMin = new Vector2(0.5f, 0.6f);
        levelTextRect.anchorMax = new Vector2(0.5f, 0.6f);
        levelTextRect.sizeDelta = new Vector2(100, 60);

        GameObject starsContainerGO = new GameObject("StarsContainer");
        starsContainerGO.transform.SetParent(buttonGO.transform, false);
        RectTransform starsRect = starsContainerGO.GetComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(0.5f, 0.2f);
        starsRect.anchorMax = new Vector2(0.5f, 0.2f);
        starsRect.sizeDelta = new Vector2(200, 40);
        
        HorizontalLayoutGroup starsLayout = starsContainerGO.AddComponent<HorizontalLayoutGroup>();
        starsLayout.spacing = 10;
        starsLayout.childAlignment = TextAnchor.MiddleCenter;
        starsLayout.childControlWidth = false;
        starsLayout.childControlHeight = false;
        starsLayout.childForceExpandWidth = false;
        starsLayout.childForceExpandHeight = false;

        for (int i = 0; i < 3; i++)
        {
            GameObject starGO = new GameObject($"Star_{i + 1}");
            starGO.transform.SetParent(starsContainerGO.transform, false);
            Image starImage = starGO.AddComponent<Image>();
            starImage.color = new Color(0.3f, 0.3f, 0.3f);
            RectTransform starRect = starGO.GetComponent<RectTransform>();
            starRect.sizeDelta = new Vector2(35, 35);
        }

        GameObject lockIconGO = new GameObject("LockIcon");
        lockIconGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI lockText = lockIconGO.AddComponent<TextMeshProUGUI>();
        lockText.text = "🔒";
        lockText.fontSize = 64;
        lockText.alignment = TextAlignmentOptions.Center;
        lockText.color = new Color(1, 1, 1, 0.8f);
        RectTransform lockRect = lockIconGO.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.sizeDelta = new Vector2(100, 100);
        
        lockIconGO.SetActive(levelNumber > 1);

        LevelButtonData buttonData = buttonGO.AddComponent<LevelButtonData>();
        buttonData.levelNumber = levelNumber;
        buttonData.lockIcon = lockIconGO;
        buttonData.stars = starsContainerGO.GetComponentsInChildren<Image>();
    }
}
