using UnityEngine;
using Zenject;

public class EnvironmentInstaller : MonoInstaller
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera sceneCamera;
    public override void InstallBindings()
    {
        Container.Bind<GridManager>().FromInstance(gridManager).AsSingle();
        Container.Bind<Camera>().FromInstance(sceneCamera).AsSingle();
    }
}
