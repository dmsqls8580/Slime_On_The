using System;
using System.Collections;
using System.Collections.Generic;
using Enemyststes;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : BaseController<EnemyController, EnemyState>, IDamageable
{
    [SerializeField] private EnemySO enemySo;
    [SerializeField] private Collider2D senseRangeCollider;
    [SerializeField] private Collider2D attackRangeCollider;
    
    public bool IsDead { get; }
    
    public Collider2D Collider { get; }
    
    public GameObject ChaseTarget;                         // 인식된 플레이어, 추격
    public bool isDead           { get; private set; }     // 사망 여부
    public Animator Animator     { get; private set; }     // 애니메이터
    public Vector3 SpawnPos      { get; private set; }     // 스폰 위치
    public float WanderRadius    { get; private set; }     // 배회 반경
    public float SenseRange      { get; private set; }     // 감지 범위
    public float MinMoveDelay    { get; private set; }     // Idle 상태 최소 지속 시간
    public float MaxMoveDelay    { get; private set; }     // Idle 상태 최대 지속 시간
    public float AttackRange     { get; private set; }     // 공격 범위
    public float AttackCooldown  { get; private set; }     // 공격 쿨타임
    public NavMeshAgent Agent    { get; private set; }

    private float attackCooldownTimer;
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private SpriteRenderer spriteRenderer;
    
    protected override void Awake()
    {
        base.Awake();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChaseTarget = other.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChaseTarget = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            
            // 콜라이더가 겹치면 ChaseTarget이 null이 되는 오류
            // 플레이어가 감지 범위보다 멀리 있어야 null이 되도록 수정
            if (distance > SenseRange)
            {
                ChaseTarget = null;
            }
        }
    }

    // EnemySO에서 데이터를 가져와 몬스터 인스턴스에 적용
    private void SetEnemySOData()
    {
        WanderRadius = enemySo.WanderRadius;
        SenseRange = enemySo.SenseRange;
        MinMoveDelay = enemySo.MinMoveDelay;
        MaxMoveDelay = enemySo.MaxMoveDelay;
        Agent.speed = enemySo.MoveSpeed;
        AttackRange = enemySo.AttackRange;
        AttackCooldown = enemySo.AttackCooldown;
        
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

    public void SetAttackCooldown()
    {
        attackCooldownTimer = AttackCooldown;
    }

    public bool CanAttack()
    {
        attackCooldownTimer -= Time.deltaTime;
        return attackCooldownTimer <= 0f;
    }

    
    public void TakeDamage(IAttackable attacker)
    {
        throw new NotImplementedException();
    }

    public void Dead()
    {
        throw new NotImplementedException();
    }
}
