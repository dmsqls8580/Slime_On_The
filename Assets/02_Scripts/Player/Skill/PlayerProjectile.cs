using System;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerProjectile : MonoBehaviour, IAttackable
{
    private Rigidbody2D rigid;
    
    private float speed;
    private StatBase damage;
    private Vector3 direction;
    private StatManager statManager;
    public StatBase AttackStat => damage;
    public IDamageable Target => null;
    
    private IObjectPool<PlayerProjectile> pool;
    
    private float lifeTime = 3f;
    private float timer;
    
    private void Awake()
    {
        statManager= GetComponentInParent<StatManager>();
        rigid= GetComponent<Rigidbody2D>();
    }

    public void SetPool(IObjectPool<PlayerProjectile> pool)
    {
        this.pool = pool;
    }

    public void Init(Vector2 dir, float speed)
    {
        direction = dir.normalized;
        this.speed = speed;
        damage = statManager.Stats[StatType.Attack];
        
        timer = 0f;
        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        rigid.velocity = direction * speed;

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            ReturnToPool(); // 일정 시간 지나면 반환
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target)&& !other.CompareTag("Player"))
        {
            target.TakeDamage(this);
            pool?.Release(this);
        }
    }
    
    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Release(this); // 풀로 되돌림
        }
        else
        {
            Destroy(gameObject); // 풀 없을 시 직접 파괴 (예외 상황)
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
    
    public void Attack() { }
}