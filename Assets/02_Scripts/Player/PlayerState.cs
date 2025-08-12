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

        public void OnUpdate(PlayerController _owner)
        {
        }

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
        private float walkSoundInterval = 0.4f; // 걷기 사운드 재생 간격
        private float walkSoundTimer;
        
        private bool isLeftFoot = true;
        
        public void OnEnter(PlayerController _owner)
        {
            _owner.AnimationController.SetMove(true);
            walkSoundTimer = 0f;
            isLeftFoot = true;
        }

        public void OnUpdate(PlayerController _owner)
        {
            if (!_owner.CanMove)
            {
                _owner.Rigid2D.velocity = Vector2.zero;
                return;
            }

            _owner.Movement();
            
            walkSoundTimer -= Time.deltaTime;
            if (walkSoundTimer <= 0f)
            {
                // 왼발/오른발 번갈아가며 SFX 재생
                var sfx = isLeftFoot ? SFX.PlayerWalkLeft : SFX.PlayerWalkRight;
                SoundManager.Instance.PlaySFX(sfx);
            
                isLeftFoot = !isLeftFoot;
                walkSoundTimer = walkSoundInterval;
            }
            
            Vector2 lookDir = _owner.AnimationController.UpdatePlayerDirectionByMouse();
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
        private const float consumeAmount = 50f;
        private float dashDuration = 0.2f;
        private float dashSpeed = 15f;
        private float timer;
        private Vector2 dashDirection;
        private Vector2 lookDir;

        public void OnEnter(PlayerController _owner)
        {
            if (_owner.PlayerStatusManager.CurrentStamina < 50)
            {
                return;
            }
            
            SoundManager.Instance.PlaySFX(SFX.PlayerDash);
            
            _owner.PlayerStatusManager.ConsumeStamina(consumeAmount);
            lookDir = _owner.AnimationController.UpdatePlayerDirectionByMouse();
            dashDirection = _owner.LastMoveDir.sqrMagnitude > 0.01f ? _owner.LastMoveDir : Vector2.right;

            _owner.AnimationController.TriggerDash();
            _owner.AnimationController.SetLookDir(lookDir);

            timer = 0f;
            _owner.Rigid2D.velocity = dashDirection * dashSpeed;
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
        private int attackSlot = 0;
        private PlayerSkillSO _skill;
        private float timer;
        private bool attackDone;

        public Attack0State(int _attackSlot)
        {
            attackSlot = _attackSlot;
        }

        public void OnEnter(PlayerController _owner)
        {
            _skill = _owner.PlayerSkillMananger.GetSkillSlot(attackSlot);
            if (_skill == null)
            {
                attackDone = true;
                return;
            }

            _owner.PlayerSkillMananger.UseSkill(attackSlot, _owner);
            _owner.Attack();
            timer = 0f;
            attackDone = false;

            SoundManager.Instance.PlaySFX(SFX.SlimeImpactStart);
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
        private int attackSlot = 1;
        private PlayerSkillSO _skill;
        private float timer;
        private bool attackDone;

        public Attack1State(int _attackSlot)
        {
            attackSlot = _attackSlot;
        }

        public void OnEnter(PlayerController _owner)
        {
            _skill = _owner.PlayerSkillMananger.GetSkillSlot(attackSlot);
            if (_skill == null)
            {
                attackDone = true;
                return;
            }

            _owner.PlayerSkillMananger.UseSkill(attackSlot, _owner);
            _owner.Attack();
            timer = 0f;
            attackDone = false;

            SoundManager.Instance.PlaySFX(SFX.SlimeImpactStart);
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
        private float gatherDuration = 0.5f; // 이동 제한 시간

        public void OnEnter(PlayerController _owner)
        {
            _owner.Rigid2D.velocity = Vector2.zero;
            _owner.SetCanMove(false);
        }

        public void OnUpdate(PlayerController _owner)
        {
            if(_owner.actCoolDown>0f)
            {
                _owner.actCoolDown -= Time.deltaTime;
            }

            // 쿨타임이 끝났고, 키가 계속 눌려 있고, 다시 채집 가능한 상태라면
            if (_owner.actCoolDown <= 0f &&
                InputController.Instance.PlayerActions.Gathering.IsPressed() &&
                _owner.CanGathering())
            {
               _owner.Gathering();
            }
        }

        public void OnFixedUpdate(PlayerController _owner) { }

        public void OnExit(PlayerController _owner)
        {
            _owner.SetCanMove(true);
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            // 누르고 있지 않으면 종료
            if (!InputController.Instance.PlayerActions.Gathering.IsPressed())
            {
                return _owner.MoveInput.sqrMagnitude > 0.01f ? PlayerState.Move : PlayerState.Idle;
            }

            return PlayerState.Gathering;
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
            
        }

        public PlayerState CheckTransition(PlayerController _owner)
        {
            if (_owner.CanRespawn)
                return PlayerState.Idle;
            return PlayerState.Dead;
        }
    }
}