using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public PlayerInput PlayerInputs{get; private set;}
    public PlayerInput.PlayerActions PlayerActions {get; private set;}
    
    private Vector2 _lookDirection;
    public Vector2 LookDirection
    {
        get => _lookDirection;
        set => _lookDirection = value;
    }

    private void Awake()
    {
        PlayerInputs = new PlayerInput();
        PlayerActions = PlayerInputs.Player;
    }

    public void SetEnable(bool _enable)
    {
        if (_enable)
        {
            PlayerInputs.Enable();
        }
        else
        {
            PlayerInputs.Disable();
        }
    }

    private void OnEnable()
    {
        PlayerInputs?.Enable();
    }

    private void OnDisable()
    {
        PlayerInputs?.Disable();
    }
}

