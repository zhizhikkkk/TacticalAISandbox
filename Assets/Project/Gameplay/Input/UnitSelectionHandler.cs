using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.IO;
public class UnitSelectionHandler : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField] private LayerMask unitLayer;


    [Title("Debug")]
    [ReadOnly, ShowInInspector] private Unit _selectedUnit;

    private Camera _mainCamera;
    private PlayerInput _inputActions;

    private Pathfinder _pathfinder;
    private GridManager _gridManager;
    private List<GridCell> _currentPath = new List<GridCell>();
    [Inject]
    private void Construct(PlayerInput inputActions, Camera sceneCamera, Pathfinder pathfinder, GridManager gridManager)
    {
        _inputActions = inputActions;
        _mainCamera = sceneCamera;
        _pathfinder = pathfinder;
        _gridManager = gridManager;
    }
    private void Start()
    {
        _inputActions.Gameplay.Click.performed += OnClickPerformed;
        _inputActions.Gameplay.RightClick.performed += OnRightClickPerformed;
    }

    private void OnRightClickPerformed(InputAction.CallbackContext context)
    {
        if (_selectedUnit == null) return;

        Vector2 mousePos = _inputActions.Gameplay.Point.ReadValue<Vector2>();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector2Int targetCoords = _gridManager.WorldToGrid(hit.point);
            targetCoords = GetNearestValidCoords(targetCoords, _selectedUnit.transform.position);

            Vector2Int startCoords = _gridManager.WorldToGrid(_selectedUnit.transform.position);
            _currentPath = _pathfinder.FindPath(startCoords, targetCoords);

            if (_currentPath != null)
            {
                Debug.Log($"<color=yellow>Путь найден! Длина: {_currentPath.Count} шагов.</color>");
                _gridManager.SetDebugPath(_currentPath);
                Unit unitScript = _selectedUnit.GetComponent<Unit>();
                if (unitScript != null)
                {
                    unitScript.SetPath(_currentPath);
                }
            }
            else
            {
                Debug.LogWarning("Путь не найден или точка непроходима.");
            }
        }
    }
    private Vector2Int GetNearestValidCoords(Vector2Int target, Vector3 unitWorldPos)
    {
        GridCell targetCell = _gridManager.GetCell(target.x, target.y);

        if (targetCell != null && targetCell.IsWalkable) return target;

        Queue<GridCell> queue = new Queue<GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();

        queue.Enqueue(targetCell);
        visited.Add(targetCell);

        int maxIterations = 50;
        int iterations = 0;

        while (queue.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            GridCell current = queue.Dequeue();

            List<GridCell> neighbors = _pathfinder.GetNeighbors(current);

            neighbors.Sort((a, b) =>
                Vector3.Distance(a.WorldPosition, unitWorldPos).CompareTo(
                Vector3.Distance(b.WorldPosition, unitWorldPos)));

            foreach (var neighbor in neighbors)
            {
                if (visited.Contains(neighbor)) continue;

                if (neighbor.IsWalkable)
                {
                    return neighbor.Coordinates;
                }

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return target;
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        TrySelectUnit();
    }
    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Gameplay.Click.performed -= OnClickPerformed;
        }
    }

    private void TrySelectUnit()
    {
        Vector2 mousePos = _inputActions.Gameplay.Point.ReadValue<Vector2>();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();
            if (clickedUnit != null) SelectUnit(clickedUnit);
        }
        else
        {
            DeselectUnit();
        }
    }

    private void SelectUnit(Unit unit)
    {
        if (_selectedUnit == unit) return;
        DeselectUnit();

        _selectedUnit = unit;
        _selectedUnit.SetSelected(true);
        Debug.Log($"<color=green>New Input System: Selected {unit.name}</color>");
    }

    private void DeselectUnit()
    {
        if (_selectedUnit != null)
        {
            _selectedUnit.SetSelected(false);
            _selectedUnit = null;
        }
    }
}
