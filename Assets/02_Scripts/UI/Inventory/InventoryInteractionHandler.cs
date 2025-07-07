using UnityEngine;

public class InventoryInteractionHandler : SceneOnlySingleton<InventoryInteractionHandler>
{
    [SerializeField] private UIInventory inventoryUI;
    
    private InventoryManager inventoryManager;
    private HoldManager holdManager;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        holdManager = HoldManager.Instance;

        // inventoryUI가 비어 있으면 자동으로 찾기
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<UIInventory>();
        }
    }
    
    
    
    // 좌클릭 입력
    public void HandleLeftClick(SlotBase targetSlot, bool isShift, bool isCtrl)
    {
        // 좌클릭 (홀딩 O)
        if (holdManager.IsHolding)
            HandleLeftClick_WhenHolding(targetSlot, isShift, isCtrl);
        // 좌클릭 (홀딩 X)
        else
            HandleLeftClick_WhenNotHolding(targetSlot, isShift, isCtrl);
    }

    // 우클릭 입력
    public void HandleRightClick(SlotBase targetSlot, bool isShift, bool isCtrl)
    {
        // 우클릭 (홀딩 O)
        if (holdManager.IsHolding)
            HandleRightClick_WhenHolding(targetSlot, isShift, isCtrl);
        // 우클릭 (홀딩 X)
        else
            HandleRightClick_WhenNotHolding(targetSlot, isShift, isCtrl);
    }

    
    // 좌클릭 (홀딩 O)
    private void HandleLeftClick_WhenHolding(SlotBase slot, bool isShift, bool isCtrl)
    {
        if (isShift || isCtrl)
        {
            var data = slot.GetData();
            if (data == null || !data.IsValid) return;

            int amount = isShift ? Mathf.CeilToInt(data.Quantity / 2f) : 1;
            TryPickUp(slot, amount);
        }
        else
        {
            TrySwap(slot);
        }
    }
    
    // 좌클릭 (홀딩 X)
    private void HandleLeftClick_WhenNotHolding(SlotBase slot, bool isShift, bool isCtrl)
    {
        
        var data = slot.GetData();
        if (data == null || !data.IsValid) return;

        int amount = isShift ? Mathf.CeilToInt(data.Quantity / 2f)
            : isCtrl ? 1
            : data.Quantity;

        TryPickUp(slot, amount);
    }

    
    // 우클릭 (홀딩 O)
    private void HandleRightClick_WhenHolding(SlotBase slot, bool isShift, bool isCtrl)
    {
        if (isShift || isCtrl)
        {
            int amount = isShift ? Mathf.CeilToInt(holdManager.HeldItem.Quantity / 2f) : 1;
            TryPlace(slot, amount);
        }
        else
        {
            TryUse(slot.GetData(), slot);
        }
    }
    
    // 우클릭 (홀딩 X)
    private void HandleRightClick_WhenNotHolding(SlotBase slot, bool isShift, bool isCtrl)
    {
        TryUse(slot.GetData(), slot);
    }

    
    // 줍기 시도
    private void TryPickUp(SlotBase slot, int amount)
    {
        var data = slot.GetData();
        if (data == null || !data.IsValid) return;

        // 이미 아이템 들고 있는 경우 → 다른 종류면 픽업 불가
        if (holdManager.IsHolding && holdManager.HeldItem.ItemData != data.ItemData) return;

        int taken = holdManager.TryAddItem(data.ItemData, amount);
        slot.Clear(taken);
        holdManager.Refresh();
    }

    // 놓기 시도
    private void TryPlace(SlotBase slot, int amount)
    {
        var held = holdManager.HeldItem;
        if (held == null || !held.IsValid) return;

        var slotData = slot.GetData();

        // 대상 슬롯이 비었거나 같은 아이템일 때만 가능
        if (slotData != null && slotData.IsValid && slotData.ItemData != held.ItemData) return;
        
        // 장착 아이템을 들고있을 때 해당 슬롯이 장착 슬롯이면
        if (held.ItemData.useType == UseType.Equip && slot is EquipSlot equipSlot)
        {
            if (!equipSlot.IsCorrectEquipType(held.ItemData.equipType))
                return;
        }

        int placed = inventoryManager.TryAddItem(slot.SlotIndex, held.ItemData, amount);
        holdManager.RemoveItem(placed);

        slot.Refresh();
        holdManager.Refresh();
    }
    
    // 교체 시도
    private void TrySwap(SlotBase slot)
    {
        var targetData = slot.GetData();
        var heldData = holdManager.HeldItem;
        if (heldData == null || !heldData.IsValid) return;

        // 놓기 시도
        if (targetData == null || !targetData.IsValid)
        {
            TryPlace(slot, heldData.Quantity);
            return;
        }

        // 병합 시도
        if (heldData.ItemData == targetData.ItemData)
        {
            TryPlace(slot, heldData.Quantity);
            return;
        }

        // 장착 아이템일 경우 필터링
        if (slot is EquipSlot equipSlot && heldData.ItemData.useType == UseType.Equip)
        {
            if (!equipSlot.IsCorrectEquipType(heldData.ItemData.equipType))
                return;
        }
        
        // 홀드슬롯, 타겟슬롯 아이템 교체
        holdManager.SetItem(targetData, slot);
        InventoryManager.Instance.SetItem(slot.SlotIndex, heldData);

        slot.Refresh();
        holdManager.Refresh();
    }

    // 사용하기
    public void TryUse(ItemInstanceData item, SlotBase originSlot)
    {
        if (item == null || !item.IsValid) return;

        switch (item.ItemData.useType)
        {
            case UseType.Equip:
                TryEquip(item, originSlot);
                break;

            case UseType.Consume:
                item.Quantity--;
                //TryConsume(item);
                break;

            default:
                break;
        }
    }
    
    public void TryEquip(ItemInstanceData item, SlotBase originSlot)
    {
        if (originSlot is EquipSlot)
        {
            TryUnequipFromEquipSlot(item);
        }
        else if (originSlot is InventorySlot)
        {
            TryEquipFromInventory(item, originSlot.SlotIndex);
        }
    }

    private void TryEquipFromInventory(ItemInstanceData item, int inventorySlotIndex)
    {
        EquipType type = item.ItemData.equipType;
        int equipIndex = (int)type;

        // 기존 장착 아이템 가져오기
        var prevEquip = inventoryManager.GetEquipItem(equipIndex);

        // 인벤토리 <-> 장비 간 직접 교체
        inventoryManager.SetItem(inventorySlotIndex, prevEquip);
        inventoryManager.SetEquipItem(equipIndex, item);
    }

    private void TryUnequipFromEquipSlot(ItemInstanceData item)
    {
        if (!inventoryManager.TryAddItemGlobally(item.ItemData, item.Quantity))
        {
            // DropItem(item); // 드랍 로직 별도로 호출
        }
    
        inventoryManager.ClearEquipItem((int)item.ItemData.equipType);
    }
}
