using UnityEngine;

public class HoldManager : SceneOnlySingleton<HoldManager>, ISlotContainer
{
    [SerializeField] private HoldSlot holdSlot;
    [SerializeField] private Canvas holdCanvas;
    
    public ItemInstanceData HeldItem { get; private set; }
    public int SlotCount => 1;
    public bool IsHolding => HeldItem != null && HeldItem.IsValid;
    
    private void Update()
    {
        if (!IsHolding) return;

        Vector2 pos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            holdCanvas.transform as RectTransform,
            pos,
            holdCanvas.worldCamera,
            out Vector2 localPoint);

        holdSlot.SetPosition(localPoint);
    }
    
    // 들고있는 아이템 반환
    public ItemInstanceData GetItem(int index)
    {
        return index == 0 ? HeldItem : null;
    }
    
    // 들고있는 아이템 설정
    public void SetItem(int index, ItemInstanceData data)
    {
        if (index != 0) return;
        HeldItem = data;
        Refresh();
    }
    
    // 들고있는 아이템 수량 감소
    public void RemoveItem(int index, int amount)
    {
        if (index != 0 || !IsHolding) return;

        HeldItem.Quantity -= amount;
        if (HeldItem.Quantity <= 0)
        {
            Clear();
        }
        else
        {
            Refresh();
        }
    }
    
    // 들고 있는 아이템을 제거
    public void ClearItem(int index)
    {
        if (index != 0) return;
        Clear();
    }
    
    public void Clear()
    {
        HeldItem = null;
        Refresh();
    }
    
    // 아이템을 들기 시도 or 병합
    public int TryAddItem(ItemSO itemData, int amount)
    {
        if (itemData == null || amount <= 0) return 0;

        if (!IsHolding)
        {
            HeldItem = new ItemInstanceData(itemData, amount);
            Refresh();
            return amount;
        }

        if (HeldItem.ItemData != itemData) return 0;

        HeldItem.Quantity += amount;
        Refresh();
        return amount;
    }
    
    // 현재 들고 있는 아이템을 UI에 반영한다.
    public void Refresh()
    {
        if (holdSlot != null)
        {
            holdSlot.Refresh();
        }
    }

    // 들고 있는 아이템 드롭(미구현)
    public void Drop()
    {
        // TODO: 아이템 드롭 구현
        Clear();
    }
    
    
    // 장비타입 확인
    public bool IsHeldItemEquip()
    {
        return IsHolding && HeldItem.ItemData.itemTypes == ItemType.Equipable;
    }
    public EquipType? GetHeldEquipType()
    {
        return IsHeldItemEquip() ? (EquipType?)HeldItem.ItemData.equipableData.equipableType : null;
    }
}