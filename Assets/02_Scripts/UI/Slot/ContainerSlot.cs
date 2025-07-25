using UnityEngine;

// 인벤토리 및 창고
public class ContainerSlot : SlotBase
{
    protected override SlotPermissionFlags permissions => SlotPermissionFlags.All;
    protected override ItemType allowedItemTypes => ItemType.None;

}