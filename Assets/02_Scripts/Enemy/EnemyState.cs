using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  Enemyststes
{
    public enum EnemyState
    {
        Idle,
        Wander,
        Chase,
        Attack,
        Dead
    }

    public class IdleState : IState<EnemyController, EnemyState>
    {
        private readonly int isMovingHash = Animator.StringToHash("IsMoving");
        private readonly int attackHash = Animator.StringToHash("Attack");
        
        private float idleDuration;
        private float idleTimer;
        
        public void OnEnter(EnemyController owner)
        {
            idleDuration = Random.Range(2f, 5f);
            idleTimer = 0f;
            owner.Animator.SetBool(isMovingHash, false);
            owner.Animator.ResetTrigger(attackHash);
        }

        public void OnUpdate(EnemyController owner)
        {
            idleTimer += Time.deltaTime;
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController entity)
        {
            
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            // 몬스터 사망시 Dead 모드로 전환.
            if (owner.isDead)
            {
                return EnemyState.Dead;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우 Chase 모드로 전환.
            // EnemyController에서 설정.
            if (owner.Target != null) 
            {
                return EnemyState.Chase;
            }
            // 일정 시간이 지나면 자동으로 Wander 모드로 전환.
            if (idleTimer >= idleDuration)
            {
                return EnemyState.Wander;
            }
            // 배회모드로 전환되지 않았을 시 idle모드.
            return EnemyState.Idle;
        }
    }
    
    public class WanderState : IState<EnemyController, EnemyState>
    {
        private readonly int isMovingHash = Animator.StringToHash("IsMoving");
        private readonly int attackHash = Animator.StringToHash("Attack");
        
        private float wanderDuration;
        private float wanderTimer;
        public void OnEnter(EnemyController owner)
        {
            wanderDuration = Random.Range(2f, 5f);
            wanderTimer = 0f;
            owner.Animator.SetBool(isMovingHash, true);
            owner.Animator.ResetTrigger(attackHash);
            // 랜덤 방향으로 이동.
        }

        public void OnUpdate(EnemyController owner)
        {
            wanderTimer += Time.deltaTime;
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            // 랜덤 방향으로 이동.
        }

        public void OnExit(EnemyController entity)
        {
            
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.isDead)
            {
                return EnemyState.Dead;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우, Chase 모드로 전환.
            if (false)
            {
                return EnemyState.Chase;
            }
            // 일정 시간이 지나면 자동으로 idle 모드로 전환.
            if (wanderTimer >= wanderDuration)
            {
                return EnemyState.Idle;
            }
            return EnemyState.Wander;
        }
    }
    
    public class ChaseState : IState<EnemyController, EnemyState>
    {
        private readonly int isTargetHash = Animator.StringToHash("IsTarget");
        private readonly int attackHash = Animator.StringToHash("Attack");
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetBool(isTargetHash, true);
            owner.Animator.ResetTrigger(attackHash);
        }

        public void OnUpdate(EnemyController owner)
        {
            // owner의 위치를 추적해 이동.
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController entity)
        {
            
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.isDead)
            {
                return EnemyState.Dead;
            }
            // 플레이어가 감지 범위 밖으로 나갈 경우, Idle 모드로 전환.
            if (false)
            {
                return EnemyState.Idle;
            }
            // 플레이어가 공격 범위 내에 들어올 경우, Attack 모드로 전환.
            if (false)
            {
                return EnemyState.Attack;
            }
            return EnemyState.Chase;
        }
    }
    
    public class AttackState : IState<EnemyController, EnemyState>
    {
        private readonly int attackHash = Animator.StringToHash("Attack"); 
        
        // 공격 시 일정 시간 동안 몬스터 정지, 이후 다시 chase 모드로 전환하여 추격.
        private float attackDelay = 2f; // 공격 후 딜레이
        private float attackTimer = 0f;
        private bool isAttacking = false;
        
        public void OnEnter(EnemyController owner)
        {
            attackTimer = 0f;
            isAttacking = true;
            owner.Animator.SetTrigger(attackHash);
            // 공격 시 일시 경직
        }

        public void OnUpdate(EnemyController owner)
        {
            
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            if (isAttacking)
            {
                attackTimer +=  Time.fixedDeltaTime;
                if (attackTimer >= attackDelay)
                {
                    isAttacking = false;
                }
            }
        }

        public void OnExit(EnemyController entity)
        {
            
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            return EnemyState.Chase;
        }
    }
    
    public class DeadState : IState<EnemyController, EnemyState>
    {
        public void OnEnter(EnemyController owner)
        {
            
        }

        public void OnUpdate(EnemyController owner)
        {
            
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController entity)
        {
            
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            return owner.isDead? EnemyState.Dead : EnemyState.Idle;
        }
    }

}