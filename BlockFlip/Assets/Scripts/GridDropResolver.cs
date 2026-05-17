using UnityEngine;
using UnityEngine.EventSystems;

public class GridDropResolver : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private RectTransform gridRect;

    public float GridCellSize => gridManager.CellSize;

    public bool TryPlaceBlock(DraggableBlock block, PointerEventData eventData)
    {
        if (!TryFindBlockOrigin(block, eventData.pressEventCamera, out Vector2Int origin))
            return false;

        if (!gridManager.CanPlaceBlock(block.Shape, origin.x, origin.y))
            return false;

        int clearedRows = gridManager.PlaceBlock(block.Shape, origin.x, origin.y);

        if (clearedRows > 0)
        {
            Debug.Log($"Cleared Rows: {clearedRows}");
        }
        
        ScoreManager.instance.OnRowsCleared(clearedRows);

        return true;
    }

    private bool TryFindBlockOrigin(DraggableBlock block, Camera camera, out Vector2Int origin)
    {
        origin = Vector2Int.zero;
        bool hasOrigin = false;

        foreach (Vector2Int shapeCell in block.Shape.Cells)
        {
            if (!block.Visual.TryGetCellScreenPosition(shapeCell, camera, out Vector2 screenPosition))
                return false;

            Vector2Int gridCell = gridManager.FindOverlappingCell(screenPosition);
            if (gridCell.x == -1)
                return false;

            Vector2Int candidateOrigin = gridCell - shapeCell;
            if (!hasOrigin)
            {
                origin = candidateOrigin;
                hasOrigin = true;
                continue;
            }

            if (origin != candidateOrigin)
                return false;
        }

        return hasOrigin;
    }
}
