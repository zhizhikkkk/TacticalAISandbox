using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class GridCell 
{
    [ReadOnly] public Vector2Int Coordinates; 
    [ReadOnly] public Vector3 WorldPosition;


    public bool IsWalkable = true; 
    public bool IsStaticObstacle = false;

    [ShowInInspector, ReadOnly]
    public float DangerScore = 0f;


    public int GCost; 
    public int HCost; 
    public int FCost => GCost + HCost;

    public GridCell Parent;
    public GridCell(Vector2Int coords, Vector3 worldPos)
    {
        Coordinates = coords;
        WorldPosition = worldPos;
    }
    public void ResetPathfindingData()
    {
        GCost = int.MaxValue; 
        HCost = 0;
        Parent = null;
    }
}
