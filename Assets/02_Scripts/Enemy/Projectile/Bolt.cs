using UnityEngine;

public class Bolt : ProjectileBase
{
    public override void Init(Vector2 dir, StatBase _damage)
    {
        damage = _damage;
        
        // 발사 방향 조절
        rigid.velocity = dir * speed;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // lifeTime 후 자동 반환
        ObjectPoolManager.Instance.ReturnObject(this.gameObject, lifeTime);
        
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target)
            && other.CompareTag("Player"))
        {
            target.TakeDamage(this);

            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
    }
    
    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        rigid.velocity = Vector2.zero;
        damage = null;
    }
}
