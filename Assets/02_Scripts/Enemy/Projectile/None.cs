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
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius)
    {
        initialized = true;
        damage = _damage;
        projectileHost =  _host;
        
        if (_radius > 0f)
        {
            circleCollider2D.radius = _radius;
        }
        // lifeTime 후 자동 반환
        ObjectPoolManager.Instance.ReturnObject(this.gameObject, lifeTime);
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;
        if (other.TryGetComponent<IDamageable>(out var target)
            && other.gameObject != projectileHost)
        {
            // 공격 호스트 정보를 target에게 전달(projectileHost)
            target.TakeDamage(this, projectileHost);

            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
    }

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        Logger.Log("None spawned");
    }
    
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        initialized = false;
        damage = null;
    }
}
