[System.Flags]
public enum PlaceType
{
    None = 0,
    Building = 1 << 0,
    Seed = 1 << 1
    // 추가 방법: name = 1 << 2
}
