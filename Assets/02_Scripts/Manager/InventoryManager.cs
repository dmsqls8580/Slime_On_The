using System;
using UnityEngine;

public class InventoryManager : SceneOnlySingleton<InventoryManager>
{
    public const int MaxSlotCount = 40;
    public const int EquipSlotCount = 6;
    [SerializeField] private int unlockedSlotCount = 20;
    
    public event Action<int> OnSlotChanged;
    public event Action<int> OnEquipSlotChanged;

    private ItemInstanceData[] inventorySlots = new ItemInstanceData[MaxSlotCount];
    private ItemInstanceData[] equipSlots = new ItemInstanceData[EquipSlotCount];
    
    
    // 슬롯 유효성 확인
    private bool IsValidIndex(int index) => index >= 0 && index < unlockedSlotCount;
    private bool IsEquipIndex(int index) => index >= 0 && index < EquipSlotCount;
    private bool IsSlotUnlocked(int index) => index < unlockedSlotCount;

    //public ItemInstanceData GetEquippedItem(EquipType type) => equipSlots[(int)type];
    
    
    public ItemInstanceData GetItem(int index)
    {
        if (index < 0 || index >= MaxSlotCount) return null;
        return inventorySlots[index];
    }

    public void SetItem(int index, ItemInstanceData newData)
    {
        if (index < 0 || index >= MaxSlotCount) return;
        inventorySlots[index] = newData;
        OnSlotChanged?.Invoke(index);
    }

    public void ClearItem(int index)
    {
        if (index < 0 || index >= MaxSlotCount) return;
        inventorySlots[index] = null;
        OnSlotChanged?.Invoke(index);
    }
    
    
    // 한 슬롯에 아이템 추가시도(추가한 양 반환)
    public int TryAddItem(int index, TempItemSO itemData, int amount)
    {
        if (index < 0 || index >= MaxSlotCount || itemData == null || amount <= 0)
            return 0;

        var current = inventorySlots[index];

        if (current == null)
        {
            int placed = Mathf.Min(amount, itemData.maxStack);
            inventorySlots[index] = new ItemInstanceData(itemData, placed);
            OnSlotChanged?.Invoke(index);
            return placed;
        }

        if (current.ItemData != itemData || current.Quantity >= itemData.maxStack)
            return 0;

        int addable = itemData.maxStack - current.Quantity;
        int placedAmount = Mathf.Min(addable, amount);
        current.Quantity += placedAmount;
        OnSlotChanged?.Invoke(index);
        return placedAmount;
    }
    
    // 한 슬롯에서 아이템 제거시도
    public void RemoveItem(int index, int amount)
    {
        if (index < 0 || index >= MaxSlotCount || amount <= 0) return;

        var current = inventorySlots[index];
        if (current == null || !current.IsValid) return;

        current.Quantity -= amount;
        if (current.Quantity <= 0)
        {
            inventorySlots[index] = null;
        }

        OnSlotChanged?.Invoke(index);
    }
    
    
    // 아이템 획득시도
    public bool TryAddItemGlobally(TempItemSO itemData, int amount)
    {
        if (itemData == null || amount <= 0) return false;

        // 같은 아이템 있는 곳에 병합
        for (int i = 0; i < unlockedSlotCount; i++)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.ItemData == itemData && slot.Quantity < itemData.maxStack)
            {
                int addable = itemData.maxStack - slot.Quantity;
                int placed = Mathf.Min(addable, amount);
                slot.Quantity += placed;
                amount -= placed;
                OnSlotChanged?.Invoke(i);

                if (amount <= 0) return true;
            }
        }
        // 빈 칸에 새로 넣기
        for (int i = 0; i < unlockedSlotCount; i++)
        {
            if (inventorySlots[i] == null)
            {
                int placed = Mathf.Min(itemData.maxStack, amount);
                inventorySlots[i] = new ItemInstanceData(itemData, placed);
                amount -= placed;
                OnSlotChanged?.Invoke(i);

                if (amount <= 0) return true;
            }
        }
        
        // TODO: 인벤토리에 활성화된 빈 칸 없으면 버리기 기능 추가

        return false;
    }

    // 아이템 소거시도
    public bool TryRemoveItemGlobally(TempItemSO itemData, int amount)
    {
        if (!CanRemoveItem(itemData, amount)) return false;

        // 높은 인덱스에서부터 아이템 소거
        for (int i = unlockedSlotCount - 1; i >= 0 && amount > 0; i--)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.ItemData == itemData)
            {
                int removeAmount = Mathf.Min(slot.Quantity, amount);
                slot.Quantity -= removeAmount;
                amount -= removeAmount;

                if (slot.Quantity <= 0)
                    inventorySlots[i] = null;

                OnSlotChanged?.Invoke(i);
            }
        }

        return true;
    }
    
    // 해당 아이템이 인벤토리에 amount 이상 존재하는지 확인
    public bool CanRemoveItem(TempItemSO itemData, int amount)
    {
        if (itemData == null || amount <= 0) return false;

        int total = 0;
        for (int i = 0; i < unlockedSlotCount; i++)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.ItemData == itemData)
            {
                total += slot.Quantity;
            }
        }

        return total >= amount;
    }
    
    
    public ItemInstanceData GetEquipItem(int index)
    {
        if (index < 0 || index >= EquipSlotCount) return null;
        return equipSlots[index];
    }

    public void SetEquipItem(int index, ItemInstanceData newData)
    {
        if (index < 0 || index >= EquipSlotCount) return;
        equipSlots[index] = newData;
        OnSlotChanged?.Invoke(index);
    }

    public void ClearEquipItem(int index)
    {
        if (index < 0 || index >= EquipSlotCount) return;
        equipSlots[index] = null;
        OnSlotChanged?.Invoke(index);
    }

    public ItemInstanceData[] GetAllEquippedItems()
    {
        return equipSlots;
    }
    
    
    // 잠긴 슬롯 수 조절
    public void SetUnlockedSlotCount(int count)
    {
        unlockedSlotCount = Mathf.Clamp(count, 0, MaxSlotCount);
    }
    
    // 슬롯 잠김 여부 확인
    public bool IsSlotLocked(int index)
    {
        return index >= unlockedSlotCount;
    }
}
