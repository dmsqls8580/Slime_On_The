using System.Collections;
using UnityEngine;


namespace PlayerStates
{
    public enum PlayerState
    {
        Idle,
        Move,
        Dash,
        Attack,
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
                return PlayerState.Attack;
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
        }

        public void OnUpdate(PlayerController owner)
        {
            owner.Movement();
        }

        public void OnFixedUpdate(PlayerController owner)
        {
            
        }

        public void OnExit(PlayerController entity)
        {
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            if (owner.AttackTrigger)
                return PlayerState.Attack;
            if (owner.MoveInput.sqrMagnitude < 0.01f)
            {
                return PlayerState.Idle;
            }
            return PlayerState.Move;
        }
    }

    public class DashState : IState<PlayerController, PlayerState>
    {
        private float _dashDuration;
        private float _dashSpeed;
        private float _elapsedTime;
        private Vector2 _dashDirection;
        private bool _isDashFinished;
        public void OnEnter(PlayerController owner)
        {
            _isDashFinished = false;
            _elapsedTime = 0f;

            _dashDirection = owner.MoveInput.sqrMagnitude > 0.01f ? owner.MoveInput.normalized : Vector2.right;
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

    public class AttackState : IState<PlayerController, PlayerState>
    {
        private readonly float _atkPow;
        private readonly float _atkSpd;

        private float _timer;
        private bool _attackDone;
        private Vector2 _attackDir;

        public AttackState(float atkPow,float atkSpd)
        {
            _atkPow= atkPow;
            _atkSpd = atkSpd;
        }
        public void OnEnter(PlayerController owner)
        {
            owner.Attack();
            _timer = 0f;
            _attackDone = false;
            Vector2 mousePos = owner.GetComponent<InputController>().LookDirection;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Mathf.Abs(Camera.main.transform.position.z)));
            _attackDir= ((Vector2)(mouseWorld-owner.transform.position)).normalized;
            
            Debug.Log($"[AttackState] 공격 시작. 방향: {_attackDir}, 파워: {_atkPow}, 속도: {_atkSpd}");
        }

        public void OnUpdate(PlayerController owner)
        {
            owner.Movement();
            
            _timer+=Time.deltaTime;
            if (_timer >= _atkSpd)
            {
                _attackDone = true;
            }
        }

        public void OnFixedUpdate(PlayerController owner)
        {
        }

        public void OnExit(PlayerController entity)
        {
            Debug.Log("Attack 종료!");
        }

        public PlayerState CheckTransition(PlayerController owner)
        {
            if (_attackDone)
            {
                return owner.MoveInput.sqrMagnitude < 0.01f ? PlayerState.Idle : PlayerState.Move;
            }

            return PlayerState.Attack;
        }

        private IEnumerator Attacking()
        {
            //todo: 어택 기능, 어택 애니메이션
            yield return new WaitForSeconds(2f);
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
