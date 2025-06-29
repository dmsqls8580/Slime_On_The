using UnityEngine;

public class QuickSlot : SlotBase
{
    [SerializeField] private GameObject outlineObj; // Outline 오브젝트

    public void SetSelected(bool isSelected)
    {
        outlineObj.SetActive(isSelected);
    }

    public override void OnSlotSelectedChanged(bool isSelected)
    {
        SetSelected(isSelected);
    }
}
