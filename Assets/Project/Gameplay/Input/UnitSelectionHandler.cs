using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class UnitSelectionHandler : MonoBehaviour
{
    [Title("Settings")]
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private RectTransform selectionBoxVisual;

    [Title("Debug")]
    [ReadOnly, ShowInInspector] private List<Unit> _selectedUnits = new List<Unit>();

    private Camera _mainCamera;
    private PlayerInput _inputActions;
    private Pathfinder _pathfinder;
    private GridManager _gridManager;

    private Vector2 _startMousePos;
    private bool _isDragging;

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
        _inputActions.Gameplay.Click.started += OnClickStarted;
        _inputActions.Gameplay.Click.canceled += OnClickReleased;
        _inputActions.Gameplay.RightClick.performed += OnRightClickPerformed;
    }

    private void Update()
    {
        if (_isDragging)
        {
            UpdateSelectionBox();
        }
    }

    #region Mouse Input Events

    private void OnClickStarted(InputAction.CallbackContext context)
    {
        _startMousePos = _inputActions.Gameplay.Point.ReadValue<Vector2>();
        _isDragging = true;

        DeselectAll();
    }

    private void OnClickReleased(InputAction.CallbackContext context)
    {
        _isDragging = false;
        selectionBoxVisual.gameObject.SetActive(false);

        Vector2 endMousePos = _inputActions.Gameplay.Point.ReadValue<Vector2>();

        if (Vector2.Distance(_startMousePos, endMousePos) < 5f)
        {
            TrySelectSingleUnit(_startMousePos);
        }
        else
        {
            TrySelectUnitsInBox(_startMousePos, endMousePos);
        }
    }

    private void OnRightClickPerformed(InputAction.CallbackContext context)
    {
        if (_selectedUnits.Count == 0) return;

        Vector2 mousePos = _inputActions.Gameplay.Point.ReadValue<Vector2>();
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector2Int targetBase = _gridManager.WorldToGrid(hit.point);

            HashSet<Vector2Int> reservedCoords = new HashSet<Vector2Int>();

            foreach (var unit in _selectedUnits)
            {
                Vector2Int finalTarget = GetNearestValidCoords(targetBase, unit.transform.position, reservedCoords);
                reservedCoords.Add(finalTarget);

                Vector2Int startCoords = _gridManager.WorldToGrid(unit.transform.position);
                var path = _pathfinder.FindPath(startCoords, finalTarget);

                if (path != null)
                {
                    unit.SetPath(path);
                }
            }
        }
    }

    #endregion

    #region Selection Logic

    private void UpdateSelectionBox()
    {
        if (!selectionBoxVisual.gameObject.activeSelf)
            selectionBoxVisual.gameObject.SetActive(true);

        Vector2 currentMousePos = _inputActions.Gameplay.Point.ReadValue<Vector2>();

        float minX = Mathf.Min(_startMousePos.x, currentMousePos.x);
        float maxX = Mathf.Max(_startMousePos.x, currentMousePos.x);
        float minY = Mathf.Min(_startMousePos.y, currentMousePos.y);
        float maxY = Mathf.Max(_startMousePos.y, currentMousePos.y);

        selectionBoxVisual.position = new Vector2(minX, minY);

        Canvas canvas = selectionBoxVisual.GetComponentInParent<Canvas>();
        float scaleFactor = canvas.scaleFactor;

        selectionBoxVisual.sizeDelta = new Vector2(maxX - minX, maxY - minY) / scaleFactor;
    }

    private void TrySelectSingleUnit(Vector2 pos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            Unit unit = hit.collider.GetComponentInParent<Unit>();
            if (unit != null) SelectUnit(unit);
        }
    }

    private void TrySelectUnitsInBox(Vector2 start, Vector2 end)
    {
        Rect selectionRect = new Rect(
            Mathf.Min(start.x, end.x),
            Mathf.Min(start.y, end.y),
            Mathf.Abs(start.x - end.x),
            Mathf.Abs(start.y - end.y)
        );

        var allUnits = _gridManager.AllUnits;

        for (int i = 0; i < allUnits.Count; i++)
        {
            Unit unit = allUnits[i];

            if (unit == null) continue;

            Vector3 screenPos = _mainCamera.WorldToScreenPoint(unit.transform.position);

            if (screenPos.z < 0) continue;

            if (selectionRect.Contains((Vector2)screenPos))
            {
                SelectUnit(unit);
            }
        }
    }

    private void SelectUnit(Unit unit)
    {
        if (!_selectedUnits.Contains(unit))
        {
            _selectedUnits.Add(unit);
            unit.SetSelected(true);
        }
    }

    private void DeselectAll()
    {
        foreach (var unit in _selectedUnits)
        {
            unit.SetSelected(false);
        }
        _selectedUnits.Clear();
    }

    #endregion

    private Vector2Int GetNearestValidCoords(Vector2Int target, Vector3 unitWorldPos, HashSet<Vector2Int> reserved)
    {
        GridCell targetCell = _gridManager.GetCell(target.x, target.y);
        if (targetCell == null) return target;
        if (targetCell != null && targetCell.IsWalkable && !reserved.Contains(target)) return target;

        Queue<GridCell> queue = new Queue<GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();

        queue.Enqueue(targetCell);
        visited.Add(targetCell);

        int maxIterations = 100;
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

                if (neighbor.IsWalkable && !reserved.Contains(neighbor.Coordinates))
                {
                    return neighbor.Coordinates;
                }

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }
        return target;
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Gameplay.Click.started -= OnClickStarted;
            _inputActions.Gameplay.Click.canceled -= OnClickReleased;
            _inputActions.Gameplay.RightClick.performed -= OnRightClickPerformed;
        }
    }
}