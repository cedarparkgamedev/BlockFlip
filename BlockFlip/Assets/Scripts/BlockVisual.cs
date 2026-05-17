using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockVisual : MonoBehaviour
{
    [SerializeField] private RectTransform tilePrefab;
    [SerializeField] private RectTransform tileRoot;
    [SerializeField] private Color blockTileColor = new Color(0.045f, 0.045f, 0.042f, 1f);
    [SerializeField] private float tileGap = 6f;
    public float tileSize = 40f;

    private readonly List<VisualTile> visualTiles = new List<VisualTile>();

    public float TileSize => tileSize;
    public Vector2 TileRootSize => tileRoot.sizeDelta;

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
        Build(shape, tileSize);
    }

    public void Build(BlockShape shape, float newTileSize)
    {
        if (shape == null || shape.Cells.Count == 0)
            return;

        tileSize = newTileSize;

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

        int columns = maxX - minX + 1;
        int rows = maxY - minY + 1;
        float blockWidth = columns * tileSize + Mathf.Max(0, columns - 1) * tileGap;
        float blockHeight = rows * tileSize + Mathf.Max(0, rows - 1) * tileGap;

        tileRoot.anchoredPosition = Vector2.zero;
        tileRoot.sizeDelta = new Vector2(blockWidth, blockHeight);

        foreach (Vector2Int cell in shape.Cells)
        {
            RectTransform tile = Instantiate(tilePrefab, tileRoot);

            tile.sizeDelta = new Vector2(tileSize, tileSize);

            float normalizedX = cell.x - minX;
            float normalizedY = cell.y - minY;

            tile.anchoredPosition = new Vector2(
                normalizedX * (tileSize + tileGap) - blockWidth * 0.5f + tileSize * 0.5f,
                -normalizedY * (tileSize + tileGap) + blockHeight * 0.5f - tileSize * 0.5f
            );

            ConfigureTileVisual(tile);
            visualTiles.Add(new VisualTile(cell, tile));
        }
    }

    public void BuildToFit(BlockShape shape, Vector2 availableSize, float padding, float maxTileSize)
    {
        if (shape == null || shape.Cells.Count == 0)
            return;

        Vector2Int bounds = GetShapeBounds(shape);
        float usableWidth = Mathf.Max(1f, availableSize.x - padding * 2f);
        float usableHeight = Mathf.Max(1f, availableSize.y - padding * 2f);
        float fitTileSize = Mathf.Min(
            maxTileSize,
            (usableWidth - Mathf.Max(0, bounds.x - 1) * tileGap) / bounds.x,
            (usableHeight - Mathf.Max(0, bounds.y - 1) * tileGap) / bounds.y
        );

        Build(shape, Mathf.Max(1f, Mathf.Floor(fitTileSize)));
    }

    private Vector2Int GetShapeBounds(BlockShape shape)
    {
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

        return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
    }

    private void ConfigureTileVisual(RectTransform tile)
    {
        Image[] images = tile.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            image.enabled = true;
            image.raycastTarget = false;
            image.color = blockTileColor;
        }

        Shadow shadow = tile.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = tile.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.36f);
        shadow.effectDistance = new Vector2(3f, -4f);
        shadow.useGraphicAlpha = true;
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

    public float GetBottomScreenY(Camera camera)
    {
        Vector3[] corners = new Vector3[4];
        tileRoot.GetWorldCorners(corners);

        float bottomY = float.MaxValue;
        foreach (Vector3 corner in corners)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, corner);
            bottomY = Mathf.Min(bottomY, screenPoint.y);
        }

        return bottomY;
    }
}
