using UnityEngine;
using Zenject;
public class GameplayInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PlayerInput>().AsSingle().NonLazy();
        Container.BindInterfacesTo<InputSystemManager>().AsSingle();

        Container.BindInterfacesAndSelfTo<UnitSelectionHandler>().FromComponentInHierarchy().AsSingle();

        Container.Bind<GridNavigationService>().AsSingle();
        Container.Bind<Pathfinder>().AsSingle();
    }
}
