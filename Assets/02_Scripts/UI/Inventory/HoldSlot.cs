using UnityEngine;
using UnityEngine.EventSystems;

public class HoldSlot : SlotBase
{
    private ItemInstanceData currentItem;
    public override ItemInstanceData GetData() => currentItem;
    public bool IsEmpty => currentItem == null || !currentItem.IsValid;

    // 초기화
    public void SetItem(ItemInstanceData _item)
    {
        currentItem = _item;
        Refresh();
    }
    
    // 위치설정
    public void SetPosition(Vector2 _localPos)
    {
        (transform as RectTransform).localPosition = _localPos;
    }

    public void Clear()
    {
        currentItem = null;
        Refresh();
    }

    // 갱신
    public override void Refresh()
    {
        if (IsEmpty)
        {
            iconImage.enabled = false;
            quantityText.enabled = false;
            iconImage.sprite = null;
            quantityText.text = "";
        }
        else
        {
            iconImage.enabled = true;
            quantityText.enabled = true;
            iconImage.sprite = currentItem.ItemData.icon;
            quantityText.text = currentItem.ItemData.maxStack > 1 ? currentItem.Quantity.ToString() : "";
        }
    }
    
    public override void OnPointerClick(PointerEventData eventData)
    {
        //
    }
}