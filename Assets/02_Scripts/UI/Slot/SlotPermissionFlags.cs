using System;

[System.Flags]
public enum SlotPermissionFlags
{
    None        = 0,
    CanClick    = 1 << 0,
    CanPickUp   = 1 << 1,
    CanPlace    = 1 << 2,
    CanSwap     = 1 << 3,
    CanUse      = 1 << 4,

    All         = ~0
}