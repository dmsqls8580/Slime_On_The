using BossStates;
using PlayerStates;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using DeadState = BossStates.DeadState;
using IdleState = BossStates.IdleState;

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
    public bool IsBerserked => BossStatus.CurrentHealth <= BossStatus.MaxHealth * 0.5f;
    public float IdleDuration => BossStatus.BossSO.IdleDuration;
    public float Cast1Duration => BossStatus.BossSO.Cast1Duration;
    public float Cast2Duration  => BossStatus.BossSO.Cast2Duration;
    public float StompDuration  => BossStatus.BossSO.StompDuration;
    
    private CameraController cameraController => AttackTarget != null
        ? AttackTarget.GetComponent<CameraController>() : null;
    private StatManager statManager;
    private float lastAngle;                               // 몬스터 공격 범위 각도 기억용 필드
    private bool lastFlipX = false;                        // 몬스터 회전 상태 기억용 필드
    private Rigidbody2D dropItemRigidbody;
    private SpriteRenderer spriteRenderer;                 // 몬스터 스프라이트 (보는 방향에 따라 수정) 
    private List<GameObject> leafSpells = new List<GameObject>();
    private string lastLogMessage = ""; // Todo : 나중에 삭제 
    
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
            if (BossStatus.CurrentHealth <= 0)
            {
                Dead();
            }
        }
    }

    public void Dead()
    {
        if (BossStatus.CurrentHealth <= 0)
        {
            ChangeState(BossState.Dead);

            // 오브젝트 풀 반환
            // Todo : 몬스터 사망 후 풀로 반횐될 때까지 시간 const로 만들어주기
            ObjectPoolManager.Instance.ReturnObject(gameObject, 2f);
            
            DropItems(this.gameObject.transform);
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
    public string PoolID => BossStatus.BossSO != null
        ? BossStatus.BossSO.BossID.ToString() :  "Invalid";

    public int PoolSize { get; } = 1;
    public void OnSpawnFromPool()
    {
        statManager.Init(BossStatus.BossSO);
        
        // 상태머신이 초기화되지 않았다면 초기화
        if (stateMachine == null || states == null)
        {
            SetupState();
        }
        ChangeState(BossState.Idle);
        
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

        // Todo : 나중에 삭제
        string currentMessage = $"CurrentState = {CurrentState}";
        if (currentMessage != lastLogMessage)
        {
            Logger.Log(currentMessage);
            lastLogMessage = currentMessage;
        }
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
    
    public BossState EnterRandomPattern()
    {
        // Pattern1, Pattern2, Pattern3 중 랜덤 선택
        BossState[] patterns = { BossState.Pattern1, BossState.Pattern2, BossState.Pattern3 };
        BossState nextPattern = patterns[Random.Range(0, patterns.Length)];
        ChangeState(nextPattern);
        return nextPattern;
    }

    // Cast1 상태에서 호출할 공격 패턴
    public void Cast1()
    {
        if (!IsBerserked)
        {
            StartCoroutine(Spikedelay(Constants.Boss.SPIKE_DELAY_NOTBERSERKED));
        }
        else
        {
            StartCoroutine(Spikedelay(Constants.Boss.SPIKE_DELAY_BERSERKED));
        }
    }
    
    
    // 바닥에서 가시가 순차적으로 소환되는 패턴
    // 가시가 솟아오르고, 멈췄다가 한번에 애니메이션 재생
    private IEnumerator Spikedelay(float _delay)
    {
        yield return new WaitForSeconds(_delay);
        var spikes1 = SpawnSpike(this.transform.position, 
            Constants.Boss.SPAWN_SPIKE_RADIUS_1, Constants.Boss.SPAWN_SPIKE_1);
        
        yield return new WaitForSeconds(_delay);
        var spikes2 = SpawnSpike(this.transform.position, 
            Constants.Boss.SPAWN_SPIKE_RADIUS_2, Constants.Boss.SPAWN_SPIKE_2);
        
        yield return new WaitForSeconds(_delay);
        var spikes3 = SpawnSpike(this.transform.position, 
            Constants.Boss.SPAWN_SPIKE_RADIUS_3, Constants.Boss.SPAWN_SPIKE_3);
        
        yield return new WaitForSeconds(_delay);
        foreach (var spike in spikes1)
        {
            TryPlaySpikeAnimation(spike);
        }
        
        yield return new WaitForSeconds(_delay);
        foreach (var spike in spikes2)
        {
            TryPlaySpikeAnimation(spike);
        }
        
        yield return new WaitForSeconds(_delay);
        foreach (var spike in spikes3)
        {
            TryPlaySpikeAnimation(spike);
        }
    }
    
    private void TryPlaySpikeAnimation(GameObject spike)
    {
        if (spike.TryGetComponent<TurtleSpell4>(out TurtleSpell4 spell4))
        {
            spell4.AnimationPlay();
        }
        if (spike.TryGetComponent<TurtleSpell5>(out TurtleSpell5 spell5))
        {
            spell5.AnimationPlay();
        }
    }
    
    // BabyTurtle이 바닥에서 가시를 소환하는 메서드 ( 보스 위치, 소환 반경, 소환 갯수)
    private List<GameObject> SpawnSpike(Vector2 _bossPos, float _radius, int _count)
    {
        // 보스가 광폭화되었을 경우, 디버프를 주는 효과 추가
        string objectName = !IsBerserked
            ? BossStatus.BossSO.ProjectileID[4].ToString() 
            : BossStatus.BossSO.ProjectileID[5].ToString();
        
        List<GameObject> spawnedSpikes = new List<GameObject>();
        
        for (int i = 0; i < _count; i++)
        {
            // 원의 둘레 2π를 count로 나눠 i에 대항하는 각도에 스폰 위치를 할당
            float angle = (2 * Mathf.PI / _count) * i;
            Vector2 spawnPos = _bossPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
            
            GameObject spike = ObjectPoolManager.Instance.GetObject(objectName);
            spike.transform.position = spawnPos;
            spike.transform.rotation = Quaternion.identity;

            if (spike.TryGetComponent<ProjectileBase>(out var projectile))
            {
                projectile.Init(Vector2.zero, AttackStat);
            }
            spawnedSpikes.Add(spike);
        }
        return spawnedSpikes;
    }
    
    // Cast2 상태에서 호출할 공격 패턴
    public void Cast2()
    {
        if (!IsBerserked)
        {
            SpawnTentacle();
        }
        else
        {
            StartCoroutine(DelayTentacle(1f));
        }
    }

    private void SpawnTentacle()
    {
        // 보스가 광폭화되었을 경우, 디버프를 주는 효과 추가
        string objectName = !IsBerserked
            ? BossStatus.BossSO.ProjectileID[2].ToString()
            : BossStatus.BossSO.ProjectileID[3].ToString();
        
        // 플레이어 위치
        Vector2 playerPos = ChaseTarget.transform.position;
        
        // 플레이어 왼쪽 or 오른쪽 방향 랜덤으로 선택
        float offsetX = Random.value < 0.5f 
            ? -Constants.Boss.SPAWN_TENTACLE_DISTANCE
            : Constants.Boss.SPAWN_TENTACLE_DISTANCE;
        Vector2 spawnPos = playerPos + new Vector2(offsetX, 0); 
        
        // Tentacle 방향 벡터
        Vector2 dirTarget = (playerPos - spawnPos).normalized;
        
        // Tentacle 소환 및 초기화
        GameObject tentacle = ObjectPoolManager.Instance.GetObject(objectName);
        tentacle.transform.position = spawnPos;
        tentacle.transform.rotation = Quaternion.FromToRotation(Vector3.right, dirTarget); // 텐타클 기본이 왼쪽 공격

        if (tentacle.TryGetComponent<ProjectileBase>(out var projectile))
        {
            projectile.Init(dirTarget, AttackStat);
        }
    }

    private IEnumerator DelayTentacle(float _delay)
    {
        SpawnTentacle();
        yield return new WaitForSeconds(_delay);
        SpawnTentacle();
    }
    
    // Stomp 상태에서 호출할 공격 패턴
    public void Stomp()
    {
        if (!cameraController.IsUnityNull())
        {
            cameraController.CameraShake(2, 1, 1);
        }
        
        if (!IsBerserked)
        {
            /*
            var playerRigidbody = AttackTarget.GetComponent<Rigidbody2D>();
            Vector2 dir = (AttackTarget.transform.position - transform.position).normalized;
            float knockbackPower = 10f;
            playerRigidbody.velocity = Vector2.zero; // 기존 속도 초기화
            playerRigidbody.AddForce(dir * knockbackPower, ForceMode2D.Impulse);
            */
            
            StartCoroutine(SpawnLeafSpell(transform.position, 
                Constants.Boss.SPAWN_LEAF_RADIUS, Constants.Boss.SPAWN_LEAF_DELAY_NOTBERSERKED));
        }
        else
        {
            /*
            var playerRigidbody = AttackTarget.GetComponent<Rigidbody2D>();
            Vector2 dir = (AttackTarget.transform.position - transform.position).normalized;
            float knockbackPower = 10f;
            playerRigidbody.velocity = Vector2.zero; // 기존 속도 초기화
            playerRigidbody.AddForce(dir * knockbackPower, ForceMode2D.Impulse);
            */
            
            StartCoroutine(SpawnLeafSpell(transform.position, 
                Constants.Boss.SPAWN_LEAF_RADIUS, Constants.Boss.SPAWN_LEAF_DELAY_BERSERKED));
        }
        
    }
    
    // 풀잎이 터지는 패턴 구현
    // 풀잎이 소환되고, 애니메이션 중지, 모든 풀잎이 소환되면 잠시 흔들림 효과 후,
    // 풀잎이 터지는 애니메이션 재생
    private IEnumerator SpawnLeafSpell(Vector2 _bossPos, float _radius, float _delay)
    {
        string objectName = !IsBerserked
            ? BossStatus.BossSO.ProjectileID[0].ToString() 
            : BossStatus.BossSO.ProjectileID[1].ToString();
        
        // 원의 둘레 2π를 count로 나눠 i에 대항하는 각도에 스폰 위치를 할당, 리스트에 저장
        List<Vector2> directions = new List<Vector2>();
        for (int i = 0; i < 12; i++)
        {
            float angle = Mathf.Deg2Rad * (i * 30f); // 0, 30, 60, ..., 330도
            directions.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        }
        
        List<int> randomIdx = Enumerable.Range(0, 12).OrderBy(x => Random.value).Take(8).ToList();
        leafSpells.Clear();
        
        foreach (int idx in randomIdx)
        {
            Vector2 spawnPos = _bossPos + directions[idx] * _radius;
            GameObject leaf = ObjectPoolManager.Instance.GetObject(objectName);
            leaf.transform.position = spawnPos;
            leaf.transform.rotation = Quaternion.identity;

            if (leaf.TryGetComponent<ProjectileBase>(out var projectile))
            {
                projectile.Init(Vector2.zero, AttackStat);
            }
            leafSpells.Add(leaf);
            
            yield return new WaitForSeconds(_delay);
        }
        
        // 모든 leaf가 생성되고 잠깐 흔들림 효과
        float shakeDuration = Constants.Boss.LEAF_SHAKE_DURATION;
        float shakeMagnitude = Constants.Boss.LEAF_SHAKE_MAGNITUDE;
        float timer = 0f;
        
        // 현재 leaf의 위치를 List에 저장
        List<Vector3> originalPositions = new List<Vector3>();
        foreach (var leaf in leafSpells)
        {
            originalPositions.Add(leaf.transform.position);
        }
        
        // timer 동안 leaf 흔들림
        while (timer < shakeDuration)
        {
            for (int i = 0; i < leafSpells.Count; i++)
            {
                float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
                float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
                leafSpells[i].transform.position = originalPositions[i] + new Vector3(offsetX, offsetY, 0f);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // 원래 자리로 복원
        for (int i = 0; i < leafSpells.Count; i++)
        {
            leafSpells[i].transform.position = originalPositions[i];
        }
        
        // 모든 오브젝트 애니메이션 실행
        foreach (var leaf in leafSpells)
        {
            TryPlayAnimation(leaf);
        }
    }
    
    private void TryPlayAnimation(GameObject spike)
    {
        if (spike.TryGetComponent<TurtleSpell0>(out TurtleSpell0 spell0))
        {
            spell0.AnimationPlay();
        }
        if (spike.TryGetComponent<TurtleSpell1>(out TurtleSpell1 spell1))
        {
            spell1.AnimationPlay();
        }
    }
    
    
    private void DropItems(Transform transform)
    {
        float randomChance = Random.value;
        Transform itemTarget = ChaseTarget.transform;
        
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
}
