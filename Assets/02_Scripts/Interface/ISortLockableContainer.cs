public interface ISortLockableContainer
{
    bool IsSortLockMode { get; }
    void ToggleSlotSortLock(int index);
    bool IsSlotSortLocked(int index);
}