using UnityEngine;

public class GridDropResolver : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private RectTransform gridRect;

    public bool TryPlaceBlock(DraggableBlock block, Vector2 screenPosition)
    {

        Vector2Int overlappingCell = gridManager.FindOverlappingCell(screenPosition);
        if(overlappingCell.x == -1)
        {
            return false;
        }
        
        int gridX = overlappingCell.x;
        int gridY = overlappingCell.y;

        int clearedRows = gridManager.PlaceBlock(block.Shape, gridX, gridY);

        if (clearedRows > 0)
        {
            Debug.Log($"Cleared Rows: {clearedRows}");
        }
        
        ScoreManager.instance.OnRowsCleared(clearedRows);

        return true;
    }
}