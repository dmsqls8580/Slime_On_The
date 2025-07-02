using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerAnimationDataSO animationDataSo;
    public PlayerAnimationDataSO AnimationDataSo => animationDataSo;

    public Animator Animator { get; private set; }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
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

    public void SetLook(Vector2 _lookDir)
    {
        Animator.SetFloat(AnimationDataSo.MouseXParameterHash,Mathf.Abs(_lookDir.x));
        Animator.SetFloat(AnimationDataSo.MouseYParameterHash,_lookDir.y);
    }
}