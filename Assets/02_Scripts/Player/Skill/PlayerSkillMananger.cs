using PlayerStates;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerSkillMananger : MonoBehaviour
{
    [SerializeField] private List<PlayerSkillSO> _skills;
    private Dictionary<int, float> _skillCooldowns = new();

    // 인덱스로 SO 반환
    public PlayerSkillSO GetSkillIndex(int _index)
    {
        return _skills.FirstOrDefault(_skill => _skill.skillIndex == _index);
    }

    // 스킬 사용 요청
    public void UseSkill(int _index, PlayerController _owner)
    {
        var _skill = GetSkillIndex(_index);
        if (_skill == null) return;
        if (!CanUseSkill(_index)) return;

        _skill.Execute(_owner);
        _skillCooldowns[_index] = _skill.cooldown;
    }

    public bool CanUseSkill(int _index)
    {
        return !_skillCooldowns.ContainsKey(_index) || _skillCooldowns[_index] <= 0;
    }

    private void Update()
    {
        var _keys = _skillCooldowns.Keys.ToList();
        foreach (var _idx in _keys)
        {
            _skillCooldowns[_idx] -= Time.deltaTime;
        }
    }
}