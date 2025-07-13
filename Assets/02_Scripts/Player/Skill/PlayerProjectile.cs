using PlayerStates;
using System;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerProjectile : MonoBehaviour, IAttackable
{
    private StatManager statManager;
    private StatBase damage;
    public StatBase AttackStat => damage;
    public IDamageable Target => null;
    private IObjectPool<PlayerProjectile> pool;
    private Rigidbody2D rigid;
    
    private float speed;
    private float calcDamage; 
    private float range;

    private Vector3 direction;
    private Vector2 startAttackPos;

    private bool isCritical;


    private void Awake()
    {
        statManager = GetComponentInParent<StatManager>();
        rigid = GetComponent<Rigidbody2D>();
    }

    public void SetPool(IObjectPool<PlayerProjectile> pool)
    {
        this.pool = pool;
    }

    public void Init(Vector2 _dir, float _speed, float _range)
    {
        direction = _dir.normalized;
        speed = _speed;
        range = _range; 
        startAttackPos = transform.position;

        //크리티컬 계산식
        float baseAtk = statManager.GetValue(StatType.Attack);
        float critChance = statManager.GetValue(StatType.CriticalChance); // 0~100
        float critMultiplier = statManager.GetValue(StatType.CriticalMultiplier);

        isCritical = UnityEngine.Random.Range(0, 100) < critChance;
        calcDamage = isCritical ? baseAtk * critMultiplier : baseAtk;

        damage = new CalculateStat(StatType.Attack, calcDamage);

        gameObject.SetActive(true);
    }

    private void FixedUpdate()
    {
        rigid.velocity = direction * speed;

        float shootingRange = Vector2.Distance(startAttackPos, transform.position);
        if (shootingRange >= range)
            ReturnToPool(); // 일정 시간 지나면 반환
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out var target) && !other.CompareTag("Player"))
        {
            if (isCritical)
            {
                Logger.Log("Critical Hit");
            }
            var player = FindObjectOfType<PlayerController>();
            player.ShowDamageText(calcDamage, 
                other.transform.position,
                isCritical ? Color.yellow : Color.red);

            target.TakeDamage(this);
            pool?.Release(this);
        }
    }

    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Release(this); // 풀로 되돌림
        }
        else
        {
            Destroy(gameObject); // 풀 없을 시 직접 파괴 (예외 상황)
        }
    }

    private void OnDisable()
    {
        rigid.velocity = Vector2.zero;
        CancelInvoke();
    }

    public void Attack() { }
}