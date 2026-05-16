using System.Collections.Generic;
using UnityEngine;

public static class BlockShapeLibrary
{
    public static List<BlockShape> CreateDefaultShapes()
    {
        return new List<BlockShape>
        {
            new BlockShape(
                "Single",
                new Vector2Int(0, 0)
            ),

            new BlockShape(
                "Line2",
                new Vector2Int(0, 0),
                new Vector2Int(1, 0)
            ),

            new BlockShape(
                "Line3",
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0)
            ),

            new BlockShape(
                "Line4",
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(3, 0)
            ),

            new BlockShape(
                "Square2x2",
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 1)
            ),

            new BlockShape(
                "L",
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, 2),
                new Vector2Int(1, 2)
            ),

            new BlockShape(
                "T",
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(2, 0),
                new Vector2Int(1, 1)
            ),

            new BlockShape(
                "Z",
                new Vector2Int(0, 0),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(2, 1)
            ),
        };
    }
}