using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;
public class Unit : MonoBehaviour
{
    [Title("Grid Info")]
    [ReadOnly, SerializeField] private Vector2Int gridPosition;

    [Title("Selection Settings")]
    [SerializeField] private GameObject selectionVisual;

    private GridManager _gridManager;

    [Inject]
    public void Construct(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    private void Start()
    {
        SnapToGrid();
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
