using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerActions : InputController, IEnumerable
{
    public InputActionMap Player;
    public InputAction Move;
    public InputAction Run;
    public InputAction Attack;
    public IEnumerator<InputAction> GetEnumerator()
    {
        return Player.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Player).GetEnumerator();
    }

    public bool Contains(InputAction action)
    {
        return Player.Contains(action);
    }

    public void Enable()
    {
        Player.Enable();
    }

    public void Disable()
    {
        Player.Disable();
    }

    public InputBinding? bindingMask
    {
        get => Player.bindingMask;
        set => Player.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => Player.devices;
        set => Player.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => Player.controlSchemes;
}
