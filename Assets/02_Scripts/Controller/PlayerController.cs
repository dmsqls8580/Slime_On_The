using System.Collections.Generic;
using UnityEngine;

namespace PlayerStates
{
    [RequireComponent(typeof(InputController))]
    [RequireComponent(typeof(PlayerAnimationController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(ForceReceiver))]
    [RequireComponent(typeof(PlayerStatus))]
    public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable
    {
        private static readonly int MOUSE_X = Animator.StringToHash("mouseX");
        private static readonly int MOUSE_Y = Animator.StringToHash("mouseY");

        public PlayerStatus PlayerStatus{get; private set;}
        
        private ToolController toolController;
        private PlayerAnimationController animationController;
        public PlayerAnimationController AnimationController => animationController;

        private InputController inputController;
        private SkillExecutor skillExecutor;
        public SkillExecutor SkillExecutor => skillExecutor;
        
        private ForceReceiver forceReceiver;
        private Animator animator;
        private Rigidbody2D rigid2D;
        private SpriteRenderer spriteRenderer;

        public Transform attackPivotRotate;
        public Transform attackPivot;
        public Transform AttackPivot => attackPivot;

        private Vector2 moveInput;
        private Vector2 lastMoveDir = Vector2.right;

        public Vector2 MoveInput => moveInput;
        public Vector2 LastMoveDir => lastMoveDir;
        public Rigidbody2D Rigid2D => rigid2D;
        private List<IDamageable> targets = new List<IDamageable>();

        private float finalAtk;
        private float finalAtkSpd;

        private bool dashTrigger;
        public bool DashTrigger
        {
            get => dashTrigger;
            set => dashTrigger = value;
        }

        private bool canAttack = true;
        private bool attackQueued = false;
        public bool AttackTrigger => attackQueued && canAttack;

        public StatBase AttackStat { get; }
        
        public IDamageable Target { get; private set; }
        
        public bool IsDead { get; }
        public Collider2D Collider { get; }

        protected override void Awake()
        {
            base.Awake();
            inputController = GetComponent<InputController>();
            rigid2D = GetComponent<Rigidbody2D>();
            PlayerStatus = GetComponent<PlayerStatus>();
            forceReceiver = GetComponent<ForceReceiver>();
            skillExecutor = GetComponent<SkillExecutor>();
            animationController = GetComponent<PlayerAnimationController>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        }

        protected override void Start()
        {
            base.Start();
            PlayerTable playerTable= TableManager.Instance.GetTable<PlayerTable>(); 
            PlayerSO playerData = playerTable.GetDataByID(0); 
           
            PlayerStatus.Init(playerData);
             
            StatManager.Init(playerData);

            
            var action = inputController.PlayerActions;
            action.Move.performed += context =>
            {
                moveInput = context.ReadValue<Vector2>();
                if (moveInput.sqrMagnitude > 0.01f)
                    lastMoveDir = moveInput.normalized;
            };
            action.Move.canceled += context => moveInput = rigid2D.velocity = Vector2.zero;

            action.Attack0.performed += context =>
            {
                if (canAttack)
                    attackQueued = true;
            };

            action.Dash.performed += context => dashTrigger = true;
        }

        private void LateUpdate()
        {
            Vector2 lookDir = UpdatePlayerDirectionByMouse();
            UpdateAnimatorParameters(lookDir);
            UpdateSpriteFlip(lookDir);
            UpdateAttackPivotRotation();

            if (Input.GetKeyDown(KeyCode.K))
            {
                PlayerStatus.RecoverSlimeGauge(5);
                Debug.Log($"Recover! +SlimeGauge : 5");
            }
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
                    return new Attack0State(PlayerSkillMananger.Instance.GetSkill(false));
                case PlayerState.Attack1:
                    return new Attack0State(PlayerSkillMananger.Instance.GetSkill(true));
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

        public void Attack()
        {
            attackQueued = false;
            canAttack = false;
        }

        public void ResetAttackTrigger() => attackQueued = false;
        public void EnableAttack() => canAttack = true;

        public override void Movement()
        {
            base.Movement();

            float speed = PlayerStatus.MoveSpeed;

            Vector2 moveVelocity = Vector2.zero;

            if (moveInput.magnitude < 0.01f)
            {
                moveVelocity = forceReceiver.Force;
            }
            else
            {
                moveVelocity = moveInput.normalized * speed + forceReceiver.Force;
            }

            rigid2D.velocity = moveVelocity;
        }

        public Vector2 UpdatePlayerDirectionByMouse()
        {
            Vector2 mouseScreenPos = inputController.LookDirection;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
            Vector3 playerPos = transform.position;

            return (mouseWorldPos - playerPos).normalized;
        }

        private void UpdateSpriteFlip(Vector2 lookDir)
        {
            if (lookDir.x != 0)
                spriteRenderer.flipX = lookDir.x < 0;
        }

        private void UpdateAnimatorParameters(Vector2 lookDir)
        {
            animator.SetFloat(MOUSE_X, Mathf.Abs(lookDir.x));
            animator.SetFloat(MOUSE_Y, lookDir.y);
        }

        private void UpdateAttackPivotRotation()
        {
            Vector2 mouseScreenPos = inputController.LookDirection;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
            Vector2 dir = (mouseWorldPos - attackPivotRotate.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            attackPivotRotate.rotation = Quaternion.Euler(0, 0, angle + 180);
        }
        public void Dead()
        {
            throw new System.NotImplementedException();
        }
        
    }
}
