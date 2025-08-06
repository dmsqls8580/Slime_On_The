using PlayerStates;
using UnityEngine;
using UnityEngine.Serialization;

public enum SkillActiveType
{
    Instant,
    Press,
}

public abstract class PlayerSkillSO : ScriptableObject
{
    public int skillIndex;
    public string skillName;
    public SkillActiveType skillActiveType;
    private string scriptName;
    public float damage;
    public float speed;
    public float range;
    public float cooldown;
    public float actionDuration;
    public float useSlimeGauge;

    // 스킬 실제 동작 추상 메서드
    public abstract void Execute(PlayerController _owner);
}