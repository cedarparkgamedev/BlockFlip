using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeMenuView : MonoBehaviour
{
    private const string HomeSceneName = "HomeScene";
    private const string GameSceneName = "GameScene";
    private const string BestScorePrefsKey = "BestScore";

    private static Sprite roundedSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureHomeMenu(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureHomeMenu(scene);
    }

    private static void EnsureHomeMenu(Scene scene)
    {
        if (scene.name != HomeSceneName || FindAnyObjectByType<HomeMenuView>() != null)
            return;

        new GameObject("HomeMenu").AddComponent<HomeMenuView>();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Build();
    }

    private void Build()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color(0.935f, 0.92f, 0.895f, 1f);
        }

        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();

        Transform root = canvas.transform;
        CreateBackground(root);

        RectTransform content = CreateRect("Content", root);
        content.anchorMin = new Vector2(0.08f, 0.04f);
        content.anchorMax = new Vector2(0.92f, 0.96f);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;

        CreateText(content, "Title", "FLIPLINE", new Vector2(0f, 0.82f), new Vector2(1f, 0.94f), 74f, 42f, FontStyles.Normal);
        CreateDivider(content, "TitleDivider", 0.79f, 0.28f);
        CreateText(content, "Subtitle", "Flip tiles. Clear lines.", new Vector2(0f, 0.72f), new Vector2(1f, 0.785f), 26f, 18f, FontStyles.Normal);

        CreatePreviewGrid(content);

        CreateModeButton(content, "ClassicButton", "CLASSIC", new Vector2(0f, 0.215f), new Vector2(1f, 0.305f), true, StartClassic);
        CreateModeButton(content, "ZenButton", "ZEN", new Vector2(0f, 0.115f), new Vector2(1f, 0.19f), false, StartZen);

        CreateDivider(content, "FooterDivider", 0.07f, 0.42f);
        CreateFooterButton(content, "SettingsButton", "SETTINGS", "SET", new Vector2(0.08f, 0f), new Vector2(0.42f, 0.065f), ShowSettingsPlaceholder);
        CreateFooterButton(content, "BestButton", "BEST", "BEST", new Vector2(0.58f, 0f), new Vector2(0.92f, 0.065f), ShowBestScore);
        CreateVerticalRule(content);
    }

    private Canvas EnsureCanvas()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
            return canvas;

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void CreateBackground(Transform parent)
    {
        RectTransform background = CreateRect("SoftBackground", parent);
        background.anchorMin = Vector2.zero;
        background.anchorMax = Vector2.one;
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;

        Image image = background.gameObject.AddComponent<Image>();
        image.color = new Color(0.935f, 0.92f, 0.895f, 1f);
        image.raycastTarget = false;
        background.SetAsFirstSibling();
    }

    private void CreatePreviewGrid(Transform parent)
    {
        RectTransform frame = CreateRect("PuzzlePreview", parent);
        frame.anchorMin = new Vector2(0.08f, 0.345f);
        frame.anchorMax = new Vector2(0.92f, 0.68f);
        frame.offsetMin = Vector2.zero;
        frame.offsetMax = Vector2.zero;

        Image frameImage = frame.gameObject.AddComponent<Image>();
        frameImage.sprite = GetRoundedSprite();
        frameImage.type = Image.Type.Sliced;
        frameImage.color = new Color(0.94f, 0.925f, 0.9f, 0.96f);
        frameImage.raycastTarget = false;
        GameManager.ApplyShadow(frame.gameObject);

        CanvasGroup previewVisibility = frame.gameObject.AddComponent<CanvasGroup>();
        previewVisibility.alpha = 0f;
        previewVisibility.blocksRaycasts = false;

        RectTransform board = CreateRect("Board", frame);
        board.anchorMin = new Vector2(0.5f, 0.5f);
        board.anchorMax = new Vector2(0.5f, 0.5f);
        board.pivot = new Vector2(0.5f, 0.5f);
        board.anchoredPosition = Vector2.zero;

        HomePreviewGridSizer sizer = board.gameObject.AddComponent<HomePreviewGridSizer>();
        sizer.Configure(frame, 8, previewVisibility);

        int[] darkCells = { 2, 5, 10, 13, 14, 16, 19, 20, 25, 27, 38, 40, 42, 44, 46, 47, 50, 51, 52, 60, 61 };
        RectTransform[] cells = new RectTransform[64];
        for (int i = 0; i < 64; i++)
        {
            RectTransform cell = CreateRect($"PreviewCell_{i:00}", board);
            cell.anchorMin = new Vector2(0.5f, 0.5f);
            cell.anchorMax = new Vector2(0.5f, 0.5f);
            cell.pivot = new Vector2(0.5f, 0.5f);
            cells[i] = cell;

            Image cellImage = cell.gameObject.AddComponent<Image>();
            cellImage.sprite = GetRoundedSprite();
            cellImage.type = Image.Type.Sliced;
            cellImage.color = Contains(darkCells, i)
                ? new Color(0.055f, 0.055f, 0.052f, 0.95f)
                : new Color(1f, 0.985f, 0.955f, 0.9f);
            cellImage.raycastTarget = false;
        }

        sizer.SetCells(cells);
        sizer.ResizeNow(true);
    }

    private Button CreateModeButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, bool primary, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = primary
            ? new Color(0.055f, 0.055f, 0.052f, 1f)
            : new Color(1f, 0.985f, 0.955f, 0.88f);
        GameManager.ApplyShadow(rect.gameObject);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = primary ? new Color(0.88f, 0.88f, 0.86f, 1f) : new Color(0.96f, 0.945f, 0.92f, 1f);
        colors.pressedColor = primary ? new Color(0.68f, 0.68f, 0.66f, 1f) : new Color(0.88f, 0.86f, 0.82f, 1f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        TextMeshProUGUI text = CreateText(rect, "Label", label, Vector2.zero, Vector2.one, 42f, 22f, FontStyles.Bold);
        text.color = primary ? Color.white : new Color(0.045f, 0.045f, 0.045f, 1f);
        text.characterSpacing = 18f;
        return button;
    }

    private void CreateFooterButton(Transform parent, string name, string label, string iconText, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform group = CreateRect(name, parent);
        group.anchorMin = anchorMin;
        group.anchorMax = anchorMax;
        group.offsetMin = Vector2.zero;
        group.offsetMax = Vector2.zero;

        RectTransform icon = CreateRect("IconButton", group);
        icon.anchorMin = new Vector2(0.32f, 0.28f);
        icon.anchorMax = new Vector2(0.68f, 1f);
        icon.offsetMin = Vector2.zero;
        icon.offsetMax = Vector2.zero;

        Image iconImage = icon.gameObject.AddComponent<Image>();
        iconImage.sprite = GetRoundedSprite();
        iconImage.type = Image.Type.Sliced;
        iconImage.color = new Color(1f, 0.985f, 0.955f, 0.9f);
        GameManager.ApplyShadow(icon.gameObject);

        Button button = icon.gameObject.AddComponent<Button>();
        button.targetGraphic = iconImage;
        button.onClick.AddListener(onClick);

        TextMeshProUGUI iconLabel = CreateText(icon, "IconText", iconText, Vector2.zero, Vector2.one, 26f, 14f, FontStyles.Bold);
        iconLabel.characterSpacing = 2f;

        TextMeshProUGUI footerLabel = CreateText(group, "Label", label, new Vector2(0f, 0f), new Vector2(1f, 0.23f), 22f, 12f, FontStyles.Normal);
        footerLabel.characterSpacing = 14f;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, float fontSize, float minSize, FontStyles style)
    {
        RectTransform rect = CreateRect(name, parent);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.color = new Color(0.045f, 0.045f, 0.045f, 1f);
        label.fontSize = fontSize;
        label.fontSizeMax = fontSize;
        label.fontSizeMin = minSize;
        label.enableAutoSizing = true;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.characterSpacing = 0f;
        return label;
    }

    private void CreateDivider(Transform parent, string name, float centerY, float lineWidth)
    {
        RectTransform divider = CreateRect(name, parent);
        divider.anchorMin = new Vector2(0f, centerY);
        divider.anchorMax = new Vector2(1f, centerY);
        divider.sizeDelta = new Vector2(0f, 18f);
        divider.anchoredPosition = Vector2.zero;

        CreateDividerLine(divider, "LeftLine", new Vector2(0.08f, 0.5f), new Vector2(0.5f - lineWidth * 0.15f, 0.5f));
        CreateDividerLine(divider, "RightLine", new Vector2(0.5f + lineWidth * 0.15f, 0.5f), new Vector2(0.92f, 0.5f));

        RectTransform dot = CreateRect("Dot", divider);
        dot.anchorMin = new Vector2(0.5f, 0.5f);
        dot.anchorMax = new Vector2(0.5f, 0.5f);
        dot.sizeDelta = new Vector2(9f, 9f);
        Image dotImage = dot.gameObject.AddComponent<Image>();
        dotImage.color = new Color(0.58f, 0.56f, 0.53f, 1f);
        dotImage.raycastTarget = false;
    }

    private void CreateDividerLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform line = CreateRect(name, parent);
        line.anchorMin = anchorMin;
        line.anchorMax = anchorMax;
        line.sizeDelta = new Vector2(0f, 1f);
        Image lineImage = line.gameObject.AddComponent<Image>();
        lineImage.color = new Color(0.58f, 0.56f, 0.53f, 0.32f);
        lineImage.raycastTarget = false;
    }

    private void CreateVerticalRule(Transform parent)
    {
        RectTransform line = CreateRect("FooterVerticalRule", parent);
        line.anchorMin = new Vector2(0.5f, 0f);
        line.anchorMax = new Vector2(0.5f, 0.055f);
        line.sizeDelta = new Vector2(1f, 0f);
        Image image = line.gameObject.AddComponent<Image>();
        image.color = new Color(0.58f, 0.56f, 0.53f, 0.32f);
        image.raycastTarget = false;
    }

    private void StartClassic()
    {
        GameSessionSettings.Mode = GameMode.Classic;
        LoadGameScene();
    }

    private void StartZen()
    {
        GameSessionSettings.Mode = GameMode.Zen;
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (Application.CanStreamedLevelBeLoaded(GameSceneName))
        {
            SceneManager.LoadScene(GameSceneName);
            return;
        }

        SceneManager.LoadScene(1);
    }

    private void ShowSettingsPlaceholder()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            SettingsMenuPresenter.Show(canvas.transform);
        }
    }

    private void ShowBestScore()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            BestMenuPresenter.Show(canvas.transform);
        }
    }

    private void ShowModal(string title, string body)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("HomeModal");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        RectTransform overlay = CreateRect("HomeModal", canvas.transform);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;

        Image overlayImage = overlay.gameObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.18f);

        Button overlayButton = overlay.gameObject.AddComponent<Button>();
        overlayButton.targetGraphic = overlayImage;
        overlayButton.onClick.AddListener(() => Destroy(overlay.gameObject));

        RectTransform panel = CreateRect("Panel", overlay);
        panel.anchorMin = new Vector2(0.16f, 0.42f);
        panel.anchorMax = new Vector2(0.84f, 0.58f);
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.sprite = GetRoundedSprite();
        panelImage.type = Image.Type.Sliced;
        panelImage.color = new Color(1f, 0.985f, 0.955f, 0.98f);
        GameManager.ApplyShadow(panel.gameObject);

        CreateText(panel, "Title", title, new Vector2(0.08f, 0.56f), new Vector2(0.92f, 0.88f), 30f, 18f, FontStyles.Bold).characterSpacing = 10f;
        CreateText(panel, "Body", body, new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.56f), 28f, 16f, FontStyles.Normal);
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj.GetComponent<RectTransform>();
    }

    private static bool Contains(int[] values, int target)
    {
        foreach (int value in values)
        {
            if (value == target)
                return true;
        }

        return false;
    }

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        roundedSprite = CreateRoundedSprite("GeneratedRoundedButton", 64, 14f);
        return roundedSprite;
    }

    private static Sprite CreateRoundedSprite(string name, int size, float radius)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = name;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];
        float max = size - 1f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, x - (max - radius), 0f);
                float dy = Mathf.Max(radius - y, y - (max - radius), 0f);
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius + 1f - distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius)
        );
    }
}

