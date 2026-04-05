using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class Obstacle : MonoBehaviour
{
    private GridManager _gridManager;

    [Inject]
    public void Construct(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    private void Start()
    {
        RegisterObstacle(false);
    }

    private void OnDestroy()
    {
        RegisterObstacle(true);
    }
    private void RegisterObstacle(bool state)
    {
        if (_gridManager != null)
        {
            _gridManager.SetCellWalkable(transform.position, state);

        }
    }

}
