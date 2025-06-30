using System;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerProjectile : MonoBehaviour
{
    private Rigidbody2D _rb;
    
    private float _speed;
    private float _damage;
    private Vector3 _direction;
    
    private IAttackable _attacker;
    private IObjectPool<PlayerProjectile> _pool;
    
    private float _lifeTime = 3f;
    private float _timer;
    
    private void Awake()
    {
        _rb= GetComponent<Rigidbody2D>();
    }

    public void SetPool(IObjectPool<PlayerProjectile> pool)
    {
        _pool = pool;
    }

    public void Init(Vector2 dir, float speed,float damage, IAttackable attacker)
    {
        _direction = dir.normalized;
        _speed = speed;
        _attacker = attacker;
        _damage = damage;
        
        _timer = 0f;
        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        _rb.velocity = _direction * _speed;

        _timer += Time.deltaTime;
        if (_timer >= _lifeTime)
        {
            ReturnToPool(); // 일정 시간 지나면 반환
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target) && target != _attacker)
        {
            target.TakeDamage(_attacker);
            _pool?.Release(this);
        }
    }
    
    private void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Release(this); // 풀로 되돌림
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
