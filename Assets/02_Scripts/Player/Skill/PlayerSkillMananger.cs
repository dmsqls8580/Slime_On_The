using UnityEngine;

[System.Serializable]
public class PlayerSkillMananger : MonoBehaviour
{
    [Header("각 공격, 스킬 데이터")]
    public PlayerSkillSO normalAttack;
    public PlayerSkillSO specialAttack;

    public PlayerSkillSO GetSkill(bool _isSpecial)
    {
        return _isSpecial ? specialAttack : normalAttack;
    }

    public void SetSkill(PlayerSkillSO _newSkill, bool _isSpecial)
    {
        if (_isSpecial) 
            specialAttack = _newSkill;
        else
            normalAttack = _newSkill;
    }
}
