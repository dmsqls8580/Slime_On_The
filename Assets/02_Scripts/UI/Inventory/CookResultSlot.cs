using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookResultSlot : InventorySlot
{
    public override bool IsItemAllowed(ItemInstanceData data)
    {
        return false;
    }
}
