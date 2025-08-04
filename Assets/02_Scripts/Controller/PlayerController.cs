using _02_Scripts.Manager;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


namespace PlayerStates
{
    [RequireComponent(typeof(InputController))]
    [RequireComponent(typeof(PlayerAnimationController))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlayerStatusManager))]
    public class PlayerController : BaseController<PlayerController, PlayerState>, IAttackable, IDamageable
    {
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private Canvas damageTextCanvas;
        [SerializeField] private UIDead uiDead;
        [SerializeField] private UIQuickSlot uiQuickSlot;
        [SerializeField] private PlaceMode placeMode;
        
        public Transform attackPivotRotate;
        public Transform attackPivot;
        public Transform AttackPivot => attackPivot;
        public PlayerStatusManager PlayerStatusManager { get; private set; }
        
        private ToolController toolController;
        
        private InputController inputController;
        
        private PlayerAnimationController animationController;
        public PlayerAnimationController AnimationController => animationController;
        
        private PlayerSkillMananger playerSkillMananger;
        public PlayerSkillMananger PlayerSkillMananger => playerSkillMananger;

        private InteractionHandler interactionHandler;
        private InteractionSelector interactionSelector;
        
        private PlayerAfterEffect playerAfterEffect;
        public PlayerAfterEffect PlayerAfterEffect => playerAfterEffect;

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

        public StatBase AttackStat
        {
            get
            {
                if (PlayerStatusManager == null) return null;// StatManager 접근 방식에 맞게
                if (StatManager == null) return null;
                if (StatManager.Stats.TryGetValue(StatType.Attack, out var stat))
                    return stat;
                return null;
            }
        }

        public IDamageable Target { get; }

        public bool IsDead { get; set; }
        public bool CanRespawn { get; set; } 
        public bool CanMove { get; private set; } = true;

        public Collider2D Collider => GetComponent<Collider2D>();

        protected override void Awake()
        {
            base.Awake();
            inputController = GetComponent<InputController>();
            PlayerStatusManager = GetComponent<PlayerStatusManager>();
            playerSkillMananger = GetComponent<PlayerSkillMananger>();
            animationController = GetComponent<PlayerAnimationController>();
            toolController = GetComponent<ToolController>();
            interactionHandler = GetComponentInChildren<InteractionHandler>();
            interactionSelector = GetComponentInChildren<InteractionSelector>();
            playerAfterEffect = GetComponentInChildren<PlayerAfterEffect>();
            rigid2D = GetComponent<Rigidbody2D>();
        }

        protected override void Start()
        {
            base.Start();
            PlayerTable playerTable = TableManager.Instance.GetTable<PlayerTable>();
            PlayerSO playerData = playerTable.GetDataByID(0);

            PlayerStatusManager.Init(playerData);
            StatManager.Init(playerData);

            var action = inputController.PlayerActions;
            
            //Look
            action.Look.performed+=OnLook;
            // Move
            action.Move.performed += OnMove;
            action.Move.canceled += CancelMove ;
            // Attack
            action.Attack0.performed += Attack0;
            action.Attack1.performed += Attack1;
            // Dash
            action.Dash.performed += OnDash;
            // Interaction
            action.Interaction.performed += OnInteraction;
            // Gathering
            action.Gathering.performed += OnGathering;
            // Inventory
            action.Inventory.performed += OnInventory;
            // Crafting
            action.Crafting.performed += OnCrafting;
            // Place
            action.Place.performed += OnPlace;
            // Use
            action.Use.performed += OnUse;
            //Setting
            action.Settings.performed += OnSetting;
        }

        private void LateUpdate()
        {
            UpdateAttackPivotRotation();
            ScanAndAttractItems();
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

            if (Input.GetKey(KeyCode.T))
            {
                TestDeath();
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
                    return new Attack1State(1);
                case PlayerState.Dead:
                    return new DeadState();
                case PlayerState.Gathering:
                    return new GatherState();
                default:
                    return null;
            }
        }

        public override void FindTarget(){ }

        //------------------------------------------------------------

        private void OnLook(InputAction.CallbackContext _context)
        {
            inputController.LookDirection = _context.ReadValue<Vector2>();
        }
        private void OnMove(InputAction.CallbackContext _context)
        {
            moveInput = _context.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0.01f)
                lastMoveDir = moveInput.normalized;
        }
        private void CancelMove(InputAction.CallbackContext _context)
        {
            moveInput = rigid2D.velocity = Vector2.zero;
        }

        private void Attack0(InputAction.CallbackContext _context)
        {
            if (EventSystem.current.IsPointerOverGameObject() || placeMode.CanPlace ||
                PlayerStatusManager.CurrentSlimeGauge < 5)
                return;
            if (HoldManager.Instance.IsHolding)
            {
                HoldManager.Instance.DropHeldItem();
                return;
            }
            if (CanAttack)
                attackQueued = true;
        }
        private void Attack1(InputAction.CallbackContext _context)
        {
            if (EventSystem.current.IsPointerOverGameObject() || placeMode.CanPlace ||
                PlayerStatusManager.CurrentSlimeGauge < 5) 
                return;
            
            if (HoldManager.Instance.IsHolding)
            {
                HoldManager.Instance.ReturnToOrigin();
                return;
            }
            
            if (CanAttack)
                attackQueued = true;
        }

        private void OnDash(InputAction.CallbackContext _context)
        {
            if (PlayerStatusManager.CurrentStamina <50)
            {
                return;
            }
            dashTrigger = true;
        }
        
        private void OnInteraction(InputAction.CallbackContext _context)
        {
            Interaction();
        }
        
        private void OnGathering(InputAction.CallbackContext _context)
        {
            Gathering();
        }

        private void OnInventory(InputAction.CallbackContext _context)
        {
            UIManager.Instance.Toggle<UIInventory>();
        }
        
        private void OnCrafting(InputAction.CallbackContext _context)
        {
            UIManager.Instance.Toggle<UICrafting>();
        }

        private void OnSetting(InputAction.CallbackContext _context)
        {
            if (!UIManager.Instance.CloseTop())
            {
                UIManager.Instance.Toggle<UIPauseMenu>();
            }
        }
        
        private void OnUse(InputAction.CallbackContext _context)
        {
            var quickSlot = uiQuickSlot.GetSelectedSlot();
            if (quickSlot == null) return;

            var data = quickSlot.GetData();
            if (data == null || !data.IsValid) return;

            var uiInventory = UIManager.Instance.GetUIComponent<UIInventory>();
            if (uiInventory == null) return;

            var realSlot = uiInventory.GetInventorySlotByIndex(uiQuickSlot.SelectedIndex);
            if (realSlot == null) return;
            
            InventoryInteractionHandler.Instance.TryUse(data, realSlot);
        }

        private void OnPlace(InputAction.CallbackContext _context)
        {
            placeMode.Place();
        }
        
        //--------------------------------------------------------------
        
        public void Attack()
        {
            attackQueued = false;
        }
        
        private float attackCooldown = 0f;

        public void SetAttackCoolDown(float _coolDown)
        {
            attackCooldown = _coolDown;
        }

        /// <summary>
        /// 일반공격버튼이 눌리고있는지 확인하는 bool메서드
        /// </summary>
        /// <returns></returns>
        public bool IsMouse01Pressed()
        {
            return inputController.PlayerActions.Attack0.IsPressed();
        }
        /// <summary>
        /// 특수공격버튼이 눌리고있는지 확인하는 bool메서드
        /// </summary>
        /// <returns></returns>
        public bool IsMouse02Pressed()
        {
            return inputController.PlayerActions.Attack1.IsPressed();
        }
        
        public void ResetAttackTrigger() => attackQueued = false;

        public override void Movement()
        {
            if (!CanMove || moveInput.sqrMagnitude < 0.01f)
            {
                rigid2D.velocity = Vector2.zero;
                return;
            }
            base.Movement();
            
            float speed = PlayerStatusManager.MoveSpeed;

            Vector2 moveVelocity = Vector2.zero;

            moveVelocity = moveInput.normalized * speed;

            rigid2D.velocity = moveVelocity;
        }
        
        public void SetCanMove(bool _canMove) => CanMove = _canMove;
        
        // NPC, 창고, 제작대 등 이용
        public void Interaction()
        {
            var target = interactionSelector.FInteractable;

            if (target == null)
            {
                Logger.Log("Target is null");
                return;
            }
            
            interactionHandler.HandleInteraction(target, InteractionCommandType.F, this);
        }
        
        // 스페이스바 눌렀을 때 오브젝트 피깎기.
        
        private bool CanGathering()
        {
            if (uiQuickSlot.IsUnityNull())
            {
                return false;
            }
            QuickSlot selectedSlot = uiQuickSlot.GetSelectedSlot();
            if (selectedSlot.IsUnityNull())
            {
                return false;
            }
            
            if (interactionSelector.IsUnityNull())
            {
                return false;
            }
            
            var target = interactionSelector.SpaceInteractable;
            
            if (target.IsUnityNull())
            {
                return false;
            }
            
            if (!target.TryGetComponent(out BaseInteractableObject resource) || resource == null)
            {
                return false;
            }
            if(resource.IsInteracted)
                return false;
            
            ToolType toolType = selectedSlot.GetToolType();
            ToolType requiredToolType = resource.GetRequiredToolType();
            
            if (requiredToolType == ToolType.None)
            {
                // 맨손 상호작용을 허용 (ex: Berry)
                return true;
            }
            return toolType == requiredToolType;
        }

        private void Gathering()
        {
            if (actCoolDown > 0) return;
            if(!CanGathering())
            {
                Logger.Log("Not Selected Tool");
                return;
            }
            
            var target = interactionSelector.SpaceInteractable;

            if (target == null)
            {
                Logger.Log("Target is null");
                return;
            }
            
            interactionHandler.HandleInteraction(target, InteractionCommandType.Space, this);

            float toolActSpd = toolController.GetAttackSpd();
            actCoolDown = 1f / Mathf.Max(toolActSpd, 0.01f);
            
            SetCanMove(false);
            moveInput = Vector2.zero;

            ChangeState(PlayerState.Gathering);
        }
        private void ScanAndAttractItems()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.5f, LayerMask.GetMask("DropItem"));

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<ItemDrop>(out var itemDrop))
                {
                    itemDrop.AttractSetPlayer(transform);
                }
            }
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
                if (PlayerStatusManager == null)
                {
                    Logger.Log("EnemyStatus is null");
                }

                if (_attacker.AttackStat == null)
                {
                    Logger.Log("AttackStat is null");
                }
                PlayerStatusManager.TakeDamage(_attacker.AttackStat.GetCurrent());
                animationController.TakeDamageAnim(new Color(1f,0,0,0.7f));
                damageDelayTimer = damageDelay;
                
                float damage = _attacker.AttackStat.GetCurrent();
                var textObj = Instantiate(damageTextPrefab, damageTextCanvas.transform);
                var damageText = textObj.GetComponent<DamageTextUI>();
                damageText.Init(transform.position, damage, Color.red);
                
                if (PlayerStatusManager.CurrentHp <= 0)
                {
                    Dead();
                }
            }
        }

        public void Dead()
        {
            if (PlayerStatusManager.CurrentHp <= 0)
            {
                IsDead = true;
                animationController.TriggerDead();
                StartCoroutine(DelayDeathUI(3f, "아기 거북이"));
                ChangeState(PlayerState.Dead);
            }
        }

        public void TestDeath()
        {
            PlayerStatusManager.ConsumeHp(50f);
            IsDead = true;
            animationController.TriggerDead();
            StartCoroutine(DelayDeathUI(3f, "행복사"));
            ChangeState(PlayerState.Dead);
        }
        
        // 죽음 연출 후 UI 딜레이 호출
        private IEnumerator DelayDeathUI(float _delay, string _reason)
        {
            yield return new WaitForSeconds(_delay);
            uiDead.TriggerDeath(1, _reason);
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