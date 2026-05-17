using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class SettingsMenuPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefabs/Resources/SettingsMenu.prefab";
    private static readonly Color ShadowEffectColor = new Color(0f, 0f, 0f, 0.3f);
    private static readonly Vector2 ShadowEffectDistance = new Vector2(0f, -6f);
    private const bool ShadowUseGraphicAlpha = true;

    [MenuItem("Tools/BlockFlip/Rebuild Settings Menu Prefab")]
    public static void Rebuild()
    {
        EnsureFolder("Assets/Prefabs/Resources");

        GameObject root = CreateRectObject("SettingsMenu", null, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image overlay = root.AddComponent<Image>();
        overlay.color = new Color(1f, 0.985f, 0.955f, 0.96f);
        overlay.raycastTarget = true;

        SettingsMenuView view = root.AddComponent<SettingsMenuView>();

        Button backButton = CreateBackButton(root.transform);
        CreateLabel("Title", root.transform, "S E T T I N G S", new Vector2(0.18f, 0.905f), new Vector2(0.82f, 0.985f), Vector2.zero, Vector2.zero, 42f, 12f, TextAlignmentOptions.Center, TextColor);

        GameObject panel = CreateRectObject("Panel", root.transform, new Vector2(0.08f, 0.175f), new Vector2(0.92f, 0.855f), Vector2.zero, Vector2.zero);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(1f, 0.985f, 0.955f, 0.72f);
        panelImage.raycastTarget = true;
        AddShadow(panel);

        CreateSectionHeader(panel.transform, "AudioHeader", "A U D I O", 0.93f);
        Toggle music = CreateSettingsToggleRow(panel.transform, "Music", "MusicToggle", 0.83f, true);
        Toggle soundEffects = CreateSettingsToggleRow(panel.transform, "Sound Effects", "SoundEffectsToggle", 0.735f, true);
        Toggle haptics = CreateSettingsToggleRow(panel.transform, "Haptics", "HapticsToggle", 0.64f, true);

        CreateSectionHeader(panel.transform, "GameplayHeader", "G A M E P L A Y", 0.53f);
        Toggle placementPreview = CreateSettingsToggleRow(panel.transform, "Show Placement Preview", "PlacementPreviewToggle", 0.435f, true);
        Toggle confirmRestart = CreateSettingsToggleRow(panel.transform, "Confirm Restart", "ConfirmRestartToggle", 0.34f, false);
        Toggle reduceMotion = CreateSettingsToggleRow(panel.transform, "Reduce Motion", "ReduceMotionToggle", 0.245f, false);

        CreateSectionHeader(panel.transform, "DisplayHeader", "D I S P L A Y", 0.145f);
        Button language = CreateLanguageRow(panel.transform);
        Button resetTutorial = CreatePanelButton(panel.transform, "ResetTutorialButton", "R E S E T  T U T O R I A L", new Vector2(0.065f, 0.03f), new Vector2(0.935f, 0.095f), false);

        Button privacy = CreateFooterButton(root.transform, "PrivacyButton", "♢", "P R I V A C Y", new Vector2(0.08f, 0.095f), new Vector2(0.49f, 0.145f));
        Button credits = CreateFooterButton(root.transform, "CreditsButton", "ⓘ", "C R E D I T S", new Vector2(0.51f, 0.095f), new Vector2(0.92f, 0.145f));
        Button done = CreatePanelButton(root.transform, "DoneButton", "D O N E", new Vector2(0.08f, 0.025f), new Vector2(0.92f, 0.075f), true);

        SerializedObject serializedView = new SerializedObject(view);
        serializedView.FindProperty("backButton").objectReferenceValue = backButton;
        serializedView.FindProperty("doneButton").objectReferenceValue = done;
        serializedView.FindProperty("languageButton").objectReferenceValue = language;
        serializedView.FindProperty("resetTutorialButton").objectReferenceValue = resetTutorial;
        serializedView.FindProperty("privacyButton").objectReferenceValue = privacy;
        serializedView.FindProperty("creditsButton").objectReferenceValue = credits;
        serializedView.FindProperty("musicToggle").objectReferenceValue = music;
        serializedView.FindProperty("soundEffectsToggle").objectReferenceValue = soundEffects;
        serializedView.FindProperty("hapticsToggle").objectReferenceValue = haptics;
        serializedView.FindProperty("placementPreviewToggle").objectReferenceValue = placementPreview;
        serializedView.FindProperty("confirmRestartToggle").objectReferenceValue = confirmRestart;
        serializedView.FindProperty("reduceMotionToggle").objectReferenceValue = reduceMotion;
        serializedView.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static readonly Color TextColor = new Color(0.045f, 0.045f, 0.043f, 1f);
    private static readonly Color DividerColor = new Color(0.58f, 0.56f, 0.53f, 0.28f);

    private static void CreateSectionHeader(Transform parent, string name, string title, float y)
    {
        GameObject group = CreateRectObject(name, parent, new Vector2(0.065f, y - 0.035f), new Vector2(0.935f, y + 0.035f), Vector2.zero, Vector2.zero);
        CreateLabel("Label", group.transform, title, new Vector2(0f, 0.22f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 25f, 10f, TextAlignmentOptions.Left, TextColor);
        CreateDivider(group.transform, 0f);
    }

    private static Toggle CreateSettingsToggleRow(Transform parent, string label, string toggleName, float y, bool isOn)
    {
        GameObject row = CreateRectObject(label.Replace(" ", "") + "Row", parent, new Vector2(0.065f, y - 0.043f), new Vector2(0.935f, y + 0.043f), Vector2.zero, Vector2.zero);
        CreateLabel("Label", row.transform, label, new Vector2(0f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero, 32f, 0f, TextAlignmentOptions.Left, TextColor);
        Toggle toggle = CreateToggle(row.transform, toggleName, isOn);
        CreateRowRule(row.transform);
        return toggle;
    }

    private static Toggle CreateToggle(Transform parent, string name, bool isOn)
    {
        GameObject toggleObject = CreateRectObject(name, parent, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(112f, 58f), new Vector2(-56f, 0f));
        Image background = toggleObject.AddComponent<Image>();
        background.color = isOn ? new Color(0.055f, 0.055f, 0.052f, 1f) : new Color(0.78f, 0.76f, 0.72f, 0.42f);
        background.raycastTarget = true;
        AddShadow(toggleObject);

        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.isOn = isOn;

        GameObject handle = CreateRectObject("Handle", toggleObject.transform, new Vector2(isOn ? 1f : 0f, 0.5f), new Vector2(isOn ? 1f : 0f, 0.5f), new Vector2(52f, 52f), new Vector2(isOn ? -32f : 32f, 0f));
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1f, 0.985f, 0.955f, 1f);
        handleImage.raycastTarget = false;
        AddShadow(handle);
        toggle.graphic = handleImage;
        return toggle;
    }

    private static Button CreateLanguageRow(Transform parent)
    {
        GameObject row = CreateRectObject("LanguageRow", parent, new Vector2(0.065f, 0.095f), new Vector2(0.935f, 0.16f), Vector2.zero, Vector2.zero);
        CreateLabel("Label", row.transform, "Language", new Vector2(0f, 0f), new Vector2(0.42f, 1f), Vector2.zero, Vector2.zero, 31f, 0f, TextAlignmentOptions.Left, TextColor);
        CreateLabel("Value", row.transform, "English", new Vector2(0.54f, 0f), new Vector2(0.92f, 1f), Vector2.zero, Vector2.zero, 29f, 0f, TextAlignmentOptions.Right, new Color(0.28f, 0.27f, 0.25f, 1f));
        CreateLabel("Arrow", row.transform, "›", new Vector2(0.92f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 48f, 0f, TextAlignmentOptions.Right, new Color(0.28f, 0.27f, 0.25f, 1f));
        Button button = row.AddComponent<Button>();
        button.targetGraphic = null;
        return button;
    }

    private static Button CreateBackButton(Transform parent)
    {
        GameObject buttonObject = CreateRectObject("BackButton", parent, new Vector2(0.052f, 0.92f), new Vector2(0.135f, 0.985f), Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 0.985f, 0.955f, 0.9f);
        image.raycastTarget = true;
        AddShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Icon", buttonObject.transform, "‹", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-3f, 3f), 64f, 0f, TextAlignmentOptions.Center, TextColor);
        return button;
    }

    private static Button CreateFooterButton(Transform parent, string name, string icon, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObject = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 0.985f, 0.955f, 0.72f);
        image.raycastTarget = true;
        AddShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Label", buttonObject.transform, $"{icon}   {label}", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 25f, 3f, TextAlignmentOptions.Center, TextColor);
        return button;
    }

    private static Button CreatePanelButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, bool dark)
    {
        GameObject buttonObject = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.color = dark ? new Color(0.055f, 0.055f, 0.052f, 1f) : new Color(1f, 0.985f, 0.955f, 0.72f);
        image.raycastTarget = true;
        AddShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Label", buttonObject.transform, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, dark ? 31f : 24f, dark ? 11f : 8f, TextAlignmentOptions.Center, dark ? Color.white : TextColor);
        return button;
    }

    private static void CreateDivider(Transform parent, float yOffset)
    {
        GameObject left = CreateRectObject("DividerLeft", parent, new Vector2(0f, 0.06f), new Vector2(0.47f, 0.06f), new Vector2(0f, 1f), new Vector2(0f, yOffset));
        Image leftImage = left.AddComponent<Image>();
        leftImage.color = DividerColor;
        leftImage.raycastTarget = false;

        GameObject right = CreateRectObject("DividerRight", parent, new Vector2(0.53f, 0.06f), new Vector2(1f, 0.06f), new Vector2(0f, 1f), new Vector2(0f, yOffset));
        Image rightImage = right.AddComponent<Image>();
        rightImage.color = DividerColor;
        rightImage.raycastTarget = false;

        GameObject dot = CreateRectObject("DividerDot", parent, new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f), new Vector2(9f, 9f), new Vector2(0f, yOffset));
        Image dotImage = dot.AddComponent<Image>();
        dotImage.color = new Color(0.58f, 0.56f, 0.53f, 0.9f);
        dotImage.raycastTarget = false;
    }

    private static void CreateRowRule(Transform parent)
    {
        GameObject line = CreateRectObject("Rule", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), Vector2.zero);
        Image image = line.AddComponent<Image>();
        image.color = DividerColor;
        image.raycastTarget = false;
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
        if (sizeDelta == Vector2.zero)
        {
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        return gameObject;
    }

    private static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition, float fontSize, float characterSpacing, TextAlignmentOptions alignment, Color color)
    {
        GameObject labelObject = CreateRectObject(name, parent, anchorMin, anchorMax, sizeDelta, anchoredPosition);
        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.color = color;
        label.fontSize = fontSize;
        label.fontSizeMax = fontSize;
        label.fontSizeMin = Mathf.Max(12f, fontSize * 0.55f);
        label.enableAutoSizing = true;
        label.characterSpacing = characterSpacing;
        label.alignment = alignment;
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

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
