using System;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerProjectile : MonoBehaviour, IAttackable
{
    private Rigidbody2D rigid;
    
    private float speed;
    private float lifeTime = 3f;
    private float timer;
    
    private StatBase damage;
    private Vector3 direction;
    private StatManager statManager;
    public StatBase AttackStat => damage;
    public IDamageable Target => null;
    
    private IObjectPool<PlayerProjectile> pool;
    
    private bool isCritical;
    
    
    private void Awake()
    {
        statManager= GetComponentInParent<StatManager>();
        rigid= GetComponent<Rigidbody2D>();
    }

    public void SetPool(IObjectPool<PlayerProjectile> pool)
    {
        this.pool = pool;
    }

    public void Init(Vector2 _dir, float _speed)
    {
        direction = _dir.normalized;
        this.speed = _speed;
        
        float baseAtk = statManager.GetValue(StatType.Attack);
        float critChance = statManager.GetValue(StatType.CriticalChance); // 0~100
        float critMultiplier = statManager.GetValue(StatType.CriticalMultiplier);

        isCritical = UnityEngine.Random.Range(0, 100) < critChance;
        float calcDamage = isCritical ? baseAtk * critMultiplier : baseAtk;

        damage = new CalculateStat(StatType.Attack, calcDamage); 
        
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
            if (isCritical)
            {
                Logger.Log("Critical Hit");
            }
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