using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerSpawner : MonoBehaviour
{
    [Header("플레이어 프리팹")]
    [SerializeField] private GameObject playerPrefab;

    [Header("초기 스폰 위치")]
    [SerializeField] private Vector3 defaultSpawnPosition = Vector3.zero;

    private GameObject spawnedPlayer;

    // 플레이어 기본 위치/저장된 위치에 생성
    public void SpawnPlayer()
    {
        Vector3 spawnPos = GetSpawnPosition();
        SpawnPlayerAt(spawnPos);
    }

    // 특정 위치에 플레이어 생성
    public void SpawnPlayerAt(Vector3 position)
    {
        if (playerPrefab == null)
        {
            Logger.LogError("[PlayerSpawner] 플레이어 프리팹이 설정되지 않았습니다.");
            return;
        }

        spawnedPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
    }

    // 저장된 위치가 있다면 그 위치를 반환하고, 없다면 기본 위치 반환
    private Vector3 GetSpawnPosition()
    {
        // 현재는 저장기능 없음
        //default 위치 사용
        // 추후 PlayerDataManager.Load().lastPosition 등으로 대체
        return defaultSpawnPosition;
    }

    public GameObject GetSpawnedPlayer()
    {
        return spawnedPlayer;
    }
}

