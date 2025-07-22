using _02_Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 


namespace PlayerStates
{
    [RequireComponent(typeof(InputController))]
    [RequireComponent(typeof(PlayerAnimationController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlayerStatus))]
    public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable, IDamageable
    {
        
        public Transform attackPivotRotate;
        public Transform attackPivot;
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private Canvas damageTextCanvas;

        [SerializeField] private UIDead uiDead;
        public UIDead UiDead => uiDead;
        
        public Transform AttackPivot => attackPivot;
        
        public PlayerStatus PlayerStatus { get; private set; }

        private ToolController toolController;
        public ToolController ToolController => toolController;

        private InputController inputController;
        private PlayerAnimationController animationController;
        private PlayerSkillMananger  playerSkillMananger;
        public PlayerSkillMananger PlayerSkillMananger => playerSkillMananger;
        public PlayerAnimationController AnimationController => animationController;
        
        private InteractionHandler interactionHandler;
        private InteractionSelector interactionSelector;
        
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

        public bool IsDead { get; private set; }
        public bool CanRespawn{get; set;}
        public Collider2D Collider => GetComponent<Collider2D>();

        protected override void Awake()
        {
            base.Awake();
            inputController = GetComponent<InputController>();
            PlayerStatus = GetComponent<PlayerStatus>();
            playerSkillMananger= GetComponent<PlayerSkillMananger>();
            animationController =  GetComponent<PlayerAnimationController>();
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
            
            // Inventory
            action.Inventory.performed += context =>
            {
                UIManager.Instance.Toggle<UIInventory>();
            };
            
            // Crafting
            action.Crafting.performed += context =>
            {
                UIManager.Instance.Toggle<UICrafting>();
            };
            
            //Quit
            action.Settings.performed += context =>
            {
                if (!UIManager.Instance.CloseTop())
                {
                    UIManager.Instance.Toggle<UIPauseMenu>();
                }
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
                    return new Attack0State(0);
                
                case PlayerState.Attack1:
                    
                //todo: 스킬 구조 바꿔서 적용
                
                case PlayerState.Dead:
                    return new DeadState();
                case PlayerState.Gathering:
                    return new GatherState();
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

                moveVelocity = moveInput.normalized * speed;

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
            
            ChangeState(PlayerState.Gathering);
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

        public void TakeDamage(IAttackable _attacker, GameObject _attackerObj)
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

        public void ShowDamageText(float _damage, Vector3 _worldPos, Color _color)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(_worldPos);

            var textObj = Instantiate(damageTextPrefab, damageTextCanvas.transform);
            textObj.transform.position= screenPos;

            var damageText = textObj.GetComponent<DamageTextUI>();
            damageText.Init(_damage, _color);
        }

        public void Dead()
        {
            if (PlayerStatus.CurrentHp <= 0)
            {
                IsDead = true;
                animationController.TriggerDead();
                StartCoroutine(DelayDeathUi(3f, "테스트당함."));
                ChangeState(PlayerState.Dead);
            }
        }

// 죽음 연출 후 UI 딜레이 호출
        private IEnumerator DelayDeathUi(float _delay, string _reason)
        {
            yield return new WaitForSeconds(_delay);
            UiDead.TriggerDeath(1, _reason);
        }

        public void Respawn(Vector3 _spawnPos)
        {
            //PlayerStatus.CurrentHp = PlayerStatus.MaxHp;
            IsDead = false;
            transform.position = _spawnPos;
            ChangeState(PlayerState.Idle);
        }
        
    }
} 