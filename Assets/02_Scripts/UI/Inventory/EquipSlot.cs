using PlayerStates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipSlot : InventorySlot
{
    [SerializeField] private Image outLineImage;
    [SerializeField] private EquipType equipType;

    public EquipType EquipType => equipType;
    
    public override bool IsItemAllowed(ItemInstanceData data)
    {
        if (data == null || !data.IsValid) return false;
        // 임시 테스트용. 이후에는 equipableType까지 검사하는 방식으로 변경
        //return (data.ItemData.itemTypes & ItemType.Equipable) != 0;
        
        if ((data.ItemData.itemTypes & ItemType.Equipable) == 0) return false;
        return data.ItemData.equipableData.equipableType == EquipType;
    }

    public override void Initialize(int _index)
    {
        base.Initialize(_index);
        int intEquipType = _index - 90;
        equipType = (EquipType)intEquipType;
    }
    
    public override void OnSlotSelectedChanged(bool _isSelected)
    {
        if (outLineImage == null) return;

        outLineImage.gameObject.SetActive(_isSelected);
        outLineImage.color = _isSelected
            ? new Color32(242, 109, 91, 255) // 강조색
            : Color.white; // 평상시 색상 (또는 꺼진 상태)
    }
    
    // public override ItemInstanceData GetData()
    // {
    //     return InventoryManager.Instance.GetEquipItem(SlotIndex);
    // }
    //
    // public void SetItem(ItemInstanceData _data)
    // {
    //     var prevItem = InventoryManager.Instance.GetEquipItem(SlotIndex);
    //
    //     if (!prevItem.IsUnityNull() && prevItem.IsValid)
    //     {
    //         ApplyEquipStat(prevItem, false);
    //     }
    //     
    //     InventoryManager.Instance.SetEquipItem(SlotIndex, _data);
    //     
    //     if (!_data.IsUnityNull() && _data.IsValid)
    //     {
    //         ApplyEquipStat(_data, true);
    //     }
    //     Refresh();
    // }
    
    // public override void OnSlotChanged(int index)
    // {
    //     base.OnSlotChanged(index);
    //
    //     if (index != SlotIndex) return;
    //
    //     var data = GetData();
    //     var statManager = FindObjectOfType<PlayerController>()?.GetComponent<StatManager>();
    //     if (statManager == null) return;
    //
    //     // 먼저 모든 장비 스탯 제거
    //     statManager.ClearStatModifierBySource(StatModifierType.Equipment, equipType);
    //
    //     if (data != null && data.IsValid)
    //     {
    //         var equipData = data.ItemData.equipableData;
    //         statManager.ApplyStat(StatType.MaxHp,     StatModifierType.Equipment, equipData.maxHealth);
    //         statManager.ApplyStat(StatType.Attack,    StatModifierType.Equipment, equipData.atk);
    //         statManager.ApplyStat(StatType.Defense,   StatModifierType.Equipment, equipData.def);
    //         statManager.ApplyStat(StatType.MoveSpeed, StatModifierType.Equipment, equipData.spd);
    //
    //         if (equipData.equipableType == EquipType.Core)
    //         {
    //             var formChanger = FindObjectOfType<SlimeFormChanger>();
    //             formChanger?.RequestFormChange(equipData.formId);
    //         }
    //     }
    //     else
    //     {
    //         if (equipType == EquipType.Core)
    //             FindObjectOfType<SlimeFormChanger>()?.ResetForm();
    //     }
    // }
    
    
    
    

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
                formChanger.RequestFormChange(equipData.formId);
            }
            else
                formChanger.ResetForm();
        }
    }

    // public override void Clear(int _amount)
    // {
    //     InventoryManager.Instance.ClearEquipItem(SlotIndex);
    //     Refresh();
    // }
    //
    // public override void OnPointerClick(PointerEventData _eventData)
    // {
    //     if (_eventData == null) return;
    //
    //     bool isLeft = _eventData.button == PointerEventData.InputButton.Left;
    //     bool isRight = _eventData.button == PointerEventData.InputButton.Right;
    //
    //     bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    //     bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    //
    //     if (isLeft)
    //     {
    //         var handIcon = FindObjectOfType<ItemHandler>();
    //         if (!handIcon.IsUnityNull())
    //         {
    //             var data = GetData();
    //             handIcon.ShowItemIcon(data?.ItemData);
    //         }
    //         
    //         InventoryInteractionHandler.Instance.HandleLeftClick(this, isShift, isCtrl);
    //     }
    //     else if (isRight)
    //     {
    //         InventoryInteractionHandler.Instance.HandleRightClick(this, isShift, isCtrl);
    //     }
    // }
}