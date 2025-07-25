using System.Collections.Generic;
using UnityEngine;

public class ContainerManager : SceneOnlySingleton<ContainerManager>
{
    private readonly List<ISlotContainer> openedContainers = new();

    // 컨테이너가 열릴때 등록
    public void RegisterContainer(ISlotContainer container)
    {
        if (container == null || openedContainers.Contains(container)) return;
        openedContainers.Add(container);
    }

    // 컨테이너가 닫힐때 등록 해제
    public void UnregisterContainer(ISlotContainer container)
    {
        if (container == null) return;
        openedContainers.Remove(container);
    }
    
    // 등록된 모든 컨테이너 반환
    private List<ISlotContainer> GetAllContainers()
    {
        List<ISlotContainer> result = new();

        result.Add(PlayerInventory.Instance);
        result.Add(EquipContainer.Instance);
        result.Add(HoldManager.Instance);

        foreach (var c in openedContainers)
        {
            // 중복 방지
            if (!ReferenceEquals(c, PlayerInventory.Instance)
                && !ReferenceEquals(c, EquipContainer.Instance)
                && !ReferenceEquals(c, HoldManager.Instance))
            {
                result.Add(c);
            }
        }

        return result;
    }
    
    // 아이템을 PlayerInventory에 추가 시도 (병합 우선 -> 빈칸 삽입)
    public bool TryAddItemGlobally(ItemSO item, int amount)
    {
        if (item == null || amount <= 0) return false;

        var inventory = PlayerInventory.Instance;

        // 병합
        for (int i = 0; i < inventory.SlotCount; i++)
        {
            var data = inventory.GetItem(i);
            if (data != null && data.ItemData == item && data.Quantity < item.maxStack)
            {
                int addable = item.maxStack - data.Quantity;
                int placed = Mathf.Min(addable, amount);
                data.Quantity += placed;
                inventory.SetItem(i, data);
                amount -= placed;
                if (amount <= 0) return true;
            }
        }

        // 빈 칸 삽입
        for (int i = 0; i < inventory.SlotCount; i++)
        {
            if (inventory.GetItem(i) == null)
            {
                int placed = Mathf.Min(item.maxStack, amount);
                inventory.SetItem(i, new ItemInstanceData(item, placed));
                amount -= placed;
                if (amount <= 0) return true;
            }
        }

        // HoldSlot에 추가 시도
        var hold = HoldManager.Instance;
        if (!hold.IsHolding)
        {
            hold.SetItem(0, new ItemInstanceData(item, amount));
            return true;
        }

        // 남는거 Drop 처리
        // DropItem(item, amount);

        return false;
    }
    
    // 열린 컨테이너 전체에서 해당 아이템이 충분히 있는지 확인
    public bool CanRemoveItem(ItemSO item, int amount)
    {
        int total = 0;
        foreach (var container in GetAllContainers())
        {
            for (int i = 0; i < container.SlotCount; i++)
            {
                var data = container.GetItem(i);
                if (data != null && data.ItemData == item)
                    total += data.Quantity;
            }
        }
        return total >= amount;
    }

    // 열린 컨테이너 전체에서 해당 아이템 개수 합산
    public int CountItem(ItemSO item)
    {
        int total = 0;
        foreach (var container in GetAllContainers())
        {
            for (int i = 0; i < container.SlotCount; i++)
            {
                var data = container.GetItem(i);
                if (data != null && data.ItemData == item)
                    total += data.Quantity;
            }
        }
        return total;
    }
    
    // 열린 컨테이너 전체에서 아이템을 제거 시도
    public bool TryRemoveItemGlobally(ItemSO item, int amount)
    {
        if (!CanRemoveItem(item, amount)) return false;

        // HoldManager에서 최우선 제거시도
        var hold = HoldManager.Instance;
        var heldData = hold.GetItem(0);
        if (heldData != null && heldData.ItemData == item)
        {
            int heldQuantity = heldData.Quantity;
            int take = Mathf.Min(heldQuantity, amount);
            hold.RemoveItem(0, take);
            amount -= take;
            if (amount <= 0) return true;
        }
        
        // 2. 열린 컨테이너에서 제거시도
        foreach (var container in openedContainers)
        {
            if (container == null) continue;
            for (int i = container.SlotCount - 1; i >= 0 && amount > 0; i--)
            {
                var data = container.GetItem(i);
                if (data != null && data.ItemData == item)
                {
                    int take = Mathf.Min(data.Quantity, amount);
                    container.RemoveItem(i, take);
                    amount -= take;
                    if (amount <= 0) return true;
                }
            }
        }

        // 인벤토리에서 제거시도
        var inventory = PlayerInventory.Instance;
        for (int i = inventory.SlotCount - 1; i >= 0 && amount > 0; i--)
        {
            var data = inventory.GetItem(i);
            if (data != null && data.ItemData == item)
            {
                int take = Mathf.Min(data.Quantity, amount);
                inventory.RemoveItem(i, take);
                amount -= take;
                if (amount <= 0) return true;
            }
        }
        
        // 장착칸에서 제거시도
        var equip = EquipContainer.Instance;
        for (int i = equip.SlotCount - 1; i >= 0 && amount > 0; i--)
        {
            var data = equip.GetItem(i);
            if (data != null && data.ItemData == item)
            {
                int take = Mathf.Min(data.Quantity, amount);
                equip.RemoveItem(i, take);
                amount -= take;
                if (amount <= 0) return true;
            }
        }

        return true;
    }
}
