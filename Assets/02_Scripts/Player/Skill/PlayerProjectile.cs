using _02_Scripts.Player.Effect;
using PlayerStates;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour, IAttackable, IPoolObject
{
    private StatManager statManager;
    private StatBase damage;
    public StatBase AttackStat => damage;
    public IDamageable Target => null;
    private Rigidbody2D rigid;
    
    private float speed;
    private float calcDamage; 
    private float range;

    private Vector3 direction;
    private Vector2 startAttackPos;

    private bool isCritical;
    
    public GameObject GameObject => gameObject;

    [SerializeField] private string poolID = "playerProjectile";
    public string PoolID => poolID;
    [SerializeField] private int poolSize = 5;
    public int PoolSize => poolSize;
    
    private EffectTable effectTable;
    

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        effectTable= TableManager.Instance.GetTable<EffectTable>();
        effectTable.CreateTable();
    }

    public void Init(StatManager _statManager, Vector2 _dir, float _speed, float _range)
    {
        statManager = _statManager;
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
            ObjectPoolManager.Instance.ReturnObject(gameObject); // 일정 시간 지나면 반환
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


            var effectData = effectTable.GetDataByID(1);
            if (!effectData.IsUnityNull())
            {
                var effectObj = ObjectPoolManager.Instance.GetObject(effectData.poolID);
                if (!effectObj.IsUnityNull() && effectObj.TryGetComponent<ImpactEffect>(out var impactEffect))
                {
                    var hit = other.ClosestPoint(transform.position);
                    impactEffect.PlayEffect(hit, effectData.duration);
                    SoundManager.Instance.ChangeSFXVolume(0.1f);
                    SoundManager.Instance.PlaySFX(SFX.SlimeNormalAttack);
                }
            }
            
            target.TakeDamage(this, gameObject);
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
    }
    
    public void Attack() { }

    private void OnDisable()
    {
        rigid.velocity = Vector2.zero;
        CancelInvoke();
    }
    
    public void OnSpawnFromPool()
    {
        statManager = GetComponentInParent<StatManager>();
        gameObject.SetActive(true);
    }

    public void OnReturnToPool()
    {
        gameObject.SetActive(false);
    }
}