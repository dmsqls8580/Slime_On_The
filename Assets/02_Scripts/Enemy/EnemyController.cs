using System;
using Enemyststes;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : BaseController<EnemyController, EnemyState>, IDamageable, IAttackable, IPoolObject
{
    [SerializeField] private Collider2D senseRangeCollider;
    [SerializeField] private Collider2D attackRangeCollider;
    
    public EnemyStatus EnemyStatus;
    
    public GameObject ChaseTarget;                         // 인식된 플레이어, 추격
    public EnemyState PreviousState      { get; set; }     // 이전 State
    public Animator Animator     { get; private set; }     // 애니메이터

    public Vector3 SpawnPos      { get; private set; }     // 스폰 위치
    public NavMeshAgent Agent    { get; private set; }     // NavMesh Agent
    public bool IsPlayerInAttackRange {get; private set; } // 플레이어 공격 범위 내 존재 여부
    
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private SpriteRenderer spriteRenderer;                 // 몬스터 스프라이트 (보는 방향에 따라 수정) 
    
    
    /************************ IDamageable ***********************/
    public bool IsDead => EnemyStatus.IsDead;                  // 사망 여부

    public Collider2D Collider => GetComponent<Collider2D>();  // 몬스터 피격 콜라이더
    
    // Enemy 공격 메서드
    public void TakeDamage(IAttackable attacker)
    {
        if (IsDead) return;
        if (attacker != null)
        {
            // 피격
            EnemyStatus.TakeDamage(attacker.AttackStat.GetCurrent(),StatModifierType.Base);
            if (EnemyStatus.CurrentHealth <= 0)
            {
                Dead();
            }
        }
    }
    
    // Enemy 사망 여부 판별
    public void Dead()
    {
        if (EnemyStatus.CurrentHealth <= 0)
        {
            ChangeState(EnemyState.Dead);
            // TODO: 오브젝트 풀 반환
        }
        
    }
    
    /************************ IAttackable ***********************/
    // Todo
    // AttackStat, Target 수정해서 데미지 실제로 적용되도록 하기
    public StatBase AttackStat { get; }

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
    public string PoolID { get; }
    public int PoolSize { get; }
    
    public void OnSpawnFromPool()
    {
        // Todo
        // 몬스터 스폰 시 Enemy 상태, 위치, NavMeshAgent, FSM 등 모든 초기화
        
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
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;                       // NavMeshAgent는 월드의 수직방향으로 생성되기 때문에
        Agent.updateUpAxis = false;                         // 회전 비활성화
        Animator = GetComponent<Animator>();
        spriteRenderer =  GetComponent<SpriteRenderer>();
        EnemyStatus = GetComponent<EnemyStatus>();
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
    
    
    // 플레이어가 Enemy 공격 범위 진입 여부 메서드 추가
    public void SetPlayerInAttackRange(bool inRange)
    {
        IsPlayerInAttackRange = inRange;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.TryGetComponent(out IAttackable attackable ))
        {
            TakeDamage(attackable);
        }
    }
}
