using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
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

    private PuzzleCell[,] cells;
    private float cellSize;

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

        int clearedRows = ClearCompletedRows();
        return clearedRows;
    }

    public bool TryPlaceBlockAnimated(BlockShape shape, int originX, int originY, Action<int> onComplete)
    {
        if (!CanPlaceBlock(shape, originX, originY))
            return false;

        foreach (Vector2Int offset in shape.Cells)
        {
            int x = originX + offset.x;
            int y = originY + offset.y;

            cells[x, y].FlipAnimated(flipAnimationDuration, flipPeakScale);
        }

        List<int> completedRows = FindCompletedRows();
        StartCoroutine(ResolvePlacementAnimation(completedRows, onComplete));
        return true;
    }

    private int ClearCompletedRows()
    {
        int clearCount = 0;

        foreach (int y in FindCompletedRows())
        {
            clearCount++;
            RegenerateRow(y);
        }

        return clearCount;
    }

    private List<int> FindCompletedRows()
    {
        List<int> completedRows = new List<int>();

        for (int y = 0; y < height; y++)
        {
            if (IsRowUniform(y))
            {
                completedRows.Add(y);
            }
        }

        return completedRows;
    }

    private bool IsRowUniform(int y)
    {
        CellState firstState = cells[0, y].State;

        for (int x = 1; x < width; x++)
        {
            if (cells[x, y].State != firstState)
                return false;
        }

        return true;
    }

    private void RegenerateRow(int y)
    {
        CellState[] rowStates = CreateRandomNonUniformRow();

        for (int x = 0; x < width; x++)
        {
            cells[x, y].SetState(rowStates[x]);
        }
    }

    private IEnumerator ResolvePlacementAnimation(List<int> completedRows, Action<int> onComplete)
    {
        yield return new WaitForSeconds(flipAnimationDuration);

        if (completedRows.Count > 0)
        {
            foreach (int y in completedRows)
            {
                PlayRowClearHighlight(y);
            }

            yield return new WaitForSeconds(rowClearHighlightDuration + rowClearHoldDuration);

            for (int i = 0; i < completedRows.Count; i++)
            {
                int y = completedRows[i];
                float slideDirection = i % 2 == 0 ? -1f : 1f;
                RegenerateRowAnimated(y, slideDirection);
            }

            yield return new WaitForSeconds(GetRowRegenerateTotalDuration());
        }

        onComplete?.Invoke(completedRows.Count);
    }

    private void PlayRowClearHighlight(int y)
    {
        for (int x = 0; x < width; x++)
        {
            cells[x, y].PlayClearHighlight(
                rowClearHighlightDuration,
                rowClearPeakScale,
                rowClearHighlightColor,
                rowClearFlashCount
            );
        }
    }

    private void RegenerateRowAnimated(int y, float slideDirection)
    {
        CellState[] rowStates = CreateRandomNonUniformRow();

        for (int x = 0; x < width; x++)
        {
            float delay = slideDirection < 0f
                ? x * rowRegenerateStagger
                : (width - 1 - x) * rowRegenerateStagger;

            cells[x, y].RegenerateSlideAnimated(
                rowStates[x],
                rowRegenerateDuration,
                delay,
                rowRegenerateSlideDistance * slideDirection,
                rowRegeneratePeakScale
            );
        }
    }

    private float GetRowRegenerateTotalDuration()
    {
        return rowRegenerateDuration + Mathf.Max(0, width - 1) * rowRegenerateStagger;
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
