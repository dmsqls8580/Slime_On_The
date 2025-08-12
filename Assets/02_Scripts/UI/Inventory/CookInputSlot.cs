using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CookInputSlot : InventorySlot
{
    public override bool IsItemAllowed(ItemInstanceData data)
    {
        if (data == null || !data.IsValid) return false;
        return (data.ItemData.itemTypes & ItemType.Cookable) != 0;
    }

    public override void Refresh()
    {
        base.Refresh();
        
    }
}
