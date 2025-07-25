using _02_Scripts.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

/*
A. 좌클릭 액션
1. 들고있지않을때
좌클릭 : 목표 칸 아이템 전부 들기
Shift좌클릭 : 목표 칸 아이템 절반 들기
Ctrl좌클릭 : 목표 칸 아이템 1개 들기

2. 들고있을때
2-1. 목표 칸이 빈칸
좌클릭 : 놓기
Shift좌클릭 : 기능X
Ctrl좌클릭 : 기능X

2-2. 들고 있는 아이템과 목표 칸 아이템 일치
좌클릭 : 놓기(병합시도)
Shift좌클릭 : 목표 칸 아이템 절반을 들고 있는 아이템에 추가 시도
Ctrl좌클릭 : 목표 칸 아이템 1개를 들고 있는 아이템에 추가 시도

2-3. 들고 있는 아이템과 목표 칸 아이템 불일치
좌클릭 : 교체
Shift좌클릭 : 기능X
Ctrl좌클릭 : 기능X


B. 우클릭 액션
1. 들고있지않을때
우클릭 : 목표 칸 아이템 사용
Shift우클릭 : 열린 ISlotContainerUI간 이동
Ctrl우클릭 : 기능X

2. 들고있을때
2-1. 들고 있는 아이템과 목표 칸 아이템 일치
좌클릭 : 목표 칸 아이템 사용
Shift좌클릭 : 들고 있는 아이템 절반을 목표 칸에 추가 시도
Ctrl좌클릭 : 들고 있는 아이템 1개를 목표 칸에 추가 시도

2-2. 들고 있는 아이템과 목표 칸 아이템 불일치
좌클릭 : 목표 칸 아이템 사용
Shift좌클릭 : 열린 ISlotContainerUI간 이동
Ctrl좌클릭 : 기능X
 */

public class SlotInteractionHandler : SceneOnlySingleton<SlotInteractionHandler>
{
    private HoldManager holdManager;
    
    private void Awake()
    {
        holdManager = HoldManager.Instance;
    }
    
