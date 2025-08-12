using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : ProjectileBase
{
    private CircleCollider2D circleCollider2D;
    private EnemyController hostController;

    public override void Awake()
    {
        base.Awake();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    public void Update()
    {
        transform.position = hostController.projectileTransform.position;
    }
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius)
    {
        base.Init(dir, _damage, _host, _radius);
        initialized = true;
        damage = _damage;
        projectileHost =  _host;
        hostController = projectileHost.GetComponent<EnemyController>();
        
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

    private void OnDrawGizmos()
    {
        if (circleCollider2D == null) return;
        {
            circleCollider2D = GetComponent<CircleCollider2D>();
        }

        Gizmos.color = Color.red;
        Vector3 center = circleCollider2D.transform.TransformPoint(circleCollider2D.offset);
        Gizmos.DrawWireSphere(center, circleCollider2D.radius);
    }
}
