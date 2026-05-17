using System.Collections.Generic;
using UnityEngine;

public class BlockVisual : MonoBehaviour
{
    [SerializeField] private RectTransform tilePrefab;
    [SerializeField] private RectTransform tileRoot;
    public float tileSize = 40f;

    private readonly List<VisualTile> visualTiles = new List<VisualTile>();

    private struct VisualTile
    {
        public Vector2Int ShapeCell;
        public RectTransform RectTransform;

        public VisualTile(Vector2Int shapeCell, RectTransform rectTransform)
        {
            ShapeCell = shapeCell;
            RectTransform = rectTransform;
        }
    }

    public void Build(BlockShape shape)
    {
        if (shape == null || shape.Cells.Count == 0)
            return;

        foreach (Transform child in tileRoot)
        {
            Destroy(child.gameObject);
        }

        visualTiles.Clear();

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (Vector2Int cell in shape.Cells)
        {
            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxY = Mathf.Max(maxY, cell.y);
        }

        float blockWidth = (maxX - minX + 1) * tileSize;
        float blockHeight = (maxY - minY + 1) * tileSize;

        tileRoot.anchoredPosition = Vector2.zero;
        tileRoot.sizeDelta = new Vector2(blockWidth, blockHeight);

        foreach (Vector2Int cell in shape.Cells)
        {
            RectTransform tile = Instantiate(tilePrefab, tileRoot);

            tile.sizeDelta = new Vector2(tileSize, tileSize);

            float normalizedX = cell.x - minX;
            float normalizedY = cell.y - minY;

            tile.anchoredPosition = new Vector2(
                normalizedX * tileSize - blockWidth * 0.5f + tileSize * 0.5f,
                -normalizedY * tileSize + blockHeight * 0.5f - tileSize * 0.5f
            );

            visualTiles.Add(new VisualTile(cell, tile));
        }
    }

    public bool TryGetCellScreenPosition(Vector2Int shapeCell, Camera camera, out Vector2 screenPosition)
    {
        foreach (VisualTile visualTile in visualTiles)
        {
            if (visualTile.ShapeCell != shapeCell)
                continue;

            screenPosition = RectTransformUtility.WorldToScreenPoint(
                camera,
                visualTile.RectTransform.position
            );
            return true;
        }

        screenPosition = Vector2.zero;
        return false;
    }
}
