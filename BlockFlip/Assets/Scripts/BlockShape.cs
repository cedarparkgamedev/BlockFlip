using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockShape
{
    public string Name;
    public List<Vector2Int> Cells = new List<Vector2Int>();

    public BlockShape(string name, params Vector2Int[] cells)
    {
        Name = name;
        Cells.AddRange(cells);
    }

    public BlockShape Rotated90()
    {
        BlockShape rotated = new BlockShape(Name + "_Rotated");

        foreach (Vector2Int cell in Cells)
        {
            // 90도 회전: (x, y) -> (y, -x)
            rotated.Cells.Add(new Vector2Int(cell.y, -cell.x));
        }

        rotated.Normalize();
        return rotated;
    }

    public void Normalize()
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        foreach (Vector2Int cell in Cells)
        {
            minX = Mathf.Min(minX, cell.x);
            minY = Mathf.Min(minY, cell.y);
        }

        for (int i = 0; i < Cells.Count; i++)
        {
            Cells[i] = new Vector2Int(
                Cells[i].x - minX,
                Cells[i].y - minY
            );
        }
    }
}