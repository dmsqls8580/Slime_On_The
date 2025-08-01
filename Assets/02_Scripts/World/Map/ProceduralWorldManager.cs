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
    public SpecialStructurePlacer specialStructurePlacer;
    public SetPiecePlacer setPiecePlacer;
    public BiomeDecorationPlacer biomeDecorationPlacer;
    public EnemySpawnerPlacer enemySpawnerPlacer;

    [Header("타일 설정")]
    public Tilemap groundTilemap;
    public TileBase grassTile;
    public TileBase forestTile;
    public TileBase desertTile;
    public TileBase rockyTile;
    public TileBase marshTile;

    [Header("설정")]
    public int seed = 12345;
    public int regionCount = 50;

    public void GenerateWorld(int _seed)
    {
        seed = _seed;

        // Step 1: 기본 육각형 타일 맵 생성
        hexMapBase.GenerateHexMap();

        // Step 2: 지역 분할 (Voronoi 기반)
        regionGenerator.GenerateRegions(hexMapBase.GeneratedTiles, regionCount, seed);

        // Step 3: 바이옴 할당
        biomeAssigner.AssignBiomes(regionGenerator.RegionCenters, seed);

        // Step 4: 바이옴 타일 색칠
        var biomeTiles = new Dictionary<BiomeType, TileBase>
        {
            { BiomeType.Grass, grassTile },
            { BiomeType.Forest, forestTile },
            { BiomeType.Desert, desertTile },
            { BiomeType.Rocky, rockyTile },
            { BiomeType.Marsh, marshTile }
        };

        var biomePainter = new BiomeTilePainter(groundTilemap, biomeTiles);
        biomePainter.PaintTiles(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);

        // Step 5: 도로 연결 (MST 기반)
        roadConnector.Connect(regionGenerator.RegionCenters, hexMapBase.GeneratedTiles);

        // Step 6.5: 특수 구조물 배치
        specialStructurePlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);

        // Step 6: 자원 군집 배치
        setPiecePlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);

        // Step 7: 바이옴에 데코 타일 생성
        biomeDecorationPlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);

        // Step 8: 몬스터 스포너 배치
        enemySpawnerPlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);
    }
}
