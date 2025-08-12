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
    
    private void ReturnToPool()
    {
        ObjectPoolManager.Instance.ReturnObject(this.gameObject);
    }
    
    // 애니메이션 이벤트로 호출
    private void OnDamageFrame()
    {
        canDealDamage = true;
    }

    private void OffDamageFrame()
    {
        canDealDamage = false;
    }
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius)
    {
        base.Init(dir, _damage, _host, _radius);
        damage = _damage;
        projectileHost =  _host;
    }
    
    // 애니메이션 정지 메서드
    public void AnimationStop()
    {
        animator.speed = 0;
    }
    
    // 애니메이션 재생 메서드
    public void AnimationPlay()
    {
        animator.speed = 1f;
    }
    
    protected override void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target) 
            && canDealDamage && other.gameObject != projectileHost)
        {
            // 공격 호스트 정보를 target에게 전달(projectileHost)
            target.TakeDamage(this, projectileHost);
            canDealDamage =  false;
        }
    }

}
