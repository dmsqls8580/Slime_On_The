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

        // TreeDestroyEffect, OreDestroyEffect 등 확장 가능
        destroyEffect = GetComponent<TreeDestroyEffect>() as IDestroyEffect;

        if (destroyEffect != null)
        {
            Debug.Log($"[DestroyableObject] IDestroyEffect 할당 성공 → {destroyEffect.GetType().Name}", this);
        }
        else
        {
            destroyEffect = gameObject.AddComponent<DefaultDestroyEffect>();
            Debug.LogWarning("[DestroyableObject] IDestroyEffect를 찾지 못해 DefaultDestroyEffect를 추가했습니다.", this);
        }
    }

    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        var toolController = _playerController.GetComponent<ToolController>();
        float toolPower = toolController.IsUnityNull() ? 1f : toolController.GetAttackPow();

        TakeInteraction(toolPower);

        if (destroyEffect is IHitReactive hitEffect)
        {
            hitEffect.OnHit();
        }

        if (_type == InteractionCommandType.F)
            UIManager.Instance.Toggle<UICrafting>();

        if (currentHealth <= 0)
        {
            DropItems(_playerController.transform);

            destroyEffect.TriggerDestroyEffect(_playerController.transform);
        }
    }
}
