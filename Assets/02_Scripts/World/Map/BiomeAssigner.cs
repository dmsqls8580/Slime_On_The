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
    private int _seed;
    private System.Random _prng;

    public Dictionary<int, BiomeType> RegionBiomes { get; private set; } = new();

    public BiomeAssigner(int seed)
    {
        _seed = seed;
        _prng = new System.Random(seed);
    }

    public void AssignBiomes(List<Vector3Int> regionCenters)
    {
        RegionBiomes.Clear();

        float maxDistance = 0f;
        foreach (var center in regionCenters)
        {
            float d = Vector2.Distance(new Vector2(center.x, center.y), Vector2.zero);
            if (d > maxDistance) maxDistance = d;
        }

        for (int i = 0; i < regionCenters.Count; i++)
        {
            var center = regionCenters[i];
            float distNorm = Vector2.Distance(new Vector2(center.x, center.y), Vector2.zero) / maxDistance;

            float noise = Mathf.PerlinNoise(center.x * 0.01f + _seed, center.y * 0.01f + _seed);
            float finalValue = Mathf.Clamp01((distNorm * 0.7f) + (noise * 0.3f)); // 거리 + 노이즈 가중치 조정

            BiomeType biome = finalValue switch
            {
                <= 0.25f => BiomeType.Grass,
                <= 0.45f => BiomeType.Forest,
                <= 0.65f => BiomeType.Desert,
                <= 0.85f => BiomeType.Rocky,
                _ => BiomeType.Marsh
            };

            RegionBiomes[i] = biome;
        }
    }
}

