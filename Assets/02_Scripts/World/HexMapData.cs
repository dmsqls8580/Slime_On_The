using System.Collections.Generic;
using UnityEngine;

public enum TerrainType
{
    Plains,
    Forest,
    Quarry
}

public class HexMapData
{
    public TerrainType[,] terrainMap;
    public int width, height;

    public void GenerateCellularTerrain(int _width, int _height, int iterations, float fillPercent)
    {
        width = _width;
        height = _height;
        terrainMap = new TerrainType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float roll = Random.value;
                terrainMap[x, y] = roll < fillPercent ? TerrainType.Forest : (roll < 0.9f ? TerrainType.Plains : TerrainType.Quarry);
            }
        }

        for (int i = 0; i < iterations; i++)
        {
            TerrainType[,] newMap = new TerrainType[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int forest = 0, plains = 0, quarry = 0;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                var t = terrainMap[nx, ny];
                                if (t == TerrainType.Forest) forest++;
                                else if (t == TerrainType.Plains) plains++;
                                else if (t == TerrainType.Quarry) quarry++;
                            }
                        }
                    }

                    if (forest >= 5) newMap[x, y] = TerrainType.Forest;
                    else if (plains >= 5) newMap[x, y] = TerrainType.Plains;
                    else if (quarry >= 4) newMap[x, y] = TerrainType.Quarry;
                    else newMap[x, y] = terrainMap[x, y];
                }
            }

            terrainMap = newMap;
        }
    }
}