public class HomePreviewGridSizer : MonoBehaviour
{
    private const float Spacing = 4f;
    private const float FramePadding = 10f;

    private RectTransform rectTransform;
    private RectTransform frameRect;
    private RectTransform revealRect;
    private CanvasGroup revealGroup;
    private RectTransform[] cells;
    private int columns = 8;
    private Vector2 lastSize;
    private Vector2 revealBasePosition;
    private bool hasRevealed;
    private Coroutine revealCoroutine;

    public void Configure(RectTransform targetFrame, int columnCount, CanvasGroup visibilityGroup)
    {
        rectTransform = transform as RectTransform;
        frameRect = targetFrame;
        revealGroup = visibilityGroup;
        revealRect = visibilityGroup != null ? visibilityGroup.transform as RectTransform : null;
        revealBasePosition = revealRect != null ? revealRect.anchoredPosition : Vector2.zero;
        columns = Mathf.Max(1, columnCount);
        ResizeNow(true);
    }

    public void SetCells(RectTransform[] targetCells)
    {
        cells = targetCells;
        ResizeNow(true);

        if (revealGroup != null)
        {
            revealGroup.alpha = 0f;
        }

        if (revealRect != null)
        {
            revealRect.anchoredPosition = revealBasePosition + new Vector2(0f, -28f);
        }

        if (revealCoroutine != null)
        {
            StopCoroutine(revealCoroutine);
        }

        revealCoroutine = StartCoroutine(RevealAfterStableLayout());
    }

