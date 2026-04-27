using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
public enum UnitFaction

{

    Player,

    Enemy,

    Neutral

}
public class Unit : MonoBehaviour
{
    #region Variables

    [TitleGroup("Movement Settings")]
    [SerializeField, Min(0)] private float moveSpeed = 5f;
    [SerializeField, Min(0)] private float rotationSpeed = 10f;
    [SerializeField, Min(0)] private float stopDistance = 0.05f;

    [TitleGroup("Visuals")]
    [SerializeField] private GameObject selectionVisual;

    [TitleGroup("Grid & State")]
    [ReadOnly, ShowInInspector] private Vector2Int gridPosition;
    [ReadOnly, ShowInInspector] private bool _isMoving = false;

    public event System.Action OnMovementFinished;
    public event System.Action OnMovementStarted;

    [Title("Faction Settings")]
    [SerializeField] private UnitFaction faction;
    public UnitFaction Faction => faction;

    private GridManager _gridManager;
    private Coroutine _moveCoroutine;
    private Queue<List<GridCell>> _commandQueue = new Queue<List<GridCell>>();

    private Vector2Int _lastTargetPos;

    public bool IsMoving => _isMoving;
    

    #endregion

    #region Initialization

    [Inject]
    public void Construct(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    private void Start()
    {
        SnapToGrid();
        _lastTargetPos = gridPosition;
        _gridManager.SetCellOccupied(gridPosition, true);
    }

    private void OnEnable() => _gridManager?.RegisterUnit(this);

    private void OnDisable()
    {
        if (!gameObject.scene.isLoaded) return;
        _gridManager?.UnregisterUnit(this);
        StopCurrentMovement();
    }

    #endregion

    #region Public API

    public void AddPath(List<GridCell> newPath, bool clearQueue)
    {
        if (newPath == null || newPath.Count == 0) return;

        if (clearQueue)
        {
            StopCurrentMovement();
            _commandQueue.Clear();
        }

        _commandQueue.Enqueue(newPath);
        _lastTargetPos = newPath[newPath.Count - 1].Coordinates;

        if (!_isMoving)
        {
            ExecuteNextCommand();
        }
    }

    public void SetSelected(bool isSelected) => selectionVisual?.SetActive(isSelected);

    public Vector2Int GetLastQueuedPosition() => _lastTargetPos;

    #endregion

    #region Movement Logic

    private void ExecuteNextCommand()
    {
        if (_commandQueue.Count > 0)
        {
            List<GridCell> nextPath = _commandQueue.Dequeue();
            _moveCoroutine = StartCoroutine(FollowPathRoutine(nextPath));
        }
        else
        {
            _isMoving = false;
            OnMovementFinished?.Invoke();
        }
    }

    private IEnumerator FollowPathRoutine(List<GridCell> path)
    {
        _isMoving = true;
        OnMovementStarted?.Invoke();
        _gridManager.SetCellOccupied(gridPosition, false);

        float sqrStopDistance = stopDistance * stopDistance;

        for (int i = 0; i < path.Count; i++)
        {
            GridCell targetCell = path[i];
            Vector3 targetPosition = new Vector3(targetCell.WorldPosition.x, transform.position.y, targetCell.WorldPosition.z);

            while ((transform.position - targetPosition).sqrMagnitude > sqrStopDistance)
            {
                MoveAndRotateTowards(targetPosition);
                yield return null;
            }

            gridPosition = targetCell.Coordinates;
        }

        _gridManager.SetCellOccupied(gridPosition, true);
        ExecuteNextCommand();

        if (!_isMoving)
        {
            _lastTargetPos = gridPosition;
            OnMovementFinished?.Invoke();
        }
    }

    private void MoveAndRotateTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    private void StopCurrentMovement()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }

        _isMoving = false;
        OnMovementFinished?.Invoke();
        gridPosition = _gridManager.WorldToGrid(transform.position);
        

        if (!_isDead)
        {
            _gridManager.SetCellOccupied(gridPosition, true);
            _lastTargetPos = gridPosition;
        }
    }

    #endregion

    #region Helpers

    [Button]
    public void SnapToGrid()
    {
        if (_gridManager == null) return;
        gridPosition = _gridManager.WorldToGrid(transform.position);
        var cell = _gridManager.GetCell(gridPosition.x, gridPosition.y);
        if (cell != null)
            transform.position = new Vector3(cell.WorldPosition.x, transform.position.y, cell.WorldPosition.z);
    }

    #endregion
    private bool _isDead = false;
    public void ReleaseCell()
    {
        _isDead = true; 
        StopCurrentMovement();

        if (_gridManager != null)
        {
            _gridManager.SetCellOccupied(gridPosition, false);
            _gridManager.SetCellOccupied(_lastTargetPos, false);

            Vector2Int actualCoords = _gridManager.WorldToGrid(transform.position);
            _gridManager.SetCellOccupied(actualCoords, false);
        }
    }
}