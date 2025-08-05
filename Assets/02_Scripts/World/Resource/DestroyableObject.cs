using _02_Scripts.Manager;
using PlayerStates;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyableObject : BaseInteractableObject, IInteractable
{
    private IDestroyEffect destroyEffect;

    protected void Awake()
    {
        base.Awake();

        destroyEffect = GetComponent<TreeDestroyEffect>() as IDestroyEffect;

        if (destroyEffect == null)
        {
            destroyEffect = gameObject.AddComponent<DefaultDestroyEffect>();
        }
    }

    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        var toolController = _playerController.GetComponent<ToolController>();
        float toolPower = toolController.IsUnityNull() ? 1f : toolController.GetAttackPow();

        if (_type == InteractionCommandType.F && ObjectType == ObjectType.Placed)
        {
            return;
        }
        
        switch (ObjectType)
        {
            case ObjectType.Tree:
                SoundManager.Instance.PlaySFX(SFX.ToolAxe);
                break;
            case ObjectType.Ore:
                SoundManager.Instance.PlaySFX(SFX.ToolPickaxe);
                break;
            case ObjectType.Placed:
                SoundManager.Instance.PlaySFX(SFX.ToolHammer);
                break;
        }
        
        TakeInteraction(toolPower);

        if (destroyEffect is IHitReactive hitEffect)
        {
            hitEffect.OnHit(toolPower); // 데미지 전달
        }

        if (_type == InteractionCommandType.F)
            UIManager.Instance.Toggle<UICrafting>();

        if (currentHealth <= 0)
        {
            DropItems(_playerController.transform);

            destroyEffect.TriggerDestroyEffect(_playerController.transform);
        }

    }   
    
    public void DropFirstBreakItems()
    {
        DropItems(transform, dropItemsOnFirstBreak);
    }
}
