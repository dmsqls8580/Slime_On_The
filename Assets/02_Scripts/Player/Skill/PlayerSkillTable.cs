


using UnityEngine;

[System.Serializable]
public class PlayerSkillTable
{
    [Header("각 공격, 스킬 데이터")]
    public PlayerSkillSO normalAttack;
    public PlayerSkillSO specialAttack;

    public PlayerSkillSO GetSkill(bool isSpecial)
    {
        return isSpecial ? specialAttack : normalAttack;
    }
}
