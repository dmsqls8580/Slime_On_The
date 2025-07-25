using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralWorldManager : MonoBehaviour
{
    [Header("기본 모듈")]
    public HexMapBase hexMapBase;
    public RegionVoronoiGenerator regionGenerator;
    public BiomeAssigner biomeAssigner;
    public RoadConnector roadConnector;
    public SetPiecePlacer setPiecePlacer;
    public WaterRingApplier waterRingApplier;

    [Header("설정")]
    public int seed = 12345;

    [Header("타일 및 타일맵")]
    public Tilemap groundTilemap;
    public TileBase grassTile;
    public TileBase forestTile;
    public TileBase desertTile;
    public TileBase rockyTile;
    public TileBase marshTile;

    void Start()
    {
        // Step 1: 맵 생성
        hexMapBase.GenerateHexMap();

        // Step 2: 지역 분할
        regionGenerator.seed = seed;
        regionGenerator.GenerateRegions(hexMapBase.GeneratedTiles);

        // Step 3: 바이옴 할당
        biomeAssigner.AssignBiomes(regionGenerator.RegionCenters);

        // Step 4: 타일 색칠
        var biomeTiles = new Dictionary<BiomeType, TileBase>
        {
            { BiomeType.Grass, grassTile },
            { BiomeType.Forest, forestTile },
            { BiomeType.Desert, desertTile },
            { BiomeType.Rocky, rockyTile },
            { BiomeType.Marsh, marshTile }
        };

        var painter = new BiomeTilePainter(groundTilemap, biomeTiles);
        painter.PaintTiles(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);

        // Step 5: 길 연결
        roadConnector.Connect(regionGenerator.RegionCenters, hexMapBase.GeneratedTiles);

        // Step 6: 세트 피스 배치
        setPiecePlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);

        // Step 7: 외곽 물 타일
        waterRingApplier.Apply(hexMapBase.GeneratedTiles, regionGenerator.TileToRegionMap);
    }
}
