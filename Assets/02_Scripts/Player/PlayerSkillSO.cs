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
    public float power;
    public float speed;
    public float range;

    public GameObject projectilePrefab;
    public PlayerSkillType skillType;
     
}
