using UnityEngine;
using Zenject;

public class InputSystemManager : IInitializable, System.IDisposable
{
    private readonly PlayerInput _input;

    public InputSystemManager(PlayerInput input)
    {
        _input = input;
    }
    public void Initialize()
    {
        _input.Gameplay.Enable();
        Debug.Log("<color=cyan>Input System: Gameplay Map Enabled</color>");
    }

    public void Dispose()
    {
        _input.Gameplay.Disable();
        Debug.Log("<color=cyan>Input System: Gameplay Map Disabled</color>");
    }
}