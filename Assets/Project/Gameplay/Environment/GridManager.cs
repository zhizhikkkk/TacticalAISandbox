using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System;

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

    [Title("Auto-Scanning")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private float obstacleCheckHeight = 1.0f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showBlockedCells = true;
    [SerializeField] private Color blockedColor = new Color(1f, 0f, 0f, 0.5f);

    private List<GridCell> _debugPath = new List<GridCell>();

    [Button("Scan Scene for Obstacles")]

    void Start()
    {
        ScanSceneForObstacles();
    }
    public void ScanSceneForObstacles()
    {
        if (cells == null || cells.Count == 0) GenerateGridData();

        int blockedCount = 0;

        foreach (var cell in cells.Values)
        {
            Vector3 halfExtents = new Vector3(cellSize * 0.45f, obstacleCheckHeight / 2f, cellSize * 0.45f);

            Collider[] hitColliders = Physics.OverlapBox(cell.WorldPosition + Vector3.up * (obstacleCheckHeight / 2f),
                                                        halfExtents,
                                                        Quaternion.identity,
                                                        obstacleLayers);

            if (hitColliders.Length > 0)
            {
                cell.IsWalkable = false;
                cell.IsStaticObstacle = true;
                blockedCount++;
            }
            else
            {
                if (cell.IsStaticObstacle)
                {
                    cell.IsWalkable = true;
                    cell.IsStaticObstacle = false;
                }
            }
        }
        Debug.Log($"<color=orange>Scan complete! Blocked {blockedCount} cells.</color>");
    }
    public void SetDebugPath(List<GridCell> path)
    {
        _debugPath = path;
    }
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
                Vector3 worldPos = transform.position + new Vector3(x * cellSize + cellSize / 2, 0, z * cellSize + cellSize / 2);
                GridCell newCell = new GridCell(coords, worldPos);
                

                cells.Add(coords, newCell);
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
        DrawGridLines();
        DrawBlockedCells();
        DrawPath();

    }
    private void DrawPath()
    {
        if (_debugPath != null && _debugPath.Count > 0)
        {
            Gizmos.color = Color.yellow; 

            for (int i = 0; i < _debugPath.Count; i++)
            {
                Vector3 center = _debugPath[i].WorldPosition + Vector3.up * 0.1f;
                Vector3 size = new Vector3(cellSize * 0.8f, 0.05f, cellSize * 0.8f);
                Gizmos.DrawCube(center, size);

                if (i < _debugPath.Count - 1)
                {
                    Gizmos.DrawLine(_debugPath[i].WorldPosition + Vector3.up * 0.15f,
                                    _debugPath[i + 1].WorldPosition + Vector3.up * 0.15f);
                }
            }
        }

    }
    private void DrawBlockedCells()
    {

        if (!showBlockedCells || cells == null || cells.Count == 0) return;

        foreach (var cell in cells.Values)
        {
            if (!cell.IsWalkable)
            {
                Gizmos.color = blockedColor;

                Vector3 center = cell.WorldPosition + Vector3.up * 0.05f;
                Vector3 size = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);

                Gizmos.DrawCube(center, size);
            }
        }
    }
    private void DrawGridLines()
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
            cell.IsStaticObstacle = !isWalkable;
        }
    }

    public IEnumerable<GridCell> GetAllCells()
    {
        return cells.Values;
    }
}
