using UnityEngine;

public class CookResultSlot : SlotBase
{
    protected override SlotPermissionFlags permissions => SlotPermissionFlags.CanClick | SlotPermissionFlags.CanPickUp;

    protected override ItemType allowedItemTypes => ItemType.Eatable | ItemType.Cookable | ItemType.Cooked;
}
