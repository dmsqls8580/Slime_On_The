using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour, ISpawner
{
    public Transform Transform { get; set; }
    
    [Header("스폰 몬스터 설정")]
    public int SpawnCount = 1;
    public float SpawnRadius = 10f;
    public BossSO BossSO;
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
    
    private List<GameObject> spawnedboss = new List<GameObject>();
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

    public void SpawnBoss()
    {
        string poolID = BossSO.BossID.ToString();
        
        // 남은 스폰 가능한 적 수 계산
        int canSpawnCount = SpawnCount - spawnedboss.Count;
        if (canSpawnCount <= 0)
        {
            return;
        }

        for (int i = 0; i < canSpawnCount; i++)
        {
            Vector3 spawnPos = transform.position;
            GameObject boss = ObjectPoolManager.Instance.GetObject(poolID);
            
            // boss 위치 옮겨주기
            if (boss == null)
            {
                Logger.Log($"Pool에서 Enemy를 가져올 수 없습니다. PoolID: {poolID}");
            }
            boss.transform.position = spawnPos;
            boss.transform.rotation = Quaternion.identity;

            if (boss.TryGetComponent<IBossController>(out var controller)
                && boss.TryGetComponent<IPoolObject>(out var poolObj))
            {
                controller.SpawnPos = spawnPos;
                poolObj.OnSpawnFromPool();
            }
            else
            {
                Debug.LogError($"Pool에서 Boss를 가져올 수 없습니다. PoolID: {BossSO.BossID.ToString()}");
            }
        
            // Boss의 SpriteCuller에 Player 등록
            if (boss.TryGetComponent<SpriteCuller>(out var culler))
            {
                culler.Player = player;
                culler.Spawner = this;
            }
        
            spawnedboss.Add(boss);
        }
        
    }
    
    public void RemoveObject(GameObject _boss, float _returnTime = 0)
    {
        spawnedboss.Remove(_boss);
        ObjectPoolManager.Instance.ReturnObject(_boss, _returnTime);
    }
    
    // 생성된 적 모두를 풀에 반환하고 리스트 초기화
    public void ClearBoss()
    {
        foreach (var enemy in spawnedboss)
        {
            if (enemy != null)
            {
                ObjectPoolManager.Instance.ReturnObject(enemy);
            }
        }
        spawnedboss.Clear();
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
            SpawnBoss();
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
