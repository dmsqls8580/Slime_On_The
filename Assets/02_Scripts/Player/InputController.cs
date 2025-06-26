using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public PlayerInput PlayerInputs{get; private set;}
    public PlayerInput.PlayerActions PlayerActions {get; private set;}
    
    private Vector2 _lookDirection;
    public Vector2 LookDirection =>  _lookDirection;

    private void Awake()
    {
        PlayerInputs = new PlayerInput();
        PlayerActions = PlayerInputs.Player;
        
        PlayerActions.Look.performed += context => _lookDirection = context.ReadValue<Vector2>();
        PlayerActions.Look.canceled += context => _lookDirection = Vector2.zero;
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

