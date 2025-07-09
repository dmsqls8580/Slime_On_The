using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bolt : ProjectileBase
{
    public override void Init(Vector2 dir, GameObject _target, StatBase _damage, float _speed)
    {
        target = _target;
        damage = _damage;
        speed = _speed;
        
        // 발사 방향 조절
        rigid.velocity = dir * speed;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // lifeTime 후 자동 반환
        ObjectPoolManager.Instance.ReturnObject(this.gameObject, lifeTime);
        
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target)&& other.CompareTag("Player"))
        {
            target.TakeDamage(this);

            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
    }
}
