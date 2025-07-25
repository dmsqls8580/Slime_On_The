using UnityEngine;

public class HoldSlot : SlotBase
{
    protected override SlotPermissionFlags permissions => SlotPermissionFlags.None;

    public void SetPosition(Vector2 _localPos)
    {
        (transform as RectTransform).localPosition = _localPos;
    }
}