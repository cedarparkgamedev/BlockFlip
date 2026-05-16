using System.Collections.Generic;
using UnityEngine;
public class BlockVisual : MonoBehaviour
{
    [SerializeField] private RectTransform tilePrefab;
    [SerializeField] private RectTransform tileRoot;
    public float tileSize = 40f;


    public void Build(BlockShape shape)
    {
        foreach (Transform child in tileRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (Vector2Int cell in shape.Cells)
        {
            RectTransform tile = Instantiate(tilePrefab, tileRoot);

            tile.sizeDelta = new Vector2(tileSize, tileSize);

            tile.anchoredPosition = new Vector2(
                cell.x * tileSize,
                -cell.y * tileSize
            );
        }    
    }
}