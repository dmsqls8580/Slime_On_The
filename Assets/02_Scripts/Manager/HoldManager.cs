using UnityEngine;

public class HoldManager : SceneOnlySingleton<HoldManager>
{
    [SerializeField] private HoldSlot holdSlot;
    [SerializeField] private Canvas holdCanvas;
    
    public ItemInstanceData HeldItem { get; private set; }
    public SlotBase OriginSlot { get; set; }
    
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
    

    // 홀드슬롯에 아이템 추가
    public int TryAddItem(TempItemSO itemData, int amount)
    {
        if (itemData == null || amount <= 0)
            return 0;

        // 아무것도 안 들고 있으면 새로 추가
        if (!IsHolding)
        {
            int addAmount = Mathf.Clamp(amount, 1, itemData.maxStack);
            HeldItem = new ItemInstanceData(itemData, addAmount);
            Refresh();
            return addAmount;
        }

        // 다른 아이템을 들고있다면 추가 불가
        if (HeldItem.ItemData != itemData) return 0;

        // 같은 아이템을 들고있다면 병합
        int space = itemData.maxStack - HeldItem.Quantity;
        int added = Mathf.Min(space, amount);
        HeldItem.Quantity += added;
        Refresh();
        return added;
    }

    // 홀드슬롯에서 아이템 제거
    public void RemoveItem(int amount)
    {
        if (!IsHolding) return;

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
    
    // 홀드슬롯 초기화
    public void SetItem(ItemInstanceData data, SlotBase origin)
    {
        HeldItem = data;
        OriginSlot = origin;
        Refresh();
    }
    
    // 홀드슬롯 비우기
    public void Clear()
    {
        HeldItem = null;
        OriginSlot = null;
        Refresh();
    }

    
    
    // 홀드 취소
    public void ReturnToOrigin()
    {
        if (!IsHolding || OriginSlot == null) return;

        InventoryManager.Instance.TryAddItem(OriginSlot.SlotIndex, HeldItem.ItemData, HeldItem.Quantity);
        OriginSlot.Refresh();
        Clear();
    }
    
    
    // 장비타입 확인
    public bool IsHeldItemEquip()
    {
        return IsHolding && HeldItem.ItemData.useType == UseType.Equip;
    }
    public EquipType? GetHeldEquipType()
    {
        return IsHeldItemEquip() ? (EquipType?)HeldItem.ItemData.equipType : null;
    }
    
    
    // 아이템 갱신
    public void Refresh()
    {
        if (holdSlot == null) return;

        if (!IsHolding)
        {
            holdSlot.gameObject.SetActive(false);
        }
        else
        {
            holdSlot.SetItem(HeldItem);
            holdSlot.gameObject.SetActive(true);
        }
    }
}
