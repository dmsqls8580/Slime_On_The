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
        outLineImage.color = _isSelected ? Color.yellow : Color.white;

        if (_isSelected)
        {
            if (placeMode.gameObject.activeSelf == true)
                placeMode.SetActiveFalsePlaceMode();

            var data = GetData();
            if (!data.IsUnityNull())
            {
                if (data.ItemData.itemTypes == ItemType.Placeable)
                    placeMode.SetActiveTruePlaceMode(data.ItemData);
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

            InventoryInteractionHandler.Instance.TryUse(data, this);
        }
    }

    public void EquipToolToController(ToolController _controller)
    {   
        var data = GetData();
        if (data.IsUnityNull()) {
            return ;
        }
        if (data.ItemData.IsUnityNull()) {
            return ;
        }

        if ((data.ItemData.itemTypes & ItemType.Tool) != 0)
        {
            _controller.EquipTool(data.ItemData as ITool);
        }
        else
        {
            _controller.EquipTool(null);
        }
    }

    public ToolType GetToolType()
    {
        var data = GetData();
        if(data.IsUnityNull()||data.ItemData.IsUnityNull()) 
            return ToolType.None;
        if(data.ItemData.itemTypes!=ItemType.Tool)
            return ToolType.None;
        
        return data.ItemData.toolData.toolType;
        
    }
}