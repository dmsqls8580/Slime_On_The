using _02_Scripts.Manager;
using PlayerStates;
using UnityEngine;

public class ChestObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject dropItemPrefab; //DropItem 스크립트가 붙은 빈 오브젝트 프리팹
    [SerializeField] private int chestIndex = -1;
    public int GetChestIndex() => chestIndex;

    [Header("Drop Animation")]
    [SerializeField] private float dropUpForce = 5f;
    [SerializeField] private float dropSideForce = 2f;
    [SerializeField] private float dropAngleRange = 60f;

    [Header("Drop Item Health")]
    [SerializeField] private float maxHealth;
    private float currentHealth;
    private Rigidbody2D rigid;
    
    private void Start()
    {
        if (chestIndex < 0)
        {
            chestIndex = InventoryManager.Instance.GetNextAvailableChestIndex();
        }
    }
    
    public void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                UIManager.Instance.Toggle<UIChest>();
                var ui = UIManager.Instance.GetUIComponent<UIChest>();
                ui.Initialize(chestIndex);
                break;
            case InteractionCommandType.Space:
                var toolController = _playerController.GetComponent<ToolController>();
                float toolPower = toolController != null ? toolController.GetAttackPow() : 1f;
                TakeInteraction(toolPower);

                if (currentHealth <= 0)
                {
                    DropItems(_playerController.transform);
                    InventoryManager.Instance.ReleaseChestIndex(chestIndex);
                    Destroy(gameObject);
                }
                break;
        }
    }
    
    public void TakeInteraction(float _damage)
    {
        currentHealth -= _damage;
        Logger.Log($"{currentHealth}");
        currentHealth = Mathf.Max(currentHealth, 0);
    }
    
    private void DropItems(Transform _player)
    {
        int start = SlotIndexScheme.GetChestStart(chestIndex);
        int count = SlotIndexScheme.ChestSlotCount;
    
        for (int i = 0; i < count; i++)
        {
            int slotIndex = start + i;
            var data = InventoryManager.Instance.GetItem(slotIndex);
            if (data == null || !data.IsValid) continue;

            for (int j = 0; j < data.Quantity; j++)
            {
                var dropObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
                var itemDrop = dropObj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.Init(data.ItemData, 1, _player);
                }

                rigid = dropObj.GetComponent<Rigidbody2D>();
                itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
            }

            InventoryManager.Instance.ClearItem(slotIndex);
        }
    }
    
}
