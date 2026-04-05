using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class GridNavigationService 
{
    private readonly GridManager _gridManager;

    public GridNavigationService(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();

        Vector2Int[] directions = {
            new Vector2Int(1, 0),  
            new Vector2Int(-1, 0), 
            new Vector2Int(0, 1),  
            new Vector2Int(0, -1)  
        };

        foreach (var dir in directions)
        {

            GridCell neighbor = _gridManager.GetCell(cell.Coordinates.x + dir.x, cell.Coordinates.y + dir.y);

            if (neighbor != null && neighbor.IsWalkable)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public int GetDistance(GridCell a, GridCell b)
    {
        return Mathf.Abs(a.Coordinates.x - b.Coordinates.x) + Mathf.Abs(a.Coordinates.y - b.Coordinates.y);
    }
}
