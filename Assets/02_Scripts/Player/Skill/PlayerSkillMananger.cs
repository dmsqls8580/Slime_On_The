using PlayerStates;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerSkillMananger : MonoBehaviour
{
    private Dictionary<int, float> _skillCooldowns = new();

    public PlayerSkillSO attack0Slot { get; private set; }
    public PlayerSkillSO attack1Slot { get; private set; }
    
    [Header("디버깅 확인용")]
    [SerializeField]private PlayerSkillSO currentSkill0;
    [SerializeField]private PlayerSkillSO currentSkill1;

    private float skillDamage;
    
    // 인덱스로 SO 반환
    public void SetSkillSlot(PlayerSkillSO _attack0, PlayerSkillSO _attack1)
    {
        attack0Slot = _attack0;
        attack1Slot = _attack1;

        currentSkill0 = attack0Slot;
        currentSkill1 = attack1Slot;
    }

    public PlayerSkillSO GetSkillSlot(int _slot)
    {
        return _slot switch
        {
            0 => attack0Slot,
            1 => attack1Slot,
            _ => null
        };
    }

// 스킬 사용 요청
    public void UseSkill(int _index, PlayerController _owner)
    {
        var _skill = GetSkillSlot(_index);
        if (_skill == null) return;
        if (!CanUseSkill(_index)) return;
        
        skillDamage = _skill.damage;
        _skill.Execute(_owner,skillDamage);
        _owner.PlayerStatusManager.ConsumeSlimeGauge(_skill.useSlimeGauge);
        Logger.Log($"남은 슬라임 게이지: {_owner.PlayerStatusManager.CurrentSlimeGauge}");
        
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