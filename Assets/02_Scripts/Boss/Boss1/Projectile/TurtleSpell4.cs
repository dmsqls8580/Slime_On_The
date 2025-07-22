using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleSpell4 : ProjectileBase
{
    public override int PoolSize => 50;
    
    private Animator animator;
    private CircleCollider2D circleCollider;
    private bool canDealDamage = false;
    
    public override void Awake()
    {
        animator = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();
    }
    
    private void ReturnToPool()
    {
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
    }

    public void SetColliderToTrigger()
    {
        circleCollider.isTrigger = true;
    }

    public void SetColliderToCollision()
    {
        circleCollider.isTrigger = false;
        animator.speed = 0f;
    }

    // 애니메이션 이벤트에서 호출
    public void CanDealDamage()
    {
        canDealDamage = true;
    }

    public void AnimationPlay()
    {
        animator.speed = 1f;
    }
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius)
    {
        damage = _damage;
        projectileHost =  _host;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target)
            && canDealDamage && other.gameObject != projectileHost)
        {
            // 공격 호스트 정보를 target에게 전달(projectileHost)
            target.TakeDamage(this, projectileHost);
            canDealDamage = false;
        }
    }
    
    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        
        // 콜라이더 isTrigger true
        SetColliderToTrigger();
    }

    public override void OnReturnToPool()
    {
        // 콜라이더 isTrigger 다시 true
        SetColliderToTrigger();
    }
}
