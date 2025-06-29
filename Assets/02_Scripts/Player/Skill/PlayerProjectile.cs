using System;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerProjectile : MonoBehaviour
{
    private float _speed;
    private IAttackable _attacker;
    private Vector3 _direction;

    private IObjectPool<PlayerProjectile> _pool;

    public void Init(Vector2 dir, float speed, IAttackable attacker, IObjectPool<PlayerProjectile> pool)
    {
        _direction = dir.normalized;
        this._speed = speed;
        this._attacker = attacker;
        this._pool = pool;
        
        gameObject.SetActive(true);
    }

    private void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target) && !target.IsDead)
        {
            target.TakeDamage(_attacker);
            Release();
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Release()
    {
        _pool?.Release(this);
    }
}
