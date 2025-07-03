using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour, IAttackable, IPoolObject
{
    private Rigidbody2D rigid;
    private EnemyController enemyController;
    
    private GameObject target;
    private StatBase damage;
    private float speed;
    private float lifeTime = 3f;
    
    /************************ IAttackable ***********************/
    public StatBase AttackStat => damage;
    public IDamageable Target => null; // OntriggerEnter2D에서 처리하기 때문에 불필요
    
    public void Attack()
    {
        // 층돌 시 피격 판정
        // OntriggerEnter2D에서 처리 
    }
    
    /************************ IPoolObject ***********************/
    public GameObject GameObject => this.gameObject;
    public string PoolID => gameObject.name;
    public int PoolSize => 5;
    
    public void OnSpawnFromPool()
    {
        
    }

    public void OnReturnToPool()
    {
        rigid.velocity = Vector2.zero;
        enemyController = null;
        target = null;
        damage = null;
    }


    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    
    public void Init(EnemyController _enemy, GameObject _target, StatBase _damage, float _speed)
    {
        enemyController = _enemy;
        target = _target;
        damage = _damage;
        speed = _speed;
        
        // 발사 방향 계산
        Vector2 dir = (target.transform.position - transform.position).normalized;
        rigid.velocity = dir * speed;
        
        // lifeTime 후 자동 반환
        ObjectPoolManager.Instance.ReturnObject(this.gameObject, lifeTime);
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target)&& !other.CompareTag("Player"))
        {
            target.TakeDamage(this);

            ObjectPoolManager.Instance.ReturnObject(this.gameObject);
        }
    }
}
