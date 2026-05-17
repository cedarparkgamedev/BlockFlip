using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GameOverMenuPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/GameOverMenu.prefab";
    private static readonly Color ShadowEffectColor = new Color(0f, 0f, 0f, 0.3f);
    private static readonly Vector2 ShadowEffectDistance = new Vector2(0f, -6f);
    private const bool ShadowUseGraphicAlpha = true;

    [MenuItem("Tools/BlockFlip/Rebuild Game Over Menu Prefab")]
    public static void Rebuild()
    {
        GameObject root = CreateRectObject("GameOverMenu", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image overlay = root.AddComponent<Image>();
        overlay.color = new Color(1f, 0.985f, 0.955f, 0.58f);
        overlay.raycastTarget = true;

        GameOverMenuView view = root.AddComponent<GameOverMenuView>();

        GameObject panel = CreateRectObject("Panel", root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(650f, 980f), Vector2.zero);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 0.985f, 0.955f, 0.97f);
        panelImage.raycastTarget = true;
        AddShadow(panel);

        CreateLabel("Title", panel.transform, "G A M E  O V E R", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 90f), new Vector2(0f, 380f), 44f, 5f, new Color(0.05f, 0.05f, 0.048f, 1f));

        GameObject stats = CreateRectObject("Stats", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 390f), new Vector2(0f, 110f));
        TextMeshProUGUI scoreValue = CreateStatRow(stats.transform, "ScoreRow", "SCORE", "ScoreValue", "0", new Vector2(0f, 140f), false);
        TextMeshProUGUI bestValue = CreateStatRow(stats.transform, "BestRow", "BEST", "BestValue", "0", new Vector2(0f, 45f), true);
        TextMeshProUGUI linesValue = CreateStatRow(stats.transform, "LinesRow", "LINES", "LinesValue", "0", new Vector2(0f, -50f), false);
        TextMeshProUGUI maxComboValue = CreateStatRow(stats.transform, "MaxComboRow", "MAX COMBO", "MaxComboValue", "x0", new Vector2(0f, -145f), false);

        Button retry = CreateMenuButton(panel.transform, "RetryButton", "↻", "R E T R Y", new Vector2(0f, -255f), true);
        Button home = CreateMenuButton(panel.transform, "HomeButton", "↪", "B A C K  T O  M E N U", new Vector2(0f, -405f), false);

        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("scoreValueText").objectReferenceValue = scoreValue;
        serializedView.FindProperty("bestValueText").objectReferenceValue = bestValue;
        serializedView.FindProperty("linesValueText").objectReferenceValue = linesValue;
        serializedView.FindProperty("maxComboValueText").objectReferenceValue = maxComboValue;
        serializedView.FindProperty("newBestBadge").objectReferenceValue = stats.transform.Find("BestRow/NewBestBadge")?.gameObject;
        serializedView.FindProperty("retryButton").objectReferenceValue = retry;
        serializedView.FindProperty("homeButton").objectReferenceValue = home;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static TextMeshProUGUI CreateStatRow(Transform parent, string rowName, string label, string valueName, string value, Vector2 position, bool includeBadge)
    {
        GameObject row = CreateRectObject(rowName, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 90f), position);
        CreateLabel("Label", row.transform, label, new Vector2(0f, 0f), new Vector2(0.42f, 1f), Vector2.zero, Vector2.zero, 27f, 4f, new Color(0.05f, 0.05f, 0.048f, 1f));
        CreateDivider(row.transform);
        TextMeshProUGUI valueText = CreateLabel(valueName, row.transform, value, new Vector2(0.52f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 47f, 1f, new Color(0.04f, 0.04f, 0.038f, 1f));

        if (includeBadge)
        {
            GameObject badge = CreateRectObject("NewBestBadge", row.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(130f, 38f), new Vector2(88f, 14f));
            Image badgeImage = badge.AddComponent<Image>();
            badgeImage.color = new Color(0.78f, 0.75f, 0.7f, 0.48f);
            badgeImage.raycastTarget = false;
            CreateLabel("Label", badge.transform, "N E W  B E S T", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 17f, 1f, new Color(0.14f, 0.13f, 0.12f, 1f));
        }

        return valueText;
    }

    private static void CreateDivider(Transform parent)
    {
        GameObject divider = CreateRectObject("Divider", parent, new Vector2(0.5f, 0.2f), new Vector2(0.5f, 0.8f), new Vector2(2f, 0f), Vector2.zero);
        Image image = divider.AddComponent<Image>();
        image.color = new Color(0.72f, 0.69f, 0.64f, 0.42f);
        image.raycastTarget = false;
    }

    private static Button CreateMenuButton(Transform parent, string name, string icon, string label, Vector2 position, bool emphasized)
    {
        GameObject buttonObject = CreateRectObject(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(540f, 110f), position);
        Image image = buttonObject.AddComponent<Image>();
        image.color = emphasized ? new Color(1f, 1f, 1f, 0.9f) : new Color(1f, 0.985f, 0.955f, 0.62f);
        image.raycastTarget = true;
        AddShadow(buttonObject);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        CreateLabel("Label", buttonObject.transform, $"{icon}   {label}", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, emphasized ? 32f : 27f, 1f, new Color(0.08f, 0.08f, 0.076f, 1f));
        return button;
    }

    private static GameObject CreateRectObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
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
        if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        return gameObject;
    }

    private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition, float fontSize, float characterSpacing, Color color)
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
