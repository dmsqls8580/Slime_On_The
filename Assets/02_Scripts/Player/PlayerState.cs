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
        Interact,
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
        public void OnEnter(PlayerController owner)
        {
            owner.AnimationController.SetMove(true);
        }

        public void OnUpdate(PlayerController owner)
        {
            owner.Movement();

            Vector2 lookDir = owner.UpdatePlayerDirByMouse();
            owner.AnimationController.SetLook(lookDir);
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
        private float _dashDuration = 0.2f;
        private float _dashSpeed = 15f;
        private float _timer;
        private Vector2 _dashDirection;

        public void OnEnter(PlayerController owner)
        {
            _timer = 0f;
            owner.Rigidbody2D.velocity = _dashDirection * _dashSpeed;
            _dashDirection = owner.LastMoveDir.sqrMagnitude > 0.01f ? owner.LastMoveDir : Vector2.right;
        }

        public void OnUpdate(PlayerController owner)
        {
            _timer += Time.deltaTime;

            if (_timer >= _dashDuration)
            {
                owner.Rigidbody2D.velocity = Vector2.zero;
            }
        }

        public void OnFixedUpdate(PlayerController owner)
        {
            owner.Rigidbody2D.velocity = _dashDirection * _dashSpeed;
        }

        public void OnExit(PlayerController entity)
        {
            entity.Rigidbody2D.velocity = Vector2.zero;
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            if (_timer >= _dashDuration)
            {
                return owner.MoveInput.sqrMagnitude > 0.01f ? PlayerState.Move : PlayerState.Idle;
            }

            return PlayerState.Dash;
        }
    }

    public class Attack0State : IState<PlayerController, PlayerState>
    {
        private readonly PlayerSkillSO _skill;

        private float _timer;
        private bool _attackDone;

        public Attack0State(PlayerSkillSO skill)
        {
            _skill = skill;
        }

        public void OnEnter(PlayerController owner)
        {
            owner.Attack();
            _timer = 0f;
            _attackDone = false;
            Vector2 mousePos = owner.GetComponent<InputController>().LookDirection;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y,
                Mathf.Abs(Camera.main.transform.position.z)));
            Vector2 attackDir = ((Vector2)(mouseWorld - owner.transform.position)).normalized;
            owner.AnimationController.TriggerAttack();

            owner.SkillExecutor.Executor(_skill, owner.AttackPivot.gameObject, attackDir);
        }

        public void OnUpdate(PlayerController owner)
        {
            owner.Movement();

            _timer += Time.deltaTime;
            if (_timer >= _skill.cooldown)
            {
                _attackDone = true;
            }
        }

        public void OnFixedUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController entity)
        {
            entity.StartCoroutine(ResetCoolDown(entity));
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            if (_attackDone)
            {
                return owner.MoveInput.sqrMagnitude < 0.01f ? PlayerState.Idle : PlayerState.Move;
            }

            return PlayerState.Attack0;
        }

        private IEnumerator ResetCoolDown(PlayerController owner)
        {
            yield return new WaitForSeconds(_skill.cooldown);
            owner.EnableAttack();
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

    public class InteractState : IState<PlayerController, PlayerState>
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