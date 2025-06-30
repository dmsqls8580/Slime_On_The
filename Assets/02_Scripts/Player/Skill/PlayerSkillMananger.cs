using UnityEngine;

[System.Serializable]
public class PlayerSkillMananger : Singleton<PlayerSkillMananger>
{
    [Header("각 공격, 스킬 데이터")]
    public PlayerSkillSO normalAttack;
    public PlayerSkillSO specialAttack;

    public PlayerSkillSO GetSkill(bool isSpecial)
    {
        return isSpecial ? specialAttack : normalAttack;
    }

    public void SetSkill(PlayerSkillSO newSkill, bool isSpecial)
    {
        if (isSpecial) 
            specialAttack = newSkill;
        else
            normalAttack = newSkill;
    }
}
