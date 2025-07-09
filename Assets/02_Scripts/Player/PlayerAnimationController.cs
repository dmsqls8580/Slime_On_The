using System;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerAnimationDataSO animationDataSo;

    public PlayerAnimationDataSO AnimationDataSo => animationDataSo;

    private static readonly int MOUSE_X = Animator.StringToHash("mouseX");
    private static readonly int MOUSE_Y = Animator.StringToHash("mouseY");

    private InputController inputController;
    private SpriteRenderer spriteRenderer;

    private bool isLook = false;
    private Vector2 lastLookDir = Vector2.right;

    public Animator Animator { get; private set; }

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

    public void SetMove(bool _isMoving)
    {
        Animator.SetBool(AnimationDataSo.isMoveParameter, _isMoving);
    }

    public void TriggerAttack()
    {
        Animator.SetTrigger(AnimationDataSo.IsAttackTriggerHash);
    }

    public void TriggerDash()
    {
        Animator.SetTrigger(AnimationDataSo.DashTriggerHash);
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

    private void UpdateSpriteFlip(Vector2 lookDir)
    {
        if (lookDir.x != 0)
            spriteRenderer.flipX = lookDir.x < 0;
    }

    public Vector2 UpdatePlayerDirectionByMouse()
    {
        Vector2 mouseScreenPos = inputController.LookDirection;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y,
            Mathf.Abs(Camera.main.transform.position.z)));
        Vector3 playerPos = transform.position;
        return (mouseWorldPos - playerPos).normalized;
    }

    public void UpdateAnimatorParameters(Vector2 _lookDir)
    {
        Animator.SetFloat(MOUSE_X, Mathf.Abs(_lookDir.x));
        Animator.SetFloat(MOUSE_Y, _lookDir.y);
    }
}