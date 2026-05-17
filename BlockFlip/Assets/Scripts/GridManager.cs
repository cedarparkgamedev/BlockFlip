using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private PuzzleCell cellPrefab;
    [SerializeField] private Transform cellRoot;

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
            for (int x = 0; x < width; x++)
            {
                PuzzleCell cell = Instantiate(cellPrefab, cellRoot);

                CellState randomState = Random.value > 0.5f
                    ? CellState.White
                    : CellState.Black;

                cell.Initialize(x, y, randomState);
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

    private int ClearCompletedRows()
    {
        int clearCount = 0;

        for (int y = 0; y < height; y++)
        {
            if (IsRowUniform(y))
            {
                clearCount++;
                RegenerateRow(y);
            }
        }

        return clearCount;
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
        for (int x = 0; x < width; x++)
        {
            CellState randomState = Random.value > 0.5f
                ? CellState.White
                : CellState.Black;

            cells[x, y].SetState(randomState);
        }
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
