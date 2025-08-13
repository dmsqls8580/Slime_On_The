using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class ProceduralWorldManager : MonoBehaviour
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
    public BossSpawnerPlacer bossSpawnerPlacer;

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

    public IEnumerator GenerateWorldAsync(int _seed, Action<string, float> onProgress = null)
    {
        seed = _seed;

        onProgress?.Invoke("육각 타일맵 생성 중...", 0.05f);
        hexMapBase.GenerateHexMap();
        yield return null;

        onProgress?.Invoke("지역 분할 중...", 0.15f);
        regionGenerator.GenerateRegions(hexMapBase.GeneratedTiles, regionCount, seed);
        yield return null;

        onProgress?.Invoke("바이옴 할당 중...", 0.25f);
        biomeAssigner.AssignBiomes(regionGenerator.RegionCenters, seed);
        yield return null;

        onProgress?.Invoke("바이옴 타일 색칠 중...", 0.35f);
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
        yield return null;

        onProgress?.Invoke("도로 연결 중...", 0.45f);
        roadConnector.Connect(regionGenerator.RegionCenters, hexMapBase.GeneratedTiles);
        yield return null;

        onProgress?.Invoke("특수 구조물 배치 중...", 0.55f);
        specialStructurePlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);
        yield return null;

        onProgress?.Invoke("자원 군집 배치 중...", 0.65f);
        setPiecePlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);
        yield return null;

        onProgress?.Invoke("바이옴 장식 배치 중...", 0.75f);
        biomeDecorationPlacer.Place(regionGenerator.TileToRegionMap, biomeAssigner.RegionBiomes);
        yield return null;

        onProgress?.Invoke("월드 생성 완료!", 1.0f);
    }
}
