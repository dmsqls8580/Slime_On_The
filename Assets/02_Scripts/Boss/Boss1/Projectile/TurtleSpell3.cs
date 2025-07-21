using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleSpell3 : ProjectileBase
{
    public override int PoolSize => 5;
    
    private Animator animator;
    private bool canDealDamage = false;
    
    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    public void Start()
    {
        animator.speed = Constants.Boss.TENTACLE_SPEED_BERSERKED;
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
    
    public override void Init(Vector2 dir, StatBase _damage, GameObject _host, float _radius)
    {
        damage = _damage;
        projectileHost =  _host;
    }
    
    
    protected override void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target) 
            && other.CompareTag("Player") && canDealDamage)
        {
            target.TakeDamage(this, gameObject);
            canDealDamage =  false;
        }
    }
}
