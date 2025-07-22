using PlayerStates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquipSlot : SlotBase
{
    [SerializeField] private EquipType equipType;

    public EquipType EquipType => equipType;
    
    public bool IsCorrectEquipType(EquipType _type)
    {
        return equipType == _type;
    }
    
    public override ItemInstanceData GetData()
    {
        return InventoryManager.Instance.GetEquipItem(SlotIndex);
    }
    
    public void SetItem(ItemInstanceData _data)
    {
        var prevItem = InventoryManager.Instance.GetEquipItem(SlotIndex);

        if (!prevItem.IsUnityNull() && prevItem.IsValid)
        {
            ApplyEquipStat(prevItem, false);
        }
        
        InventoryManager.Instance.SetEquipItem(SlotIndex, _data);
        
        if (!_data.IsUnityNull() && _data.IsValid)
        {
            ApplyEquipStat(_data, true);
        }
        Refresh();
    }

    private void ApplyEquipStat(ItemInstanceData _item, bool _apply)
    {
        var player = FindObjectOfType<PlayerController>();
        if(player.IsUnityNull()) return;

        var statManager = player.GetComponent<StatManager>();
        if(statManager.IsUnityNull()) return;
        var equipData = _item.ItemData.equipableData;
        if(equipData.IsUnityNull()) return;

        int stack = _apply ? 1 : -1;
        statManager.ApplyStat(StatType.MaxHp,      StatModifierType.Equipment, equipData.maxHealth * stack);
        statManager.ApplyStat(StatType.Attack,     StatModifierType.Equipment, equipData.atk * stack);
        statManager.ApplyStat(StatType.Defense,    StatModifierType.Equipment, equipData.def * stack);
        statManager.ApplyStat(StatType.MoveSpeed,  StatModifierType.Equipment, equipData.spd * stack);
        
        Logger.Log($"[장비스탯 적용:{(_apply ? "장착" : "해제")}] " +
                   $"MaxHp: {statManager.GetValue(StatType.MaxHp)}, " +
                   $"Atk: {statManager.GetValue(StatType.Attack)}, " +
                   $"Def: {statManager.GetValue(StatType.Defense)}, " +
                   $"Spd: {statManager.GetValue(StatType.MoveSpeed)}");


        if (equipData.equipableType == EquipType.Core)
        {
            var formChanger = FindObjectOfType<SlimeFormChanger>();
            if (_apply)
            {
                formChanger.ChangeForm(equipData.formId);
            }
            else
                formChanger.ResetForm();
        }
    }

    public override void Clear(int _amount)
    {
        InventoryManager.Instance.ClearEquipItem(SlotIndex);
        Refresh();
    }

    public override void OnPointerClick(PointerEventData _eventData)
    {
        if (_eventData == null) return;

        bool isLeft = _eventData.button == PointerEventData.InputButton.Left;
        bool isRight = _eventData.button == PointerEventData.InputButton.Right;

        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (isLeft)
        {
            var handIcon = FindObjectOfType<ItemHandler>();
            if (!handIcon.IsUnityNull())
            {
                var data = GetData();
                handIcon.ShowItemIcon(data?.ItemData);
            }
            
            InventoryInteractionHandler.Instance.HandleLeftClick(this, isShift, isCtrl);
        }
        else if (isRight)
        {
            InventoryInteractionHandler.Instance.HandleRightClick(this, isShift, isCtrl);
        }
    }
}