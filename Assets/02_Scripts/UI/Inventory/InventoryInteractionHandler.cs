using System;
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
    public void HandleLeftClick(SlotBase _targetSlot, bool _isShift, bool _isCtrl)
    {
        if (holdManager.IsHolding)
            HandleLeftClick_WhenHolding(_targetSlot, _isShift, _isCtrl);
        else
            HandleLeftClick_WhenNotHolding(_targetSlot, _isShift, _isCtrl);
    }

    // 우클릭 입력
    public void HandleRightClick(SlotBase _targetSlot, bool _isShift, bool _isCtrl)
    {
        if (holdManager.IsHolding)
            HandleRightClick_WhenHolding(_targetSlot, _isShift, _isCtrl);
        else
            HandleRightClick_WhenNotHolding(_targetSlot, _isShift, _isCtrl);
    }

    // 좌클릭 (홀딩 O)
    private void HandleLeftClick_WhenHolding(SlotBase _slot, bool _isShift, bool _isCtrl)
    {
        if (_isShift || _isCtrl)
        {
            var data = _slot.GetData();
            if (data == null || !data.IsValid) return;

            int amount = _isShift ? Mathf.CeilToInt(data.Quantity / 2f) : 1;
            TryPickUp(_slot, amount);
        }
        else
        {
            TrySwap(_slot);
        }
    }

    // 좌클릭 (홀딩 X)
    private void HandleLeftClick_WhenNotHolding(SlotBase _slot, bool _isShift, bool _isCtrl)
    {
        var data = _slot.GetData();
        if (data == null || !data.IsValid) return;

        int amount = _isShift ? Mathf.CeilToInt(data.Quantity / 2f)
            : _isCtrl ? 1
            : data.Quantity;

        TryPickUp(_slot, amount);
    }

    // 우클릭 (홀딩 O)
    private void HandleRightClick_WhenHolding(SlotBase _slot, bool _isShift, bool _isCtrl)
    {
        if (_isShift || _isCtrl)
        {
            int amount = _isShift ? Mathf.CeilToInt(holdManager.HeldItem.Quantity / 2f) : 1;
            TryPlace(_slot, amount);
        }
        else
        {
            TryUse(_slot.GetData(), _slot);
        }
    }

    // 우클릭 (홀딩 X)
    private void HandleRightClick_WhenNotHolding(SlotBase _slot, bool _isShift, bool _isCtrl)
    {
        TryUse(_slot.GetData(), _slot);
    }

    // 줍기 시도
    private void TryPickUp(SlotBase _slot, int _amount)
    {
        var data = _slot.GetData();
        if (data == null || !data.IsValid) return;

        if (holdManager.IsHolding && holdManager.HeldItem.ItemData != data.ItemData) return;

        int taken = holdManager.TryAddItem(data.ItemData, _amount);
        _slot.Clear(taken);
        holdManager.Refresh();
    }

    // 놓기 시도
    private void TryPlace(SlotBase _slot, int _amount)
    {
        var held = holdManager.HeldItem;
        if (held == null || !held.IsValid) return;

        var slotData = _slot.GetData();

        if (slotData != null && slotData.IsValid && slotData.ItemData != held.ItemData) return;
        
        if (held.ItemData.itemTypes == ItemType.Equipable && _slot is EquipSlot equipSlot)
        {
            if (!equipSlot.IsCorrectEquipType(held.ItemData.equipableData.equipableType))
                return;
        }

        int placed = inventoryManager.TryAddItem(_slot.SlotIndex, held.ItemData, _amount);
        holdManager.RemoveItem(placed);

        _slot.Refresh();
        holdManager.Refresh();
    }

    // 교체 시도
    private void TrySwap(SlotBase _slot)
    {
        var targetData = _slot.GetData();
        var heldData = holdManager.HeldItem;
        if (heldData == null || !heldData.IsValid) return;

        if (targetData == null || !targetData.IsValid)
        {
            TryPlace(_slot, heldData.Quantity);
            return;
        }

        if (heldData.ItemData == targetData.ItemData)
        {
            TryPlace(_slot, heldData.Quantity);
            return;
        }

        if (_slot is EquipSlot equipSlot && heldData.ItemData.itemTypes== ItemType.Equipable)
        {
            if (!equipSlot.IsCorrectEquipType(heldData.ItemData.equipableData.equipableType))
                return;
        }
        
        holdManager.SetItem(targetData, _slot);
        InventoryManager.Instance.SetItem(_slot.SlotIndex, heldData);

        _slot.Refresh();
        holdManager.Refresh();
    }

    // 사용하기
    public void TryUse(ItemInstanceData _item, SlotBase _originSlot)
    {
        if (_item == null || !_item.IsValid) return;

        switch (_item.ItemData.itemTypes)
        {
            case ItemType.Equipable:
                TryEquip(_item, _originSlot);
                break;

            case ItemType.Eatable:
                _item.Quantity--;
                //TryConsume(_item);
                break;
            case ItemType.Placeable:
                inventoryManager.placeMode.SetActiveTruePlaceMode(_item.ItemData.placeableData.placeableInfo);
                break;
            default:
                break;
        }
    }

    public void TryEquip(ItemInstanceData _item, SlotBase _originSlot)
    {
        if (_originSlot is EquipSlot)
        {
            TryUnequipFromEquipSlot(_item);
        }
        else if (_originSlot is InventorySlot)
        {
            TryEquipFromInventory(_item, _originSlot.SlotIndex);
        }
    }

    private void TryEquipFromInventory(ItemInstanceData _item, int _inventorySlotIndex)
    {
        EquipType type = _item.ItemData.equipableData.equipableType;
        int equipIndex = (int)type;

        var prevEquip = inventoryManager.GetEquipItem(equipIndex);
        
        inventoryManager.SetItem(_inventorySlotIndex, prevEquip);

        var equipSlots = inventoryUI.GetEquipSlots();
        equipSlots[equipIndex].SetItem(_item);
    }

    private void TryUnequipFromEquipSlot(ItemInstanceData _item)
    {
        if (!inventoryManager.TryAddItemGlobally(_item.ItemData, _item.Quantity))
        {
            // DropItem(_item);
        }   
        var equipSlots = inventoryUI.GetEquipSlots();
        int equipIndex = (int)_item.ItemData.equipableData.equipableType;
        equipSlots[equipIndex].SetItem(null); 
    }
}