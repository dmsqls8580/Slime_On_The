using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boss2States
{
    public enum Boss2State
    {
        Idle,
        Wander,
        Chase,
        Melee,
        Bubble1,
        Bubble2,
        Dead
    }

    public class IdleState : IState<Boss2Controller, Boss2State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isMeleeHash = Animator.StringToHash("IsMelee");
        private readonly int isBubble1Hash = Animator.StringToHash("IsBubble1");
        private readonly int isBubble2Hash = Animator.StringToHash("isBubble2");
        
        private float idleDuration;
        private float idleTimer;
        
        public void OnEnter(Boss2Controller owner)
        {
            // MinMoveDelay와 MaxMoveDelay 사이 랜덤한 시간만큼 IdleState 유지
            idleDuration = Random.Range(owner.BossStatus.MinMoveDelay, owner.BossStatus.MaxMoveDelay);
            
            // 초기화
            idleTimer = 0f;
            
            owner.Animator.SetBool(isWanderingHash, false);
            owner.Animator.SetBool(isChasingHash, false);
            owner.Animator.SetBool(isMeleeHash, false);
            owner.Animator.SetBool(isBubble1Hash, false);
            owner.Animator.SetBool(isBubble2Hash, false);
        }

        public void OnUpdate(Boss2Controller owner)
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

        public void OnFixedUpdate(Boss2Controller owner)
        {
            
        }

        public void OnExit(Boss2Controller owner)
        {
            
        }

        public Boss2State CheckTransition(Boss2Controller owner)
        {
            // 몬스터 사망시 Dead 모드로 전환.
            if (owner.IsDead)
            {
                return Boss2State.Dead;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우 Chase 모드로 전환.
            if (owner.ChaseTarget != null)
            {
                return Boss2State.Chase;
            }
            // 일정 시간이 지나면 자동으로 Wander 모드로 전환.
            if (idleTimer >= idleDuration)
            {
                return Boss2State.Wander;
            }
            return Boss2State.Idle;
        }
    }
    
    public class WanderState : IState<Boss2Controller, Boss2State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isMeleeHash = Animator.StringToHash("IsMelee");
        private readonly int isBubble1Hash = Animator.StringToHash("IsBubble1");
        private readonly int isBubble2Hash = Animator.StringToHash("isBubble2");
        
        public void OnEnter(Boss2Controller owner)
        {
            owner.Animator.SetBool(isWanderingHash, true);
            // 랜덤 방향으로 이동.
            OnMoveRandom(owner);
        }

        public void OnUpdate(Boss2Controller owner)
        {
            
        }

        public void OnFixedUpdate(Boss2Controller owner)
        {
            
        }

        public void OnExit(Boss2Controller owner)
        {
            owner.Animator.SetBool(isWanderingHash, false);
        }

        public Boss2State CheckTransition(Boss2Controller owner)
        {
            if (owner.IsDead)
            {
                return Boss2State.Dead;
            }
            // 플레이어가 몬스터 감지 범위 내에 들어갈 경우, Chase 모드로 전환.
            if (owner.ChaseTarget != null)
            {
                return Boss2State.Chase;
            }
            // 목적지로 이동이 끝나면 idle 모드로 전환.
            if (ReachedDesination(owner))
            {
                return Boss2State.Idle;
            }
            return Boss2State.Wander;
        }
        
        // wanderRadius 내 랜덤한 위치로 이동
        private void OnMoveRandom(Boss2Controller owner)
        {
            Vector2 randomCircle = Random.insideUnitCircle * owner.BossStatus.WanderRadius;
            Vector3 randomPos =  owner.SpawnPos + new Vector3(randomCircle.x, 0, randomCircle.y);

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, owner.BossStatus.WanderRadius, UnityEngine.AI.NavMesh.AllAreas))
            {
                owner.Agent.SetDestination(hit.position);
            }

        }

        // 이동이 끝났는지 판별
        private bool ReachedDesination(Boss2Controller owner)
        {
            return !owner.Agent.pathPending && owner.Agent.remainingDistance <= owner.Agent.stoppingDistance;
        }
    }

    public class ChaseState : IState<Boss2Controller, Boss2State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isMeleeHash = Animator.StringToHash("IsMelee");
        private readonly int isBubble1Hash = Animator.StringToHash("IsBubble1");
        private readonly int isBubble2Hash = Animator.StringToHash("isBubble2");
        
        public void OnEnter(Boss2Controller owner)
        {
            owner.Animator.SetBool(isChasingHash, true);
        }

        public void OnUpdate(Boss2Controller owner)
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

        public void OnFixedUpdate(Boss2Controller owner)
        {
            
        }

        public void OnExit(Boss2Controller owner)
        {
            owner.Agent.ResetPath();
            owner.Animator.SetBool(isChasingHash, false);
        }

        public Boss2State CheckTransition(Boss2Controller owner)
        {
            return Boss2State.Chase;
        }
    }
    
    public class MeleeState :  IState<Boss2Controller, Boss2State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isMeleeHash = Animator.StringToHash("IsMelee");
        private readonly int isBubble1Hash = Animator.StringToHash("IsBubble1");
        private readonly int isBubble2Hash = Animator.StringToHash("isBubble2");
        
        public void OnEnter(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnExit(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public Boss2State CheckTransition(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class Bubble1State :  IState<Boss2Controller, Boss2State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isMeleeHash = Animator.StringToHash("IsMelee");
        private readonly int isBubble1Hash = Animator.StringToHash("IsBubble1");
        private readonly int isBubble2Hash = Animator.StringToHash("isBubble2");
        
        public void OnEnter(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnExit(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public Boss2State CheckTransition(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class Bubble2State :  IState<Boss2Controller, Boss2State>
    {
        private readonly int isWanderingHash = Animator.StringToHash("IsWandering");
        private readonly int isChasingHash = Animator.StringToHash("IsChasing");
        private readonly int isMeleeHash = Animator.StringToHash("IsMelee");
        private readonly int isBubble1Hash = Animator.StringToHash("IsBubble1");
        private readonly int isBubble2Hash = Animator.StringToHash("isBubble2");
        
        public void OnEnter(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnExit(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }

        public Boss2State CheckTransition(Boss2Controller owner)
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class DeadState :  IState<Boss2Controller, Boss2State>
    {
        private readonly int isDeadHash = Animator.StringToHash("Die");
        
        public void OnEnter(Boss2Controller owner)
        {
            owner.Animator.SetTrigger(isDeadHash);
        }

        public void OnUpdate(Boss2Controller owner)
        {
            
        }

        public void OnFixedUpdate(Boss2Controller owner)
        {
            
        }

        public void OnExit(Boss2Controller owner)
        {
            
        }

        public Boss2State CheckTransition(Boss2Controller owner)
        {
            return owner.IsDead? Boss2State.Dead : Boss2State.Idle;
        }
    }
}

