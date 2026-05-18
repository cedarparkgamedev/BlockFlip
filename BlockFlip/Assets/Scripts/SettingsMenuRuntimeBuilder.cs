using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuRuntimeBuilder : MonoBehaviour
{
    private static Sprite roundedSprite;

    private readonly Color textColor = new Color(0.045f, 0.045f, 0.043f, 1f);
    private readonly Color dividerColor = new Color(0.58f, 0.56f, 0.53f, 0.28f);

    private void Awake()
    {
        if (transform.Find("SafeAreaRoot") != null)
            return;

        ClearGeneratedChildren();
        Build();
    }

    private void Build()
    {
        RectTransform rootRect = GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image overlay = GetComponent<Image>();
        if (overlay == null)
        {
            overlay = gameObject.AddComponent<Image>();
        }

        overlay.color = new Color(1f, 0.985f, 0.955f, 0.96f);
        overlay.raycastTarget = true;

        Transform safeAreaRoot = CreateSafeAreaRoot(transform);

        CreateBackButton(safeAreaRoot);
        CreateLabel("Title", safeAreaRoot, "S E T T I N G S", new Vector2(0.18f, 0.905f), new Vector2(0.82f, 0.985f), Vector2.zero, Vector2.zero, 42f, 12f, TextAlignmentOptions.Center, textColor);

        GameObject panel = CreateRectObject("Panel", safeAreaRoot, new Vector2(0.08f, 0.175f), new Vector2(0.92f, 0.855f), Vector2.zero, Vector2.zero);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.sprite = GetRoundedSprite();
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(1f, 0.985f, 0.955f, 0.72f);
        panelImage.raycastTarget = true;
        GameManager.ApplyShadow(panel);

        CreateSectionHeader(panel.transform, "AudioHeader", "A U D I O", 0.93f);
        CreateSettingsToggleRow(panel.transform, "Music", "MusicToggle", 0.83f, true);
        CreateSettingsToggleRow(panel.transform, "Sound Effects", "SoundEffectsToggle", 0.735f, true);
        CreateSettingsToggleRow(panel.transform, "Haptics", "HapticsToggle", 0.64f, true);

        CreateSectionHeader(panel.transform, "GameplayHeader", "G A M E P L A Y", 0.53f);
        CreateSettingsToggleRow(panel.transform, "Show Placement Preview", "PlacementPreviewToggle", 0.435f, true);
        CreateSettingsToggleRow(panel.transform, "Confirm Restart", "ConfirmRestartToggle", 0.34f, false);
        CreateSettingsToggleRow(panel.transform, "Reduce Motion", "ReduceMotionToggle", 0.245f, false);

        CreateSectionHeader(panel.transform, "DisplayHeader", "D I S P L A Y", 0.145f);
        CreateLanguageRow(panel.transform);
        CreatePanelButton(panel.transform, "ResetTutorialButton", "R E S E T  T U T O R I A L", new Vector2(0.065f, 0.03f), new Vector2(0.935f, 0.095f), false);

        CreateFooterButton(safeAreaRoot, "PrivacyButton", "◇", "P R I V A C Y", new Vector2(0.08f, 0.095f), new Vector2(0.49f, 0.145f));
        CreateFooterButton(safeAreaRoot, "CreditsButton", "ⓘ", "C R E D I T S", new Vector2(0.51f, 0.095f), new Vector2(0.92f, 0.145f));
        CreatePanelButton(safeAreaRoot, "DoneButton", "D O N E", new Vector2(0.08f, 0.025f), new Vector2(0.92f, 0.075f), true);
    }

    private void CreateSectionHeader(Transform parent, string name, string title, float y)
    {
        GameObject group = CreateRectObject(name, parent, new Vector2(0.065f, y - 0.035f), new Vector2(0.935f, y + 0.035f), Vector2.zero, Vector2.zero);
        CreateLabel("Label", group.transform, title, new Vector2(0f, 0.22f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 25f, 10f, TextAlignmentOptions.Left, textColor);
        CreateDivider(group.transform);
    }

    private void CreateSettingsToggleRow(Transform parent, string label, string toggleName, float y, bool isOn)
    {
        GameObject row = CreateRectObject(label.Replace(" ", "") + "Row", parent, new Vector2(0.065f, y - 0.043f), new Vector2(0.935f, y + 0.043f), Vector2.zero, Vector2.zero);
        CreateLabel("Label", row.transform, label, new Vector2(0f, 0f), new Vector2(0.74f, 1f), Vector2.zero, Vector2.zero, 32f, 0f, TextAlignmentOptions.Left, textColor);
        CreateToggle(row.transform, toggleName, isOn);
        CreateRowRule(row.transform);
    }

    private void CreateToggle(Transform parent, string name, bool isOn)
    {
        GameObject toggleObject = CreateRectObject(name, parent, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(112f, 58f), new Vector2(-56f, 0f));
        Image background = toggleObject.AddComponent<Image>();
        background.sprite = GetRoundedSprite();
        background.type = Image.Type.Sliced;
        background.color = isOn ? new Color(0.055f, 0.055f, 0.052f, 1f) : new Color(0.78f, 0.76f, 0.72f, 0.42f);
        background.raycastTarget = true;
        GameManager.ApplyShadow(toggleObject);

        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.isOn = isOn;

        GameObject handle = CreateRectObject("Handle", toggleObject.transform, new Vector2(isOn ? 1f : 0f, 0.5f), new Vector2(isOn ? 1f : 0f, 0.5f), new Vector2(52f, 52f), new Vector2(isOn ? -32f : 32f, 0f));
        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = GetRoundedSprite();
        handleImage.type = Image.Type.Sliced;
        handleImage.color = new Color(1f, 0.985f, 0.955f, 1f);
        handleImage.raycastTarget = false;
        GameManager.ApplyShadow(handle);
        toggle.graphic = handleImage;
    }

    private void CreateLanguageRow(Transform parent)
    {
        GameObject row = CreateRectObject("LanguageRow", parent, new Vector2(0.065f, 0.095f), new Vector2(0.935f, 0.16f), Vector2.zero, Vector2.zero);
        CreateLabel("Label", row.transform, "Language", new Vector2(0f, 0f), new Vector2(0.42f, 1f), Vector2.zero, Vector2.zero, 31f, 0f, TextAlignmentOptions.Left, textColor);
        CreateLabel("Value", row.transform, "English", new Vector2(0.54f, 0f), new Vector2(0.92f, 1f), Vector2.zero, Vector2.zero, 29f, 0f, TextAlignmentOptions.Right, new Color(0.28f, 0.27f, 0.25f, 1f));
        CreateLabel("Arrow", row.transform, "›", new Vector2(0.92f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 48f, 0f, TextAlignmentOptions.Right, new Color(0.28f, 0.27f, 0.25f, 1f));
        Button button = row.AddComponent<Button>();
        button.targetGraphic = null;
    }

    private void CreateBackButton(Transform parent)
    {
        GameObject buttonObject = CreateRectObject("BackButton", parent, new Vector2(0.052f, 0.92f), new Vector2(0.135f, 0.985f), Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 0.985f, 0.955f, 0.9f);
        image.raycastTarget = true;
        GameManager.ApplyShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Icon", buttonObject.transform, "‹", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-3f, 3f), 64f, 0f, TextAlignmentOptions.Center, textColor);
    }

    private void CreateFooterButton(Transform parent, string name, string icon, string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObject = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 0.985f, 0.955f, 0.72f);
        image.raycastTarget = true;
        GameManager.ApplyShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Label", buttonObject.transform, $"{icon}   {label}", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 25f, 3f, TextAlignmentOptions.Center, textColor);
    }

    private void CreatePanelButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, bool dark)
    {
        GameObject buttonObject = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = dark ? new Color(0.055f, 0.055f, 0.052f, 1f) : new Color(1f, 0.985f, 0.955f, 0.72f);
        image.raycastTarget = true;
        GameManager.ApplyShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Label", buttonObject.transform, label, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, dark ? 31f : 24f, dark ? 11f : 8f, TextAlignmentOptions.Center, dark ? Color.white : textColor);
    }

    private void CreateDivider(Transform parent)
    {
        CreateDividerLine(parent, "DividerLeft", new Vector2(0f, 0.06f), new Vector2(0.47f, 0.06f));
        CreateDividerLine(parent, "DividerRight", new Vector2(0.53f, 0.06f), new Vector2(1f, 0.06f));
        GameObject dot = CreateRectObject("DividerDot", parent, new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f), new Vector2(9f, 9f), Vector2.zero);
        Image dotImage = dot.AddComponent<Image>();
        dotImage.color = new Color(0.58f, 0.56f, 0.53f, 0.9f);
        dotImage.raycastTarget = false;
    }

    private void CreateDividerLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject line = CreateRectObject(name, parent, anchorMin, anchorMax, new Vector2(0f, 1f), Vector2.zero);
        Image image = line.AddComponent<Image>();
        image.color = dividerColor;
        image.raycastTarget = false;
    }

    private void CreateRowRule(Transform parent)
    {
        GameObject line = CreateRectObject("Rule", parent, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), Vector2.zero);
        Image image = line.AddComponent<Image>();
        image.color = dividerColor;
        image.raycastTarget = false;
    }

    private void ClearGeneratedChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private static Transform CreateSafeAreaRoot(Transform parent)
    {
        GameObject root = CreateRectObject("SafeAreaRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        root.AddComponent<SafeAreaFitter>();
        return root.transform;
    }

    private static GameObject CreateRectObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        obj.layer = LayerMask.NameToLayer("UI");
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
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

        return obj;
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

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        float radius = 14f;
        float max = 63f;
        Color[] pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Max(radius - x, x - (max - radius), 0f);
                float dy = Mathf.Max(radius - y, y - (max - radius), 0f);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius + 1f - distance);
                pixels[y * 64 + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        roundedSprite = Sprite.Create(texture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return roundedSprite;
    }
}
