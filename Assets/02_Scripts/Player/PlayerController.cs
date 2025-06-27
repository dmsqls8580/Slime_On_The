using System;
using UnityEngine;

namespace PlayerStates
{
    
    [RequireComponent(typeof(InputController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(ForceReceiver))]
    public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable
    {
        private static readonly int MouseX = Animator.StringToHash("mouseX");
        private static readonly int MouseY = Animator.StringToHash("mouseY");
        private ToolController _toolController;
        private InputController _inputController;
        
        private ForceReceiver _forceReceiver;
        
        private Animator _animator;
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;

        private Vector2  _moveInput;
        private bool _attackTrigger;
        
        private float _finalAtk;
        private float _finalAtkSpd;
        
        public Vector2 MoveInput=>_moveInput;
        public bool AttackTrigger
        {
            get => _attackTrigger;
            set => _attackTrigger = value;
        }


        protected override void Awake()
        {
            base.Awake();
            _inputController = GetComponent<InputController>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _forceReceiver = GetComponent<ForceReceiver>();
            
            _animator = GetComponentInChildren<Animator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected override void Start()
        {
            base.Start();

            var action = _inputController.PlayerActions;
            action.Move.performed += context => _moveInput = context.ReadValue<Vector2>();
            action.Move.canceled += context => _moveInput = Vector2.zero;
            action.Attack0.performed += context => _attackTrigger = true;

            _finalAtk = _toolController.GetAttackPow();
            _finalAtkSpd = _toolController.GetAttackSpd();
        }

        private void LateUpdate()
        {
            UpdatePlayerSpriteDirByMouse();
        }

        protected override IState<PlayerController, PlayerState> GetState(PlayerState state)
        {
            return state switch
            {
                PlayerState.Idle => new IdleState(),
                PlayerState.Move => new MoveState(),
                PlayerState.Dash => new DashState(),
                PlayerState.Attack => new AttackState(5,2),
                PlayerState.Interact => new InteractState(),
                PlayerState.Dead => new DeadState(),
                _ => null
            };
        }

        public override void FindTarget()
        {
            throw new System.NotImplementedException();
        }

        public override void Movement()
        {
            base.Movement();      
            
            float baseSpeed = 5f;
            
            Vector2 moveVelocity = Vector2.zero;
            //이동이 감지되지 않았을 때 외부힘을 통한 넉백
            if (_moveInput.magnitude < 0.01f)
            {
                moveVelocity = _forceReceiver.Force;
            }

            //todo: 베이스 스피드에 더해질 스탯의 값 (지금은 하드코딩)
            
            Vector2 move = _moveInput.normalized;

            moveVelocity = move * baseSpeed + _forceReceiver.Force;

            _rigidbody2D.velocity = moveVelocity;
        }

        public IDamageable Target { get; private set; }

        public void Attack()
        {
            _attackTrigger = false;
        }

        private void UpdatePlayerSpriteDirByMouse()
        {
            Vector2 mouseScreenPos = _inputController.LookDirection;

            Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z));
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos3D);
            Vector3 playerPos = transform.position;

            Vector2 lookDir = (mouseWorldPos - playerPos).normalized;

            if (lookDir.x != 0)
                _spriteRenderer.flipX = lookDir.x < 0;
            
            _animator.SetFloat(MouseX, Mathf.Abs(lookDir.x));
            _animator.SetFloat(MouseY, lookDir.y);
        }
    }
}