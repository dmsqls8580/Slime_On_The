using _02_Scripts.Manager;
using PlayerStates;
using Unity.VisualScripting;
using UnityEngine;

public enum BushType
{
    BerryBush,  // 베리 덤불: 상호작용 시 베리 드롭 + 스프라이트 변경
    TwigBush    // 잔가지 덤불: 잔가지 드롭, 스프라이트 변경 없음
}

public class UndestroyableObject : BaseInteractableObject, IInteractable
{
    [Header("Bush Type")]
    [SerializeField] private BushType bushType;

    [Header("Sprite Settings (for BerryBush only)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite berrylessSprite;

    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        if (isInteracted)
            return;

        isInteracted = true;

        var toolController = _playerController.GetComponent<ToolController>();
        float toolPower = toolController == null ? 1f : toolController.GetAttackPow();

        SoundManager.Instance.PlaySFX(SFX.ToolHand);

        TakeInteraction(toolPower);

        if (currentHealth <= 0)
        {
            DropItems(_playerController.transform);

            // 베리 덤불일 경우 스프라이트 변경
            if (bushType == BushType.BerryBush && spriteRenderer != null && berrylessSprite != null)
            {
                spriteRenderer.sprite = berrylessSprite;
            }
        }
    }
}