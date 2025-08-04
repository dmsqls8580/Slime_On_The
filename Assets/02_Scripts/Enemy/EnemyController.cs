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
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Canvas damageTextCanvas;
    
    public Transform projectileTransform;                  // 발사체 생성 Transform
    
    public EnemyStatus EnemyStatus;                        // EnemyStatus
   
    private GameObject attackTarget;
    public GameObject AttackTarget                         // 공격 대상, 인스펙터에서 확인하기 위해 GameObject로 설정
    {
        get => attackTarget;
        private set => attackTarget = value;  // 내부에서만 설정 가능 (혹은 protected)
    } 
    public EnemyState PreviousState      { get; set; }     // 이전 State
    public Vector3 SpawnPos      { get;  set; }            // 스폰 위치
    public Animator Animator     { get; private set; }     // 애니메이터
    public NavMeshAgent Agent    { get; private set; }     // NavMesh Agent
    public SpriteCuller SpriteCuller { get; private set; }
    public bool IsPlayerInSenseRange { get; private set; } // 플레이어 인식 범위 내 존재 여부
    public bool IsIDamageableInAttackRange {get; private set; } // 공격 대상 공격 범위 내 존재 여부
    
    private bool isAttackCooldown = false;
    public bool IsAttackCooldown => isAttackCooldown;

    // 현재 텔레포트하고 있는지 여부
    private bool isTeleporting = false;
    public bool IsTeleporting => isTeleporting;
    
    // 현재 대쉬하고 있는지 여부
    private bool isDashing = false;
    public bool IsDashing => isDashing;
    
    // 현재 자폭하고 있는지 여부
    private bool isBombing;
    public bool IsBombing  => isBombing;
    
    private float attackCooldownTimer = 0f;
    
    private StatManager statManager;
    private Rigidbody2D dropItemRigidbody;
    private SpriteRenderer spriteRenderer;                 // 몬스터 스프라이트 (보는 방향에 따라 수정) 
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private float curAccelation;                           // 몬스터 Agent 현재 가속도
    private float lastDistanceToTarget = float.MaxValue;
    private float distanceChangeThreshold = 0.3f;          // 최소 변화 거리

    /************************ AggroSystem ***********************/
    
    [Header("Aggro Settings")] 
    public AggroSystem Aggro;
    [SerializeField] private float stickTime = 1f;             // 타겟 최소 유지 시간
    [SerializeField] private float decreaseDelayValue = 2f;    // 초당 감소하는 어그로 수치
    [SerializeField] private float decreaseDelayTime = 1f;     // 어그로 수치 감소 사이 시간
    [SerializeField] private float hitAggroValue = 30f;        // 피격 시 추가 어그로 (플레이어/몬스터 공통)
    
    public Coroutine aggroCoroutine;
    
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
            EnemyStatus.TakeDamage(_attacker.AttackStat.GetCurrent(), StatModifierType.Base);
            
            // 어그로 수치 증가, 피격 시 30 증가
            if (_attackerObj != null)
            {
                Aggro.ModifyAggro(_attackerObj, hitAggroValue);
                // 코루틴이 없을 때만 시작 (중복 실행 방지)
                if (aggroCoroutine == null)
                {
                    aggroCoroutine = StartCoroutine(DecreaseAggroValue());
                }
            }
            
            if (EnemyStatus.CurrentHealth <= 0)
            {
                Dead();
            }
            StartCoroutine(ShakeIDamageable());
        }

    }
    
    // Enemy 사망 여부 판별
    public void Dead()
    {
        ChangeState(EnemyState.Dead);
            
        // 현재 위치에서 아이템 드롭
        DropItems(this.gameObject.transform);
                
        // 오브젝트 풀 반환
        SpriteCuller.Spawner.RemoveObject(gameObject, 2f);
        
    }
    
    private IEnumerator ShakeIDamageable()
    {
        float timer = 0f;
        float shakeDuration = 0.2f;
        Vector2 currentPos = transform.position;
        Color currentColor = spriteRenderer.color;
        
        while (timer <= shakeDuration)
        {
            // shakeDuration 동안 몬스터 흔들림, 붉은색 피격 모션
            float offsetX = Random.Range(-1f, 1f) * 0.1f;
            float offsetY = Random.Range(-1f, 1f) * 0.1f;
            transform.position = new Vector2(currentPos.x + offsetX, currentPos.y + offsetY);
            
            spriteRenderer.color = new Color(1f, 200/255f,200/255f,1);
            
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = currentPos;
        spriteRenderer.color = currentColor;
    }
    
    /************************ IAttackable ***********************/
    public StatBase AttackStat
    {
        get
        {
            if (statManager == null) return null;
            if (statManager.Stats.TryGetValue(StatType.Attack, out StatBase stat))
            {
                return stat;
            }
            return null;
        }
    }

    public IDamageable Target 
        => AttackTarget.TryGetComponent<IDamageable>(out var damageable)? damageable : null;

    public void Attack()
    {
        if (Target != null && !Target.IsDead && IsIDamageableInAttackRange)
        {
            Target.TakeDamage(this, this.gameObject);
            float cooldown = StatManager.Stats[StatType.AttackCooldown].GetCurrent();
            StartAttackCooldown(cooldown);
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
        ResetAttackState();
        
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

    private void ResetAttackState()
    {
        Aggro.Clear();
        AttackTarget = null;
        IsIDamageableInAttackRange = false;
        isAttackCooldown = false;
        attackCooldownTimer = 0f;
        if (aggroCoroutine != null)
        {
            StopCoroutine(aggroCoroutine);
            aggroCoroutine = null;
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
        SpriteCuller = GetComponent<SpriteCuller>();
        Aggro = new AggroSystem(EnemyStatus.enemySO.AttackType,
            target => IsPlayerInSenseRange,stickTime);
        Aggro.OnTargetChanged += OnAggroTargetChanged;
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

        Spriteflip();

        if (isAttackCooldown)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0f)
            {
                isAttackCooldown = false;
                attackCooldownTimer = 0f;
            }
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

    private void Spriteflip()
    {
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
        if (AttackTarget != null && EnemyStatus.enemySO.AttackType != AttackType.Neutral)
        {
            Vector2 targetDir = AttackTarget.transform.position - transform.position;
            lastFlipX = targetDir.x < 0;
        }
        
        // Enemy의 이동 방향에 따라 SpriteRenderer flipX, AttackType이 None인 경우, ChaseState를 제외하고 반대로 flip
        if (EnemyStatus.enemySO.AttackType == AttackType.None
            && CurrentState != EnemyState.Chase)
        {
            spriteRenderer.flipX = !lastFlipX; 
        }
        else
        {
            spriteRenderer.flipX = lastFlipX; 
        }
    }
    
    public bool IsEnoughDistanceChange()
    {
        if (AttackTarget == null) return false;

        float currentDist = Vector2.Distance(transform.position, AttackTarget.transform.position);
        float diff = Mathf.Abs(currentDist - lastDistanceToTarget);
        lastDistanceToTarget = currentDist;

        return diff >= distanceChangeThreshold;
    }

    public void StartAttackCooldown(float cooldown)
    {
        isAttackCooldown = true;
        attackCooldownTimer = cooldown;
    }
    
    // 공격 대상이 Enemy 공격 범위 진입 여부 메서드
    // 플레이어뿐만 아니라 몬스터도 공격 대상이 될 수 있음
    public void SetIDamageableInAttackRange(bool _inRange)
    {
        IsIDamageableInAttackRange = _inRange;
    }
    
    // 플레이어가 Enemy 인식 범위 진입 여부 메서드
    // 인식 범위 진입 여부는 플레이어만 판별
    public void SetPlayerInSenseRange(bool _inRange)
    {
        IsPlayerInSenseRange = _inRange;
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
            && AttackTarget != null)
        {
            Vector2 shootdir = AttackTarget.transform.position - projectileTransform.position;
            Vector2 direction = shootdir.normalized;
            projectile.Init(direction, AttackStat, gameObject, EnemyStatus.AttackRadius);
        }
        else
        {
            ObjectPoolManager.Instance.ReturnObject(projectileObject);
        }
    }

    public void SelfBombStart()
    {
        isBombing = true;
    }

    public void SelfBombEnd()
    {
        isBombing = false;
        // 사망
        ChangeState(EnemyState.Dead);
                
        // 오브젝트 풀 반환
        SpriteCuller.Spawner.RemoveObject(gameObject, 0.1f);
    }

    public void TeleportStart()
    {
        if (AttackTarget != null)
        {
            isTeleporting = true;
            
            // 플레이어 피봇이 아래에 있기 때문에 위치 조정
            Vector3 currentPlayerPos = AttackTarget.transform.position + Vector3.up;
            
            // 플레이어 왼쪽 or 오른쪽 방향 랜덤으로 선택
            float offsetX = Random.value < 0.5f 
                ? -EnemyStatus.AttackRange
                : EnemyStatus.AttackRange;
            Vector3 spawnPos = currentPlayerPos + new Vector3(offsetX, 0); 
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 1.0f, NavMesh.AllAreas))
            {
                Agent.Warp(hit.position);
            }
        }
    }

    public void TeleportEnd()
    {
        isTeleporting = false;
    }
    
    // 몬스터 Dash 메서드
    public void Dash()
    {
        if (AttackTarget != null)
        {
            // 초기 가속도 기억
            curAccelation = Agent.acceleration;
            Agent.acceleration = 50f;
            
            isDashing = true;
            
            // 플레이어 피봇이 아래에 있기 때문에 위치 조정
            Vector3 currentPlayerPos = AttackTarget.transform.position + Vector3.up;
            // 돌진 방향 설정
            Vector3 dashDir = currentPlayerPos - transform.position;
            
            // 대시 출발 위치를 미리 이동시켜 속도 증가하는 느낌 추가
            // Agent.Warp(transform.position + dashDir.normalized);
            
            float dashSpeed = EnemyStatus.MoveSpeed * 5f; // 기존 이동속도의 3배
            Agent.speed = dashSpeed;
            
            // Agent가 돌진할 위치 설정(플레이어보다 조금 뒤로 설정해서 역동성 추가)
            Vector3 dashTargetPos = transform.position + dashDir.normalized * 5f;
            
            // NavMeshAgent를 통해 도착지로 돌진
            Agent.isStopped = false;
            Agent.SetDestination(dashTargetPos);
            
            // 일정 시간 후 원래 속도 복귀 코루틴 예시
            StartCoroutine(ResetAgentSpeedAfterDash());
            
        }
    }
    
    // Dash 후 원래 속도 복귀용 코루틴
    private IEnumerator ResetAgentSpeedAfterDash()
    {
        yield return new WaitForSeconds(0.5f); // 돌진 지속 시간(0.5초), 실제 상황에 맞게 조정
        Agent.speed = EnemyStatus.MoveSpeed;
        Agent.acceleration = curAccelation;
        isDashing  = false;
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
                    itemDrop.Init(item.itemSo,1);
                    
                }
                
                dropItemRigidbody= dropObj.GetComponent<Rigidbody2D>();
                itemDrop.DropAnimation(dropItemRigidbody, dropAngleRange, dropUpForce, dropSideForce); 
            } 
        }
    }
    
    // 1초에 어그로 수치 2씩 감소 
    public IEnumerator DecreaseAggroValue()
    {
        while (true)
        {
            Aggro.DecreaseAllAggro(decreaseDelayValue);
            // 만약 타겟이 전부 사라지면 코루틴 종료
            if (Aggro.IsEmpty)
            {
                aggroCoroutine = null;
                yield break;
            }
            yield return new WaitForSeconds(decreaseDelayTime);
        }
    }

    private void OnAggroTargetChanged(GameObject newTarget, float newValue)
    {
        AttackTarget = newTarget;
    }
    
    
}
