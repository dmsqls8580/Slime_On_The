using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerAnimationDataSO animationDataSo;
    [SerializeField]private CameraController cameraController;
    public PlayerAnimationDataSO AnimationDataSo => animationDataSo;

    private static readonly int ToolTypeIndex = Animator.StringToHash("toolTypeIndex");
    
    private InputController inputController;
    private SpriteRenderer spriteRenderer;

    private bool isLook = false;
    private Vector2 lastLookDir = Vector2.right;
    private Camera _camera;

    public Animator Animator { get; private set; }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        inputController = GetComponent<InputController>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        Vector2 lookDir;
        if (isLook)
        {
            lookDir = lastLookDir;
        }
        else
        {
            lookDir = UpdatePlayerDirectionByMouse();
        }

        UpdateAnimatorParameters(lookDir);
        UpdateSpriteFlip(lookDir);
    }

    public void FormChangeAnimation()
    {
        
    }

    public void SetMove(bool _isMoving)
    {
        Animator.SetBool(AnimationDataSo.isMoveParameter, _isMoving);
    }

    public void TriggerDead()
    {
        Animator.SetTrigger(AnimationDataSo.IsDeadParameterHash);
    }

    public void TriggerAttack()
    {
        Animator.SetTrigger(AnimationDataSo.IsAttackTriggerHash);
    }

    public void TriggerDash()
    {
        Animator.SetTrigger(AnimationDataSo.DashTriggerHash);
    }

    public void TriggerGather()
    {
        Animator.SetTrigger(AnimationDataSo.GatherTriggerHash);
    }

    public void RegisterToolController(ToolController _toolController)
    {
        _toolController.OnToolTypeChanged += SetToolType;
    }
    
    public void SetToolType(ToolType _toolIndex)
    {
        Animator.SetFloat(ToolTypeIndex, (float)_toolIndex); 
    }

    public void SetLookDir(Vector2 _lookDir)
    {
        isLook = true;
        lastLookDir = _lookDir;
    }

    public void ReleaseLookDir()
    {
        isLook = false;
    }

    private void UpdateSpriteFlip(Vector2 _lookDir)
    {
        if (_lookDir.x != 0)
            spriteRenderer.flipX = _lookDir.x < 0;
    }

    public Vector2 UpdatePlayerDirectionByMouse()
    {
        Vector2 mouseScreenPos = inputController.LookDirection;
        Vector3 mouseWorldPos = _camera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y,
            Mathf.Abs(_camera.transform.position.z)));
        Vector3 playerPos = transform.position;
        return (mouseWorldPos - playerPos).normalized;
    }

    public void TakeDamageAnim(Color _color)
    {
        cameraController.CameraShake(0.2f,0.3f,0.3f);
        StartCoroutine(TakeDamageAnimRoutine(_color));
    }

    private IEnumerator TakeDamageAnimRoutine(Color _damageColor)
    {
        spriteRenderer.color = _damageColor;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = new Color(1, 1, 1,1f);
    }
    
    public void UpdateAnimatorParameters(Vector2 _lookDir)
    {
        Animator.SetFloat(AnimationDataSo.MouseXParameterHash, Mathf.Abs(_lookDir.x));
        Animator.SetFloat(AnimationDataSo.MouseYParameterHash, _lookDir.y);
    }
}