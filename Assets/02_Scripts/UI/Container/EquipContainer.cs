using System;
using UnityEngine;

public class EquipContainer : SceneOnlySingleton<EquipContainer>, ISlotContainer
{
    private const int SlotSize = 6;
    [SerializeField] private ItemInstanceData[] slots = new ItemInstanceData[SlotSize];

    public int SlotCount => slots.Length;

    public event Action<int> OnSlotChanged;
    
    protected override void Awake()
    {
        base.Awake();

        if (slots == null || slots.Length != SlotCount)
        {
            slots = new ItemInstanceData[SlotCount];
        }
    }
    
    // 유효한 인덱스인지 검사
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < SlotSize;
    }

    // 슬롯에서 아이템 데이터 반환
    public ItemInstanceData GetItem(int index)
    {
        return IsValidIndex(index) ? slots[index] : null;
    }

    // 슬롯에 아이템 데이터 설정
    public void SetItem(int index, ItemInstanceData data)
    {
        if (!IsValidIndex(index)) return;
        slots[index] = data;
        OnSlotChanged?.Invoke(index);
    }
    
    // 슬롯에서 아이템을 amount만큼 제거
    public void RemoveItem(int index, int amount)
    {
        if (!IsValidIndex(index)) return;

        var data = slots[index];
        if (data == null || !data.IsValid) return;

        data.Quantity -= amount;
        if (data.Quantity <= 0)
            slots[index] = null;

        OnSlotChanged?.Invoke(index);
    }

    // 슬롯에서 아이템 제거
    public void ClearItem(int index)
    {
        if (!IsValidIndex(index)) return;
        slots[index] = null;
        OnSlotChanged?.Invoke(index);
    }
    
    // EquipType으로 장비 아이템 데이터 반환
    public ItemInstanceData GetItemByType(EquipType type)
    {
        return GetItem((int)type);
    }

    // EquipType으로 장비 아이템 데이터 설정
    public void SetItemByType(EquipType type, ItemInstanceData data)
    {
        SetItem((int)type, data);
    }

    // 해당 EquipType에 장착된 아이템이 있는지 여부 확인
    public bool IsEquipped(EquipType type)
    {
        var data = GetItem((int)type);
        return data != null && data.IsValid;
    }
}
