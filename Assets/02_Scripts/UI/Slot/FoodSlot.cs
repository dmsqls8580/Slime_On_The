using UnityEngine;

public class FoodSlot : SlotBase
{
    protected override ItemType allowedItemTypes => ItemType.Eatable | ItemType.Cookable | ItemType.Cooked;
}