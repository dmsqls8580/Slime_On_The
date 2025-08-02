using _02_Scripts.Manager;
using PlayerStates;
using Unity.VisualScripting;

public class DestroyableObject : BaseInteractableObject, IInteractable
{
    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        var toolController = _playerController.GetComponent<ToolController>();
        float toolPower = toolController.IsUnityNull() ? toolController.GetAttackPow() : 1f;
        
        TakeInteraction(toolPower);

        if (_type == InteractionCommandType.F)
            UIManager.Instance.Toggle<UICrafting>();

        if (currentHealth <= 0)
        {
            DropItems(_playerController.transform);
            Destroy(gameObject);
        }
    }
}
