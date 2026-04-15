using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

public class UnitSelectionHandler : MonoBehaviour
{
    #region Variables
    [Title("Settings")]
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private SelectionBoxVisualizer visualizer;

    [Title("Debug")]
    [ReadOnly, ShowInInspector] private List<Unit> _selectedUnits = new List<Unit>();

    private Camera _mainCamera;
    private PlayerInput _inputActions;
    private GridManager _gridManager;
    private Pathfinder _pathfinder;

    private Vector2 _startMousePos;
    private bool _isDragging;
    #endregion

    #region Initialization
    [Inject]
    private void Construct(PlayerInput input, Camera cam, GridManager grid, Pathfinder path)
    {
        _inputActions = input;
        _mainCamera = cam;
        _gridManager = grid;
        _pathfinder = path;
    }

    private void Start()
    {
        _inputActions.Gameplay.Click.started += OnClickStarted;
        _inputActions.Gameplay.Click.canceled += OnClickReleased;
        _inputActions.Gameplay.RightClick.performed += OnRightClickPerformed;
    }
    #endregion

    private void Update()
    {
        if (_isDragging)
            visualizer.Show(_startMousePos, _inputActions.Gameplay.Point.ReadValue<Vector2>());
    }

    #region Mouse Events
    private void OnClickStarted(InputAction.CallbackContext ctx)
    {
        _startMousePos = _inputActions.Gameplay.Point.ReadValue<Vector2>();
        _isDragging = true;

        if (!Keyboard.current.shiftKey.isPressed) DeselectAll();
    }

    private void OnClickReleased(InputAction.CallbackContext ctx)
    {
        _isDragging = false;
        visualizer.Hide();

        Vector2 endPos = _inputActions.Gameplay.Point.ReadValue<Vector2>();
        if (Vector2.Distance(_startMousePos, endPos) < 5f)
            TrySelectSingleUnit(_startMousePos);
        else
            TrySelectUnitsInBox(_startMousePos, endPos);
    }
    #endregion

    #region Selection Logic
    private void TrySelectSingleUnit(Vector2 pos)
    {
        Ray ray = _mainCamera.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayer))
        {
            Unit unit = hit.collider.GetComponentInParent<Unit>();
            if (unit != null) SelectUnit(unit);
        }
    }
    //TODO: Optimize : OverlapBox / Octree/Quadtree
    private void TrySelectUnitsInBox(Vector2 start, Vector2 end)
    {
        Rect rect = new Rect(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y),
                             Mathf.Abs(start.x - end.x), Mathf.Abs(start.y - end.y));

        foreach (var unit in _gridManager.PlayerUnits)
        {
            if (unit == null) continue;
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(unit.transform.position);
            if (screenPos.z > 0 && rect.Contains((Vector2)screenPos)) SelectUnit(unit);
        }
    }

    private void SelectUnit(Unit unit)
    {
        if (_selectedUnits.Contains(unit)) return;
        _selectedUnits.Add(unit);
        unit.SetSelected(true);
    }

    private void DeselectAll()
    {
        _selectedUnits.ForEach(u => { if (u != null) u.SetSelected(false); });
        _selectedUnits.Clear();
    }
    #endregion

    #region Movement Commands
    private void OnRightClickPerformed(InputAction.CallbackContext ctx)
    {
        if (_selectedUnits.Count == 0) return;

        Ray ray = _mainCamera.ScreenPointToRay(_inputActions.Gameplay.Point.ReadValue<Vector2>());
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            IssueMoveCommand(hit.point);
        }
    }

    private void IssueMoveCommand(Vector3 worldPoint)
    {
        Vector2Int targetBase = _gridManager.WorldToGrid(worldPoint);
        if (_gridManager.GetCell(targetBase.x, targetBase.y) == null) return;

        bool isShift = Keyboard.current.shiftKey.isPressed;
        HashSet<Vector2Int> reserved = new HashSet<Vector2Int>();

        foreach (var unit in _selectedUnits)
        {
            if (unit == null) continue;

            Vector2Int start = isShift ? unit.GetLastQueuedPosition() : _gridManager.WorldToGrid(unit.transform.position);
            Vector2Int finalTarget = GetNearestValidCoords(targetBase, unit.transform.position, reserved);
            reserved.Add(finalTarget);

            var path = _pathfinder.FindPath(start, finalTarget);
            if (path != null) unit.AddPath(path, !isShift);
        }
    }
    #endregion

    #region BFS Positioning
    private Vector2Int GetNearestValidCoords(Vector2Int target, Vector3 unitPos, HashSet<Vector2Int> reserved)
    {
        GridCell targetCell = _gridManager.GetCell(target.x, target.y);
        if (targetCell == null) return target;

        if (targetCell.IsWalkable && !reserved.Contains(target)) return target;

        var  queue = new PriorityQueue<GridCell, float>();
        HashSet<GridCell> visited = new HashSet<GridCell>();

        queue.Enqueue(targetCell, 0f);
        visited.Add(targetCell);

        int iterations = 0;
        while (queue.Count > 0 && iterations < 100)
        {
            iterations++;
            GridCell current = queue.Dequeue();

            foreach (var n in _pathfinder.GetNeighbors(current))
            {
                if(visited.Contains(n)) continue;

                if (n.IsWalkable && !reserved.Contains(n.Coordinates)) return n.Coordinates;

                visited.Add(n);
                float distSq = (n.WorldPosition - unitPos).sqrMagnitude;
                queue.Enqueue(current, distSq);
                
            }
        }
        return target;
    }
    #endregion

    private void OnDestroy()
    {
        if (_inputActions == null) return;
        _inputActions.Gameplay.Click.started -= OnClickStarted;
        _inputActions.Gameplay.Click.canceled -= OnClickReleased;
        _inputActions.Gameplay.RightClick.performed -= OnRightClickPerformed;
    }
}