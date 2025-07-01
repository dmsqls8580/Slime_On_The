using System;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerProjectile : MonoBehaviour
{
    private Rigidbody2D rigid;
    
    private float speed;
    private float damage;
    private Vector3 direction;
    
    private IAttackable attacker;
    private IObjectPool<PlayerProjectile> pool;
    
    private float lifeTime = 3f;
    private float timer;
    
    private void Awake()
    {
        rigid= GetComponent<Rigidbody2D>();
    }

    public void SetPool(IObjectPool<PlayerProjectile> pool)
    {
        this.pool = pool;
    }

    public void Init(Vector2 dir, float speed,float damage, IAttackable attacker)
    {
        direction = dir.normalized;
        this.speed = speed;
        this.attacker = attacker;
        this.damage = damage;
        
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
        if (other.TryGetComponent<IDamageable>(out var target) && target != attacker)
        {
            target.TakeDamage(attacker);
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

}
