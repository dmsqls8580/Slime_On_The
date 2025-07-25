public interface ISlotContainer
{
    int SlotCount { get; }

    ItemInstanceData GetItem(int index);
    void SetItem(int index, ItemInstanceData data);
    void RemoveItem(int index, int amount);
    void ClearItem(int index);
}