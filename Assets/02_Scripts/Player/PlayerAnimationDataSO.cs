using UnityEngine;


[CreateAssetMenu(fileName = "PlayerAnimation", menuName = "PlayerAnimationData")]
public class PlayerAnimationDataSO : ScriptableObject
{
    public string isMoveParameter = "IsMove";
    public string attackTriggerParameter = "Attack";
    public string dashTriggerParameter = "Dash";
    
    private int mouseX = Animator.StringToHash("mouseX");
    private int mouseY = Animator.StringToHash("mouseY");

    public int MouseXParameterHash => mouseX;
    public int MouseYParameterHash => mouseY;
    
    public int IsMoveHash=> Animator.StringToHash(isMoveParameter);
    public int IsAttackTriggerHash => Animator.StringToHash(attackTriggerParameter);
    public int DashTriggerHash => Animator.StringToHash(dashTriggerParameter);

}
