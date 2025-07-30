using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(BossSpawner))]
public class BossSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BossSpawner spawner = (BossSpawner)target;

        if (GUILayout.Button("보스 스폰"))
        {
            spawner.SpawnBoss();
        }
        
        if (GUILayout.Button("보스 제거"))
        {
            spawner.ClearBoss();
        }
    }
}
public class BossSpawner : MonoBehaviour
{
    [Header("스폰 몬스터 설정")]
    public float SpawnRadius = 10f;
    public int BossTableIDX = 0;     // 스포너에 따라 다른 IDX로 몬스터 스폰

    private BossTable bossTable;
    private List<GameObject> spawnedboss = new List<GameObject>();
    private CircleCollider2D circleCollider2D;

    private void Awake()
    {
        bossTable = TableManager.Instance.GetTable<BossTable>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }
    
    private void Start()
    {
        InitCollider();
    }

    public void SpawnBoss()
    {
        if (bossTable == null)
        {
            Logger.Log("BossTable == null");
            return;
        }

        BossSO bossSO = bossTable.GetDataByID(BossTableIDX);
        if (bossSO == null)
        {
            Logger.Log($"BossSO == null, IDX: {BossTableIDX}");
            return;
        }
        
        string poolID = bossSO.BossID.ToString();

        Vector3 spawnPos = transform.position;
        GameObject boss = ObjectPoolManager.Instance.GetObject(poolID);
            
        // boss 위치 옮겨주기
        if (boss == null)
        {
            Logger.Log($"Pool에서 Enemy를 가져올 수 없습니다. PoolID: {poolID}");
        }
        boss.transform.position = spawnPos;
        boss.transform.rotation = Quaternion.identity;

        if (boss.TryGetComponent<BossController>(out var controller))
        {
            controller.SpawnPos = spawnPos;
            controller.OnSpawnFromPool();
        }
        else
        {
            Debug.LogError($"Pool에서 Boss를 가져올 수 없습니다. PoolID: {bossSO.BossID.ToString()}");
        }
        spawnedboss.Add(boss);
    }
    // 생성된 적 모두를 풀에 반환하고 리스트 초기화
    public void ClearBoss()
    {
        foreach (var enemy in spawnedboss)
        {
            if (enemy != null)
            {
                ObjectPoolManager.Instance.ReturnFixedObject(enemy);
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
        SpawnBoss();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        ClearBoss();
    }
}
