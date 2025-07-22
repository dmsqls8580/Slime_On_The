using System;
using Enemystates;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyController : BaseController<EnemyController, EnemyState>, IDamageable, IAttackable, IPoolObject
{
    [SerializeField] private Collider2D senseRangeCollider;
    [SerializeField] private Collider2D attackRangeCollider;
    
    public Transform projectileTransform;                  // 발사체 생성 Transform
    
    public EnemyStatus EnemyStatus;                        // EnemyStatus
    
    public GameObject AttackTarget;                        // 공격 대상, 인스펙터에서 확인하기 위해 GameObject로 설정
    public EnemyState PreviousState      { get; set; }     // 이전 State
    public Vector3 SpawnPos      { get;  set; }            // 스폰 위치
    public Animator Animator     { get; private set; }     // 애니메이터
    public NavMeshAgent Agent    { get; private set; }     // NavMesh Agent
    public bool IsPlayerInAttackRange {get; private set; } // 플레이어 공격 범위 내 존재 여부
    
    public bool IsAttacked = false;  // 중립 몬스터가 공격받았는지 여부
    
    private StatManager statManager;
    private Rigidbody2D dropItemRigidbody;
    private SpriteRenderer spriteRenderer;                 // 몬스터 스프라이트 (보는 방향에 따라 수정) 
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private float neutralTargetResetTime = 10f;            // 중립 몬스터 타겟 리셋 타이머
    private Coroutine aggroCoroutine;
    public Dictionary<GameObject, float> AttackTargets = new Dictionary<GameObject, float>();
    
    /************************ Item Drop ***********************/
    [Header("Drop Item Prefab")]
    [SerializeField]private GameObject dropItemPrefab; //DropItem 스크립트가 붙은 빈 오브젝트 프리팹
    private List<DropItemData> dropItems => EnemyStatus.enemySO.DropItems;
    
    private float dropUpForce = 3f;
    private float dropSideForce = 2f;
    private float dropAngleRange = 60f;
    
    /************************ IDamageable ***********************/
    public bool IsDead => EnemyStatus.IsDead;                  // 사망 여부
    public Collider2D Collider => GetComponent<Collider2D>();  // 몬스터 피격 콜라이더
    
    // Enemy 피격 메서드
    public void TakeDamage(IAttackable _attacker,  GameObject _attackerObj)
    {
        if (IsDead)
        {
            return;
        }
        if (_attacker != null)
        {
            // 피격
            if (EnemyStatus == null)
            {
                Logger.Log("EnemyStatus is null");
            }

            if (_attacker.AttackStat == null)
            {
                Logger.Log("AttackStat is null");
            }
            EnemyStatus.TakeDamage(_attacker.AttackStat.GetCurrent(),StatModifierType.Base);
            
            // 어그로 수치 증가, 피격 시 30 증가
            if (_attackerObj != null)
            {
                ModifyAggro(_attackerObj, 30f);
                // 어그로 코루틴 시작
                StartAggroCoroutine();
            }
            
            if (EnemyStatus.enemySO.AttackType == EnemyAttackType.Neutral)
            {
                IsAttacked = true;
            }
            
            if (EnemyStatus.CurrentHealth <= 0)
            {
                Dead();
            }
        }
        else
        {
            Logger.Log("No attacker found");
        }
    }
    
    // Enemy 사망 여부 판별
    public void Dead()
    {
        if (EnemyStatus.CurrentHealth <= 0)
        {
            ChangeState(EnemyState.Dead);
            
            // 현재 위치에서 아이템 드롭
            DropItems(this.gameObject.transform);
                
            // 오브젝트 풀 반환
            // Todo : 몬스터 사망 후 풀로 반횐될 때까지 시간 const로 만들어주기
            ObjectPoolManager.Instance.ReturnObject(gameObject, 2f);
        }
        
    }
    
    /************************ IAttackable ***********************/
    public StatBase AttackStat => StatManager.Stats[StatType.Attack];

    public IDamageable Target 
        => AttackTarget.TryGetComponent<IDamageable>(out var damageable)? damageable : null;

    public void Attack()
    {
        if (Target != null && !Target.IsDead && IsPlayerInAttackRange)
        {
            Target.TakeDamage(this, this.gameObject);
        }
    }
    
    /************************ IPoolObject ***********************/
    public GameObject GameObject => this.gameObject;
    public string PoolID => EnemyStatus.enemySO != null
        ? EnemyStatus.enemySO.EnemyID.ToString() : "Invalid";
    public int PoolSize { get; } = 10;
    
    public void OnSpawnFromPool()
    {
        statManager.Init(EnemyStatus.enemySO);
        
        // 상태머신이 초기화되지 않았다면 초기화
        if (stateMachine == null || states == null)
        {
            SetupState();
        }
        ChangeState(EnemyState.Idle);
        
        transform.position = SpawnPos; // 혹은 원하는 위치
        if (Agent.isOnNavMesh)
        {
            Agent.Warp(transform.position);
        }
        
    }

    public void OnReturnToPool()
    {
        gameObject.SetActive(false);
    }
    
    /************************ EnemyController ***********************/
    
    
    protected override void Awake()
    {
        base.Awake();
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;                       // NavMeshAgent는 월드의 수직방향으로 생성되기 때문에
        Agent.updateUpAxis = false;                         // 회전 비활성화
        Animator = GetComponent<Animator>();
        spriteRenderer =  GetComponent<SpriteRenderer>();
        EnemyStatus = GetComponent<EnemyStatus>();
        statManager = GetComponent<StatManager>();
    }

    protected override void Start()
    {
        base.Start();
        ChangeState(EnemyState.Idle);
        
        // SpawnPos에 현재 위치 저장
        SpawnPos = transform.position;
        // Collider에 Range 적용
        EnemyStatus.InitCollider(senseRangeCollider, attackRangeCollider);
        // Agent 이동 속도 적용
        Agent.speed = EnemyStatus.MoveSpeed;
    }

    protected override void Update()
    {
        base.Update();
        
        Vector2 moveDir = Agent.velocity.normalized; // velocity는 목적지로 향하는 방향, 속도
        float velocityMagnitude = Agent.velocity.magnitude;
        
        // 이동 중일 때만 각도/flipX 갱신
        if (velocityMagnitude > 0.01f)
        {
            lastAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            lastFlipX = Agent.velocity.x < 0;
        }
        
        // 멈췄을 때는 마지막 값을 유지 (멈추면 velocity가 0이 되기 때문에 마지막 값을 기억해 각도와 방향 지정 
        attackRangeCollider.transform.localRotation = Quaternion.Euler(0, 0, lastAngle);
        
        // AttackTarget이 존재하는 경우, 그 방향으로 각도 갱신
        if (AttackTarget != null && EnemyStatus.enemySO.AttackType != EnemyAttackType.Neutral)
        {
            Vector2 targetDir = AttackTarget.transform.position - transform.position;
            lastFlipX = targetDir.x < 0;
        }
        
        // Enemy의 이동 방향에 따라 SpriteRenderer flipX, AttackType이 None인 경우 반대로 flip
        if (EnemyStatus.enemySO.AttackType == EnemyAttackType.None)
        {
            spriteRenderer.flipX = !lastFlipX; 
        }
        else
        {
            spriteRenderer.flipX = lastFlipX; 
        }
        
    }
    
    protected override IState<EnemyController, EnemyState> GetState(EnemyState state)
    {
        return state switch
        {
            EnemyState.Idle => new IdleState(),
            EnemyState.Wander => new WanderState(),
            EnemyState.Chase => new ChaseState(),
            EnemyState.Attack => new AttackState(),
            EnemyState.Dead => new DeadState(),
            _ => null
        };
    }
    
    public override void FindTarget()
    {
        
    }
    
    // 플레이어가 Enemy 공격 범위 진입 여부 메서드 추가
    public void SetPlayerInAttackRange(bool _inRange)
    {
        IsPlayerInAttackRange = _inRange;
    }
    
    // 애니메이션 이벤트로 호출
    public void ShootProjectile()
    {
        string projectileID = EnemyStatus.enemySO.ProjectileID.ToString();
        GameObject projectileObject = ObjectPoolManager.Instance.GetObject(projectileID);
        projectileObject.transform.position = projectileTransform.position;
        
        // 애니메이션 실행 시점과 애니메이션 이벤트 호출 타이밍 사이
        // 플레이어가 AttackTarget에서 벗어나는 문제로 인해 새로운 게임 오브젝트 SensedAttackTarget 추가
        // SensedAttackTarget을 이용해 초기화
        if (projectileObject.TryGetComponent<ProjectileBase>(out var projectile)
            && !AttackTarget.IsUnityNull())
        {
            Vector2 shootdir = AttackTarget.transform.position - projectileTransform.position;
            Vector2 direction = shootdir.normalized;
            projectile.Init(direction, AttackStat, gameObject, EnemyStatus.AttackRadius);
            Logger.Log("Init 호출");
        }
        else
        {
            Logger.Log("SensedAttackTarget == null");
        }
    }
    
    // 아이템 드롭 메서드
    private void DropItems(Transform transform)
    {
        float randomChance = Random.value;
        Transform itemTarget = AttackTarget.transform;
        
        if (dropItems.IsUnityNull() || dropItemPrefab.IsUnityNull())
        {
            return;
        }
        
        foreach (var item in dropItems)
        {
            if (randomChance * 100f > item.dropChance)
            {
                continue;
            }
            for (int i = 0; i < item.amount; i++)
            {
                var dropObj = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
                var itemDrop = dropObj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.Init(item.itemSo,1, itemTarget);
                    
                }
                
                dropItemRigidbody= dropObj.GetComponent<Rigidbody2D>();
                itemDrop.DropAnimation(dropItemRigidbody, dropAngleRange, dropUpForce, dropSideForce); 
            } 
        }
    }
    
    // 몬스터 어그로 코루틴 시작
    public void StartAggroCoroutine()
    {
        if (aggroCoroutine != null)
        {
            StopCoroutine(aggroCoroutine);
        }

        aggroCoroutine = StartCoroutine(DecreaseAggroValue());
    }
    
    // 1초에 어그로 수치 2씩 감소 
    private IEnumerator DecreaseAggroValue()
    {
        while (true)
        {
            var keys = new List<GameObject>(AttackTargets.Keys);
            foreach (var key in keys)
            {
                if (key != null && AttackTargets.ContainsKey(key))
                {
                    ModifyAggro(key, -2f);
                    float afterAggro = 0f;
                    if (AttackTargets.TryGetValue(key, out afterAggro))
                    {
                        Logger.Log(afterAggro.ToString());
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
    
    // 어그로 계산 메서드
    public void ModifyAggro(GameObject aggroObject, float value)
    {
        if (AttackTargets.ContainsKey(aggroObject))
        {
            AttackTargets[aggroObject] += value;
        }
        else
        {
            AttackTargets[aggroObject] = value;
        }
        
        AttackTargets[aggroObject] = Mathf.Clamp(AttackTargets[aggroObject], 0f, 100f);

        if (AttackTargets[aggroObject] == 0f)
        {
            AttackTargets.Remove(aggroObject);
        }
        
        UpdateAggro();
    }

    public void SetAggro(GameObject aggroObject, float value)
    {
        AttackTargets[aggroObject] = Mathf.Clamp(value, 0f, 100f);

        if (AttackTargets[aggroObject] == 0f)
        {
            AttackTargets.Remove(aggroObject);
        }
        UpdateAggro();
    }
    
    // 공격 타겟 업데이트
    // 딕셔너리 내 가장 높은 어그로 수치를 가진 오브젝트를 AttackTarget으로 설정
    private void UpdateAggro()
    {
        GameObject maxValueTarget = null;
        float maxvalue = 0f;
        foreach (var target in AttackTargets)
        {
            if (target.Key != null && target.Value >= maxvalue)
            {
                maxvalue = target.Value;
                maxValueTarget = target.Key;
            }
        }
        AttackTarget = maxValueTarget;
        
        if (maxvalue <= 0)
        {
            IsAttacked = false;
        }
    }
    
    
}
