using _02_Scripts.Manager;
using PlayerStates;

public class WorkbenchObject : BaseInteractableObject, IStationType
{
    public CraftingStation GetStationType() => CraftingStation.Workbench;
    UIManager uiManager;
    private UIBase ui;
    
    protected override void Awake()
    {
        base.Awake();
        
        uiManager = UIManager.Instance;
        ui = UIManager.Instance.GetUIComponent<UICrafting>();
    }

    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                if (!ui.IsOpen)
                {
                    uiManager.Toggle<UICrafting>();
                }
                break;
            case InteractionCommandType.Space:
                var toolController = _playerController.GetComponent<ToolController>();
                float toolPower = toolController != null ? toolController.GetAttackPow() : 1f;
                TakeInteraction(toolPower);
                if (currentHealth <= 0)
                {
                    if (ui.IsOpen)
                    {
                        uiManager.Toggle<UICrafting>();
                    }
                    DropItems(_playerController.transform);
                    Destroy(gameObject);
                }
                break;
        }
    }
}
