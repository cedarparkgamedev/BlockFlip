using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    private class ClearResult
    {
        public readonly List<int> Rows = new List<int>();
        public readonly List<int> Columns = new List<int>();
        public readonly HashSet<Vector2Int> Cells = new HashSet<Vector2Int>();

        public int LineCount => Rows.Count + Columns.Count;
    }

    [Header("Grid")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private PuzzleCell cellPrefab;
    [SerializeField] private Transform cellRoot;

    [Header("Placement Animation")]
    [SerializeField] private float flipAnimationDuration = 0.16f;
    [SerializeField] private float flipPeakScale = 1.12f;
    [SerializeField] private float rowClearHighlightDuration = 0.58f;
    [SerializeField] private float rowClearPeakScale = 1.28f;
    [SerializeField] private Color rowClearHighlightColor = new Color(1f, 0.88f, 0.2f, 1f);
    [SerializeField] private int rowClearFlashCount = 3;
    [SerializeField] private float rowClearHoldDuration = 0.08f;
    [SerializeField] private float rowRegenerateDuration = 0.46f;
    [SerializeField] private float rowRegenerateSlideDistance = 240f;
    [SerializeField] private float rowRegeneratePeakScale = 1.08f;
    [SerializeField] private float rowRegenerateStagger = 0.045f;

    [Header("Preview")]
    [SerializeField] private bool showFlipPreview = true;
    [SerializeField] private bool showClearPreview = true;
    [SerializeField] private Color previewColor = new Color(0.35f, 0.75f, 1f, 0.55f);
    [SerializeField] private float previewScale = 1.08f;
    [SerializeField] private Color clearPreviewColor = new Color(1f, 0.86f, 0.15f, 0.72f);
    [SerializeField] private float clearPreviewScale = 1.16f;

    [Header("Visual Theme")]
    [SerializeField] private bool applyReferenceStyle = true;
    [SerializeField] private Color backgroundColor = new Color(0.93f, 0.915f, 0.89f, 1f);
    [SerializeField] private Color panelColor = new Color(1f, 0.985f, 0.955f, 0.78f);
    [SerializeField] private Color slotColor = new Color(1f, 1f, 1f, 0.48f);
    [SerializeField] private Color textColor = new Color(0.045f, 0.045f, 0.045f, 1f);
    [SerializeField] private Color subtleTextColor = new Color(0.22f, 0.205f, 0.19f, 1f);

    private PuzzleCell[,] cells;
    private float cellSize;
    private readonly List<PuzzleCell> previewCells = new List<PuzzleCell>();
    private DangerManager dangerManager;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public DangerManager DangerManager => dangerManager;

    private void Start()
    {
        ApplyReferenceStyle();
        CreateGrid();
    }

    private void ApplyReferenceStyle()
    {
        if (!applyReferenceStyle)
            return;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = backgroundColor;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            EnsureBackground(canvas.transform);
        }

        float anchor_offset_y = -0.05f;
        ConfigurePanel("TopHUD", new Vector2(0f, 0.86f+anchor_offset_y), new Vector2(1f, 1f+anchor_offset_y), Color.clear, false);
        ConfigurePanel("GridRoot", new Vector2(0.075f, 0.315f+anchor_offset_y), new Vector2(0.925f, 0.845f+anchor_offset_y), Color.clear, false);
        ConfigurePanel("BlockTray", new Vector2(0.075f, 0.135f+anchor_offset_y), new Vector2(0.925f, 0.29f+anchor_offset_y), panelColor, true);
        ConfigurePanel("Controls", new Vector2(0f, 0f+anchor_offset_y), new Vector2(1f, 0.12f+anchor_offset_y), Color.clear, false);

        ConfigureTopHud();
        ConfigureTray();
        ConfigureGrid();
    }

    private void EnsureBackground(Transform canvasTransform)
    {
        Transform existing = canvasTransform.Find("SoftBackground");
        GameObject backgroundObject = existing != null
            ? existing.gameObject
            : new GameObject("SoftBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        backgroundObject.transform.SetParent(canvasTransform, false);
        backgroundObject.transform.SetAsFirstSibling();

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);

        Image backgroundImage = backgroundObject.GetComponent<Image>();
        backgroundImage.color = backgroundColor;
        backgroundImage.raycastTarget = false;
    }

    private void ConfigurePanel(string objectName, Vector2 anchorMin, Vector2 anchorMax, Color color, bool addShadow)
    {
        GameObject panelObject = GameObject.Find(objectName);
        if (panelObject == null)
            return;

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = anchorMin;
            panelRect.anchorMax = anchorMax;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;
        }

        Image panelImage = panelObject.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.enabled = color.a > 0f;
            panelImage.color = color;
            panelImage.raycastTarget = false;
        }

        if (addShadow)
        {
            AddShadow(panelObject);
        }
    }

    private void ConfigureTopHud()
    {
        ConfigureHudPanel("ScorePanel", new Vector2(0.18f, 0.14f), new Vector2(0.43f, 0.9f));
        ConfigureHudPanel("ComboPanel", new Vector2(0.47f, 0.14f), new Vector2(0.64f, 0.9f));

        ConfigureText("Score_Label", 28f, true, subtleTextColor, "SCORE");
        ConfigureText("Combo_Label", 28f, true, subtleTextColor, "COMBO");
        ConfigureText("Score_Value", 56f, false, textColor, null);
        ConfigureText("Combo_Value", 50f, false, textColor, null);

        EnsureDangerHud();
    }

    private void ConfigureHudPanel(string objectName, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panelObject = GameObject.Find(objectName);
        if (panelObject == null)
            return;

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        if (rectTransform == null)
            return;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void ConfigureText(string objectName, float fontSize, bool labelStyle, Color color, string text)
    {
        GameObject textObject = GameObject.Find(objectName);
        if (textObject == null)
            return;

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
            return;

        if (!string.IsNullOrEmpty(text))
        {
            textComponent.text = text;
        }

        textComponent.color = color;
        textComponent.fontSize = fontSize;
        textComponent.fontSizeMax = fontSize;
        textComponent.fontSizeMin = Mathf.Max(12f, fontSize * 0.55f);
        textComponent.enableAutoSizing = true;
        textComponent.characterSpacing = labelStyle ? 12f : 2f;
        textComponent.alignment = TextAlignmentOptions.Center;

        AddShadow(textObject);
    }

    private void EnsureDangerHud()
    {
        GameObject topHud = GameObject.Find("TopHUD");
        if (topHud == null)
            return;

        Transform existing = topHud.transform.Find("DangerPanel");
        GameObject dangerPanel = existing != null
            ? existing.gameObject
            : new GameObject("DangerPanel", typeof(RectTransform));

        dangerPanel.transform.SetParent(topHud.transform, false);
        RectTransform panelRect = dangerPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.68f, 0.16f);
        panelRect.anchorMax = new Vector2(0.95f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        EnsureHudText(dangerPanel.transform, "Danger_Label", "DANGER", new Vector2(0f, 0.55f), new Vector2(1f, 1f), 28f, true);
        Image dangerFillImage = EnsureDangerBar(dangerPanel.transform);
        dangerManager = FindAnyObjectByType<DangerManager>();
        if (dangerManager == null)
        {
            dangerManager = new GameObject("DangerManager").AddComponent<DangerManager>();
        }

        dangerManager.SetFillImage(dangerFillImage);
    }

    private void EnsureHudText(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, float fontSize, bool labelStyle)
    {
        Transform existing = parent.Find(name);
        GameObject textObject = existing != null
            ? existing.gameObject
            : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));

        textObject.transform.SetParent(parent, false);
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.color = subtleTextColor;
        textComponent.fontSize = fontSize;
        textComponent.fontSizeMax = fontSize;
        textComponent.fontSizeMin = Mathf.Max(12f, fontSize * 0.55f);
        textComponent.enableAutoSizing = true;
        textComponent.characterSpacing = labelStyle ? 12f : 2f;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.raycastTarget = false;

        AddShadow(textObject);
    }

    private Image EnsureDangerBar(Transform parent)
    {
        Transform existing = parent.Find("Danger_Bar");
        GameObject barObject = existing != null
            ? existing.gameObject
            : new GameObject("Danger_Bar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        barObject.transform.SetParent(parent, false);
        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0f, 0.16f);
        barRect.anchorMax = new Vector2(1f, 0.44f);
        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = Vector2.zero;

        Image barImage = barObject.GetComponent<Image>();
        barImage.color = new Color(0.08f, 0.075f, 0.07f, 0.16f);
        barImage.raycastTarget = false;
        AddShadow(barObject);

        Transform fillTransform = barObject.transform.Find("Danger_Fill");
        GameObject fillObject = fillTransform != null
            ? fillTransform.gameObject
            : new GameObject("Danger_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        fillObject.transform.SetParent(barObject.transform, false);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.zero;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = new Color(0.08f, 0.075f, 0.07f, 0.86f);
        fillImage.raycastTarget = false;
        AddShadow(fillObject);

        return fillImage;
    }

    private void ConfigureTray()
    {
        GameObject trayObject = GameObject.Find("BlockTray");
        if (trayObject == null)
            return;

        GridLayoutGroup trayLayout = trayObject.GetComponent<GridLayoutGroup>();
        if (trayLayout != null)
        {
            trayLayout.padding = new RectOffset(26, 26, 20, 20);
            trayLayout.spacing = new Vector2(18f, 0f);
            trayLayout.childAlignment = TextAnchor.MiddleCenter;
            trayLayout.cellSize = new Vector2(280f, 220f);
        }

        ConfigureSlot("BlockSlot0", new Color(1f, 1f, 1f, 0.72f), true);
        ConfigureSlot("BlockSlot1", slotColor, true);
        ConfigureSlot("BlockSlot2", slotColor, true);
    }

    private void ConfigureSlot(string objectName, Color color, bool addStrongShadow)
    {
        GameObject slotObject = GameObject.Find(objectName);
        if (slotObject == null)
            return;

        Image slotImage = slotObject.GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.enabled = true;
            slotImage.color = color;
            slotImage.raycastTarget = false;
        }

        AddShadow(slotObject);
    }

    private void ConfigureGrid()
    {
        GridLayoutGroup gridLayout = cellRoot.GetComponent<GridLayoutGroup>();
        if (gridLayout != null)
        {
            gridLayout.spacing = new Vector2(4f, 4f);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
        }
    }

    private void AddShadow(GameObject target)
    {
        GameManager.ApplyShadow(target);
    }

    private void CreateGrid()
    {
        GridLayoutGroup gridLayoutGroup = cellRoot.GetComponent<GridLayoutGroup>();

        ConfigureGridLayout(gridLayoutGroup);

        foreach (Transform child in cellRoot)
        {
            Destroy(child.gameObject);
        }

        if (width <= 0 || height <= 0)
            return;

        cells = new PuzzleCell[width, height];

        for (int y = 0; y < height; y++)
        {
            CellState[] rowStates = CreateRandomNonUniformRow();

            for (int x = 0; x < width; x++)
            {
                PuzzleCell cell = Instantiate(cellPrefab, cellRoot);

                cell.Initialize(x, y, rowStates[x]);
                cells[x, y] = cell;
            }
        }

        PreventStartingCompletedLines();
    }

    private void ConfigureGridLayout(GridLayoutGroup gridLayoutGroup)
    {
        if (gridLayoutGroup == null || width <= 0 || height <= 0)
            return;

        RectTransform rootRect = cellRoot.GetComponent<RectTransform>();
        if (rootRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        Rect rect = rootRect.rect;
        RectOffset padding = gridLayoutGroup.padding;
        Vector2 spacing = gridLayoutGroup.spacing;

        float availableWidth = rect.width - padding.left - padding.right - spacing.x * (width - 1);
        float availableHeight = rect.height - padding.top - padding.bottom - spacing.y * (height - 1);
        cellSize = Mathf.Max(1f, Mathf.Floor(Mathf.Min(
            availableWidth / width,
            availableHeight / height
        )));

        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = width;
        gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

        LayoutRebuilder.MarkLayoutForRebuild(rootRect);
    }

    public bool CanPlaceBlock(BlockShape shape, int originX, int originY)
    {
        foreach (Vector2Int offset in shape.Cells)
        {
            int x = originX + offset.x;
            int y = originY + offset.y;

            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;
        }

        return true;
    }

    public int PlaceBlock(BlockShape shape, int originX, int originY)
    {
        if (!CanPlaceBlock(shape, originX, originY))
            return 0;

        foreach (Vector2Int offset in shape.Cells)
        {
            int x = originX + offset.x;
            int y = originY + offset.y;

            cells[x, y].Flip();
        }

        int clearedLines = ClearCompletedLines();
        return clearedLines;
    }

    public void ShowPlacementPreview(BlockShape shape, int originX, int originY)
    {
        ClearPlacementPreview();

        if (!CanPlaceBlock(shape, originX, originY))
            return;

        Dictionary<Vector2Int, CellState> previewStates = CreatePlacementPreviewStates(shape, originX, originY);

        if (showFlipPreview)
        {
            foreach (KeyValuePair<Vector2Int, CellState> previewState in previewStates)
            {
                Vector2Int cellPosition = previewState.Key;
                PuzzleCell cell = cells[cellPosition.x, cellPosition.y];

                cell.ShowPreview(previewState.Value, previewColor, previewScale);
                previewCells.Add(cell);
            }
        }

        if (showClearPreview)
        {
            ClearResult previewClearResult = FindCompletedLines(previewStates);

            foreach (Vector2Int cellPosition in previewClearResult.Cells)
            {
                PuzzleCell cell = cells[cellPosition.x, cellPosition.y];
                CellState previewState = GetVirtualCellState(cellPosition.x, cellPosition.y, previewStates);

                cell.ShowPreview(previewState, clearPreviewColor, clearPreviewScale, true);
                previewCells.Add(cell);
            }
        }
    }

    public void ClearPlacementPreview()
    {
        foreach (PuzzleCell cell in previewCells)
        {
            if (cell != null)
            {
                cell.ClearPreview();
            }
        }

        previewCells.Clear();
    }

    public bool TryPlaceBlockAnimated(BlockShape shape, int originX, int originY, Action<int> onComplete)
    {
        ClearPlacementPreview();

        if (!CanPlaceBlock(shape, originX, originY))
            return false;

        foreach (Vector2Int offset in shape.Cells)
        {
            int x = originX + offset.x;
            int y = originY + offset.y;

            cells[x, y].FlipAnimated(flipAnimationDuration, flipPeakScale);
        }

        ClearResult clearResult = FindCompletedLines();
        StartCoroutine(ResolvePlacementAnimation(clearResult, onComplete));
        return true;
    }

    private int ClearCompletedLines()
    {
        ClearResult clearResult = FindCompletedLines();

        Dictionary<Vector2Int, CellState> newStates = CreateReplacementStates(clearResult);
        foreach (KeyValuePair<Vector2Int, CellState> replacementState in newStates)
        {
            Vector2Int cellPosition = replacementState.Key;
            cells[cellPosition.x, cellPosition.y].SetState(replacementState.Value);
        }

        return clearResult.LineCount;
    }

    private ClearResult FindCompletedLines()
    {
        return FindCompletedLines(null);
    }

    private ClearResult FindCompletedLines(Dictionary<Vector2Int, CellState> virtualStates)
    {
        ClearResult clearResult = new ClearResult();

        for (int y = 0; y < height; y++)
        {
            if (IsRowUniform(y, virtualStates))
            {
                clearResult.Rows.Add(y);

                for (int x = 0; x < width; x++)
                {
                    clearResult.Cells.Add(new Vector2Int(x, y));
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            if (IsColumnUniform(x, virtualStates))
            {
                clearResult.Columns.Add(x);

                for (int y = 0; y < height; y++)
                {
                    clearResult.Cells.Add(new Vector2Int(x, y));
                }
            }
        }

        return clearResult;
    }

    private bool IsRowUniform(int y)
    {
        return IsRowUniform(y, null);
    }

    private bool IsRowUniform(int y, Dictionary<Vector2Int, CellState> virtualStates)
    {
        CellState firstState = GetVirtualCellState(0, y, virtualStates);

        for (int x = 1; x < width; x++)
        {
            if (GetVirtualCellState(x, y, virtualStates) != firstState)
                return false;
        }

        return true;
    }

    private bool IsColumnUniform(int x)
    {
        return IsColumnUniform(x, null);
    }

    private bool IsColumnUniform(int x, Dictionary<Vector2Int, CellState> virtualStates)
    {
        CellState firstState = GetVirtualCellState(x, 0, virtualStates);

        for (int y = 1; y < height; y++)
        {
            if (GetVirtualCellState(x, y, virtualStates) != firstState)
                return false;
        }

        return true;
    }

    private Dictionary<Vector2Int, CellState> CreatePlacementPreviewStates(BlockShape shape, int originX, int originY)
    {
        Dictionary<Vector2Int, CellState> previewStates = new Dictionary<Vector2Int, CellState>();

        foreach (Vector2Int offset in shape.Cells)
        {
            int x = originX + offset.x;
            int y = originY + offset.y;
            Vector2Int cellPosition = new Vector2Int(x, y);

            previewStates[cellPosition] = GetOppositeState(cells[x, y].State);
        }

        return previewStates;
    }

    private IEnumerator ResolvePlacementAnimation(ClearResult clearResult, Action<int> onComplete)
    {
        yield return new WaitForSeconds(flipAnimationDuration);

        int totalClearedLines = 0;

        while (clearResult.LineCount > 0)
        {
            totalClearedLines += clearResult.LineCount;

            foreach (Vector2Int cellPosition in clearResult.Cells)
            {
                cells[cellPosition.x, cellPosition.y].PlayClearHighlight(
                    rowClearHighlightDuration,
                    rowClearPeakScale,
                    rowClearHighlightColor,
                    rowClearFlashCount
                );
            }

            yield return new WaitForSeconds(rowClearHighlightDuration + rowClearHoldDuration);

            RegenerateClearedCellsAnimated(clearResult);

            yield return new WaitForSeconds(GetLineRegenerateTotalDuration());

            clearResult = FindCompletedLines();
        }

        onComplete?.Invoke(totalClearedLines);
    }

    private void RegenerateClearedCellsAnimated(ClearResult clearResult)
    {
        Dictionary<Vector2Int, CellState> newStates = CreateReplacementStates(clearResult);

        for (int i = 0; i < clearResult.Rows.Count; i++)
        {
            int y = clearResult.Rows[i];
            float slideDirection = i % 2 == 0 ? -1f : 1f;

            for (int x = 0; x < width; x++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                float delay = slideDirection < 0f
                    ? x * rowRegenerateStagger
                    : (width - 1 - x) * rowRegenerateStagger;

                cells[x, y].RegenerateSlideAnimated(
                    newStates[cellPosition],
                    rowRegenerateDuration,
                    delay,
                    rowRegenerateSlideDistance * slideDirection,
                    rowRegeneratePeakScale
                );
            }
        }

        for (int i = 0; i < clearResult.Columns.Count; i++)
        {
            int x = clearResult.Columns[i];
            float slideDirection = i % 2 == 0 ? 1f : -1f;

            for (int y = 0; y < height; y++)
            {
                Vector2Int cellPosition = new Vector2Int(x, y);
                if (clearResult.Rows.Contains(y))
                    continue;

                float delay = slideDirection > 0f
                    ? (height - 1 - y) * rowRegenerateStagger
                    : y * rowRegenerateStagger;

                cells[x, y].RegenerateSlideAnimated(
                    newStates[cellPosition],
                    rowRegenerateDuration,
                    delay,
                    Vector2.up * rowRegenerateSlideDistance * slideDirection,
                    rowRegeneratePeakScale
                );
            }
        }
    }

    private Dictionary<Vector2Int, CellState> CreateReplacementStates(ClearResult clearResult)
    {
        Dictionary<Vector2Int, CellState> newStates = new Dictionary<Vector2Int, CellState>();

        foreach (Vector2Int cellPosition in clearResult.Cells)
        {
            newStates[cellPosition] = GetRandomCellState();
        }

        PreventCompletedReplacementLines(clearResult, newStates);
        return newStates;
    }

    private float GetLineRegenerateTotalDuration()
    {
        int longestLineLength = Mathf.Max(width, height);
        return rowRegenerateDuration + Mathf.Max(0, longestLineLength - 1) * rowRegenerateStagger;
    }

    private void PreventStartingCompletedLines()
    {
        PreventCompletedLines(GetAllRows(), GetAllColumns());
    }

    private void PreventCompletedLines(List<int> rowsToCheck, List<int> columnsToCheck)
    {
        if (width <= 1 || height <= 1)
            return;

        const int maxAttempts = 32;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            bool changed = false;

            foreach (int y in rowsToCheck)
            {
                if (!IsRowUniform(y))
                    continue;

                int x = Random.Range(0, width);
                cells[x, y].SetState(GetOppositeState(cells[x, y].State));
                changed = true;
            }

            foreach (int x in columnsToCheck)
            {
                if (!IsColumnUniform(x))
                    continue;

                int y = Random.Range(0, height);
                cells[x, y].SetState(GetOppositeState(cells[x, y].State));
                changed = true;
            }

            if (!changed)
                return;
        }
    }

    private void PreventCompletedReplacementLines(ClearResult clearResult, Dictionary<Vector2Int, CellState> newStates)
    {
        if (width <= 1 || height <= 1)
            return;

        const int maxAttempts = 32;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            bool changed = false;

            foreach (int y in clearResult.Rows)
            {
                if (!IsVirtualRowUniform(y, newStates))
                    continue;

                List<Vector2Int> editableCells = GetEditableRowCells(y, newStates);
                if (editableCells.Count == 0)
                    continue;

                Vector2Int cellPosition = editableCells[Random.Range(0, editableCells.Count)];
                newStates[cellPosition] = GetOppositeState(GetVirtualCellState(cellPosition.x, cellPosition.y, newStates));
                changed = true;
            }

            foreach (int x in clearResult.Columns)
            {
                if (!IsVirtualColumnUniform(x, newStates))
                    continue;

                List<Vector2Int> editableCells = GetEditableColumnCells(x, newStates);
                if (editableCells.Count == 0)
                    continue;

                Vector2Int cellPosition = editableCells[Random.Range(0, editableCells.Count)];
                newStates[cellPosition] = GetOppositeState(GetVirtualCellState(cellPosition.x, cellPosition.y, newStates));
                changed = true;
            }

            if (!changed)
                return;
        }
    }

    private bool IsVirtualRowUniform(int y, Dictionary<Vector2Int, CellState> newStates)
    {
        CellState firstState = GetVirtualCellState(0, y, newStates);

        for (int x = 1; x < width; x++)
        {
            if (GetVirtualCellState(x, y, newStates) != firstState)
                return false;
        }

        return true;
    }

    private bool IsVirtualColumnUniform(int x, Dictionary<Vector2Int, CellState> newStates)
    {
        CellState firstState = GetVirtualCellState(x, 0, newStates);

        for (int y = 1; y < height; y++)
        {
            if (GetVirtualCellState(x, y, newStates) != firstState)
                return false;
        }

        return true;
    }

    private CellState GetVirtualCellState(int x, int y, Dictionary<Vector2Int, CellState> newStates)
    {
        Vector2Int cellPosition = new Vector2Int(x, y);
        return newStates != null && newStates.TryGetValue(cellPosition, out CellState state)
            ? state
            : cells[x, y].State;
    }

    private List<Vector2Int> GetEditableRowCells(int y, Dictionary<Vector2Int, CellState> newStates)
    {
        List<Vector2Int> editableCells = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            Vector2Int cellPosition = new Vector2Int(x, y);
            if (newStates.ContainsKey(cellPosition))
            {
                editableCells.Add(cellPosition);
            }
        }

        return editableCells;
    }

    private List<Vector2Int> GetEditableColumnCells(int x, Dictionary<Vector2Int, CellState> newStates)
    {
        List<Vector2Int> editableCells = new List<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            Vector2Int cellPosition = new Vector2Int(x, y);
            if (newStates.ContainsKey(cellPosition))
            {
                editableCells.Add(cellPosition);
            }
        }

        return editableCells;
    }

    private List<int> GetAllRows()
    {
        List<int> rows = new List<int>();

        for (int y = 0; y < height; y++)
        {
            rows.Add(y);
        }

        return rows;
    }

    private List<int> GetAllColumns()
    {
        List<int> columns = new List<int>();

        for (int x = 0; x < width; x++)
        {
            columns.Add(x);
        }

        return columns;
    }

    private CellState[] CreateRandomNonUniformRow()
    {
        CellState[] rowStates = new CellState[width];

        if (width <= 1)
        {
            rowStates[0] = GetRandomCellState();
            return rowStates;
        }

        do
        {
            for (int x = 0; x < width; x++)
            {
                rowStates[x] = GetRandomCellState();
            }
        }
        while (IsUniform(rowStates));

        return rowStates;
    }

    private bool IsUniform(CellState[] rowStates)
    {
        CellState firstState = rowStates[0];

        for (int x = 1; x < rowStates.Length; x++)
        {
            if (rowStates[x] != firstState)
                return false;
        }

        return true;
    }

    private CellState GetRandomCellState()
    {
        return Random.value > 0.5f
            ? CellState.White
            : CellState.Black;
    }

    private CellState GetOppositeState(CellState state)
    {
        return state == CellState.White
            ? CellState.Black
            : CellState.White;
    }

    public Vector2Int FindOverlappingCell(Vector2 screenPosition)
    {
        for(int x = 0;x<width;++x)
        {
            for(int y=0;y<height;++y)
            {
                PuzzleCell cell = cells[x,y];

                RectTransform cellRect = cell.GetComponent<RectTransform>();

                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        cellRect,
                        screenPosition,
                        null,
                        out Vector2 localPoint))
                {
                    continue;
                }        

                Rect rect = cellRect.rect;

                float normalizedX = (localPoint.x - rect.xMin) / rect.width;
                float normalizedY = (localPoint.y - rect.yMin) / rect.height;

                if (normalizedX < 0f || normalizedX > 1f ||
                    normalizedY < 0f || normalizedY > 1f)
                {
                    continue;
                }                

                return new Vector2Int(x,y);        
            }
        }

        return new Vector2Int(-1,-1);
    }
}
