using System.Collections.Generic;
using UnityEngine;

public enum BiomeType
{
    Grass,
    Forest,
    Desert,
    Rocky,
    Marsh
}

public class BiomeAssigner : MonoBehaviour
{
    [Header("바이옴 비율 (0~1의 합이 1이 되도록 설정)")]
    [Range(0f, 1f)] public float grassRatio = 0.3f;
    [Range(0f, 1f)] public float forestRatio = 0.3f;
    [Range(0f, 1f)] public float desertRatio = 0.1f;
    [Range(0f, 1f)] public float rockyRatio = 0.2f;
    [Range(0f, 1f)] public float marshRatio = 0.1f;

    public Dictionary<int, BiomeType> RegionBiomes { get; private set; } = new();

    public void AssignBiomes(List<Vector3Int> regionCenters, int seed)
    {
        RegionBiomes.Clear();
        System.Random prng = new(seed);

        int totalRegions = regionCenters.Count;
        int grassCount = Mathf.RoundToInt(totalRegions * grassRatio);
        int forestCount = Mathf.RoundToInt(totalRegions * forestRatio);
        int desertCount = Mathf.RoundToInt(totalRegions * desertRatio);
        int rockyCount = Mathf.RoundToInt(totalRegions * rockyRatio);
        int marshCount = totalRegions - (grassCount + forestCount + desertCount + rockyCount); // 나머지는 Marsh

        List<BiomeType> biomeList = new();
        biomeList.AddRange(CreateList(BiomeType.Grass, grassCount));
        biomeList.AddRange(CreateList(BiomeType.Forest, forestCount));
        biomeList.AddRange(CreateList(BiomeType.Desert, desertCount));
        biomeList.AddRange(CreateList(BiomeType.Rocky, rockyCount));
        biomeList.AddRange(CreateList(BiomeType.Marsh, marshCount));

        // Shuffle biomeList
        for (int i = 0; i < biomeList.Count; i++)
        {
            int swapIndex = prng.Next(i, biomeList.Count);
            (biomeList[i], biomeList[swapIndex]) = (biomeList[swapIndex], biomeList[i]);
        }

        // Assign shuffled biome types to each region
        for (int i = 0; i < regionCenters.Count; i++)
        {
            RegionBiomes[i] = biomeList[i];
        }
    }

    private List<BiomeType> CreateList(BiomeType type, int count)
    {
        List<BiomeType> list = new();
        for (int i = 0; i < count; i++)
            list.Add(type);
        return list;
    }
}