    public void HandleClick(SlotBase slot, PointerEventData eventData)
    {
        // 정렬잠금 모드일 경우 → 일반 동작 차단, 잠금 토글만 수행
        if (slot.Container is ISortLockableContainer sortLockContainer && sortLockContainer.IsSortLockMode)
        {
            sortLockContainer.ToggleSlotSortLock(slot.SlotIndex);
            return;
        }
        
        if (!slot.Allows(SlotPermissionFlags.CanClick)) return;

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        bool isHolding = holdManager.IsHolding;
        var targetItem = slot.GetData();

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                HandleLeftClick(slot, isHolding, targetItem, shift, ctrl);
                break;
            case PointerEventData.InputButton.Right:
                HandleRightClick(slot, isHolding, targetItem, shift, ctrl);
                break;
        }
    }

    #region [좌클릭 처리]
    private void HandleLeftClick(SlotBase slot, bool isHolding, ItemInstanceData targetItem, bool shift, bool ctrl)
    {
        
        /* A-1
        좌클릭 : 목표 칸 아이템 전부 들기
        Shift좌클릭 : 목표 칸 아이템 절반 들기
        Ctrl좌클릭 : 목표 칸 아이템 1개 들기
         */
        if (!isHolding)
        {
            if (!slot.HasItem() || !slot.Allows(SlotPermissionFlags.CanPickUp)) return;
            int count = ctrl ? 1 : shift ? targetItem.Quantity / 2 : targetItem.Quantity;
            TryPickUp(slot, count);
        }
        // A-2
        else
        {
            var heldItem = holdManager.HeldItem;

            /* A-2-1
            좌클릭 : 놓기
            Shift좌클릭 : 기능X
            Ctrl좌클릭 : 기능X
             */
            if (!slot.HasItem())
            {
                if (!slot.Allows(SlotPermissionFlags.CanPlace)) return;
                if (shift || ctrl) return;
                TryPlace(slot, heldItem.Quantity);
            }
            /* A-2-2
            좌클릭 : 놓기(병합시도)
            Shift좌클릭 : 목표 칸 아이템 절반을 들고 있는 아이템에 추가 시도
            Ctrl좌클릭 : 목표 칸 아이템 1개를 들고 있는 아이템에 추가 시도
             */
            else if (heldItem.ItemData == targetItem.ItemData)
            {
                if (!slot.Allows(SlotPermissionFlags.CanPlace)) return;

                if (shift || ctrl)
                {
                    int takeAmount = ctrl ? 1 : targetItem.Quantity / 2;
                    TryPickUp(slot, takeAmount);
                }
                else
                {
                    TryPlace(slot, heldItem.Quantity);
                }
            }
            /* A-2-3
            좌클릭 : 교체
            Shift좌클릭 : 기능X
            Ctrl좌클릭 : 기능X
             */
            else
            {
                if (!slot.Allows(SlotPermissionFlags.CanSwap)) return;
                if (shift || ctrl) return;
                TrySwap(slot);
            }
        }
    }
    #endregion

    #region [우클릭 처리]
    private void HandleRightClick(SlotBase slot, bool isHolding, ItemInstanceData targetItem, bool shift, bool ctrl)
    {
        var hold = HoldManager.Instance;
        /* B-1
        우클릭 : 목표 칸 아이템 사용
        Shift우클릭 : 열린 ISlotContainerUI간 이동
        Ctrl우클릭 : 기능X
         */
        if (!isHolding)
        {
            if (shift)
            {
                TryQuickTransfer(slot);
            }
            else if (slot.Allows(SlotPermissionFlags.CanUse))
            {
                TryUse(targetItem, slot);
            }
        }
        // 들고있을때
        else
        {
            var heldItem = hold.HeldItem;

            /* B-2
            우클릭 : 목표 칸 아이템 사용
            Shift우클릭 : 열린 ISlotContainerUI간 이동
            Ctrl우클릭 : 기능X
             */
            if (!slot.HasItem() || heldItem.ItemData == targetItem.ItemData)
            {
                if (shift)
                {
                    TryQuickTransfer(slot);
                }
                else if (slot.Allows(SlotPermissionFlags.CanUse))
                {
                    TryUse(targetItem, slot);
                }
            }
            /* B-1
            우클릭 : 목표 칸 아이템 사용
            Shift우클릭 : 들고 있는 아이템 절반을 목표 칸에 추가 시도
            Ctrl우클릭 : 들고 있는 아이템 1개를 목표 칸에 추가 시도
             */
            else
            {
                if (!slot.Allows(SlotPermissionFlags.CanPlace)) return;
                int count = ctrl ? 1 : shift ? heldItem.Quantity / 2 : heldItem.Quantity;
                TryPlace(slot, count);
            }
        }
    }
    #endregion

    // 줍기
    private void TryPickUp(SlotBase slot, int amount)
    {
        var data = slot.GetData();
        if (data == null || !data.IsValid) return;
        
        if (holdManager.IsHolding && holdManager.HeldItem.ItemData != data.ItemData) return;

        int taken = holdManager.TryAddItem(data.ItemData, amount);
        if (taken > 0)
        {
            slot.Container.RemoveItem(slot.SlotIndex, taken);
            //holdManager.SetOrigin(slot);
            slot.Refresh();
        }
    }

    // 놓기
    private void TryPlace(SlotBase slot, int amount)
    {
        var held = holdManager.HeldItem;
        if (held == null || !held.IsValid) return;
        
        var container = slot.Container;
        var index = slot.SlotIndex;
        var target = container.GetItem(index);
        
        if (target != null && target.IsValid && target.ItemData != held.ItemData) return;

        // 장비 아이템의 경우
        if (held.ItemData.itemTypes == ItemType.Equipable && slot is EquipSlot equipSlot)
        {
            if (!equipSlot.IsCorrectEquipType(held.ItemData.equipableData.equipableType)) return;
        }
        
        int moveAmount = amount;
        
        if (target != null && target.IsValid)
        {
            int space = held.ItemData.maxStack - target.Quantity;
            moveAmount = Mathf.Min(space, amount);
            if (moveAmount <= 0) return;

            target.Quantity += moveAmount;
            container.SetItem(index, target);
        }
        else
        {
            var newData = new ItemInstanceData(held.ItemData, moveAmount);
            container.SetItem(index, newData);
        }

        holdManager.RemoveItem(0, moveAmount);
        slot.Refresh();
        holdManager.Refresh();
    }

    private void TrySwap(SlotBase slot)
    {
        var held = holdManager.HeldItem;
        var target = slot.GetData();

        if (held == null || target == null) return;

        var container = slot.Container;
        container.SetItem(slot.SlotIndex, held);
        holdManager.SetItem(0, target);
        slot.Refresh();
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
            default:
                break;
        }
    }
    
    public void TryEquip(ItemInstanceData _item, SlotBase _originSlot)
    {
        if (_originSlot is EquipSlot)
        {
            TryUnequipFromEquipSlot(_item, _originSlot);
        }
        else if (_originSlot is InventorySlot)
        {
            TryEquipFromInventory(_item, _originSlot);
        }
    }
    
    private void TryEquipFromInventory(ItemInstanceData item, SlotBase inventorySlot)
    {
        EquipType type = item.ItemData.equipableData.equipableType;
        int equipIndex = (int)type;

        var equipSlots = UIManager.Instance.GetUIComponent<UIInventory>().GetEquipSlots();
        var equipSlot = equipSlots[equipIndex];
        
        var prevEquip = equipSlot.GetData();
        
        inventorySlot.Container.SetItem(inventorySlot.SlotIndex, prevEquip);
        equipSlot.Container.SetItem(equipSlot.SlotIndex, item);

        inventorySlot.Refresh();
        equipSlot.Refresh();

    }

    private void TryUnequipFromEquipSlot(ItemInstanceData item, SlotBase equipSlot)
    {
        var inventoryUI = UIManager.Instance.GetUIComponent<UIInventory>();
        // var inventorySlots = inventoryUI.GetInventorySlots();
        //
        // foreach (var slot in inventorySlots)
        // {
        //     if (!slot.HasItem())
        //     {
        //         slot.Container.SetItem(slot.SlotIndex, item);
        //         slot.Refresh();
        //
        //         equipSlot.Container.SetItem(equipSlot.SlotIndex, null);
        //         equipSlot.Refresh();
        //         return;
        //     }
        // }

        // 3. 빈 슬롯 없음 → Drop 처리 (TODO)
        // DropItem(item);
    }

    private void TryQuickTransfer(SlotBase from)
    {
        if (!from.HasItem() || HoldManager.Instance.IsHolding) return;

        // var other = UIManager.Instance.GetOtherOpenedContainerUI(from.Container);
        // if (other == null) return;
        //
        // var data = from.GetData();
        // int moved = other.TryAddItem(data.ItemData, data.Quantity);
        // if (moved > 0)
        // {
        //     from.Container.RemoveItem(from.SlotIndex, moved);
        //     from.Refresh();
        // }
    }
}
