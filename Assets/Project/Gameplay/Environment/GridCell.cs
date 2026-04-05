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

    public GridCell(Vector2Int coords, Vector3 worldPos)
    {
        Coordinates = coords;
        WorldPosition = worldPos;
    }
}
