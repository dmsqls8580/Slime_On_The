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
    public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable, IDamageable
    {

        public Transform attackPivotRotate;
        public Transform attackPivot;
        
        public Transform AttackPivot => attackPivot;
        
        public PlayerStatus PlayerStatus { get; private set; }

        private ToolController toolController;

        private InputController inputController;
        private PlayerAnimationController animationController;
        
        public PlayerAnimationController AnimationController => animationController;
        
        private InteractionHandler interactionHandler;
        private InteractionSelector interactionSelector;
        
        private SkillExecutor skillExecutor;
        public SkillExecutor SkillExecutor => skillExecutor;

        private ForceReceiver forceReceiver;
        
        private Rigidbody2D rigid2D;
        public Rigidbody2D Rigid2D => rigid2D;

        private Vector2 moveInput;
        private Vector2 lastMoveDir = Vector2.right;

        public Vector2 MoveInput => moveInput;
        public Vector2 LastMoveDir => lastMoveDir;

        private float actCoolDown = 0f;
        private float damageDelay = 0.5f;
        private float damageDelayTimer = 0f;
        
        private bool dashTrigger;

        public bool DashTrigger
        {
            get => dashTrigger;
            set => dashTrigger = value;
        }

        private bool attackQueued = false;

        public bool CanAttack => attackCooldown <= 0;
        public bool AttackTrigger => attackQueued && CanAttack;


        public StatBase AttackStat { get; }

        public IDamageable Target { get; private set; }

        public bool IsDead { get; }
        public Collider2D Collider => GetComponent<Collider2D>();

        protected override void Awake()
        {
            base.Awake();
            inputController = GetComponent<InputController>();
            PlayerStatus = GetComponent<PlayerStatus>();
            forceReceiver = GetComponent<ForceReceiver>();
            animationController =  GetComponent<PlayerAnimationController>();
            skillExecutor = GetComponent<SkillExecutor>();
            toolController= GetComponent<ToolController>();
            interactionHandler = GetComponentInChildren<InteractionHandler>();
            interactionSelector = GetComponentInChildren<InteractionSelector>();
            
            rigid2D = GetComponent<Rigidbody2D>();
        }

        protected override void Start()
        {
            base.Start();
            PlayerTable playerTable = TableManager.Instance.GetTable<PlayerTable>();
            PlayerSO playerData = playerTable.GetDataByID(0);

            PlayerStatus.Init(playerData);
            StatManager.Init(playerData);

            var action = inputController.PlayerActions;
            // Move
            action.Move.performed += context =>
            {
                moveInput = context.ReadValue<Vector2>();
                if (moveInput.sqrMagnitude > 0.01f)
                    lastMoveDir = moveInput.normalized;
            };
            
            action.Move.canceled += context => moveInput = rigid2D.velocity = Vector2.zero;
            
            //Attack
            action.Attack0.performed += context =>
            {
                if (CanAttack)
                    attackQueued = true;
            };
            //Dash
            action.Dash.performed += context => dashTrigger = true;
            
            //Gathering
            action.Gathering.performed+= context =>
            {
               TryInteract();
            };
        }

        private void LateUpdate()
        {
            UpdateAttackPivotRotation();

            if (attackCooldown > 0f)
            {
                attackCooldown -= Time.deltaTime;
            }

            if (actCoolDown > 0f)
            {
                actCoolDown -= Time.deltaTime;
            }

            if (damageDelayTimer > 0f)
            {
                damageDelayTimer -= Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.K))
            {
                PlayerStatus.RecoverSlimeGauge(30);
            }
        }

        protected override IState<PlayerController, PlayerState> GetState(PlayerState _state)
        {
            switch (_state)
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
                    //return new Attack1State(PlayerSkillMananger.Instance.GetSkill(true));
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
        }

        private float attackCooldown = 0f;


        public void SetAttackCoolDown(float _coolDown)
        {
            attackCooldown = _coolDown;
        }

        public void ResetAttackTrigger() => attackQueued = false;

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

        public void TryInteract()
        {
            if (actCoolDown > 0) return;
            var target = interactionSelector.FInteractable;

            if (target == null)
            {
                Logger.Log("Target is null");
                return;
            }
            
            interactionHandler.HandleInteraction(target, InteractionCommandType.F, this);

            float toolActSpd = toolController.GetAttackSpd();
            actCoolDown = 1f / Mathf.Max(toolActSpd, 0.01f);
        }
        
        public Vector2 UpdatePlayerDirectionByMouse()
        {
            Vector2 mouseScreenPos = inputController.LookDirection;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y,
                Mathf.Abs(Camera.main.transform.position.z)));
            Vector3 playerPos = transform.position;

            return (mouseWorldPos - playerPos).normalized;
        }

        private void UpdateAttackPivotRotation()
        {
            Vector2 mouseScreenPos = inputController.LookDirection;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y,
                Mathf.Abs(Camera.main.transform.position.z)));
            Vector2 dir = (mouseWorldPos - attackPivotRotate.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            attackPivotRotate.rotation = Quaternion.Euler(0, 0, angle + 180);
        }

        public void TakeDamage(IAttackable _attacker)
        {
            if (IsDead || damageDelayTimer > 0f) return;
            if (_attacker != null)
            {
                // 피격
                PlayerStatus.TakeDamage(_attacker.AttackStat.GetCurrent(), StatModifierType.Base);
                damageDelayTimer = damageDelay;
                if (PlayerStatus.CurrentHp <= 0)
                {
                    Dead();
                }
            }
        }

        public void Dead()
        {
            if (PlayerStatus.CurrentHp <= 0)
            {
                ChangeState(PlayerState.Dead);
            }
        }
        
    }
}