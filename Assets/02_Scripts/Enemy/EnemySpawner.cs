using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySpawner))]
public class EnemySpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemySpawner spawner = (EnemySpawner)target;

        if (GUILayout.Button("몬스터 스폰"))
        {
            spawner.SpawnEnemies();
        }

        if (GUILayout.Button("몬스터 제거"))
        {
            spawner.ClearEnemies();
        }
    }
}

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 몬스터 설정")]
    public int SpawnCount = 5;
    public float SpawnRadius = 10f;
    public int EnemyTableIDX = 0;     // 스포너에 따라 다른 IDX로 몬스터 스폰

    private EnemyTable enemyTable;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        enemyTable = TableManager.Instance.GetTable<EnemyTable>();
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (enemyTable == null)
        {
            Debug.LogError("EnemyTable == null");
            return;
        }
        
        EnemySO enemySO = enemyTable.GetDataByID(EnemyTableIDX);
        if (enemySO == null)
        {
            Debug.LogError($"EnemySO == null, IDX: {EnemyTableIDX}");
            return;
        }
        
        string poolID = enemySO.EnemyID.ToString();

        for (int i = 0; i < SpawnCount; i++)
        {
            Vector3 spawnPos = GetRandomPosition(transform.position, SpawnRadius);
            GameObject enemy = ObjectPoolManager.Instance.GetObject(poolID);
            
            // Enemy 위치 옮겨주기
            if (enemy == null)
            {
                Debug.LogError($"Pool에서 Enemy를 가져올 수 없습니다. PoolID: {poolID}");
                continue;
            }
            enemy.transform.position = spawnPos;
            enemy.transform.rotation = Quaternion.identity;
            
            // Todo : EnemyStatus에 EnemySO 정보 전달해주기
            // 현재 몬스터 프리팹에 EnemySO가 들어가 있어 전달해줄 필요 없음 - 구조 수정 예정
            
            if (enemy.TryGetComponent<EnemyController>(out var controller))
            {
                controller.SpawnPos = spawnPos; // 필요시
                controller.OnSpawnFromPool();
            }
            else
            {
                Debug.LogError($"Pool에서 Enemy를 가져올 수 없습니다. PoolID: {enemySO.EnemyID.ToString()}");
            }
            
            spawnedEnemies.Add(enemy); // 생성된 적 리스트에 추가
        }
        
    }
    
    // 생성된 적 모두를 풀에 반환하고 리스트 초기화
    public void ClearEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                ObjectPoolManager.Instance.ReturnObject(enemy);
            }
        }
        spawnedEnemies.Clear();
    }
    
    // SpawnRadius 내 랜덤한 위치 결정
    private Vector3 GetRandomPosition(Vector3 center, float radius)
    {
        Vector2 randomPos = Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomPos.x, center.y + randomPos.y, center.z);
    }
}
