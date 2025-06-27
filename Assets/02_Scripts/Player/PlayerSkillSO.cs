using UnityEngine;

public enum PlayerSkillType
{
    Melee,
    Ranged,
    Dash,
}

[CreateAssetMenu(menuName = "PlayerSkill/SkillData")]
public class PlayerSkillSO : ScriptableObject
{
    public string skillName;
    public float cooldown;
    public float power;
    public float speed;
    public float range;

    // 이펙트를 넣을 시 public GameObject effectPrefab;
    public PlayerSkillType skillType;
    
    public float dashDistance;
    public float dashDuration;
}
