using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfBomb : ProjectileBase
{
    private CircleCollider2D circleCollider2D;
    
    public override void Awake()
    {
        base.Awake();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius = 0)
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
    
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        initialized = false;
        damage = null;
    }
}
