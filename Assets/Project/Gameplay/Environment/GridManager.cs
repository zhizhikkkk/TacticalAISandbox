using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Title("Grid Settings")]

    [SerializeField, OnValueChanged("OnGridParamsChanged")]
    private int width = 20;

    [SerializeField, OnValueChanged("OnGridParamsChanged")]
    private int height = 20;

    [SerializeField, OnValueChanged("OnGridParamsChanged")]
    private float cellSize = 1f;

    [Title("Debug Visualization")]
    [SerializeField] private bool showGrid = true; 
    [SerializeField] private Color gridColor = Color.cyan;

    [ShowInInspector, ReadOnly]
    private Dictionary<Vector2Int, GridCell> cells = new Dictionary<Vector2Int, GridCell>();

    private void Awake()
    {
        GenerateGridData();
    }
    #region draw grid
    [Button("Generate Grid Data")]
    public void GenerateGridData()
    {
        cells.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector2Int coords = new Vector2Int(x, z);
                // —читаем центр клетки в мировых координатах
                Vector3 worldPos = transform.position + new Vector3(x * cellSize + cellSize / 2, 0, z * cellSize + cellSize / 2);

                cells.Add(coords, new GridCell(coords, worldPos));
            }
        }
        Debug.Log($"<color=green>Grid Data Generated: {cells.Count} cells</color>");
    }
    public GridCell GetCell(int x, int z)
    {
        Vector2Int key = new Vector2Int(x, z);
        return cells.TryGetValue(key, out var cell) ? cell : null;
    }
    private void OnGridParamsChanged() => GenerateGridData();


    private void OnDrawGizmos()
    {
        if (!showGrid) return; 

        Gizmos.color = gridColor; 

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = transform.position + new Vector3(x * cellSize, 0, 0);
            Vector3 end = start + new Vector3(0, 0, height * cellSize);
            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= height; z++)
        {
            Vector3 start = transform.position + new Vector3(0, 0, z * cellSize);
            Vector3 end = start + new Vector3(width * cellSize, 0, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    #endregion

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;

        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int z = Mathf.FloorToInt(localPos.z / cellSize);

        return new Vector2Int(x, z);
    }

    public void SetCellWalkable(Vector3 worldPos, bool isWalkable)
    {
        Vector2Int coords = WorldToGrid(worldPos);
        GridCell cell = GetCell(coords.x, coords.y);

        if (cell != null)
        {
            cell.IsWalkable = isWalkable;
        }
    }
}
