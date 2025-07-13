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
        Gathering,
        Dead,
    }

    public class IdleState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController _owner)
        {
        }

        public void OnUpdate(PlayerController _owner)
        {
        }

        public void OnFixedUpdate(PlayerController _owner)
        {
        }

        public void OnExit(PlayerController _entity)
        {
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            //달리기는 없음 Move만
            //공격 상태일때 Attack 상태 변환
            if (_owner.AttackTrigger)
            {
                _owner.Attack();
                return PlayerState.Attack0;
            }

            if (_owner.DashTrigger)
            {
                _owner.DashTrigger = false;
                return PlayerState.Dash;
            }

            if (_owner.MoveInput.sqrMagnitude > 0.01f)
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

        public void OnEnter(PlayerController _owner)
        {
            _owner.AnimationController.SetMove(true);
            slimeTimer = 0f;
        }

        public void OnUpdate(PlayerController _owner)
        {
            _owner.Movement();

            Vector2 lookDir = _owner.UpdatePlayerDirectionByMouse();
            _owner.AnimationController.UpdateAnimatorParameters(lookDir);

            slimeTimer += Time.deltaTime;

            if (slimeTimer >= consumeInterval)
            {
                _owner.PlayerStatus.ConsumeSlimeGauge(consumeAmount);
                slimeTimer = 0f;
            }
        }

        public void OnFixedUpdate(PlayerController _owner)
        {
        }

        public void OnExit(PlayerController _entity)
        {
            _entity.AnimationController.SetMove(false);
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            if (_owner.AttackTrigger)
            {
                _owner.Attack();
                return PlayerState.Attack0;
            }

            if (_owner.DashTrigger)
            {
                _owner.DashTrigger = false;
                return PlayerState.Dash;
            }

            if (_owner.MoveInput.sqrMagnitude < 0.01f)
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
        private const float consumeAmount = 5f;
        private float timer;
        private Vector2 dashDirection;
        private Vector2 lookDir;

        public void OnEnter(PlayerController _owner)
        {
            if (_owner.PlayerStatus.CurrentSlimeGauge <= 20) return;

            lookDir = _owner.UpdatePlayerDirectionByMouse();
            dashDirection = _owner.LastMoveDir.sqrMagnitude > 0.01f ? _owner.LastMoveDir : Vector2.right;

            _owner.AnimationController.TriggerDash();
            _owner.AnimationController.SetLookDir(lookDir);

            timer = 0f;
            _owner.Rigid2D.velocity = dashDirection * dashSpeed;
            _owner.PlayerStatus.ConsumeSlimeGauge(consumeAmount);
        }

        public void OnUpdate(PlayerController _owner)
        {
            timer += Time.deltaTime;

            if (timer >= dashDuration)
            {
                _owner.Rigid2D.velocity = Vector2.zero;
            }
        }

        public void OnFixedUpdate(PlayerController _owner)
        {
            _owner.Rigid2D.velocity = dashDirection * dashSpeed;
        }

        public void OnExit(PlayerController _entity)
        {
            _entity.Rigid2D.velocity = Vector2.zero;
            _entity.AnimationController.ReleaseLookDir();
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            if (timer >= dashDuration)
            {
                return _owner.MoveInput.sqrMagnitude > 0.01f ? PlayerState.Move : PlayerState.Idle;
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

        public void OnEnter(PlayerController _owner)
        {
            _owner.Attack();
            timer = 0f;
            _attackDone = false;
            Vector2 mousePos = _owner.GetComponent<InputController>().LookDirection;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y,
                Mathf.Abs(Camera.main.transform.position.z)));
            Vector2 attackDir = ((Vector2)(mouseWorld - _owner.transform.position)).normalized;
            _owner.AnimationController.TriggerAttack();

            _owner.SkillExecutor.Executor(skill, _owner.AttackPivot.gameObject, attackDir);

            _owner.SetAttackCoolDown(skill.cooldown);
        }

        public void OnUpdate(PlayerController _owner)
        {
            _owner.Movement();

            timer += Time.deltaTime;
            if (timer >= skill.actionDuration)
            {
                _attackDone = true;
            }
        }

        public void OnFixedUpdate(PlayerController _owner)
        {
        }

        public void OnExit(PlayerController _entity)
        {
            _attackDone = true;
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            if (_attackDone)
            {
                return _owner.MoveInput.sqrMagnitude < 0.01f ? PlayerState.Idle : PlayerState.Move;
            }

            return PlayerState.Attack0;
        }
    }

    public class Attack1State : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnExit(PlayerController _entity)
        {
            throw new System.NotImplementedException();
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }
    }

    public class GatherState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController _owner)
        {
            Logger.Log("Gather");
            var equippedToolType = _owner.ToolController.GetEquippedToolType();
            _owner.AnimationController.SetToolType(equippedToolType);
            _owner.AnimationController.TriggerGather();
        }

        public void OnUpdate(PlayerController _owner)
        {
        }

        public void OnFixedUpdate(PlayerController _owner)
        {
        }

        public void OnExit(PlayerController _entity)
        {
            Logger.Log("End Gather");
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            Logger.Log("Change Gather");
            return PlayerState.Idle;
        }
    }

    public class DeadState : IState<PlayerController, PlayerState>
    {
        public void OnEnter(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnUpdate(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnFixedUpdate(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }

        public void OnExit(PlayerController _entity)
        {
            throw new System.NotImplementedException();
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            throw new System.NotImplementedException();
        }
    }
}