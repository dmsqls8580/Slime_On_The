using _02_Scripts.Manager;
using PlayerStates;
using UnityEngine;

public class ChestObject : BaseInteractableObject, IInteractable
{
    [SerializeField] private int chestIndex = -1;

    private int chestStart;
    private int chestEnd;
    
    private InventoryManager inventoryManager;
    private UIManager uiManager;
    private UIChest uiChest;
    
    public int ChestIndex => chestIndex;
    
    protected override void Awake()
    {
        base.Awake();
        inventoryManager = InventoryManager.Instance;
        uiManager = UIManager.Instance;
        uiChest = uiManager.GetUIComponent<UIChest>();
    }
    
    private void Start()
    {
        if (chestIndex < 0)
        {
            chestIndex = InventoryManager.Instance.GetNextAvailableChestIndex();
        }
        
        chestStart = SlotIndexScheme.GetChestStart(chestIndex);
        chestEnd = chestStart + SlotIndexScheme.ChestSlotCount;
    }
    
    public override void Interact(InteractionCommandType _type, PlayerController _playerController)
    {
        switch (_type)
        {
            case InteractionCommandType.F:
                var ui = UIManager.Instance.GetUIComponent<UIChest>();
                ui.Initialize(this);
                if (!ui.IsOpen)
                {
                    uiManager.Toggle<UIChest>();
                    if (!uiManager.GetUIComponent<UIInventory>().IsOpen)
                    {
                        uiManager.Toggle<UIInventory>();
                    }
                }
                else
                {
                    uiManager.Toggle<UIChest>();
                    if (uiManager.GetUIComponent<UIInventory>().IsOpen)
                    {
                        uiManager.Toggle<UIInventory>();
                    }
                }
                break;
            case InteractionCommandType.Space:
                var toolController = _playerController.GetComponent<ToolController>();
                float toolPower = toolController != null ? toolController.GetAttackPow() : 1f;
                TakeInteraction(toolPower);
                if (currentHealth <= 0)
                {
                    if (uiManager.GetUIComponent<UIChest>().IsOpen)
                    {
                        uiManager.Toggle<UIChest>();
                    }
                    DropItems(_playerController.transform);
                    inventoryManager.ReleaseChestIndex(chestIndex);
                    Destroy(gameObject);
                }
                break;
        }
    }
    
    protected override void DropItems(Transform _player)
    {
        base.DropItems(_player);
        
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
                    itemDrop.Init(data.ItemData, 1);
                }
                rigid = dropObj.GetComponent<Rigidbody2D>();
                itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
            }
            InventoryManager.Instance.ClearItem(slotIndex);
        }
    }
    
}
