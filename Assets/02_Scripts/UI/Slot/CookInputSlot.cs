using UnityEngine;

public class CookInputSlot : SlotBase
{
    protected override ItemType allowedItemTypes => ItemType.Eatable | ItemType.Cookable | ItemType.Cooked;
}