using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleSpell5 : ProjectileBase
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
    private void Update()
    {
        // 애니메이션의 현 상태
        var state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("TurtleSpell5") && state.normalizedTime >= 1.0f)
        {
            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
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
    
    public override void Init(Vector2 dir, StatBase _damage, float _radius)
    {
        damage = _damage;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target)
            && other.CompareTag("Player") && canDealDamage)
        {
            target.TakeDamage(this);
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
