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

    // 이펙트를 넣을 시 public GameObject effectPrefab;
    public PlayerSkillType skillType;
     
}
