using System;
using System.Collections;
using System.Collections.Generic;
using Enemyststes;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : BaseController<EnemyController, EnemyState>, IDamageable, IAttackable, IPoolObject
{
    [SerializeField] private EnemySO enemySO;
    [SerializeField] private Collider2D senseRangeCollider;
    [SerializeField] private Collider2D attackRangeCollider;
    
    public GameObject ChaseTarget;                         // 인식된 플레이어, 추격
    public EnemyState PreviousState      { get; set; }     // 이전 State
    public Animator Animator     { get; private set; }     // 애니메이터

    public float MaxHealth;                                // 최대 최력
    public float CurrentHealth;                            // 현재 체력
    public float AttackDamage    { get; private set; }     // 공격 데미지
    public Vector3 SpawnPos      { get; private set; }     // 스폰 위치
    public float WanderRadius    { get; private set; }     // 배회 반경
    public float SenseRange      { get; private set; }     // 감지 범위
    public float MinMoveDelay    { get; private set; }     // Idle 상태 최소 지속 시간
    public float MaxMoveDelay    { get; private set; }     // Idle 상태 최대 지속 시간
    public float AttackRange     { get; private set; }     // 공격 범위
    public float AttackCooldown  { get; private set; }     // 공격 쿨타임
    public NavMeshAgent Agent    { get; private set; }     // 
    public bool IsPlayerInAttackRange {get; private set; } // 플레이어 공격 범위 내 존재 여부
    
    private float attackCooldownTimer;                     // 몬스터 공격 속도 타이머
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private SpriteRenderer spriteRenderer;                 // 몬스터 스프라이트 (보는 방향에 따라 수정) 
    
    /************************ IDamageable ***********************/
    public bool IsDead { get; private set; }               // 사망 여부
    
    public Collider2D Collider { get; private set; }       // 몬스터 피격 콜라이더
    
    // Enemy 공격 메서드
    public void TakeDamage(IAttackable attacker)
    {
        if (IsDead) return;
        if (attacker != null)
        {
            // CurrentHealth -= attacker.AttackDamage;
            if (CurrentHealth <= 0)
            {
                Dead();
            }
        }
    }
    
    // Enemy 사망 여부 판별
    public void Dead()
    {
        if (CurrentHealth <= 0)
        {
            IsDead = true;
            ChangeState(EnemyState.Dead);
            // TODO: 오브젝트 풀 반환
        }
        
    }
    
    /************************ IAttackable ***********************/
    public IDamageable Target => ChaseTarget != null ? 
        ChaseTarget.GetComponent<IDamageable>() : null;

    public void Attack()
    {
        if (Target != null && !Target.IsDead)
        {
            Target.TakeDamage(this);
        }
    }
    
    /************************ IPoolObject ***********************/
    public GameObject GameObject => this.gameObject;
    public string PoolID => enemySO.EnemyIDX;
    public int PoolSize { get; }
    
    public void OnSpawnFromPool()
    {
        // Enemy 상태, 위치, NavMeshAgent, FSM 등 모든 초기화
        IsDead = false;
        CurrentHealth = MaxHealth;
        transform.position = SpawnPos; // 혹은 원하는 위치
        if (Agent.isOnNavMesh) Agent.Warp(transform.position);
        ChangeState(EnemyState.Idle);
    }

    public void OnReturnToPool()
    {
        // 상태 정리, Agent.ResetPath() 등
        Agent.ResetPath();
        gameObject.SetActive(false);
    }
    
    
    
    protected override void Awake()
    {
        base.Awake();
        Collider = GetComponent<Collider2D>();
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;                       // NavMeshAgent는 월드의 수직방향으로 생성되기 때문에
        Agent.updateUpAxis = false;                         // 회전 비활성화
        Animator = GetComponentInChildren<Animator>();
        spriteRenderer =  GetComponentInChildren<SpriteRenderer>();
    }

    protected override void Start()
    {
        base.Start();
        SpawnPos = transform.position;
        ChangeState(EnemyState.Idle);
        SetEnemySOData();
    }

    protected override void Update()
    {
        base.Update();
        
        Debug.Log(CurrentState);
        
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
        
        // Enemy의 이동 방향에 따라 SpriteRenderer flipX
        spriteRenderer.flipX = lastFlipX; 
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
    

    // EnemySO에서 데이터를 가져와 몬스터 인스턴스에 적용
    private void SetEnemySOData()
    {
        // EnemyStatus
        IsDead = false;
        MaxHealth = enemySO.Health;
        CurrentHealth = MaxHealth;
        AttackDamage = enemySO.AttackDamage;
        
        // EnemyMove
        WanderRadius = enemySO.WanderRadius;
        SenseRange = enemySO.SenseRange;
        MinMoveDelay = enemySO.MinMoveDelay;
        MaxMoveDelay = enemySO.MaxMoveDelay;
        Agent.speed = enemySO.MoveSpeed;
        AttackRange = enemySO.AttackRange;
        AttackCooldown = enemySO.AttackCooldown;
        
        // SenseRange 값을 senseRangeCollider의 반지름에 적용
        if (senseRangeCollider != null && senseRangeCollider is CircleCollider2D)
        {
            CircleCollider2D senseCircle = (CircleCollider2D)senseRangeCollider;
            senseCircle.radius = SenseRange;
        }
        
        // AttackRange 값을 attackRangeCollider의 반지름에 적용
        if (attackRangeCollider != null && attackRangeCollider is CircleCollider2D)
        {
            CircleCollider2D attackCircle  = (CircleCollider2D)attackRangeCollider;
            attackCircle.radius = AttackRange;
            attackCircle.offset = new Vector2(AttackRange, 0);

        }
    }
    
    /*
    //  Enemy 공격 사이 대기 시간 초기화
    public void SetAttackCooldown()
    {
        attackCooldownTimer = AttackCooldown;
    }

    // 몬스터 공격 가능 여부 판별
    public bool CanAttack()
    {
        attackCooldownTimer -= Time.deltaTime;
        return attackCooldownTimer <= 0f;
    }
    */
    
    // 플레이어가 Enemy 공격 범위 진입 여부 메서드 추가
    public void SetPlayerInAttackRange(bool inRange)
    {
        IsPlayerInAttackRange = inRange;
    }
    
}
