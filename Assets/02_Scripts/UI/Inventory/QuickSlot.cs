using _02_Scripts.Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickSlot : SlotBase
{
    [SerializeField] private Image outLineImage;
    private UIQuickSlot owner;
    private int quickSlotIndex;

    private PlaceMode placeMode;

    private void Awake()
    {
        placeMode = InventoryManager.Instance.placeMode;
    }

    public void Initialize(int _index, UIQuickSlot _ownerUI)
    {
        quickSlotIndex = _index;
        owner = _ownerUI;
        base.Initialize(_index);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public override void OnSlotSelectedChanged(bool _isSelected)
    {
        outLineImage.gameObject.SetActive(_isSelected);
        outLineImage.color = _isSelected ? new Color32(242, 109, 91, 255) : Color.white;

        if (_isSelected)
        {
            if (placeMode.gameObject.activeSelf == true)
                placeMode.SetActiveFalsePlaceMode();

            var data = GetData();
            if (!data.IsUnityNull())
            {
                if (data.ItemData.itemTypes == ItemType.Placeable)
                    placeMode.SetActiveTruePlaceMode(data.ItemData, quickSlotIndex);
            }
        }
    }

    public override void OnPointerClick(PointerEventData _eventData)
    {
        if (_eventData.button == PointerEventData.InputButton.Left)
        {
            owner?.SelectSlot(quickSlotIndex);
        }
        else if (_eventData.button == PointerEventData.InputButton.Right)
        {
            var data = GetData();
            if (data == null || !data.IsValid) return;

            var uiInventory = UIManager.Instance.GetUIComponent<UIInventory>();
            if (uiInventory != null)
            {
                var realSlot = uiInventory.GetInventorySlotByIndex(quickSlotIndex);
                InventoryInteractionHandler.Instance.TryUse(data, realSlot);
            }
        }
    }

    public void EquipToolToController(ToolController _controller)
    {
        var data = GetData();
        if (data.IsUnityNull())
        {
            return;
        }

        if (data.ItemData.IsUnityNull())
        {
            return;
        }

        if ((data.ItemData.itemTypes & ItemType.Tool) != 0)
        {
            _controller.EquipTool(data.ItemData as ITool);
        }
        else
        {
            _controller.EquipTool(null);
        }
    }public override void OnSlotChanged(int _index)
    {
        base.OnSlotChanged(_index);

        // 슬롯에 변화가 생겼고, 내가 선택된 슬롯이라면 다시 툴 장착 처리
        if (_index == SlotIndex && owner.SelectedIndex == SlotIndex)
        {
            EquipToolToController(owner.ToolController); // ToolController 접근 필요
        }
    }
    public ToolType GetToolType()
    {
        var data = GetData();
        if (data.IsUnityNull() || data.ItemData.IsUnityNull())
            return ToolType.None;
        
        if ((data.ItemData.itemTypes & ItemType.Tool) == 0)
            return ToolType.None;

        return data.ItemData.toolData.toolType;
    }
}