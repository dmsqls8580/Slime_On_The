using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

public class EnemySpawner : MonoBehaviour, ISpawner
{
    public Transform Transform { get; set; }
    
    [Header("스폰 몬스터 설정")]
    public int SpawnCount = 5;
    public float SpawnRadius = 10f;
    public List<EnemySO> EnemySOList;
    int ISpawner.SpawnCount
    {
        get => SpawnCount;
        set => SpawnCount = value;
    }

    float ISpawner.SpawnRadius
    {
        get => SpawnRadius;
        set => SpawnRadius = value;
    }
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private CircleCollider2D circleCollider2D;
    private GameObject player;

    private void Awake()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        InitCollider();
        Transform = transform;
    }

    public void SpawnEnemies()
    {
        int SOCount = EnemySOList.Count;
        
        if (SOCount == 0)
        {
            Debug.LogWarning("EnemySpawner: EnemySOList가 비어 있습니다.");
            return;
        }
        
        // 남은 스폰 가능한 적 수 계산
        int canSpawnCount = SpawnCount - spawnedEnemies.Count;
        if (canSpawnCount <= 0)
        {
            return;
        }

        for (int i = 0; i < canSpawnCount; i++)
        {
            // 몬스터 리스트 중에서 랜덤으로 정해서 스폰
            EnemySO randomSO = EnemySOList[Random.Range(0, SOCount)];
            string poolID = randomSO.EnemyID.ToString();
            
            if (!ObjectPoolManager.Instance.HasPool(poolID))
            {
                Debug.LogWarning($"EnemySpawner: {poolID} 풀이 아직 초기화되지 않았습니다. 스폰 생략");
                return;
            }
            
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
            
            if (enemy.TryGetComponent<EnemyController>(out var controller))
            {
                controller.SpawnPos = spawnPos; // 필요시
                controller.OnSpawnFromPool();
            }
            else
            {
                Debug.LogError($"Pool에서 Enemy를 가져올 수 없습니다. PoolID: {randomSO.EnemyID.ToString()}");
            }

            // Enemy의 SpriteCuller에 Player 등록
            if (enemy.TryGetComponent<SpriteCuller>(out var culler))
            {
                culler.Player = player;
                culler.Spawner = this;
            }
            
            spawnedEnemies.Add(enemy); // 생성된 적 리스트에 추가
        }
        
    }

    public void RemoveObject(GameObject _enemy, float _returnTime = 0)
    {
        spawnedEnemies.Remove(_enemy);
        ObjectPoolManager.Instance.ReturnObject(_enemy, _returnTime);
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
        if (collision.CompareTag("Player"))
        {
            player = collision.gameObject;
            SpawnEnemies();
        }
    }

    private void OnDrawGizmos()
    {
        if (circleCollider2D == null)
        {
            circleCollider2D = GetComponent<CircleCollider2D>();            
        }
        
        if (circleCollider2D is CircleCollider2D circle)
        {
            Gizmos.color = Color.magenta;
            Vector3 center = circle.transform.TransformPoint(circle.offset);
            Gizmos.DrawWireSphere(center, circle.radius);
        }
    }
}
