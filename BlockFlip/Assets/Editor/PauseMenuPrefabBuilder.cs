using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PauseMenuPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/PauseMenu.prefab";
    private static readonly Color ShadowEffectColor = new Color(0f, 0f, 0f, 0.3f);
    private static readonly Vector2 ShadowEffectDistance = new Vector2(0f, -6f);
    private const bool ShadowUseGraphicAlpha = true;

    [MenuItem("Tools/BlockFlip/Rebuild Pause Menu Prefab")]
    public static void Rebuild()
    {
        GameObject root = CreateRectObject("PauseMenu", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image overlay = root.AddComponent<Image>();
        overlay.color = new Color(1f, 0.985f, 0.955f, 0.58f);
        overlay.raycastTarget = true;

        PauseMenuView view = root.AddComponent<PauseMenuView>();

        GameObject panel = CreateRectObject("Panel", root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(650f, 960f), Vector2.zero);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 0.985f, 0.955f, 0.97f);
        panelImage.raycastTarget = true;
        AddShadow(panel);

        CreateLabel("Title", panel.transform, "P A U S E D", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 80f), new Vector2(0f, 350f), 44f, 8f, new Color(0.05f, 0.05f, 0.048f, 1f));
        CreatePauseMedallion(panel.transform);

        Button resume = CreateMenuButton(panel.transform, "ResumeButton", "▶", "R E S U M E", new Vector2(0f, 120f), true);
        Button restart = CreateMenuButton(panel.transform, "RestartButton", "↻", "R E S T A R T", new Vector2(0f, -45f), false);
        Button settings = CreateMenuButton(panel.transform, "SettingsButton", "⚙", "S E T T I N G S", new Vector2(0f, -210f), false);
        Button home = CreateMenuButton(panel.transform, "HomeButton", "↪", "B A C K  T O  M E N U", new Vector2(0f, -375f), false);

        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("resumeButton").objectReferenceValue = resume;
        serializedView.FindProperty("restartButton").objectReferenceValue = restart;
        serializedView.FindProperty("settingsButton").objectReferenceValue = settings;
        serializedView.FindProperty("homeButton").objectReferenceValue = home;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static GameObject CreateRectObject(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 sizeDelta,
        Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        gameObject.layer = LayerMask.NameToLayer("UI");
        gameObject.transform.SetParent(parent, false);

        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;
        rect.offsetMin = anchorMin == Vector2.zero && anchorMax == Vector2.one ? Vector2.zero : rect.offsetMin;
        rect.offsetMax = anchorMin == Vector2.zero && anchorMax == Vector2.one ? Vector2.zero : rect.offsetMax;
        return gameObject;
    }

    private static void CreatePauseMedallion(Transform parent)
    {
        GameObject circle = CreateRectObject("PauseIconCircle", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(112f, 112f), new Vector2(0f, 245f));
        Image image = circle.AddComponent<Image>();
        image.color = new Color(1f, 0.985f, 0.955f, 0.95f);
        image.raycastTarget = false;
        AddShadow(circle);

        CreateIconBar(circle.transform, "PauseBarLeft", new Vector2(-14f, 0f));
        CreateIconBar(circle.transform, "PauseBarRight", new Vector2(14f, 0f));
    }

    private static void CreateIconBar(Transform parent, string name, Vector2 position)
    {
        GameObject bar = CreateRectObject(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(13f, 48f), position);
        Image image = bar.AddComponent<Image>();
        image.color = new Color(0.045f, 0.045f, 0.043f, 1f);
        image.raycastTarget = false;
    }

    private static Button CreateMenuButton(Transform parent, string name, string icon, string label, Vector2 position, bool emphasized)
    {
        GameObject buttonObject = CreateRectObject(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 115f), position);
        Image image = buttonObject.AddComponent<Image>();
        image.color = emphasized
            ? new Color(1f, 1f, 1f, 0.9f)
            : new Color(1f, 0.985f, 0.955f, 0.62f);
        image.raycastTarget = true;
        AddShadow(buttonObject);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.96f, 0.945f, 0.92f, 1f);
        colors.pressedColor = new Color(0.86f, 0.84f, 0.8f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
        button.colors = colors;

        CreateLabel("Icon", buttonObject.transform, icon, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(120f, 0f), new Vector2(86f, 0f), 44f, 0f, new Color(0.09f, 0.09f, 0.085f, 1f));
        CreateLabel("Label", buttonObject.transform, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 30f, 0f, new Color(0.08f, 0.08f, 0.076f, 1f));
        return button;
    }

    private static TextMeshProUGUI CreateLabel(
        string name,
        Transform parent,
        string text,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 sizeDelta,
        Vector2 anchoredPosition,
        float fontSize,
        float characterSpacing,
        Color color)
    {
        GameObject labelObject = CreateRectObject(name, parent, anchorMin, anchorMax, sizeDelta, anchoredPosition);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.color = color;
        label.fontSize = fontSize;
        label.fontSizeMax = fontSize;
        label.fontSizeMin = Mathf.Max(14f, fontSize * 0.55f);
        label.enableAutoSizing = true;
        label.characterSpacing = characterSpacing;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static void AddShadow(GameObject target)
    {
        Shadow shadow = target.AddComponent<Shadow>();
        shadow.effectColor = ShadowEffectColor;
        shadow.effectDistance = ShadowEffectDistance;
        shadow.useGraphicAlpha = ShadowUseGraphicAlpha;
    }
}
