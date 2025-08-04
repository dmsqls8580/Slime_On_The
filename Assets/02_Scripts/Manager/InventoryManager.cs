using _02_Scripts.Manager;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : SceneOnlySingleton<InventoryManager>
{
    [SerializeField] private Craft craft;
    public PlaceMode placeMode;
    private UICookPot uiCookPot;

    public const int MaxSlotCount = 100000;
    private const int EquipSlotStartIndex = 90;
    public const int EquipSlotCount = 6;
    [SerializeField] private int unlockedSlotCount = 40;

    private const int MaxChestCount = 999;
    private const int MaxCookPotCount = 999;
    private bool[] chestSlotInUse = new bool[MaxChestCount];
    private bool[] cookPotSlotInUse = new bool[MaxCookPotCount];

    public event Action<int> OnSlotChanged;
    public event Action<int> OnEquipSlotChanged;

    private ItemInstanceData[] inventorySlots = new ItemInstanceData[MaxSlotCount];
    //private ItemInstanceData[] equipSlots = new ItemInstanceData[EquipSlotCount];

    private bool IsValidIndex(int _index) => _index >= 0 && _index < unlockedSlotCount;
    private bool IsEquipIndex(int _index) => _index >= 0 && _index < EquipSlotCount;
    private bool IsSlotUnlocked(int _index) => _index < unlockedSlotCount;

    //public ItemInstanceData GetEquippedItem(EquipType _type) => equipSlots[(int)_type];


    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        uiCookPot = UIManager.Instance.GetUIComponent<UICookPot>();
    }

    public ItemInstanceData GetItem(int _index)
    {
        if (_index < 0 || _index >= MaxSlotCount) return null;
        return inventorySlots[_index];
    }

    public void SetItem(int _index, ItemInstanceData _newData)
    {
        if (_index < 0 || _index >= MaxSlotCount) return;

        if (_index >= EquipSlotStartIndex && _index < EquipSlotStartIndex + EquipSlotCount)
        {
            RefreshEquipStat(_index, _newData);
        }

        inventorySlots[_index] = _newData;
        OnSlotChanged?.Invoke(_index);
    }


    public void ClearItem(int _index)
    {
        if (_index < 0 || _index >= MaxSlotCount) return;

        if (_index >= EquipSlotStartIndex && _index < EquipSlotStartIndex + EquipSlotCount)
        {
            RefreshEquipStat(_index, null);
        }

        inventorySlots[_index] = null;
        OnSlotChanged?.Invoke(_index);
    }

    // 한 슬롯에 아이템 추가시도(추가한 양 반환)
    public int TryAddItem(int _index, ItemInstanceData _item, int _amount)
    {
        if (_index < 0 || _index >= MaxSlotCount || _item == null || _amount <= 0)
            return 0;

        var current = inventorySlots[_index];

        if (current == null)
        {
            if (_index >= EquipSlotStartIndex && _index < EquipSlotStartIndex + EquipSlotCount)
            {
                RefreshEquipStat(_index, _item);
            }

            int placed = Mathf.Min(_amount, _item.ItemData.maxStack);
            inventorySlots[_index] = new ItemInstanceData(_item.ItemData, placed);
            OnSlotChanged?.Invoke(_index);
            return placed;
        }

        if (current.ItemData != _item.ItemData || current.Quantity >= _item.ItemData.maxStack)
            return 0;

        int addable = _item.ItemData.maxStack - current.Quantity;
        int placedAmount = Mathf.Min(addable, _amount);
        current.Quantity += placedAmount;

        uiCookPot.IgnoreNextSlotChange();
        OnSlotChanged?.Invoke(_index);
        return placedAmount;
    }

    // 한 슬롯에서 아이템 제거시도
    public void RemoveItem(int _index, int _amount)
    {
        if (_index >= EquipSlotStartIndex && _index < EquipSlotStartIndex + EquipSlotCount)
        {
            RefreshEquipStat(_index, null);
        }

        Debug.Log($"[RemoveItem] index: {_index}, amount: {_amount}");
        if (_index < 0 || _index >= MaxSlotCount || _amount <= 0)
        {
            Debug.LogWarning("Invalid index or amount.");
            return;
        }

        var current = inventorySlots[_index];
        if (current == null || !current.IsValid)
        {
            Debug.LogWarning("Invalid item data.");
            return;
        }

        current.Quantity -= _amount;
        if (current.Quantity <= 0)
        {
            inventorySlots[_index] = null;
        }

        OnSlotChanged?.Invoke(_index);
    }

    // 아이템 획득시도
    public bool TryAddItemGlobally(ItemSO _itemData, int _amount)
    {
        if (_itemData == null || _amount <= 0) return false;
        // 같은 아이템 있는 곳에 병합
        for (int i = 0; i < unlockedSlotCount; i++)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.ItemData == _itemData && slot.Quantity < _itemData.maxStack)
            {
                int addable = _itemData.maxStack - slot.Quantity;
                int placed = Mathf.Min(addable, _amount);
                slot.Quantity += placed;
                _amount -= placed;
                OnSlotChanged?.Invoke(i);

                if (_amount <= 0)
                {
                    if (craft.CraftingSlot != null)
                    {
                        UpdateCraftingUI();
                    }

                    return true;
                }
            }
        }

        // 빈 칸에 새로 넣기
        for (int i = 0; i < unlockedSlotCount; i++)
        {
            if (inventorySlots[i] == null)
            {
                int placed = Mathf.Min(_itemData.maxStack, _amount);
                inventorySlots[i] = new ItemInstanceData(_itemData, placed);
                _amount -= placed;
                OnSlotChanged?.Invoke(i);

                if (_amount <= 0)
                {
                    if (craft.CraftingSlot != null)
                    {
                        UpdateCraftingUI();
                    }

                    return true;
                }
            }
        }

        // TODO: 인벤토리에 활성화된 빈 칸 없으면 버리기 기능 추가

        return false;
    }

    // 아이템 소거시도
    public bool TryRemoveItemGlobally(ItemSO _itemData, int _amount)
    {
        if (!CanRemoveItem(_itemData, _amount)) return false;

        // 높은 인덱스에서부터 아이템 소거
        for (int i = unlockedSlotCount - 1; i >= 0 && _amount > 0; i--)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.ItemData == _itemData)
            {
                int removeAmount = Mathf.Min(slot.Quantity, _amount);
                slot.Quantity -= removeAmount;
                _amount -= removeAmount;

                if (slot.Quantity <= 0)
                    inventorySlots[i] = null;

                OnSlotChanged?.Invoke(i);
            }
        }

        return true;
    }

    public void UpdateCraftingUI()
    {
        // 설명 패널 업데이트.
        craft.CraftingSlot.UpdateRequiredIngredientPanel();
        // 제작 버튼 업데이트.
        craft.CanCraft();
    }

    // 해당 아이템이 인벤토리에 amount 이상 존재하는지 확인
    public bool CanRemoveItem(ItemSO _itemData, int _amount)
    {
        if (_itemData == null || _amount <= 0) return false;

        int total = 0;
        for (int i = 0; i < unlockedSlotCount; i++)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.ItemData == _itemData)
            {
                total += slot.Quantity;
            }
        }

        return total >= _amount;
    }

    // 해당 아이템이 몇 개 인지 반환.
    public int CountItem(ItemSO _itemData)
    {
        int count = 0;
        for (int i = 0; i < unlockedSlotCount; i++)
        {
            var slot = inventorySlots[i];
            if (slot != null && slot.ItemData == _itemData)
                count += slot.Quantity;
        }

        return count;
    }

    public ItemInstanceData GetEquipItem(int _index) => GetItem(EquipSlotStartIndex + _index);
    public void SetEquipItem(int _index, ItemInstanceData _data) => SetItem(EquipSlotStartIndex + _index, _data);
    public void ClearEquipItem(int _index) => ClearItem(EquipSlotStartIndex + _index);


    public ItemInstanceData[] GetAllEquippedItems()
    {
        ItemInstanceData[] result = new ItemInstanceData[EquipSlotCount];
        for (int i = 0; i < EquipSlotCount; i++)
        {
            result[i] = GetItem(EquipSlotStartIndex + i);
        }

        return result;
    }

    // 잠긴 슬롯 수 조절
    public void SetUnlockedSlotCount(int _count)
    {
        unlockedSlotCount = Mathf.Clamp(_count, 0, MaxSlotCount);
    }

    // 슬롯 잠김 여부 확인
    public bool IsSlotLocked(int _index)
    {
        return _index >= unlockedSlotCount;
    }

    private void RefreshEquipStat(int _index, ItemInstanceData _newData)
    {
        var prevItem = GetItem(_index);
        if (!prevItem.IsUnityNull() && prevItem.IsValid)
        {
            PlayerStatusManager.Instance.ApplyEquipStat(prevItem, false);
        }

        inventorySlots[_index] = _newData;
        if (!_newData.IsUnityNull() && _newData.IsValid)
        {
            PlayerStatusManager.Instance.ApplyEquipStat(_newData, true);
        }
        //OnSlotChanged?.Invoke(_index);
    }

    // chest 설치
    public int GetNextAvailableChestIndex()
    {
        for (int i = 0; i < MaxChestCount; i++)
        {
            if (!chestSlotInUse[i])
            {
                chestSlotInUse[i] = true;
                return i;
            }
        }

        return -1;
    }

    // chest 파괴
    public void ReleaseChestIndex(int index)
    {
        if (index >= 0 && index < chestSlotInUse.Length)
        {
            chestSlotInUse[index] = false;
        }
    }

    // CookPot 설치
    public int GetNextAvailableCookIndex()
    {
        for (int i = 0; i < MaxCookPotCount; i++)
        {
            if (!cookPotSlotInUse[i])
            {
                cookPotSlotInUse[i] = true;
                return i;
            }
        }

        return -1;
    }

    // CookPot 파괴
    public void ReleaseCookIndex(int index)
    {
        if (index >= 0 && index < MaxCookPotCount)
            cookPotSlotInUse[index] = false;
    }
}