using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 근접 몬스터용 발사체(속도 0, 이동하지 않음)
public class None : ProjectileBase
{
    public override int PoolSize => 10;
    private CircleCollider2D circleCollider2D;

    public override void Awake()
    {
        base.Awake();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }
    
    public override void Init(Vector2 dir, StatBase _damage, float _radius)
    {
        damage = _damage;
        if (_radius > 0f)
        {
            circleCollider2D.radius = _radius;
        }
        // lifeTime 후 자동 반환
        ObjectPoolManager.Instance.ReturnObject(this.gameObject, lifeTime);
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target)
            && other.CompareTag("Player"))
        {
            target.TakeDamage(this);

            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
    }

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        Logger.Log("None spawned");
    }
}
