using UnityEngine;

namespace PlayerStates
{
    [RequireComponent(typeof(InputController))]
    [RequireComponent(typeof(PlayerAnimationController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(ForceReceiver))]
    public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable, IDamageable
    {
        private static readonly int MouseX = Animator.StringToHash("mouseX");
        private static readonly int MouseY = Animator.StringToHash("mouseY");

        private ToolController _toolController;

        private PlayerAnimationController _playerAnimationController;
        public PlayerAnimationController AnimationController => _playerAnimationController;

        private InputController _inputController;

        private SkillExecutor _skillExecutor;
        public SkillExecutor SkillExecutor => _skillExecutor;

        private ForceReceiver _forceReceiver;

        private Animator _animator;
        private Rigidbody2D _rigidbody2D;
        private SpriteRenderer _spriteRenderer;

        public Transform attackPivotRotate;
        public Transform attackPivot;

        public Transform AttackPivot => attackPivot;

        
        private Vector2 _moveInput;

        private float _finalAtk;
        private float _finalAtkSpd;

        public Vector2 MoveInput => _moveInput;
        public Rigidbody2D Rigidbody2D => _rigidbody2D;

        private bool _dashTrigger;

        public bool DashTrigger
        {
            get => _dashTrigger;
            set => _dashTrigger = value;
        }

        private Vector2 _lastMoveDir = Vector2.right; // 기본은 오른쪽
        public Vector2 LastMoveDir => _lastMoveDir;


        private bool _canAttack = true; // FSM이 공격을 받아들일 수 있는지
        private bool _attackQueued = false; // 공격 입력이 들어왔는지

        public bool AttackTrigger => _attackQueued && _canAttack;

        public void ResetAttackTrigger() => _attackQueued = false;
        public void EnableAttack() => _canAttack = true;
        
        protected override void Awake()
        {
            base.Awake();
            _inputController = GetComponent<InputController>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _forceReceiver = GetComponent<ForceReceiver>();
            _skillExecutor = GetComponent<SkillExecutor>();
            _playerAnimationController = GetComponent<PlayerAnimationController>();
            _animator = GetComponentInChildren<Animator>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        protected override void Start()
        {
            base.Start();

            var action = _inputController.PlayerActions;
            action.Move.performed += context =>
            {
                _moveInput = context.ReadValue<Vector2>();
                if (_moveInput.sqrMagnitude > 0.01f)
                {
                    _lastMoveDir = _moveInput.normalized;
                }
            };
            action.Move.canceled += context => _moveInput = _rigidbody2D.velocity = Vector2.zero;
            action.Attack0.performed += context =>
            {
                if (_canAttack)
                {
                    _attackQueued = true;
                }
            };
            action.Dash.performed += context => _dashTrigger = true;

          //  _finalAtk = _toolController.GetAttackPow();
           // _finalAtkSpd = _toolController.GetAttackSpd();
        }

        private void LateUpdate()
        {
            Vector2 lookDir = UpdatePlayerDirByMouse();
            ChangedAnimatorParams(lookDir);
            SpriteFlipX(lookDir);
            UpdateAttackPivotRotate();
        }

        protected override IState<PlayerController, PlayerState> GetState(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.Idle:
                    return new IdleState();
                case PlayerState.Move:
                    return new MoveState();
                case PlayerState.Dash:
                    return new DashState();
                case PlayerState.Attack0:
                {
                    var skill = PlayerSkillMananger.Instance.GetSkill(isSpecial: false);
                    return new Attack0State(skill);
                }
                case PlayerState.Attack1:
                {
                    var skill = PlayerSkillMananger.Instance.GetSkill(isSpecial: true);
                    return new Attack0State(skill);
                }
                case PlayerState.Interact:
                    return new InteractState();
                case PlayerState.Dead:
                    return new DeadState();
                default:
                    return null;
            }
        }

        public override void FindTarget()
        {
            throw new System.NotImplementedException();
        }

        public IDamageable Target { get; private set; }

        public void Attack()
        {            
            _attackQueued = false;     // 트리거 초기화 (입력 소비)
            _canAttack = false;    
        }

        //-------------------------------------------------------------------------

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

        public Vector2 UpdatePlayerDirByMouse()
        {
            Vector2 mouseScreenPos = _inputController.LookDirection;

            Vector3 mouseScreenPos3D = new Vector3(mouseScreenPos.x, mouseScreenPos.y,
                Mathf.Abs(Camera.main.transform.position.z));
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos3D);
            Vector3 playerPos = transform.position;

            return (mouseWorldPos - playerPos).normalized;
        }

        private void SpriteFlipX(Vector2 _lookDir)
        {
            if (_lookDir.x != 0)
                _spriteRenderer.flipX = _lookDir.x < 0;
        }

        private void ChangedAnimatorParams(Vector2 _lookDir)
        {
            _animator.SetFloat(MouseX, Mathf.Abs(_lookDir.x));
            _animator.SetFloat(MouseY, _lookDir.y);
        }

        private void UpdateAttackPivotRotate()
        {
            Vector2 mouseScreenPos = _inputController.LookDirection;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y,
                Mathf.Abs(Camera.main.transform.position.z)));
            Vector2 dir = (mouseWorldPos - attackPivotRotate.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            attackPivotRotate.rotation = Quaternion.Euler(0, 0, angle + 180);
        }

        //-------------------------------------------------------------------------
        
        
        public bool IsDead { get; }
        public Collider2D Collider { get; }
        public void TakeDamage(IAttackable attacker)
        {
            throw new System.NotImplementedException();
        }

        public void Dead()
        {
            throw new System.NotImplementedException();
        }
    }
}