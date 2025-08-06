public static class GameSettings
{
    public static int seed = 0;

    public static void GenerateRandomSeed()
    {
        seed = System.Guid.NewGuid().GetHashCode();
    }

    public static void ClearSeed()
    {
        seed = 0;
    }
}