using PlayerStates;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class PlayerSkillSO : ScriptableObject
{
    public int skillIndex;
    public string skillName;
    private string scriptName;
    public float damage;
    public float speed;
    public float range;
    public float cooldown;
    public float actionDuration;

    // 스킬 실제 동작 추상 메서드
    public abstract void Execute(PlayerController _owner);
}