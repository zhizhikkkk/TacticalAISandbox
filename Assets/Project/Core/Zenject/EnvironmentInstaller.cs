using UnityEngine;
using Zenject;
public class EnvironmentInstaller : MonoInstaller
{
    [SerializeField] private GridManager gridManager;

    public override void InstallBindings()
    {
        Container.Bind<GridManager>().FromInstance(gridManager).AsSingle();
    }
}
