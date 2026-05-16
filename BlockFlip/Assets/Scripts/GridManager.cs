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

    public int Width => width;
    public int Height => height;

    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        GridLayoutGroup gridLayoutGroup = cellRoot.GetComponent<GridLayoutGroup>();

        gridLayoutGroup.constraintCount = width;

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