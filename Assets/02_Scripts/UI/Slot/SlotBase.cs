using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class SlotBase : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 구성요소")]
    [SerializeField] protected Image iconImage;
    [SerializeField] protected Image outlineImage;
    [SerializeField] protected TextMeshProUGUI quantityText;

    protected virtual SlotPermissionFlags permissions => SlotPermissionFlags.All;
    protected virtual ItemType allowedItemTypes => ItemType.None;
    
    public int SlotIndex { get; protected set; }
    protected ISlotContainer container;
    public ISlotContainer Container => container;

    // 슬롯 초기화
    public virtual void Initialize(int index, ISlotContainer container)
    {
        SlotIndex = index;
        this.container = container;
        Refresh();
    }
    
    // 아이템 데이터 반환
    public virtual ItemInstanceData GetData()
    {
        return container?.GetItem(SlotIndex);
    }
    
    // 아이템 존재여부 확인
    public bool HasItem()
    {
        var data = GetData();
        return data != null && data.IsValid;
    }
    
    // 슬롯 UI갱신
    public virtual void Refresh()
    {
        var data = GetData();
        if (data != null && data.IsValid)
        {
            iconImage.sprite = data.ItemData.icon;
            iconImage.enabled = true;
            quantityText.text = data.Quantity > 1 ? data.Quantity.ToString() : "";
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            quantityText.text = "";
        }
    }
    
    // 해당 슬롯에 들어올 수 있는 아이템인지 반환
    public bool IsItemAllowed(ItemSO item)
    {
        if (item == null) return false;
        if (allowedItemTypes == ItemType.None) return true; // None이면 제한 없음
        return (item.itemTypes & allowedItemTypes) != 0;
    }
    
    // 허용되는 상호작용인지 반환
    public bool Allows(SlotPermissionFlags flag)
    {
        return (permissions & flag) != 0;
    }
    
    // 아이템 툴팁 활성화
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        var data = GetData();
        if (data != null && data.IsValid)
        {
            //TooltipManager.Instance?.Show(data); // 아이템 정보 표시
        }
    }

    // 아이템 툴팁 숨김 비활성화
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        //TooltipManager.Instance?.Hide(); 
    }
    
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (permissions.HasFlag(SlotPermissionFlags.CanClick))
        {
            SlotInteractionHandler.Instance?.HandleClick(this, eventData);
        }
    }

    public virtual void OnSlotSelectedChanged(bool isSelected)
    {
        if (outlineImage != null)
        {
            outlineImage.gameObject.SetActive(isSelected);
            outlineImage.color = isSelected ? new Color32(242, 109, 91, 255) : Color.white;
        }
    }
    
}