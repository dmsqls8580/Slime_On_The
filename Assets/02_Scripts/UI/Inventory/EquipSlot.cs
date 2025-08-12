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
}