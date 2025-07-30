using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Boss1States
{
    public enum Boss1State
    {
        Idle,
        Wander,
        Chase,
        Pattern1, // cast1 → stomp
        Pattern2, // cast2 → stomp
        Pattern3, // cast1 → cast2
        Dead
    }

    public class IdleState : IState<Boss1Controller, Boss1State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isCastingHash_1 = Animator.StringToHash("IsCasting_1");
        private readonly int isCastingHash_2 = Animator.StringToHash("IsCasting_2");
        private readonly int isStompingHash = Animator.StringToHash("IsStomping");
        
        private float idleDuration;
        private float idleTimer;
        
        public void OnEnter(Boss1Controller owner)
        {
            // MinMoveDelay와 MaxMoveDelay 사이 랜덤한 시간만큼 IdleState 유지
            idleDuration = Random.Range(owner.BossStatus.MinMoveDelay, owner.BossStatus.MaxMoveDelay);
            
            // 초기화
            idleTimer = 0f;
            
            owner.Animator.SetBool(isWanderingHash, false);
            owner.Animator.SetBool(isChasingHash, false);
            owner.Animator.SetBool(isStompingHash, false);
            owner.Animator.SetBool(isCastingHash_1, false);
            owner.Animator.SetBool(isCastingHash_2, false);
        }

        public void OnUpdate(Boss1Controller owner)
        {
            idleTimer += Time.deltaTime;
            
            // 플레이어가 너무 가까이 있을 경우 감지
            GameObject player = owner.ChaseTarget;
            if (player != null)
            {
                float distance = Vector2.Distance(owner.transform.position, player.transform.position);
                if (distance <= owner.BossStatus.AttackRange)
                {
                    owner.ChaseTarget = player;
                }
            }
        }

        public void OnFixedUpdate(Boss1Controller owner)
        {
            
        }

        public void OnExit(Boss1Controller entity)
        {
            
        }

        public Boss1State CheckTransition(Boss1Controller owner)
        {
            // 몬스터 사망시 Dead 모드로 전환.
            if (owner.IsDead)
            {
                return Boss1State.Dead;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우 Chase 모드로 전환.
            if (owner.ChaseTarget != null)
            {
                return Boss1State.Chase;
            }
            // 일정 시간이 지나면 자동으로 Wander 모드로 전환.
            if (idleTimer >= idleDuration)
            {
                return Boss1State.Wander;
            }
            // 배회모드로 전환되지 않았을 시 idle모드.
            return Boss1State.Idle;
        }
    }
    
    public class WanderState : IState<Boss1Controller, Boss1State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isCastingHash_1 = Animator.StringToHash("IsCasting_1");
        private readonly int isCastingHash_2 = Animator.StringToHash("IsCasting_2");
        private readonly int isStompingHash = Animator.StringToHash("IsStomping");
        public void OnEnter(Boss1Controller owner)
        {
            owner.Animator.SetBool(isWanderingHash, true);
            // 랜덤 방향으로 이동.
            OnMoveRandom(owner);
        }

        public void OnUpdate(Boss1Controller owner)
        {
            
        }

        public void OnFixedUpdate(Boss1Controller owner)
        {
            
        }

        public void OnExit(Boss1Controller owner)
        {
            owner.Animator.SetBool(isWanderingHash, false);
        }

        public Boss1State CheckTransition(Boss1Controller owner)
        {
            if (owner.IsDead)
            {
                return Boss1State.Dead;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우, Chase 모드로 전환.
            if (owner.ChaseTarget != null)
            {
                return Boss1State.Chase;
            }
            // 목적지로 이동이 끝나면 idle 모드로 전환.
            if (ReachedDesination(owner))
            {
                return Boss1State.Idle;
            }
            return Boss1State.Wander;
        }
        
        // wanderRadius 내 랜덤한 위치로 이동
        private void OnMoveRandom(Boss1Controller owner)
        {
            Vector2 randomCircle = Random.insideUnitCircle * owner.BossStatus.WanderRadius;
            Vector3 randomPos =  owner.SpawnPos + new Vector3(randomCircle.x, 0, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, owner.BossStatus.WanderRadius, NavMesh.AllAreas))
            {
                owner.Agent.SetDestination(hit.position);
            }

        }

        // 이동이 끝났는지 판별
        private bool ReachedDesination(Boss1Controller owner)
        {
            return !owner.Agent.pathPending && owner.Agent.remainingDistance <= owner.Agent.stoppingDistance;
        }
    }
    
    public class ChaseState : IState<Boss1Controller, Boss1State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isCastingHash_1 = Animator.StringToHash("IsCasting_1");
        private readonly int isCastingHash_2 = Animator.StringToHash("IsCasting_2");
        private readonly int isStompingHash = Animator.StringToHash("IsStomping");
        public void OnEnter(Boss1Controller owner)
        {
            owner.Animator.SetBool(isChasingHash, true);
        }

        public void OnUpdate(Boss1Controller owner)
        {
            // Target의 위치를 추적해 이동.
            if (owner.ChaseTarget != null)
            {
                if (owner.IsPlayerInAttackRange)
                {
                    owner.Agent.ResetPath();
                }
                else
                {
                    owner.Agent.SetDestination(owner.ChaseTarget.transform.position);
                }
            }
        }

        public void OnFixedUpdate(Boss1Controller owner)
        {
            
        }

        public void OnExit(Boss1Controller owner)
        {
            owner.Agent.ResetPath();
            owner.Animator.SetBool(isChasingHash, false);
        }

        public Boss1State CheckTransition(Boss1Controller owner)
        {
            if (owner.IsDead)
            {
                return Boss1State.Dead;
            }
            // 플레이어가 감지 범위 밖으로 나갈 경우, Idle 모드로 전환.
            if (owner.ChaseTarget == null)
            {
                return Boss1State.Idle;
            }
            // 플레이어가 공격 범위 내에 들어올 경우, 랜덤 패턴 출력
            if (owner.ChaseTarget != null &&  owner.IsPlayerInAttackRange)
            {
                return owner.EnterRandomPattern();
            }
            return Boss1State.Chase;
        }
    }
    
    // Pattern 1은 Cast1 -> Chase -> Stomp -> Chase
    public class Pattern1State : IState<Boss1Controller, Boss1State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isCastingHash_1 = Animator.StringToHash("IsCasting_1");
        private readonly int isCastingHash_2 = Animator.StringToHash("IsCasting_2");
        private readonly int isStompingHash = Animator.StringToHash("IsStomping");
        
        private float timer;
        private int patternPhase;
        
        public void OnEnter(Boss1Controller owner)
        {
            // 이동 정지
            owner.Agent.isStopped = true;
            owner.Agent.ResetPath();
            
            // Cast1 애니메이션 재생
            owner.Animator.SetBool(isChasingHash, false);
            owner.Animator.SetBool(isCastingHash_1, true);
            timer = 0f;
            patternPhase = 0;
        }

        public void OnUpdate(Boss1Controller owner)
        {
            timer += Time.deltaTime;
            
            switch (patternPhase)
            {
                case 0:
                    if (timer >= owner.Cast1Duration)
                    {
                        // Cast1 -> Chase
                        owner.Animator.SetBool(isCastingHash_1, false);
                        owner.Animator.SetBool(isChasingHash, true);
                        timer = 0f;
                        patternPhase = 1;
                    }
                    break;
                case 1:
                    if (owner.ChaseTarget != null &&  owner.IsPlayerInAttackRange)
                    {
                        // 이동 정지
                        owner.Agent.isStopped = true;
                        owner.Agent.ResetPath();
                        
                        //  Chase -> Stomp
                        owner.Animator.SetBool(isChasingHash, false);
                        owner.Animator.SetBool(isStompingHash, true);
                        timer = 0f;
                        patternPhase = 2;
                        
                        // Todo : Stomp 시 주변에 데미지
                    }
                    else if (owner.ChaseTarget != null)
                    {
                        // Chase
                        owner.Agent.SetDestination(owner.ChaseTarget.transform.position);
                    }
                    break;
                case 2:
                    if (timer >= owner.StompDuration)
                    {
                        // Stomp -> Chases
                        owner.Animator.SetBool(isStompingHash, false);

                        patternPhase = 3;
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnFixedUpdate(Boss1Controller owner)
        {
            
        }

        public void OnExit(Boss1Controller owner)
        {
            owner.Animator.SetBool(isWanderingHash, false);
            owner.Animator.SetBool(isChasingHash, false);
            owner.Animator.SetBool(isStompingHash, false);
            owner.Animator.SetBool(isCastingHash_1, false);
            owner.Animator.SetBool(isCastingHash_2, false);
        }

        public Boss1State CheckTransition(Boss1Controller owner)
        {
            if (owner.IsDead)
            {
                return Boss1State.Dead;
            }
            if (owner.ChaseTarget == null)
            {
                return Boss1State.Idle;
            }
            if (patternPhase == 3)
            {
                return Boss1State.Idle;
            }
            return Boss1State.Pattern1;
        }
    }
    
    // Pattern 2는 Cast2 -> Chase -> Stomp -> Chase
    public class Pattern2State : IState<Boss1Controller, Boss1State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isCastingHash_1 = Animator.StringToHash("IsCasting_1");
        private readonly int isCastingHash_2 = Animator.StringToHash("IsCasting_2");
        private readonly int isStompingHash = Animator.StringToHash("IsStomping");
        
        private float timer;
        private int patternPhase;
        public void OnEnter(Boss1Controller owner)
        {
            // Cast2
            // 이동 정지
            owner.Agent.isStopped = true;
            owner.Agent.ResetPath();
            
            timer = 0f;
            patternPhase = 0;
            owner.Animator.SetBool(isCastingHash_2, true);
        }

        public void OnUpdate(Boss1Controller owner)
        {
            timer += Time.deltaTime;
            switch (patternPhase)
            {
                case 0: // Chase
                    if (timer >= owner.Cast2Duration)
                    {
                        owner.Animator.SetBool(isCastingHash_2, false);
                        owner.Animator.SetBool(isChasingHash, true);
                        timer = 0f;
                        patternPhase = 1;
                    }
                    break;
                case 1: // Stomp
                    if (owner.ChaseTarget != null && owner.IsPlayerInAttackRange)
                    {
                        // 이동 정지
                        owner.Agent.isStopped = true;
                        owner.Agent.ResetPath();
                        
                        owner.Animator.SetBool(isChasingHash, false);
                        owner.Animator.SetBool(isStompingHash, true);
                        timer = 0f;
                        patternPhase = 2;
                    }
                    else if (owner.ChaseTarget != null)
                    {
                        owner.Agent.SetDestination(owner.ChaseTarget.transform.position);
                    }
                    break;
                case 2: // Chase
                    if (timer >= owner.StompDuration)
                    {
                        owner.Animator.SetBool(isStompingHash, false);
                        owner.Animator.SetBool(isChasingHash, true);
                        timer = 0f;
                        patternPhase = 3;
                    }
                    break;
                default:
                    break;
                
            }
        }

        public void OnFixedUpdate(Boss1Controller owner)
        {
            
        }

        public void OnExit(Boss1Controller owner)
        {
            owner.Animator.SetBool(isWanderingHash, false);
            owner.Animator.SetBool(isChasingHash, false);
            owner.Animator.SetBool(isStompingHash, false);
            owner.Animator.SetBool(isCastingHash_1, false);
            owner.Animator.SetBool(isCastingHash_2, false);
        }

        public Boss1State CheckTransition(Boss1Controller owner)
        {
            if (owner.IsDead)
            {
                return Boss1State.Dead;
            }
            if (owner.ChaseTarget == null)
            {
                return Boss1State.Idle;
            }
            if (patternPhase == 3)
            {
                return owner.EnterRandomPattern();
            }
            return Boss1State.Pattern2;
        }
    }
    
    // Pattern 3는 Cast -> Chase -> Cast2 -> Chase
    public class Pattern3State : IState<Boss1Controller, Boss1State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isCastingHash_1 = Animator.StringToHash("IsCasting_1");
        private readonly int isCastingHash_2 = Animator.StringToHash("IsCasting_2");
        private readonly int isStompingHash = Animator.StringToHash("IsStomping");
        
        private float timer;
        private int patternPhase; // 0: Cast1, 1: Chase, 2: Cast2, 3: Chase, 4: Done
        
        public void OnEnter(Boss1Controller owner)
        {
            // Cast1
            // 이동 정지
            owner.Agent.isStopped = true;
            owner.Agent.ResetPath();
            
            timer = 0f;
            patternPhase = 0;
            owner.Animator.SetBool(isCastingHash_1, true);
        }

        public void OnUpdate(Boss1Controller owner)
        {
            timer += Time.deltaTime;
            switch (patternPhase)
            {
                case 0: // Chase
                    if (timer >= owner.Cast1Duration)
                    {
                        owner.Animator.SetBool(isCastingHash_1, false);
                        owner.Animator.SetBool(isChasingHash, true);
                        timer = 0f;
                        patternPhase = 1;
                    }
                    break;
                case 1: // Cast2
                    if (owner.ChaseTarget != null)
                    {
                        // 이동 정지
                        owner.Agent.isStopped = true;
                        owner.Agent.ResetPath();
                        
                        owner.Animator.SetBool(isChasingHash, false);
                        owner.Animator.SetBool(isCastingHash_2, true);
                        timer = 0f;
                        patternPhase = 2;
                    }
                    else if (owner.ChaseTarget != null)
                    {
                        owner.Agent.SetDestination(owner.ChaseTarget.transform.position);
                    }
                    break;
                case 2: // Chase
                    if (timer >= owner.Cast2Duration)
                    {
                        owner.Animator.SetBool(isCastingHash_2, false);
                        owner.Animator.SetBool(isChasingHash, true);
                        timer = 0f;
                        patternPhase = 3;
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnFixedUpdate(Boss1Controller owner)
        {
            
        }

        public void OnExit(Boss1Controller owner)
        {
            owner.Animator.SetBool(isWanderingHash, false);
            owner.Animator.SetBool(isChasingHash, false);
            owner.Animator.SetBool(isStompingHash, false);
            owner.Animator.SetBool(isCastingHash_1, false);
            owner.Animator.SetBool(isCastingHash_2, false);
        }

        public Boss1State CheckTransition(Boss1Controller owner)
        {
            if (owner.IsDead) 
            {
                return Boss1State.Dead;
            }
            if (owner.ChaseTarget == null)
            {
                return Boss1State.Idle;
            }
            if (patternPhase == 3)
            {
                return owner.EnterRandomPattern();
            }
            return Boss1State.Pattern3;
        }
    }
    
    public class DeadState : IState<Boss1Controller, Boss1State>
    {
        private readonly int isDeadHash = Animator.StringToHash("Die");
        
        public void OnEnter(Boss1Controller owner)
        {
            owner.Animator.SetTrigger(isDeadHash);
        }

        public void OnUpdate(Boss1Controller owner)
        {
            
        }

        public void OnFixedUpdate(Boss1Controller owner)
        {
            
        }

        public void OnExit(Boss1Controller entity)
        {
            
        }

        public Boss1State CheckTransition(Boss1Controller owner)
        {
            return owner.IsDead? Boss1State.Dead : Boss1State.Idle;
        }
    }
    
    
}