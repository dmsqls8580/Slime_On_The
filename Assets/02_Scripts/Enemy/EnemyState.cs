using UnityEngine;
using UnityEngine.AI;

namespace  Enemystates
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
        private readonly int isTargetHash = Animator.StringToHash("IsTarget");
        private readonly int isAttackHash = Animator.StringToHash("Attack");
        
        private float idleDuration;
        private float idleTimer;
        private bool isAttackCooldown;
        
        public void OnEnter(EnemyController owner)
        {
            // 이전 State가 AttackState인 경우, AttackCooldown만큼 IdleState 유지
            if (owner.PreviousState == EnemyState.Attack)
            {
                idleDuration = owner.EnemyStatus.AttackCooldown;
                isAttackCooldown = true;
            }
            // 일반적인 경우, MinMoveDelay와 MaxMoveDelay 사이 랜덤한 시간만큼 IdleState 유지
            else
            {
                idleDuration = Random.Range(owner.EnemyStatus.MinMoveDelay, owner.EnemyStatus.MaxMoveDelay);
                isAttackCooldown = false;
            }
            idleTimer = 0f;
            owner.Animator.SetBool(isMovingHash, false);
            owner.Animator.SetBool(isTargetHash, false);
        }

        public void OnUpdate(EnemyController owner)
        {
            idleTimer += Time.deltaTime;
            
            // 플레이어가 너무 가까이 있을 경우 감지
            GameObject player = owner.AttackTarget;
            if (player != null)
            {
                float distance = Vector2.Distance(owner.transform.position, player.transform.position);
                if (distance <= owner.EnemyStatus.AttackRange)
                {
                    owner.AttackTarget = player;
                }
            }
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.Animator.SetBool(isMovingHash, true);
            owner.PreviousState = EnemyState.Idle;
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            // 몬스터 사망시 Dead 모드로 전환.
            if (owner.IsDead)
            {
                return EnemyState.Dead;
            }
            // AttackType이 None일 경우 Idle, Wander, Chase(Run) State만 순환
            if (owner.EnemyStatus.enemySO.AttackType == EnemyAttackType.None)
            {
                // 플레이어가 몬스터 감지 범위 내에 들어갈 경우 Chase 모드로 전환.
                if (owner.AttackTarget != null) 
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
            
            // AttackType이 Neutral인 경우, Idle, Wander State만 순환
            if (owner.EnemyStatus.enemySO.AttackType == EnemyAttackType.Neutral
                && !owner.IsAttacked)
            {
                // 일정 시간이 지나면 자동으로 Wander 모드로 전환.
                if (idleTimer >= idleDuration)
                {
                    return EnemyState.Wander;
                }
                // 배회모드로 전환되지 않았을 시 idle모드.
                return EnemyState.Idle;
            }
            
            // AttackCooldown 동안은 Idle 상태를 계속 유지
            if (isAttackCooldown)
            {
                if (idleTimer >= idleDuration)
                {
                    // 쿨타임 끝, 다음 조건으로 넘어감
                    isAttackCooldown = false; 
                }
                else
                {
                    // 쿨타임 중에는 무조건 Idle 유지
                    return EnemyState.Idle; 
                }
            }
            
            // 공격 범위 내에 있을 시 Attack 모드로 전환
            if (owner.AttackTarget != null && owner.IsPlayerInAttackRange)
            {
                return EnemyState.Attack;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우 Chase 모드로 전환.
            if (owner.AttackTarget != null) 
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
        private readonly int isTargetHash = Animator.StringToHash("IsTarget");
        private readonly int isAttackHash = Animator.StringToHash("Attack");
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetBool(isMovingHash, true);
            owner.Animator.SetBool(isTargetHash, false);
            // 랜덤 방향으로 이동.
            OnMoveRandom(owner);
        }

        public void OnUpdate(EnemyController owner)
        {
            
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.Animator.SetBool(isMovingHash, false);
            owner.PreviousState = EnemyState.Wander;
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.IsDead)
            {
                return EnemyState.Dead;
            }
            // AttackType이 None일 경우 Idle, Wander, Chase(Run) State만 순환
            if (owner.EnemyStatus.enemySO.AttackType == EnemyAttackType.None)
            {
                // 플레이어가 몬스터 감지 범위 내에 들어갈 경우, Chase 모드로 전환.
                if (owner.AttackTarget != null)
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
            
            // AttackType이 Neutral인 경우, Idle, Wander State만 순환
            if (owner.EnemyStatus.enemySO.AttackType == EnemyAttackType.Neutral
                && !owner.IsAttacked)
            {
                // 목적지로 이동이 끝나면 idle 모드로 전환.
                if (ReachedDesination(owner))
                {
                    return EnemyState.Idle;
                }
                return EnemyState.Wander;
            }
            
            // 공격 범위 내에 있을 시 Attack 모드로 전환
            if (owner.AttackTarget != null && owner.IsPlayerInAttackRange)
            {
                return EnemyState.Attack;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우, Chase 모드로 전환.
            if (owner.AttackTarget != null)
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
            Vector2 randomCircle = Random.insideUnitCircle * owner.EnemyStatus.WanderRadius;
            Vector3 randomPos =  owner.SpawnPos + new Vector3(randomCircle.x, 0, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, owner.EnemyStatus.WanderRadius, NavMesh.AllAreas))
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
        private readonly int isAttackHash = Animator.StringToHash("Attack");
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetBool(isTargetHash, true);
        }

        public void OnUpdate(EnemyController owner)
        {
            // AttackType이 None일 경우 Chase State에서 플레이어에게서 도망
            if (owner.EnemyStatus.enemySO.AttackType == EnemyAttackType.None)
            {
                if (owner.AttackTarget != null)
                {
                    Vector3 dir = (owner.transform.position - owner.AttackTarget.transform.position).normalized;
                    float distance = owner.EnemyStatus.FleeDistance;
                    Vector3 fleeDir = owner.transform.position + dir * distance;
                    
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(fleeDir, out hit, distance, NavMesh.AllAreas))
                    {
                        owner.Agent.SetDestination(hit.position);
                    }
                }
                
                return;
            }
            // Target의 위치를 추적해 이동.
            if (owner.AttackTarget != null)
            {
                if (owner.IsPlayerInAttackRange)
                {
                    owner.Agent.ResetPath();
                }
                else
                {
                    owner.Agent.SetDestination(owner.AttackTarget.transform.position);
                }
            }
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.Agent.ResetPath();
            owner.PreviousState = EnemyState.Chase;
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            if (owner.IsDead)
            {
                return EnemyState.Dead;
            }
            // AttackType이 None일 경우 Idle, Wander, Chase(Run) State만 순환
            if (owner.EnemyStatus.enemySO.AttackType == EnemyAttackType.None)
            {
                // 플레이어가 감지 범위 밖으로 나갈 경우, Idle 모드로 전환.
                if (owner.AttackTarget == null || !owner.IsAttacked)
                {
                    return EnemyState.Idle;
                }
                return EnemyState.Chase;
            }
            
            // 플레이어가 감지 범위 밖으로 나갈 경우, Idle 모드로 전환.
            if (owner.AttackTarget == null || !owner.IsAttacked)
            {
                return EnemyState.Idle;
            }
            // 플레이어가 공격 범위 내에 들어올 경우, Attack 모드로 전환.
            if (owner.AttackTarget != null &&  owner.IsPlayerInAttackRange)
            {
                return EnemyState.Attack;
            }
            return EnemyState.Chase;
        }
    }
    
    public class AttackState : IState<EnemyController, EnemyState>
    {
        private readonly int isAttackHash = Animator.StringToHash("Attack"); 
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetTrigger(isAttackHash);
        }

        public void OnUpdate(EnemyController owner)
        {

        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.PreviousState = EnemyState.Attack;
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            // 플레이어 사망 시 Dead 모드로 전환
            if (owner.IsDead)
            {
                return EnemyState.Dead;
            }
            // AttackState가 끝나면 반드시 IdleState로 전환
            return EnemyState.Idle;
        }
    }
    
    public class DeadState : IState<EnemyController, EnemyState>
    {
        private readonly int isDeadHash = Animator.StringToHash("Die");
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetTrigger(isDeadHash);
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
            return owner.IsDead? EnemyState.Dead : EnemyState.Idle;
        }
    }

}