    private void LateUpdate()
    {
        if (hasRevealed)
        {
            ResizeNow(false);
        }
    }

    private IEnumerator RevealAfterStableLayout()
    {
        hasRevealed = false;

        Vector2 previousSize = new Vector2(-1f, -1f);
        int stableFrameCount = 0;

        for (int i = 0; i < 12 && stableFrameCount < 2; i++)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            bool resized = ResizeNow(true);
            Vector2 currentSize = lastSize;

            if (resized && Approximately(currentSize, previousSize))
            {
                stableFrameCount++;
            }
            else
            {
                stableFrameCount = 0;
            }

            previousSize = currentSize;
        }

        ResizeNow(true);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        if (revealGroup != null && revealRect != null)
        {
            yield return PlayRevealAnimation();
        }
        else if (revealGroup != null)
        {
            revealGroup.alpha = 1f;
        }

        hasRevealed = true;
        revealCoroutine = null;
    }

    private IEnumerator PlayRevealAnimation()
    {
        const float duration = 0.24f;
        Vector2 startPosition = revealBasePosition + new Vector2(0f, -28f);
        float elapsed = 0f;

        revealGroup.alpha = 0f;
        revealRect.anchoredPosition = startPosition;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            revealGroup.alpha = eased;
            revealRect.anchoredPosition = Vector2.Lerp(startPosition, revealBasePosition, eased);
            yield return null;
        }

        revealGroup.alpha = 1f;
        revealRect.anchoredPosition = revealBasePosition;
    }

    public bool ResizeNow(bool force)
    {
        if (rectTransform == null)
            return false;

        Vector2 size = ResolveCurrentSize();
        if (!force && size == lastSize)
            return hasRevealed;

        lastSize = size;
        float boardSize = Mathf.Floor(Mathf.Min(size.x, size.y));
        if (boardSize <= 0f)
            return false;

        rectTransform.sizeDelta = Vector2.one * boardSize;

        if (cells == null || cells.Length == 0)
            return false;

        float cellSize = Mathf.Floor((boardSize - Spacing * (columns - 1)) / columns);
        if (cellSize <= 0f)
            return false;

        float gridSize = cellSize * columns + Spacing * (columns - 1);
        rectTransform.sizeDelta = Vector2.one * gridSize;
        float start = -gridSize * 0.5f + cellSize * 0.5f;

        for (int i = 0; i < cells.Length; i++)
        {
            RectTransform cell = cells[i];
            if (cell == null)
                continue;

            int x = i % columns;
            int y = i / columns;
            cell.sizeDelta = Vector2.one * cellSize;
            cell.anchoredPosition = new Vector2(
                start + x * (cellSize + Spacing),
                -start - y * (cellSize + Spacing)
            );
        }

        if (force)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        return true;
    }

    private Vector2 ResolveCurrentSize()
    {
        RectTransform sourceRect = frameRect != null ? frameRect : rectTransform;
        Vector2 size = sourceRect.rect.size;
        if (size.x > 0f && size.y > 0f)
            return size - Vector2.one * (FramePadding * 2f);

        if (sourceRect.parent is RectTransform parent)
        {
            Vector2 parentSize = parent.rect.size;
            Vector2 resolvedSize = new Vector2(
                parentSize.x * (sourceRect.anchorMax.x - sourceRect.anchorMin.x) + sourceRect.sizeDelta.x,
                parentSize.y * (sourceRect.anchorMax.y - sourceRect.anchorMin.y) + sourceRect.sizeDelta.y
            );
            return resolvedSize - Vector2.one * (FramePadding * 2f);
        }

        return size;
    }

    private static bool Approximately(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) < 0.5f && Mathf.Abs(a.y - b.y) < 0.5f;
    }
}
