using _02_Scripts.Manager;
using PlayerStates;
using System;
using Unity.VisualScripting;

public class UndestroyableObject : BaseInteractableObject, IInteractable
{
    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        if(isInteracted)
            return;

        isInteracted = true;
        
        var toolController = _playerController.GetComponent<ToolController>();
        float toolPower = toolController.IsUnityNull() ? 1f : toolController.GetAttackPow();

        SoundManager.Instance.PlaySFX(SFX.ToolHand);
        
        TakeInteraction(toolPower);

        if (currentHealth <= 0)
        {
            DropItems(_playerController.transform);
        }
    }
}

