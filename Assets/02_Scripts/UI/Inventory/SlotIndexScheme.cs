public static class SlotIndexScheme
{
    public const int SlotBlockSize = 100;

    public const int ChestStartOffset = 0;
    public const int ChestSlotCount = 12;

    public const int CookInputStartOffset = 50;
    public const int CookInputSlotCount = 3;

    public const int CookResultOffset = 53;

    public static int GetChestStart(int index)
        => index * SlotBlockSize + ChestStartOffset;

    public static int GetCookInputStart(int index)
        => index * SlotBlockSize + CookInputStartOffset;

    public static int GetCookResultIndex(int index)
        => index * SlotBlockSize + CookResultOffset;
}