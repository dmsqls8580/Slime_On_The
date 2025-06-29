using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryManager : SceneOnlySingleton<InventoryManager>
{
    // 사용가능한 인벤토리의 최대치는 40으로 고정되어있으니 const로 명시해 초기화 
    private const int slotCount = 40;
    private ItemInstanceData[] inventoryData = new ItemInstanceData[slotCount];

    public event UnityAction<int> OnSlotChanged;
    
    public int SlotCount => slotCount;
    
    //테스트용
    public void SetAll(ItemInstanceData[] data)
    {
        int copyCount = Mathf.Min(slotCount, data.Length);
        Array.Copy(data, inventoryData, copyCount);

        for (int i = 0; i < copyCount; i++)
        {
            OnSlotChanged?.Invoke(i);
        }
    }
    
    
    public ItemInstanceData GetItem(int idx)
    {
        if (idx < 0 || idx >= slotCount) return null;
        return inventoryData[idx];
    }

    public void TrySwapOrMerge(int fromIdx, int toIdx)
    {
        if (fromIdx == toIdx) return;

        var from = inventoryData[fromIdx];
        var to = inventoryData[toIdx];

        // 둘 다 비었으면 무시
        if (from == null && to == null) return;

        // 한쪽만 비었으면 Swap
        if (from == null || to == null)
        {
            Swap(fromIdx, toIdx);
            return;
        }

        // 아이디 다르면 Swap
        if (from.ItemSO != to.ItemSO)
        {
            Swap(fromIdx, toIdx);
            return;
        }

        // 아이디 같으면 병합 시도
        int total = from.Quantity + to.Quantity;
        int max = from.ItemSO.MaxStack;

        if (total <= max)
        {
            to.Quantity = total;
            inventoryData[fromIdx] = null;
        }
        else
        {
            to.Quantity = max;
            from.Quantity = total - max;
        }

        OnSlotChanged?.Invoke(fromIdx);
        OnSlotChanged?.Invoke(toIdx);
    }

    public void Swap(int a, int b)
    {
        var temp = inventoryData[a];
        inventoryData[a] = inventoryData[b];
        inventoryData[b] = temp;

        OnSlotChanged?.Invoke(a);
        OnSlotChanged?.Invoke(b);
    }
}
