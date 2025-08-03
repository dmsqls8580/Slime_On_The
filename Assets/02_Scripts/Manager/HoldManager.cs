using _02_Scripts.Manager;
using PlayerStates;
using Unity.VisualScripting;
using UnityEngine;

public class HoldManager : SceneOnlySingleton<HoldManager>
{
    [SerializeField] private HoldSlot holdSlot;
    [SerializeField] private Canvas holdCanvas;
    
    public ItemInstanceData HeldItem { get; private set; }
    public SlotBase OriginSlot { get; set; }
    
    [Header("Drop Settings")]
    [SerializeField] protected GameObject dropItemPrefab;
    [SerializeField] protected float dropUpForce = 5f;
    [SerializeField] protected float dropSideForce = 2f;
    [SerializeField] protected float dropAngleRange = 60f;
    
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
    public int TryAddItem(ItemSO _itemData, int _amount)
    {
        if (_itemData == null || _amount <= 0)
            return 0;

        if (!IsHolding)
        {
            int addAmount = Mathf.Clamp(_amount, 1, _itemData.maxStack);
            HeldItem = new ItemInstanceData(_itemData, addAmount);
            Refresh();
            return addAmount;
        }

        if (HeldItem.ItemData != _itemData) return 0;

        int space = _itemData.maxStack - HeldItem.Quantity;
        int added = Mathf.Min(space, _amount);
        HeldItem.Quantity += added;
        Refresh();
        return added;
    }

    // 홀드슬롯에서 아이템 제거
    public void RemoveItem(int _amount)
    {
        Logger.Log("홀드슬롯에서 아이템 제거함");
        if (!IsHolding) return;

        HeldItem.Quantity -= _amount;
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
    public void SetItem(ItemInstanceData _data, SlotBase _origin)
    {
        HeldItem = _data;
        OriginSlot = _origin;
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

        InventoryManager.Instance.TryAddItem(OriginSlot.SlotIndex, HeldItem, HeldItem.Quantity);
        OriginSlot.Refresh();
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
    
    // 아이템 드랍
    public void DropHeldItem()
    {
        if (!IsHolding) return;
        
        ItemInstanceData itemToDrop = HeldItem;
        
        Vector3 dropPosition = FindObjectOfType<PlayerController>().transform.position;

        if (dropItemPrefab.IsUnityNull())
        {
            return;
        }

        var dropObj = Instantiate(dropItemPrefab, dropPosition, Quaternion.identity);
        var itemDrop = dropObj.GetComponent<ItemDrop>();

        if (itemDrop != null)
        {
            itemDrop.Init(itemToDrop.ItemData, itemToDrop.Quantity);
            var rigid = dropObj.GetComponent<Rigidbody2D>();
            itemDrop.DropAnimation(rigid, dropAngleRange, dropUpForce, dropSideForce);
        }

        Clear();
    }
}