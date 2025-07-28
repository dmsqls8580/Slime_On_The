using System.Collections.Generic;
using UnityEngine;

public class RegionVoronoiGenerator : MonoBehaviour
{
    public List<Vector3Int> RegionCenters { get; private set; } = new();
    public Dictionary<Vector3Int, int> TileToRegionMap { get; private set; } = new();

    public void GenerateRegions(List<Vector3Int> availableTiles, int regionCount, int seed)
    {
        System.Random prng = new(seed);
        RegionCenters.Clear();
        TileToRegionMap.Clear();

        // 1. 지역 중심 좌표 선정
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

        // 2. 각 타일을 가장 가까운 지역 중심에 할당
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
