using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour, IAttackable, IPoolObject
{
    protected Rigidbody2D rigid;
    protected GameObject target;
    protected StatBase damage;
    protected float speed;
    protected float lifeTime = 3f;
    public virtual StatBase AttackStat => damage;
    public virtual IDamageable Target => null; // OntriggerEnter2D에서 처리하기 때문에 불필요

    public virtual void Attack() { }


    public virtual GameObject GameObject  => this.gameObject;
    public virtual string PoolID => gameObject.name;
    public virtual int PoolSize => 5;
    
    public virtual void OnSpawnFromPool()
    {
        
    }

    public virtual void OnReturnToPool()
    {
        rigid.velocity = Vector2.zero;
        target = null;
        damage = null;
    }
    
    public virtual void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public abstract void Init(Vector2 dir, GameObject _target, StatBase _damage, float _speed);

    protected abstract void OnTriggerEnter2D(Collider2D other);
}
