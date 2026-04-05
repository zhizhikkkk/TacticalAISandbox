using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [Title("Grid Info")]
    [ReadOnly, SerializeField] private Vector2Int gridPosition;

    [Title("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float stopDistance = 0.05f;

    [Title("Selection Settings")]
    [SerializeField] private GameObject selectionVisual;

    private GridManager _gridManager;
    private Coroutine _moveCoroutine;
    private List<GridCell> _currentPath = new List<GridCell>();

    public System.Action OnMovementFinished;

    [Inject]
    public void Construct(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    private void Start()
    {
        SnapToGrid();
    }

    [Button("Move Along Path")]
    public void SetPath(List<GridCell> path)
    {
        if (path == null || path.Count == 0) return;

        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        _currentPath = new List<GridCell>(path); 
        _moveCoroutine = StartCoroutine(FollowPathRoutine());
    }

    private IEnumerator FollowPathRoutine()
    {
        while (_currentPath.Count > 0)
        {
            GridCell targetCell = _currentPath[0];
            Vector3 targetPosition = new Vector3(targetCell.WorldPosition.x, transform.position.y, targetCell.WorldPosition.z);

            while (Vector3.Distance(transform.position, targetPosition) > stopDistance)
            {
                MoveAndRotateTowards(targetPosition);
                yield return null;
            }

            gridPosition = targetCell.Coordinates;
            _currentPath.RemoveAt(0);
        }

        _moveCoroutine = null;
        OnMovementFinished?.Invoke();
        Debug.Log($"<color=green>Unit {name} reached destination.</color>");
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

    [Button("Snap to Grid")]
    public void SnapToGrid()
    {
        if (_gridManager == null) return;

        gridPosition = _gridManager.WorldToGrid(transform.position);
        var cell = _gridManager.GetCell(gridPosition.x, gridPosition.y);

        if (cell != null)
        {
            transform.position = new Vector3(cell.WorldPosition.x, transform.position.y, cell.WorldPosition.z);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionVisual != null)
        {
            selectionVisual.SetActive(isSelected);
        }
    }
}