using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleMelee : ProjectileBase
{
    private CapsuleCollider2D capsuleCollider2D;

    public override void Awake()
    {
        base.Awake();
        capsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius)
    {
        base.Init(dir, _damage, _host, _radius);
        initialized = true;
        damage = _damage;
        projectileHost =  _host;
        
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
