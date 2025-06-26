using System;
using System.Collections;
using System.Collections.Generic;
using Enemyststes;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : BaseController<EnemyController, EnemyState>
{
    [SerializeField] private EnemySO enemySo;
    public bool isDead           { get; private set; }     // 사망 여부
    public GameObject ChaseTarget     { get; private set; }     // 인식된 플레이어, 추격
    public Animator Animator     { get; private set; }     // 애니메이터
    public Vector3 SpawnPos      { get; private set; }     // 스폰 위치
    public float WanderRadius    { get; private set; }     // 배회 반경
    public float MinMoveDelay    { get; private set; }     // Idle 상태 최소 지속 시간
    public float MaxMoveDelay    { get; private set; }     // Idle 상태 최대 지속 시간
    public float AttackRange     { get; private set; }     // 공격 범위
    public NavMeshAgent Agent    { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
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
        Debug.Log("EnemyController: " + CurrentState);
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

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChaseTarget = null;
        }
    }

    private void SetEnemySOData()
    {
        WanderRadius = enemySo.WanderRadius;
        MinMoveDelay = enemySo.MinMoveDelay;
        MaxMoveDelay = enemySo.MaxMoveDelay;
        Agent.speed = enemySo.MoveSpeed;
        AttackRange = enemySo.AttackRange;
    }
}
