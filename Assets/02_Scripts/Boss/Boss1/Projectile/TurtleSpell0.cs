using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleSpell0 : ProjectileBase
{
    public override int PoolSize => 40;
    
    private Animator animator;
    private bool canDealDamage = false;
    
    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }
    
    // 오직 한 프레임만 데미지를 줄 수 있도록 LateUpdate를 사용
    private void LateUpdate()
    {
        canDealDamage =  false;
    }
    
    // 애니메이션 이벤트로 호출
    private void OnDamageFrame()
    {
        canDealDamage = true;
    }
    
    public override void Init(Vector2 dir, StatBase _damage)
    {
        damage = _damage;
    }

    // 애니메이션 재생 메서드
    public void AnimationPlay()
    {
        animator.speed = 1f;
    }
    
    protected override void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target) 
            && other.CompareTag("Player"))
        {
            target.TakeDamage(this);
        }
    }

    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();
        animator.speed = 0;
    }
}
