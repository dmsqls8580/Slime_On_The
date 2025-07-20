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
        public void OnEnter(PlayerController _owner) { }

        public void OnUpdate(PlayerController _owner) { }

        public void OnFixedUpdate(PlayerController _owner) { }

        public void OnExit(PlayerController _owner) { }

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

            if (_owner.MoveInput.sqrMagnitude > 0.01f)
            {
                return PlayerState.Move;
            }

            return PlayerState.Idle;
        }
    }

    public class MoveState : IState<PlayerController, PlayerState>
    {


        public void OnEnter(PlayerController _owner)
        {
            _owner.AnimationController.SetMove(true);
        }

        public void OnUpdate(PlayerController _owner)
        {
            _owner.Movement();

            Vector2 lookDir = _owner.UpdatePlayerDirectionByMouse();
            _owner.AnimationController.UpdateAnimatorParameters(lookDir);
        }

        public void OnFixedUpdate(PlayerController _owner) { }

        public void OnExit(PlayerController _owner)
        {
            _owner.AnimationController.SetMove(false);
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
            _owner.PlayerAfterEffect.SetEffectActive(true);
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

        public void OnExit(PlayerController _owner)
        {
            _owner.Rigid2D.velocity = Vector2.zero;
            _owner.AnimationController.ReleaseLookDir();
            
            _owner.PlayerAfterEffect.SetEffectActive(false);
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

    // SO 기반 스킬 실행 구조로 변경된 Attack0State
    public class Attack0State : IState<PlayerController, PlayerState>
    {
        private int _skillIndex = 0;
        private PlayerSkillSO _skill;
        private float timer;
        private bool attackDone;

        public Attack0State(int skillIndex)
        {
            _skillIndex = skillIndex;
        }

        public void OnEnter(PlayerController _owner)
        {
            _skill = _owner.PlayerSkillMananger.GetSkillIndex(_skillIndex);
            if (_skill == null)
            {
                Debug.LogError($"Skill not found for index {_skillIndex}");
                attackDone = true;
                return;
            }

            _owner.PlayerSkillMananger.UseSkill(_skillIndex, _owner);
            _owner.Attack();
            timer = 0f;
            attackDone = false;

            _owner.AnimationController.TriggerAttack();
            _owner.SetAttackCoolDown(_skill.cooldown);
        }

        public void OnUpdate(PlayerController _owner)
        {
            _owner.Movement();
            timer += Time.deltaTime;
            if (timer >= _skill.actionDuration)
            {
                attackDone = true;
            }
        }

        public void OnFixedUpdate(PlayerController _owner) { }

        public void OnExit(PlayerController _owner)
        {
            attackDone = true;
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            if (attackDone)
            {
                return _owner.MoveInput.sqrMagnitude < 0.01f ? PlayerState.Idle : PlayerState.Move;
            }

            return PlayerState.Attack0;
        }
    }

    // Attack1State 예시 (동일 구조, 스킬 인덱스만 다름)
    public class Attack1State : IState<PlayerController, PlayerState>
    {
        private int _skillIndex = 1;
        private PlayerSkillSO _skill;
        private float timer;
        private bool attackDone;

        public Attack1State(int skillIndex)
        {
            _skillIndex = skillIndex;
        }

        public void OnEnter(PlayerController _owner)
        {
            _skill = _owner.PlayerSkillMananger.GetSkillIndex(_skillIndex);
            if (_skill == null)
            {
                Debug.LogError($"Skill not found for index {_skillIndex}");
                attackDone = true;
                return;
            }

            _owner.PlayerSkillMananger.UseSkill(_skillIndex, _owner);
            _owner.Attack();
            timer = 0f;
            attackDone = false;

            _owner.AnimationController.TriggerAttack();
            _owner.SetAttackCoolDown(_skill.cooldown);
        }

        public void OnUpdate(PlayerController _owner)
        {
            _owner.Movement();
            timer += Time.deltaTime;
            if (timer >= _skill.actionDuration)
            {
                attackDone = true;
            }
        }

        public void OnFixedUpdate(PlayerController _owner) { }

        public void OnExit(PlayerController _owner)
        {
            attackDone = true;
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            if (attackDone)
            {
                return _owner.MoveInput.sqrMagnitude < 0.01f ? PlayerState.Idle : PlayerState.Move;
            }

            return PlayerState.Attack1;
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

        public void OnUpdate(PlayerController _owner) { }

        public void OnFixedUpdate(PlayerController _owner) { }

        public void OnExit(PlayerController _owner)
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
        }

        public void OnUpdate(PlayerController _owner)
        {
        }

        public void OnFixedUpdate(PlayerController _owner) { }

        public void OnExit(PlayerController _owner)
        {
            _owner.CanRespawn = true;
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            if (_owner.CanRespawn)
                return PlayerState.Idle;
            return PlayerState.Dead;
        }
    }
}