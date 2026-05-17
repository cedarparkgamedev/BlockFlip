using System;
using System.Collections;
using System.Collections.Generic;
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

    private PuzzleCell[,] cells;
    private float cellSize;
    private readonly List<PuzzleCell> previewCells = new List<PuzzleCell>();

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;

    private void Start()
    {
        CreateGrid();
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

                Debug.Log("rect : "+rect.width+" / "+rect.height);

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
