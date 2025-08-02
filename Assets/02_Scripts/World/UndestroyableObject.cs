using _02_Scripts.Manager;
using PlayerStates;
using Unity.VisualScripting;

public class UndestroyableObject : BaseInteractableObject, IInteractable
{
    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        if(isInteracted)
            return;

        isInteracted = true;
        
        var toolController = _playerController.GetComponent<ToolController>();
        float toolPower = toolController.IsUnityNull() ? toolController.GetAttackPow() : 1f;

        if (objectType == ObjectType.UnDestroyed)
        {
            DropItems(_playerController.transform);
        }
        
        TakeInteraction(toolPower);

        if (_type == InteractionCommandType.F)
            UIManager.Instance.Toggle<UICrafting>();

        if (currentHealth <= 0)
        {
            DropItems(_playerController.transform);
        }
    }
}