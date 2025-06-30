using UnityEngine;
using UnityEngine.UI;

public class DragManager : SceneOnlySingleton<DragManager>
{
    [SerializeField] private Image dragImage;
    
    public SlotBase DraggedSlot { get; private set; }
    public ItemInstanceData DraggedItem { get; private set; }

    public bool IsDragging => DraggedItem != null;
    
    protected override void Awake()
    {
        base.Awake();
        dragImage.gameObject.SetActive(false);
    }
    
    public void StartDrag(SlotBase slot)
    {
        if (slot == null || !slot.HasItem())
            return;

        DraggedSlot = slot;
        DraggedItem = slot.GetData();

        dragImage.sprite = DraggedItem.Icon;
        dragImage.enabled = true;
        dragImage.transform.SetAsLastSibling();
        dragImage.gameObject.SetActive(true);
    }

    public void UpdateDrag(Vector2 pos)
    {
        if (dragImage != null)
            dragImage.transform.position = pos;
    }

    public void EndDrag()
    {
        DraggedSlot = null;
        DraggedItem = null;
        if (dragImage != null)
            dragImage.gameObject.SetActive(false);
    }
}
