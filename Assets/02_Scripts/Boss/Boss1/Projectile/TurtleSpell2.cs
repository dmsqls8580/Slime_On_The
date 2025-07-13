using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleSpell2 : ProjectileBase
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
        animator.speed = Constants.Boss.TENTACLE_SPEED_NOTBERSERKED;
    }
    
    private void Update()
    {
        // 애니메이션의 현 상태
        var state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("TurtleSpell2") && state.normalizedTime >= 1.0f)
        {
            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
    }
    
    // 애니메이션 이벤트로 호출
    private void OnDamageFrame()
    {
        canDealDamage = true;
    }
    
    public override void Init(Vector2 dir, StatBase _damage, float _radius)
    {
        damage = _damage;
    }
    
    
    protected override void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target) 
            && other.CompareTag("Player") && canDealDamage)
        {
            target.TakeDamage(this);
            canDealDamage =  false;
        }
    }
}
