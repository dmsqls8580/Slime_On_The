using UnityEngine;

public enum PlayerSkillType
{
    Melee,
    Ranged,
}

[CreateAssetMenu(menuName = "PlayerSkill/SkillData")]
public class PlayerSkillSO : ScriptableObject
{
    public string skillName;
    public float cooldown;
    public float actionDuration;
    public float speed;
    public float range;
    
    public PlayerSkillType skillType;
     
}
