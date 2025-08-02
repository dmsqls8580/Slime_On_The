using Boss2States;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Boss2Controller :  BaseController<Boss2Controller, Boss2State>, IDamageable, IAttackable, IPoolObject, IBossController
{
    [SerializeField] private Collider2D senseRangeCollider;
    [SerializeField] private Collider2D attackRangeCollider;
    
    public Animator Animator     { get; private set; }     // 애니메이터
    public NavMeshAgent Agent    { get; private set; }     // NavMesh Agent
    
    public bool IsPlayerInSenseRange { get; private set; } // 플레이어 인식 범위 내 존재 여부
    private CameraController cameraController => AttackTarget != null
        ? AttackTarget.GetComponent<CameraController>() : null;
    private StatManager statManager;
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private Rigidbody2D dropItemRigidbody;
    private SpriteRenderer spriteRenderer;                 // 몬스터 스프라이트 (보는 방향에 따라 수정) 
    private string lastLogMessage = ""; // Todo : 나중에 삭제 
    
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
    private List<DropItemData> dropItems => BossStatus.BossSO.DropItems;
    
    private float dropUpForce = 3f;
    private float dropSideForce = 2f;
    private float dropAngleRange = 60f;
    
    /************************ IDamageable ***********************/
    public bool IsDead => BossStatus.IsDead;
    public Collider2D Collider  => GetComponent<Collider2D>();
    public void TakeDamage(IAttackable _attacker, GameObject _attackerObj)
    {
        if (IsDead) return;
        if (_attacker != null)
        {
            // 피격
            BossStatus.TakeDamage(_attacker.AttackStat.GetCurrent(),StatModifierType.Base);
            SoundManager.Instance.PlaySFX(SFX.Grount);
            if (BossStatus.CurrentHealth <= 0)
            {
                Dead();
            }
            StartCoroutine(ShakeIDamageable());
        }
    }

    public void Dead()
    {
        if (BossStatus.CurrentHealth <= 0)
        {
            ChangeState(Boss2State.Dead);

            // 오브젝트 풀 반환
            // Todo : 몬스터 사망 후 풀로 반횐될 때까지 시간 const로 만들어주기
            ObjectPoolManager.Instance.ReturnObject(gameObject, 2f);
            
            DropItems(this.gameObject.transform);
        }
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
    public string PoolID => BossStatus != null
        ? BossStatus.BossSO.BossID.ToString() : "Invalid";

    public int PoolSize { get; } = 1;
    public void OnSpawnFromPool()
    {
        statManager.Init(BossStatus.BossSO);
        
        // 상태머신이 초기화되지 않았다면 초기화
        if (stateMachine == null || states == null)
        {
            SetupState();
        }
        ChangeState(Boss2State.Idle);
        
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
    
    /************************ IBossController ***********************/
    [SerializeField] private BossStatus bossStatus;
    public BossStatus BossStatus
    {
        get => bossStatus;
        set => bossStatus = value;
    }
    public Transform Transform { get; set; }
    public GameObject AttackTarget{ get; set; }            // 공격 대상, 보스의 경우 다음 패턴 진행을 위한 조건
    public Vector3 SpawnPos      { get;  set; }            // 스폰 위치
    public bool IsPlayerInAttackRange {get; private set; } // 플레이어 공격 범위 내 존재 여부
    
    public void SetPlayerInAttackRange(bool _inRange)
    {
        IsPlayerInAttackRange = _inRange;
    }

    public void SetPlayerInSenseRange(bool _inRange)
    {
        IsPlayerInSenseRange = _inRange;
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
        Aggro = new AggroSystem(BossStatus.BossSO.AttackType,
            target => IsPlayerInSenseRange,stickTime);
        Aggro.OnTargetChanged += OnAggroTargetChanged;
    }
    
    protected override void Start()
    {
        base.Start();
        ChangeState(Boss2State.Idle);
        
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
            Vector2 targetDir = AttackTarget.transform.position - transform.position;
            lastFlipX = targetDir.x < 0;
        }
        
        // Enemy의 이동 방향에 따라 SpriteRenderer flipX
        spriteRenderer.flipX = lastFlipX;

        // Todo : 나중에 삭제
        string currentMessage = $"CurrentState = {CurrentState}";
        if (currentMessage != lastLogMessage)
        {
            Logger.Log(currentMessage);
            lastLogMessage = currentMessage;
        }
    }
    
    protected override IState<Boss2Controller, Boss2State> GetState(Boss2State state)
    {
        return state switch
        {
            Boss2State.Idle => new IdleState(),
            Boss2State.Wander => new WanderState(),
            Boss2State.Chase => new ChaseState(),
            Boss2State.BubbleMelee => new BubbleMeleeState(),
            Boss2State.Bubble1 => new Bubble1State(),
            Boss2State.Bubble2 => new Bubble2State(),
            Boss2State.Dead => new DeadState(),
            _ => null
        };
    }

    public override void FindTarget()
    {
        
    }
    
    // Melee 상태에서 호출할 공격 패턴
    public void BubbleMelee()
    {
        
    }
    
    // Bubble1 상태에서 호출할 공격 패턴
    public void Bubble1()
    {
        
    }
    
    
    // Bubble2 상태에서 호출할 공격 패턴
    public void Bubble2()
    {
        
    }
    
    
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
    
    private void OnAggroTargetChanged(GameObject newtarget, float newvalue)
    {
        AttackTarget = newtarget;
    }
}
