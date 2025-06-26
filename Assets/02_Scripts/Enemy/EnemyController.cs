using System.Collections;
using System.Collections.Generic;
using Enemyststes;
using UnityEngine;

public class EnemyController : BaseController<EnemyController, EnemyState>
{
    [SerializeField] private EnemySO enemySo;

    public bool isDead;
    public GameObject Target; // 나중에 수정 예정
    public Animator Animator;
    [SerializeField] private EnemySO enemySO;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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
}
