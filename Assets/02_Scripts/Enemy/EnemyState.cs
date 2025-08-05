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
        
        public void OnEnter(EnemyController owner)
        {
            owner.Agent.ResetPath(); // Idle에서 이동 멈추기
            
            // 이전 State가 AttackState인 경우, AttackCooldown만큼 IdleState 유지
            if (owner.PreviousState == EnemyState.Attack)
            {
                idleDuration = owner.EnemyStatus.AttackCooldown;
            }
            // 일반적인 경우, MinMoveDelay와 MaxMoveDelay 사이 랜덤한 시간만큼 IdleState 유지
            else
            {
                idleDuration = Random.Range(owner.EnemyStatus.MinMoveDelay, owner.EnemyStatus.MaxMoveDelay);
            }
            idleTimer = 0f;
            owner.Animator.SetBool(isMovingHash, false);
            owner.Animator.SetBool(isTargetHash, false);
        }

        public void OnUpdate(EnemyController owner)
        {
            idleTimer += Time.deltaTime;
        }

        public void OnFixedUpdate(EnemyController owner)
        {
            
        }

        public void OnExit(EnemyController owner)
        {
            owner.PreviousState = EnemyState.Idle;
        }

        public EnemyState CheckTransition(EnemyController owner)
        {
            AttackType attackType = owner.EnemyStatus.enemySO.AttackType;
            float dist = owner.AttackTarget == null ? float.MaxValue :
                Vector2.Distance(owner.transform.position, owner.AttackTarget.transform.position);
            float minDist = owner.EnemyStatus.FleeDistance - 0.5f;
            float maxDist = owner.EnemyStatus.FleeDistance + 0.5f;
            
            // 몬스터 사망시 Dead 모드로 전환.
            if (owner.IsDead)
            {
                return EnemyState.Dead;
            }

            switch (attackType)
            {
                // AttackType이 None일 경우 Idle, Wander, Chase(Run) State만 순환
                case AttackType.None:
                    // 플레이어가 몬스터 감지 범위 내에 들어갈 경우 Chase 모드로 전환.
                    if (owner.AttackTarget != null)
                        return EnemyState.Chase;
                    // 일정 시간이 지나면 자동으로 Wander 모드로 전환.
                    if (idleTimer >= idleDuration)
                        return EnemyState.Wander;
                    // 배회모드로 전환되지 않았을 시 idle모드.
                    return EnemyState.Idle;
                
                case AttackType.Neutral:
                    // AttackType이 Neutral이고, 공격받은 경우, IsAttackCooldown이 true인 동안은
                    // idle이나 Chase 상태만 가능, 거리가 가까우면 멀어지고, 멀면 가까워지도록 설정
                    if (owner.AttackTarget != null)
                    {
                        // 쿨타임 중
                        if (owner.IsAttackCooldown)
                        {
                            // 거리 변화가 없다면 상태 유지
                            if (!owner.IsEnoughDistanceChange())
                                return EnemyState.Idle;
                            // 거리가 fleeDistance 영역 밖이면 Chase 상태로 이동
                            if (dist < minDist || dist > maxDist)
                                return EnemyState.Chase;
                            // 영역 안이면 Idle
                            return EnemyState.Idle;
                        }

                        // 쿨타임이 끝나면 공격 가능 여부 체크
                        if (!owner.IsAttackCooldown && owner.IsIDamageableInAttackRange)
                            return EnemyState.Attack;

                        // 범위 밖 → Chase
                        return EnemyState.Chase;
                    }
                    // AttackType이 Neutral이고, 공격받지 않은 경우, Idle, Wander State만 순환
                    if (idleTimer >= idleDuration)
                    {
                        return EnemyState.Wander;
                    }
                    return EnemyState.Idle;
                
                // AttackType이 Aggressive일 경우, 플레이어가 인식되면 바로 공격
                case AttackType.Aggressive:
                    if (owner.AttackTarget != null)
                    {
                        if (owner.IsAttackCooldown)
                        {
                            // 거리 변화가 없다면 상태 유지
                            if (!owner.IsEnoughDistanceChange())
                                return EnemyState.Idle;
                            // 쿨타임 중 거리 유지
                            if (dist < minDist || dist > maxDist)
                                return EnemyState.Chase;
                            return EnemyState.Idle;
                        }

                        // 공격 가능
                        if (!owner.IsAttackCooldown && owner.IsIDamageableInAttackRange)
                            return EnemyState.Attack;

                        return EnemyState.Chase;
                    }

                    if (idleTimer >= idleDuration)
                    {
                        return EnemyState.Wander;
                    }
                    return EnemyState.Idle;
            }
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
            if (owner.EnemyStatus.enemySO.AttackType == AttackType.None)
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
            if (owner.EnemyStatus.enemySO.AttackType == AttackType.Neutral)
            {
                // 목적지로 이동이 끝나면 idle 모드로 전환.
                if (ReachedDesination(owner))
                {
                    return EnemyState.Idle;
                }
                return EnemyState.Wander;
            }
            
            // 공격 범위 내에 있을 시 Attack 모드로 전환
            if (owner.AttackTarget != null && owner.IsIDamageableInAttackRange)
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
        private readonly int isMovingHash = Animator.StringToHash("IsMoving");
        private readonly int isTargetHash = Animator.StringToHash("IsTarget");
        private readonly int isAttackHash = Animator.StringToHash("Attack");
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetBool(isTargetHash, true);
        }
        
        public void OnUpdate(EnemyController owner)
        {
            // AttackType이 None일 경우 Chase State에서 플레이어에게서 도망
            if (owner.EnemyStatus.enemySO.AttackType == AttackType.None)
            {
                if (owner.AttackTarget != null)
                {
                    Vector2 ownerPos = owner.transform.position;
                    Vector2 targetPos = owner.AttackTarget.transform.position;
                    // 방향
                    Vector2 dir = (ownerPos - targetPos).normalized;
                    float fleeDistance = owner.EnemyStatus.FleeDistance;
                    Vector2 fleeDir = ownerPos + dir * fleeDistance;
                    
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(fleeDir, out hit, fleeDistance, NavMesh.AllAreas))
                    {
                        owner.Agent.SetDestination(hit.position);
                    }
                }
                
                return;
            }
            // Enemy가 텔레포트, 대쉬, 자폭하는 중이라면 이동 X
            if (owner.IsTeleporting)
            {
                return;
            }

            if (owner.IsDashing)
            {
                return;
            }

            if (owner.IsBombing)
            {
                return;
            }
            // Target의 위치를 추적해 이동.
            if (owner.AttackTarget != null)
            {
                Vector2 ownerPos = owner.Agent.transform.position;
                Vector2 targetPos = owner.AttackTarget.transform.position;
                // 방향
                float fleeDistance = owner.EnemyStatus.FleeDistance;
                float distance = Vector2.Distance(ownerPos, targetPos);
                float minDist = owner.EnemyStatus.FleeDistance - 0.5f;
                float maxDist = owner.EnemyStatus.FleeDistance + 0.5f;

                // 공격 타겟과의 거리가 fleeDistance보다 가까우면 타겟 반대방향으로 이동
                if (distance < minDist)
                {
                    Vector2 dir = (ownerPos - targetPos).normalized;
                    Vector2 destination = targetPos + dir * fleeDistance;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(destination, out hit, fleeDistance, NavMesh.AllAreas))
                    {
                        owner.Agent.SetDestination(hit.position);
                    }
                }
                // 공격 타겟과의 거리가 fleeDistance보다 멀면 타겟 방향으로 이동
                else if (distance > maxDist)
                {
                    Vector2 dir = (targetPos - ownerPos).normalized;
                    Vector2 destination = targetPos - dir * fleeDistance;
                    
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(destination, out hit, fleeDistance, NavMesh.AllAreas))
                    {
                        owner.Agent.SetDestination(hit.position);
                    }
                }
                else
                {
                    owner.Agent.ResetPath();
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
            AttackType attackType = owner.EnemyStatus.enemySO.AttackType;
            float dist = owner.AttackTarget == null ? float.MaxValue :
                Vector2.Distance(owner.transform.position, owner.AttackTarget.transform.position);
            float minDist = owner.EnemyStatus.FleeDistance - 0.5f;
            float maxDist = owner.EnemyStatus.FleeDistance + 0.5f;
            
            if (owner.IsDead)
            {
                return EnemyState.Dead;
            }
            
            switch (attackType)
            {
                // (1) 공격하지 않는 몬스터
                case AttackType.None:
                    if (owner.AttackTarget == null)
                    {
                        return EnemyState.Idle;
                    }
                    return EnemyState.Chase; // 무조건 도망 유지

                // (2) 중립 몬스터
                case AttackType.Neutral:
                    if (owner.AttackTarget == null)
                        return EnemyState.Idle;

                    if (owner.IsAttackCooldown)
                    {
                        // 거리 변화가 없다면 상태 유지
                        if (!owner.IsEnoughDistanceChange())
                            return EnemyState.Idle;
                        if (dist < minDist || dist > maxDist)
                            return EnemyState.Idle;
                        return EnemyState.Chase;
                    }

                    if (!owner.IsAttackCooldown && owner.IsIDamageableInAttackRange)
                        return EnemyState.Attack;

                    return EnemyState.Chase;

                // (3) 공격적 몬스터
                case AttackType.Aggressive:
                    if (owner.AttackTarget == null) return EnemyState.Idle;

                    if (owner.IsAttackCooldown)
                    {
                        // 거리 변화가 없다면 상태 유지
                        if (!owner.IsEnoughDistanceChange())
                            return EnemyState.Idle;
                        if (dist < minDist || dist > maxDist)
                            return EnemyState.Idle;
                        return EnemyState.Chase;
                    }

                    if (!owner.IsAttackCooldown && owner.IsIDamageableInAttackRange)
                        return EnemyState.Attack;

                    return EnemyState.Chase;
            }
            return EnemyState.Chase;
        }
    }
    
    public class AttackState : IState<EnemyController, EnemyState>
    {
        private readonly int isMovingHash = Animator.StringToHash("IsMoving");
        private readonly int isTargetHash = Animator.StringToHash("IsTarget");
        private readonly int isAttackHash = Animator.StringToHash("Attack");
        
        public void OnEnter(EnemyController owner)
        {
            owner.Animator.SetTrigger(isAttackHash);
            owner.StartAttackCooldown(owner.EnemyStatus.AttackCooldown);
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