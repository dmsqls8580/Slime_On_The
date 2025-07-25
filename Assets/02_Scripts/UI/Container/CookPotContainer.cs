using System;
using UnityEngine;

public class CookPotContainer : MonoBehaviour, ISlotContainer
{
    [SerializeField] private int inputSlotCount = 3;
    [SerializeField] private ItemInstanceData[] inputSlots;
    [SerializeField] private ItemInstanceData resultSlot;
    
    public int SlotCount => inputSlotCount + 1; // input + result

    public event Action<int> OnSlotChanged;
    
    private void Awake()
    {
        if (inputSlots == null || inputSlots.Length != inputSlotCount)
        {
            inputSlots = new ItemInstanceData[inputSlotCount];
        }
    }

    private void OnEnable()
    {
        ContainerManager.Instance.RegisterContainer(this);
    }

    private void OnDisable()
    {
        ContainerManager.Instance.UnregisterContainer(this);
    }
    
    // 유효한 인덱스인지 검사
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < SlotCount;
    }

    // 슬롯에서 아이템 데이터 반환
    public ItemInstanceData GetItem(int index)
    {
        if (!IsValidIndex(index)) return null;

        if (index < inputSlotCount)
        {
            return inputSlots[index];
        }
        else
        {
            return resultSlot;
        }
    }

    // 슬롯에 아이템 데이터 설정
    public void SetItem(int index, ItemInstanceData data)
    {
        if (!IsValidIndex(index)) return;

        if (index < inputSlotCount)
        {
            inputSlots[index] = data;
        }
        else
        {
            resultSlot = data;
        }
        
        OnSlotChanged?.Invoke(index);
    }

    // 슬롯에서 아이템을 amount만큼 제거
    public void RemoveItem(int index, int amount)
    {
        if (!IsValidIndex(index)) return;

        if (index < inputSlotCount)
        {
            var data = inputSlots[index];
            if (data == null || !data.IsValid) return;

            data.Quantity -= amount;
            if (data.Quantity <= 0)
                inputSlots[index] = null;
        }
        else
        {
            if (resultSlot == null || !resultSlot.IsValid) return;

            resultSlot.Quantity -= amount;
            if (resultSlot.Quantity <= 0)
                resultSlot = null;
        }

        OnSlotChanged?.Invoke(index);
    }

    // 슬롯에서 아이템 제거
    public void ClearItem(int index)
    {
        if (!IsValidIndex(index)) return;

        if (index < inputSlotCount)
            inputSlots[index] = null;
        else
            resultSlot = null;

        OnSlotChanged?.Invoke(index);
    }
}
