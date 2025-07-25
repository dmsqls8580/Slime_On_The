using System.Collections.Generic;
using UnityEngine;

public class RegionVoronoiGenerator : MonoBehaviour
{
    [Header("지역 설정")]
    public int regionCount = 20;
    public int seed = 12345;

    public List<Vector3Int> RegionCenters { get; private set; } = new();
    public Dictionary<Vector3Int, int> TileToRegionMap { get; private set; } = new();

    private System.Random prng;

    public void GenerateRegions(List<Vector3Int> availableTiles)
    {
        prng = new System.Random(seed);
        RegionCenters.Clear();
        TileToRegionMap.Clear();

        HashSet<Vector3Int> picked = new();
        while (RegionCenters.Count < regionCount)
        {
            int index = prng.Next(availableTiles.Count);
            Vector3Int candidate = availableTiles[index];
            if (!picked.Contains(candidate))
            {
                picked.Add(candidate);
                RegionCenters.Add(candidate);
            }
        }

        foreach (var tile in availableTiles)
        {
            float minDist = float.MaxValue;
            int closestRegion = 0;

            for (int i = 0; i < RegionCenters.Count; i++)
            {
                float dist = Vector3Int.Distance(tile, RegionCenters[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestRegion = i;
                }
            }

            TileToRegionMap[tile] = closestRegion;
        }
    }
}
