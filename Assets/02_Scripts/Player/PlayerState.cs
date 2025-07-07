using System.Collections;
using UnityEngine;

namespace PlayerStates
{
    public enum PlayerState
    {
        Idle,
        Move,
        Dash,
        Attack0,
        Attack1,
        Dead,
    }

    public class IdleState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController owner)
        {
        }

        public void OnUpdate(PlayerController owner)
        {
        }

        public void OnFixedUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController entity)
        {
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            //달리기는 없음 Move만
            //공격 상태일때 Attack 상태 변환
            if (owner.AttackTrigger)
            {
                owner.Attack();
                return PlayerState.Attack0;
            }
            if (owner.DashTrigger)
            {
                owner.DashTrigger = false;
                return PlayerState.Dash;
            }

            if (owner.MoveInput.sqrMagnitude > 0.01f)
            {
                return PlayerState.Move;
            }

            return PlayerState.Idle;
        }
    }

    public class MoveState : IState<PlayerController, PlayerState>
    {
        private float slimeTimer;
        private const float consumeInterval = 1f;
        private const float consumeAmount = 0.5f;
            
        public void OnEnter(PlayerController owner)
        {
            owner.AnimationController.SetMove(true);
            slimeTimer = 0f;
        }

        public void OnUpdate(PlayerController owner)
        {
            owner.Movement();

            Vector2 lookDir = owner.UpdatePlayerDirectionByMouse();
            owner.AnimationController.UpdateAnimatorParameters(lookDir);

            slimeTimer += Time.deltaTime;

            if (slimeTimer >= consumeInterval)
            {
                owner.PlayerStatus.ConsumeSlimeGauge(consumeAmount);
                slimeTimer = 0f;
            }
        }

        public void OnFixedUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController entity)
        {
            entity.AnimationController.SetMove(false);
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.AttackTrigger)
            {
                owner.Attack();
                return PlayerState.Attack0;
            }

            if (owner.DashTrigger)
            {
                owner.DashTrigger = false;
                return PlayerState.Dash;
            }

            if (owner.MoveInput.sqrMagnitude < 0.01f)
            {
                return PlayerState.Idle;
            }

            return PlayerState.Move;
        }
    }

    public class DashState : IState<PlayerController, PlayerState>
    {
        private float dashDuration = 0.2f;
        private float dashSpeed = 15f;
        private const float consumeAmount = 20f;
        private float timer;
        private Vector2 dashDirection;

        public void OnEnter(PlayerController owner)
        {
            if(owner.PlayerStatus.CurrentSlimeGauge<=20) return;
         
            owner.AnimationController.TriggerDash();
            
            timer = 0f;
            owner.Rigid2D.velocity = dashDirection * dashSpeed;
            dashDirection = owner.LastMoveDir.sqrMagnitude > 0.01f ? owner.LastMoveDir : Vector2.right;
            owner.PlayerStatus.ConsumeSlimeGauge(consumeAmount);
        }

        public void OnUpdate(PlayerController owner)
        {
            timer += Time.deltaTime;

            if (timer >= dashDuration)
            {
                owner.Rigid2D.velocity = Vector2.zero;
            }
        }

        public void OnFixedUpdate(PlayerController owner)
        {
            owner.Rigid2D.velocity = dashDirection * dashSpeed;
        }

        public void OnExit(PlayerController entity)
        {
            entity.Rigid2D.velocity = Vector2.zero;
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            if (timer >= dashDuration)
            {
                return owner.MoveInput.sqrMagnitude > 0.01f ? PlayerState.Move : PlayerState.Idle;
            }

            return PlayerState.Dash;
        }
    }

    public class Attack0State : IState<PlayerController, PlayerState>
    {
        private readonly PlayerSkillSO skill;

        private float timer;
        private bool _attackDone;

        public Attack0State(PlayerSkillSO skill)
        {
            this.skill = skill;
        }

        public void OnEnter(PlayerController owner)
        {
            owner.Attack();
            timer = 0f;
            _attackDone = false;
            Vector2 mousePos = owner.GetComponent<InputController>().LookDirection;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y,
                Mathf.Abs(Camera.main.transform.position.z)));
            Vector2 attackDir = ((Vector2)(mouseWorld - owner.transform.position)).normalized;
            owner.AnimationController.TriggerAttack();

            owner.SkillExecutor.Executor(skill, owner.AttackPivot.gameObject, attackDir);

            owner.SetAttackCoolDown(skill.cooldown);
        }

        public void OnUpdate(PlayerController owner)
        {
            owner.Movement();

            timer += Time.deltaTime;
            if (timer >= skill.actionDuration)
            {
                _attackDone = true;
            }
        }

        public void OnFixedUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController entity)
        {
            _attackDone = true;
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            if (_attackDone)
            {
                return owner.MoveInput.sqrMagnitude < 0.01f ? PlayerState.Idle : PlayerState.Move;
            }

            return PlayerState.Attack0;
        }
        
    }

    public class Attack1State : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnExit(PlayerController entity)
        {
            throw new System.NotImplementedException();
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }
    }

    public class DeadState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnExit(PlayerController entity)
        {
            throw new System.NotImplementedException();
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            throw new System.NotImplementedException();
        }
    }
}