using BossStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : BaseController<BossController, BossState>, IDamageable, IAttackable, IPoolObject
{
    [SerializeField] private Collider2D senseRangeCollider;
    [SerializeField] private Collider2D attackRangeCollider;
    
    public BossStatus BossStatus;
    public GameObject ChaseTarget;                         // 인식된 플레이어, 추격
    public GameObject AttackTarget;                        // 공격 대상, 보스의 경우 다음 패턴 진행을 위한 조건
    
    public Vector3 SpawnPos      { get;  set; }            // 스폰 위치
    public Animator Animator     { get; private set; }     // 애니메이터
    public NavMeshAgent Agent    { get; private set; }     // NavMesh Agent
    public bool IsPlayerInAttackRange {get; private set; } // 플레이어 공격 범위 내 존재 여부
    public float IdleDuration => BossStatus.BossSO.IdleDuration;
    public float Cast1Duration => BossStatus.BossSO.Cast1Duration;
    public float Cast2Duration  => BossStatus.BossSO.Cast2Duration;
    public float StompDuration  => BossStatus.BossSO.StompDuration;
    
    private StatManager statManager;
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private SpriteRenderer spriteRenderer;                 // 몬스터 스프라이트 (보는 방향에 따라 수정) 
    
    
    /************************ IDamageable ***********************/
    public bool IsDead { get; }
    public Collider2D Collider { get; }
    public void TakeDamage(IAttackable _attacker)
    {
        
    }

    public void Dead()
    {
        
    }

    /************************ IAttackable ***********************/
    public StatBase AttackStat { get; }
    public IDamageable Target { get; }
    public void Attack()
    {
        
    }

    /************************ IPoolObject ***********************/
    public GameObject GameObject { get; }
    public string PoolID { get; }
    public int PoolSize { get; }
    public void OnSpawnFromPool()
    {
        
    }

    public void OnReturnToPool()
    {
        
    }
    
    /************************ BossController ***********************/

    protected override void Awake()
    {
        base.Awake();
        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;                       // NavMeshAgent는 월드의 수직방향으로 생성되기 때문에
        Agent.updateUpAxis = false;                         // 회전 비활성화
        Animator = GetComponent<Animator>();
        spriteRenderer =  GetComponent<SpriteRenderer>();
        BossStatus = GetComponent<BossStatus>();
        statManager = GetComponent<StatManager>();
    }
    
    protected override void Start()
    {
        base.Start();
        ChangeState(BossState.Idle);
        
        // SpawnPos에 현재 위치 저장
        SpawnPos = transform.position;
        // Collider에 Range 적용
        BossStatus.InitCollider(senseRangeCollider, attackRangeCollider);
        // Agent 이동 속도 적용
        Agent.speed = BossStatus.MoveSpeed;
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
        if (AttackTarget != null)
        {
            Vector2 targetDir = ChaseTarget.transform.position - transform.position;
            lastFlipX = targetDir.x < 0;
        }
        
        // Enemy의 이동 방향에 따라 SpriteRenderer flipX
        spriteRenderer.flipX = lastFlipX; 
    }
    
    protected override IState<BossController, BossState> GetState(BossState state)
    {
        return state switch
        {
            BossState.Idle => new IdleState(),
            BossState.Wander => new WanderState(),
            BossState.Chase => new ChaseState(),
            BossState.Pattern1 => new Pattern1State(),
            BossState.Pattern2 => new Pattern2State(),
            BossState.Pattern3 => new Pattern3State(),
            BossState.Dead => new DeadState(),
            _ => null
        };
    }

    public override void FindTarget()
    {
        
    }
    
    public void SetPlayerInAttackRange(bool _inRange)
    {
        IsPlayerInAttackRange = _inRange;
    }
    
    public void EnterRandomPattern()
    {
        // Pattern1, Pattern2, Pattern3 중 랜덤 선택
        BossState[] patterns = { BossState.Pattern1, BossState.Pattern2, BossState.Pattern3 };
        BossState nextPattern = patterns[Random.Range(0, patterns.Length)];
        ChangeState(nextPattern);
    }
}
