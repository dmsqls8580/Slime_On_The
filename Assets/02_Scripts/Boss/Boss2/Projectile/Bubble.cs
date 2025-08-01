using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : ProjectileBase
{
    private Animator animator;

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius = 0)
    {
        initialized = true;
        damage = _damage;
        projectileHost = _host;
        
        // 발사 방향 조절
        rigid.velocity = dir * speed;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        animator.speed = 1;
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;
        if (other.TryGetComponent<IDamageable>(out var target)
            && other.gameObject != projectileHost)
        {
            // 공격 호스트 정보를 target에게 전달(projectileHost)
            target.TakeDamage(this,projectileHost);

            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
    }
    
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        initialized = false;
        rigid.velocity = Vector2.zero;
        damage = null;
    }
    
    // 애니메이션 정지, 충돌 활성화
    public void StopAnimation()
    {
        initialized = true;
        animator.speed = 0;
    }
    
    public void ReturnToPool()
    {
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
    }
}
