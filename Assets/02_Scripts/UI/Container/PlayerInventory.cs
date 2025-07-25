using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : SceneOnlySingleton<PlayerInventory>, ISlotContainer, ISortLockableContainer
{
    [SerializeField] private int slotCount = 40;
    [SerializeField] private ItemInstanceData[] slots;
    
    public int SlotCount => slots.Length;

    public event Action<int> OnSlotChanged;
    
    private bool[] sortLocked;
    private bool isSortLockMode = false;
    public bool IsSortLockMode => isSortLockMode;
    
    protected override void Awake()
    {
        base.Awake();

        if (slots == null || slots.Length != SlotCount)
        {
            slots = new ItemInstanceData[SlotCount];
        }
        sortLocked = new bool[slotCount];
    }

    // 유효한 인덱스인지 검사
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < SlotCount;
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
    
    // 인벤토리 정렬
    public void SortByItemIndex()
    {
        List<(ItemInstanceData data, int originalIndex)> unsortedItems = new();

        // 정렬되지 않은 아이템들만 추출
        for (int i = 0; i < SlotCount; i++)
        {
            if (sortLocked[i]) continue;

            var data = slots[i];
            if (data != null && data.IsValid)
                unsortedItems.Add((data, i));

            slots[i] = null;
        }

        // 인덱스 기준 정렬
        unsortedItems.Sort((a, b) => a.data.ItemData.idx.CompareTo(b.data.ItemData.idx));

        // 정렬된 아이템을 빈 슬롯에 순서대로 배치
        int writeIndex = 0;
        foreach (var (data, _) in unsortedItems)
        {
            // 정렬 잠금된 칸 건너뛰기
            while (writeIndex < SlotCount && sortLocked[writeIndex])
                writeIndex++;

            if (writeIndex >= SlotCount) break;

            slots[writeIndex] = data;
            OnSlotChanged?.Invoke(writeIndex);
            writeIndex++;
        }

        // 나머지 정렬되지 않은 슬롯들도 UI 갱신
        for (int i = 0; i < SlotCount; i++)
        {
            if (sortLocked[i]) continue;
            if (slots[i] == null)
                OnSlotChanged?.Invoke(i);
        }
    }
    
    // 정렬 잠금 모드
    public void ToggleSortLockMode()
    {
        isSortLockMode = !isSortLockMode;
    }

    public void ToggleSlotSortLock(int index)
    {
        if (!IsValidIndex(index)) return;
        if (!isSortLockMode) return;

        sortLocked[index] = !sortLocked[index];
        OnSlotChanged?.Invoke(index);
    }

    public bool IsSlotSortLocked(int index)
    {
        return IsValidIndex(index) && sortLocked[index];
    }
}
