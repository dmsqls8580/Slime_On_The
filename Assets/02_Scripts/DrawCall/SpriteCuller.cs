using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCuller : MonoBehaviour
{
    public GameObject Player;
    public ISpawner Spawner;
    public float PlayerDistance = 30f;
    public float CheckTimer = 0.2f;
    public bool ShouldCull = false;

    private SpriteRenderer spriteRenderer;
    private float sqrPlayerDistance;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sqrPlayerDistance = PlayerDistance * PlayerDistance;
    }
    
    private void OnEnable()
    {
        StartCoroutine(CheckPlayerDistance());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator CheckPlayerDistance()
    {
        while (true)
        {
            if (Player != null)
            {
                // 플레이어의 거리가 멀어지면 스프라이트 비활성화, 가까우면 활성화
                Transform playerTransform = Player.transform;
                float curDistance = (transform.position - playerTransform.position).sqrMagnitude;
        
                ShouldCull = curDistance <= sqrPlayerDistance;
        
                spriteRenderer.enabled = ShouldCull;
            }

            if (Spawner != null)
            {
                // 스포너의 SpawnRadius를 가져와 현재 거리와 비교
                float spawnRadius = Spawner.SpawnRadius;
                float sqrSpawnRadius = spawnRadius * spawnRadius;
                
                Transform spawnerTransform = Spawner.Transform;
                float curDistance = (transform.position - spawnerTransform.position).sqrMagnitude;

                if (curDistance <= sqrSpawnRadius
                    && ShouldCull == false)
                {
                    Spawner.RemoveObject(gameObject);
                }
            }
            yield return new WaitForSeconds(CheckTimer);
        }
    }

}
