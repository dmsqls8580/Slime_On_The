using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.AI;

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
            idleDuration = Random.Range(owner.MinMoveDelay, owner.MaxMoveDelay);
            idleTimer = 0f;
            owner.Animator.SetBool(isMovingHash, false);
            owner.Animator.ResetTrigger(attackHash);
        }

        public void OnUpdate(EnemyController owner)
        {
            idleTimer += Time.deltaTime;
            
            // 플레이어가 너무 가까이 있을 경우 감지
            GameObject player = owner.ChaseTarget;
            if (player != null)
            {
                float distance = Vector3.Distance(owner.transform.position, player.transform.position);
                if (distance <= owner.AttackRange)
                {
                    owner.ChaseTarget = player;
                }
            }
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.Animator.SetBool(isMovingHash, true);
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            // 몬스터 사망시 Dead 모드로 전환.
            if (owner.isDead)
            {
                return EnemyState.Dead;
            }
            // 공격 범위 내에 있을 시 Attack 모드로 전환
            if (owner.ChaseTarget != null
                && Vector3.Distance(owner.transform.position, owner.ChaseTarget.transform.position) <=
                owner.AttackRange)
            {
                return EnemyState.Attack;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우 Chase 모드로 전환.
            if (owner.ChaseTarget != null) 
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
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetBool(isMovingHash, true);
            owner.Animator.ResetTrigger(attackHash);
            // 랜덤 방향으로 이동.
            OnMoveRandom(owner);
        }

        public void OnUpdate(EnemyController owner)
        {
            GameObject player = owner.ChaseTarget;
            if (player == null)
            {
                player = GameObject.FindWithTag("Player");
            }
            if (player != null)
            {
                float dist = Vector3.Distance(owner.transform.position, player.transform.position);
                if (dist <= owner.AttackRange)
                {
                    owner.ChaseTarget = player;
                }
            }
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.Animator.SetBool(isMovingHash, false);
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.isDead)
            {
                return EnemyState.Dead;
            }
            // 공격 범위 내에 있을 시 Attack 모드로 전환
            if (owner.ChaseTarget != null
                && Vector3.Distance(owner.transform.position, owner.ChaseTarget.transform.position) <=
                owner.AttackRange)
            {
                return EnemyState.Attack;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우, Chase 모드로 전환.
            if (owner.ChaseTarget != null)
            {
                return EnemyState.Chase;
            }
            // 목적지로 이동이 끝나면 idle 모드로 전환.
            if (ReachedDesination(owner))
            {
                return EnemyState.Idle;
            }
            return EnemyState.Wander;
        }
        
        // wanderRadius 내 랜덤한 위치로 이동
        private void OnMoveRandom(EnemyController owner)
        {
            Vector2 randomCircle = Random.insideUnitCircle * owner.WanderRadius;
            Vector3 randomPos =  owner.SpawnPos + new Vector3(randomCircle.x, 0, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, owner.WanderRadius, NavMesh.AllAreas))
            {
                owner.Agent.SetDestination(hit.position);
            }

        }

        // 이동이 끝났는지 판별
        private bool ReachedDesination(EnemyController owner)
        {
            return !owner.Agent.pathPending && owner.Agent.remainingDistance <= owner.Agent.stoppingDistance;
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
            // Target의 위치를 추적해 이동.
            if (owner.ChaseTarget != null)
            {
                // 플레이어와 너무 가까이 붙으면 State가 변하지 않는 문제가 존재
                float distance = Vector3.Distance(owner.ChaseTarget.transform.position, owner.transform.position);
                
                if (distance <= owner.AttackRange)
                {
                    owner.Agent.ResetPath();
                }
                else
                {
                    owner.Agent.SetDestination(owner.ChaseTarget.transform.position);
                }
            }
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.Agent.ResetPath();
            owner.Animator.SetBool(isTargetHash, false);
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.isDead)
            {
                return EnemyState.Dead;
            }
            // 플레이어가 감지 범위 밖으로 나갈 경우, Idle 모드로 전환.
            if (owner.ChaseTarget == null)
            {
                return EnemyState.Idle;
            }
            // 플레이어가 공격 범위 내에 들어올 경우, Attack 모드로 전환.
            float distance = Vector3.Distance(owner.transform.position, owner.ChaseTarget.transform.position);
            
            if (owner.ChaseTarget != null && owner.CanAttack()
                &&  distance <= owner.AttackRange)
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
        private float attackDelay = 0.5f;
        private float attackTimer = 0f;
        private bool isAttacking = false;
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetTrigger(attackHash);
            attackTimer = 0f;
            isAttacking = false;
            // 공격 시 일시 경직
            
        }

        public void OnUpdate(EnemyController owner)
        {
            attackTimer += Time.deltaTime;
            if (!isAttacking && attackTimer >= attackDelay)
            {
                isAttacking = true;
                attackTimer = 0f;
            }
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            // 플레이어 사망 시 Dead 모드로 전환
            if (owner.isDead)
            {
                return EnemyState.Dead;
            }
            // 플레이어가 감지 범위 밖으로 나갈 시, Idle 모드로 전환
            if (owner.ChaseTarget == null)
            {
                Debug.Log("플레이어가 감지 범위를 벗어났습니다.");
                return EnemyState.Idle;
            }
            // 플레이어가 공격 범위 밖에 나갈 시 추격 모드로 전환
            if (attackTimer >= attackDelay)
            {
                owner.SetAttackCooldown();
                return EnemyState.Chase;
            }
            // 플레이어가 공격 범위 내에 있어도 쿨타임이 끝나지 않았다면 대기
            return EnemyState.Attack;
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

        public void OnExit(EnemyController owner)
        {
            
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            return owner.isDead? EnemyState.Dead : EnemyState.Idle;
        }
    }

}