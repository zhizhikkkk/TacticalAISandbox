using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Pathfinder
{
    private readonly GridManager _gridManager;
    private readonly GridNavigationService _navService;

    public Pathfinder(GridManager gridManager, GridNavigationService navService)
    {
        _gridManager = gridManager;
        _navService = navService;
    }

    public List<GridCell> FindPath(Vector2Int startCoords, Vector2Int targetCoords)
    {
        GridCell startCell = _gridManager.GetCell(startCoords.x, startCoords.y);
        GridCell targetCell = _gridManager.GetCell(targetCoords.x, targetCoords.y);

        if (startCell == null || targetCell == null) return null;
        if (!targetCell.IsWalkable) return null;

        foreach (var cell in _gridManager.GetAllCells())
        {
            cell.ResetPathfindingData();
        }

        List<GridCell> openList = new List<GridCell>(); 
        HashSet<GridCell> closedList = new HashSet<GridCell>();

        startCell.GCost = 0;
        startCell.HCost = _navService.GetDistance(startCell, targetCell);
        openList.Add(startCell);

        while (openList.Count > 0)
        {
            GridCell currentCell = openList.OrderBy(c => c.FCost).ThenBy(c => c.HCost).First();

            if (currentCell == targetCell)
            {
                return RetracePath(startCell, targetCell);
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            foreach (GridCell neighbor in _navService.GetNeighbors(currentCell))
            {
                if (closedList.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentCell.GCost + _navService.GetDistance(currentCell, neighbor);

                if (newMovementCostToNeighbor < neighbor.GCost || !openList.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = _navService.GetDistance(neighbor, targetCell);
                    neighbor.Parent = currentCell; 

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        return null; 
    }

    private List<GridCell> RetracePath(GridCell start, GridCell end)
    {
        List<GridCell> path = new List<GridCell>();
        GridCell current = end;

        while (current != start)
        {
            path.Add(current);
            current = current.Parent;
        }
        path.Reverse(); 
        return path;
    }
}
