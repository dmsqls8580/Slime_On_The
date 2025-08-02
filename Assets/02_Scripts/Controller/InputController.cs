using UnityEngine;

public class InputController : Singleton<InputController>
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
        PlayerInputs.Enable();
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

