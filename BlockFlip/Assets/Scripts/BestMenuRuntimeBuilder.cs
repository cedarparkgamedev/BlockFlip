using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BestMenuRuntimeBuilder : MonoBehaviour
{
    private static Sprite roundedSprite;
    private readonly Color textColor = new Color(0.045f, 0.045f, 0.043f, 1f);
    private readonly Color mutedTextColor = new Color(0.18f, 0.17f, 0.16f, 1f);
    private readonly Color dividerColor = new Color(0.58f, 0.56f, 0.53f, 0.28f);

    private void Awake()
    {
        if (transform.Find("StatsPanel") != null)
            return;

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

        CreateBackButton(transform);
        CreateLabel("Title", transform, "B E S T", new Vector2(0.25f, 0.92f), new Vector2(0.75f, 0.985f), Vector2.zero, Vector2.zero, 39f, 14f, TextAlignmentOptions.Center, textColor);

        GameObject statsPanel = CreatePanel("StatsPanel", transform, new Vector2(0.06f, 0.33f), new Vector2(0.94f, 0.86f));
        CreateModeHeader(statsPanel.transform, "ClassicHeader", "C L A S S I C", true, new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.965f));
        CreateStatRow(statsPanel.transform, "ClassicBestScoreRow", "Best Score", "ClassicBestScoreValue", "0", 0.775f);
        CreateStatRow(statsPanel.transform, "ClassicLinesRow", "Lines Cleared", "ClassicLinesValue", "0", 0.675f);
        CreateStatRow(statsPanel.transform, "ClassicMaxComboRow", "Max Combo", "ClassicMaxComboValue", "x0", 0.575f);
        CreateStatRow(statsPanel.transform, "ClassicLongestRunRow", "Longest Run", "ClassicLongestRunValue", "00:00", 0.475f);
        CreateModeHeader(statsPanel.transform, "ZenHeader", "Z E N", false, new Vector2(0.05f, 0.345f), new Vector2(0.95f, 0.455f));
        CreateStatRow(statsPanel.transform, "ZenLongestSessionRow", "Longest Session", "ZenLongestSessionValue", "00:00", 0.27f);
        CreateStatRow(statsPanel.transform, "ZenLinesRow", "Lines Cleared", "ZenLinesValue", "0", 0.175f);
        CreateStatRow(statsPanel.transform, "ZenMaxComboRow", "Max Combo", "ZenMaxComboValue", "x0", 0.08f);
        CreateStatRow(statsPanel.transform, "ZenFlowStreakRow", "Best Flow Streak", "ZenFlowStreakValue", "0", -0.015f);

        GameObject achievementsPanel = CreatePanel("RecentAchievementsPanel", transform, new Vector2(0.06f, 0.125f), new Vector2(0.94f, 0.305f));
        CreateAchievements(achievementsPanel.transform);

        CreateDoneButton(transform);
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = panel.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 0.985f, 0.955f, 0.72f);
        image.raycastTarget = true;
        GameManager.ApplyShadow(panel);
        return panel;
    }

    private void CreateModeHeader(Transform parent, string name, string title, bool selected, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject header = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        CreateRadio(header.transform, selected);
        CreateLabel("Label", header.transform, title, new Vector2(0.12f, 0.05f), new Vector2(1f, 0.95f), Vector2.zero, Vector2.zero, 27f, 12f, TextAlignmentOptions.Left, textColor);
        CreateRule(header.transform, new Vector2(0f, 0f), new Vector2(1f, 0f));
    }

    private void CreateRadio(Transform parent, bool selected)
    {
        GameObject outer = CreateRectObject("Radio", parent, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 44f), new Vector2(28f, 0f));
        Image outerImage = outer.AddComponent<Image>();
        outerImage.sprite = GetRoundedSprite();
        outerImage.type = Image.Type.Sliced;
        outerImage.color = new Color(0.12f, 0.12f, 0.11f, selected ? 0.2f : 0.14f);
        outerImage.raycastTarget = false;

        GameObject inner = CreateRectObject("Dot", outer.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(22f, 22f), Vector2.zero);
        Image innerImage = inner.AddComponent<Image>();
        innerImage.sprite = GetRoundedSprite();
        innerImage.type = Image.Type.Sliced;
        innerImage.color = selected ? textColor : new Color(1f, 0.985f, 0.955f, 1f);
        innerImage.raycastTarget = false;
    }

    private void CreateStatRow(Transform parent, string name, string label, string valueName, string defaultValue, float centerY)
    {
        GameObject row = CreateRectObject(name, parent, new Vector2(0.05f, centerY - 0.045f), new Vector2(0.95f, centerY + 0.045f), Vector2.zero, Vector2.zero);
        CreateLabel("Label", row.transform, label, new Vector2(0f, 0f), new Vector2(0.54f, 1f), Vector2.zero, Vector2.zero, 26f, 7f, TextAlignmentOptions.Left, mutedTextColor);
        CreateLabel(valueName, row.transform, defaultValue, new Vector2(0.54f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 43f, 2f, TextAlignmentOptions.Right, textColor);
        CreateRule(row.transform, new Vector2(0f, 0f), new Vector2(1f, 0f));
    }

    private void CreateAchievements(Transform parent)
    {
        CreateLabel("MedalIcon", parent, "♙", new Vector2(0.05f, 0.78f), new Vector2(0.12f, 0.96f), Vector2.zero, Vector2.zero, 34f, 0f, TextAlignmentOptions.Center, textColor);
        CreateLabel("Title", parent, "R E C E N T  A C H I E V E M E N T S", new Vector2(0.14f, 0.78f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero, 25f, 9f, TextAlignmentOptions.Left, textColor);
        CreateRule(parent, new Vector2(0.04f, 0.74f), new Vector2(0.96f, 0.74f));
        CreateAchievementRow(parent, "NewBestScoreAchievement", "★", "New Best Score", 0.53f);
        CreateAchievementRow(parent, "ZenLinesAchievement", "100", "100 Lines in Zen", 0.3f);
        CreateAchievementRow(parent, "ComboAchievement", "x10", "Combo x10 Reached", 0.07f);
    }

    private void CreateAchievementRow(Transform parent, string name, string icon, string label, float centerY)
    {
        GameObject row = CreateRectObject(name, parent, new Vector2(0.04f, centerY - 0.11f), new Vector2(0.96f, centerY + 0.11f), Vector2.zero, Vector2.zero);
        CreateIconBadge(row.transform, icon);
        CreateLabel("Label", row.transform, label, new Vector2(0.13f, 0f), new Vector2(0.86f, 1f), Vector2.zero, Vector2.zero, 24f, 6f, TextAlignmentOptions.Left, mutedTextColor);
        CreateLabel("Arrow", row.transform, "›", new Vector2(0.86f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, 40f, 0f, TextAlignmentOptions.Right, mutedTextColor);
        CreateRule(row.transform, new Vector2(0f, 0f), new Vector2(1f, 0f));
    }

    private void CreateIconBadge(Transform parent, string icon)
    {
        GameObject badge = CreateRectObject("Icon", parent, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(54f, 54f), new Vector2(28f, 0f));
        Image image = badge.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 0.985f, 0.955f, 0.4f);
        image.raycastTarget = false;
        CreateLabel("Label", badge.transform, icon, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, icon.Length > 2 ? 19f : 25f, 0f, TextAlignmentOptions.Center, textColor);
    }

    private void CreateBackButton(Transform parent)
    {
        GameObject buttonObject = CreateRectObject("BackButton", parent, new Vector2(0.04f, 0.925f), new Vector2(0.135f, 0.99f), Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 0.985f, 0.955f, 0.92f);
        image.raycastTarget = true;
        GameManager.ApplyShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Icon", buttonObject.transform, "‹", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-3f, 3f), 64f, 0f, TextAlignmentOptions.Center, textColor);
    }

    private void CreateDoneButton(Transform parent)
    {
        GameObject buttonObject = CreateRectObject("DoneButton", parent, new Vector2(0.11f, 0.035f), new Vector2(0.89f, 0.095f), Vector2.zero, Vector2.zero);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(0.055f, 0.055f, 0.052f, 1f);
        image.raycastTarget = true;
        GameManager.ApplyShadow(buttonObject);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        CreateLabel("Label", buttonObject.transform, "D O N E", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 29f, 12f, TextAlignmentOptions.Center, Color.white);
    }

    private void CreateRule(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject rule = CreateRectObject("Rule", parent, anchorMin, anchorMax, new Vector2(0f, 1f), Vector2.zero);
        Image image = rule.AddComponent<Image>();
        image.color = dividerColor;
        image.raycastTarget = false;
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
