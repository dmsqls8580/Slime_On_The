using UnityEngine;


[CreateAssetMenu(fileName = "PlayerAnimation", menuName = "PlayerAnimationData")]
public class PlayerAnimationDataSO : ScriptableObject
{
    public string isMoveParameter = "IsMove";
    public string attackTriggerParameter = "Attack";
    public string dashTriggerParameter = "Dash";
    public string gatherTriggerParameter = "Gather";
    
    private int mouseX = Animator.StringToHash("mouseX");
    private int mouseY = Animator.StringToHash("mouseY");
    private int toolTypeIndex = Animator.StringToHash("toolTypeIndex");
    private int isDead= Animator.StringToHash("Dead");

    public int MouseXParameterHash => mouseX;
    public int MouseYParameterHash => mouseY;
    
    public int ToolTypeParameterHash => toolTypeIndex;
    public int IsDeadParameterHash => isDead;
    public int IsMoveHash=> Animator.StringToHash(isMoveParameter);
    public int IsAttackTriggerHash => Animator.StringToHash(attackTriggerParameter);
    public int DashTriggerHash => Animator.StringToHash(dashTriggerParameter);
    public int GatherTriggerHash => Animator.StringToHash(gatherTriggerParameter);

}
