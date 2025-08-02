using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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

/*
EnemyIDX

Test
0. 테스트
1. 원거리테스트
2. 소
3. 다람쥐

Grass
10. 랫서판다
11. 찐따벌

Forest
20. 촘 봄
21. 당근이

Rocky
30. 패럿
31. 블랙푸딩
32. 고양이

Desert
40. 토끼
41. 재카로프
42. 쓰레기

Marsh
50, 어인
51. 어인전사

 */

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 몬스터 설정")]
    public int SpawnCount = 5;
    public float SpawnRadius = 10f;
    public EnemySO EnemySO;

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private CircleCollider2D circleCollider2D;
    // private EnemyTable enemyTable;

    private void Awake()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        // enemyTable = TableManager.Instance.GetTable<EnemyTable>();
        InitCollider();
    }

    public void SpawnEnemies()
    {
        // if (enemyTable.IsUnityNull())
        // {
        //     enemyTable = TableManager.Instance.GetTable<EnemyTable>();
        // }
        string poolID = EnemySO.EnemyID.ToString();

        if (!ObjectPoolManager.Instance.HasPool(poolID))
        {
            Debug.LogWarning($"EnemySpawner: {poolID} 풀이 아직 초기화되지 않았습니다. 스폰 생략");
            return;
        }

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

                bool isOnNavMesh = controller.Agent != null && controller.Agent.isOnNavMesh;
                Debug.Log($"[EnemySpawner] Enemy 스폰 완료: {enemy.name}, 위치: {spawnPos}, NavMesh 여부: {isOnNavMesh}");

            }
            else
            {
                Debug.LogError($"Pool에서 Enemy를 가져올 수 없습니다. PoolID: {EnemySO.EnemyID.ToString()}");
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
                ObjectPoolManager.Instance.ReturnFixedObject(enemy);
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
    
    // Collider 반경 설정
    private void InitCollider()
    {
        if(circleCollider2D != null)
        {
            circleCollider2D.radius = SpawnRadius;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SpawnEnemies();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ClearEnemies();
    }
}
