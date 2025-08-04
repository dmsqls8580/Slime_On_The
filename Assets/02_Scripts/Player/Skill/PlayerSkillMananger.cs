using PlayerStates;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerSkillMananger : MonoBehaviour
{
    private Dictionary<int, float> skillCooldowns = new();

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
    
    private int GetSkillSlotIndex(PlayerSkillSO _skill)
    {
        if (attack0Slot == _skill)
            return 0;
        if (attack1Slot == _skill)
            return 1;
        return -1; // 없는 경우
    }

    
// 스킬 사용 요청
    public void UseSkill(int _index, PlayerController _owner)
    {
        var skill = GetSkillSlot(_index);
        if (skill == null) return;
        if (!CanUseSkill(_index)) return;
        
        skillDamage = skill.damage;
        if(skill.skillActiveType== SkillActiveType.Instant)
        {
            skill.Execute(_owner, skillDamage);
            _owner.PlayerStatusManager.ConsumeSlimeGauge(skill.useSlimeGauge);
            skillCooldowns[_index] = skill.cooldown;
        }
        else if(skill.skillActiveType == SkillActiveType.Press)
        {
            _owner.StartCoroutine(UsePressSkillRoutine(skill, _owner));
        }
    }

    public bool CanUseSkill(int _index)
    {
        return !skillCooldowns.ContainsKey(_index) || skillCooldowns[_index] <= 0;
    }

    private IEnumerator UsePressSkillRoutine(PlayerSkillSO _skill, PlayerController _owner)
    {
        float consumeInterval = 0.1f; // 예: 0.1초마다 체크
        float slimePerTick = _skill.useSlimeGauge * consumeInterval; // 초당 소비량 × 간격

        // 누르고 있는 동안 + 게이지가 남아있는 동안
        while (_owner.IsMouse02Pressed() && _owner.PlayerStatusManager.CurrentSlimeGauge >= slimePerTick)
        {
            _skill.Execute(_owner, _skill.damage); // 스킬 효과 반복(분사 등)
            _owner.PlayerStatusManager.ConsumeSlimeGauge(slimePerTick);
            yield return new WaitForSeconds(consumeInterval);
        }
        // 종료시 쿨타임
        int slotIdx = GetSkillSlotIndex(_skill);
        skillCooldowns[slotIdx] = _skill.cooldown;
    }
    
    private void Update()
    {
        var _keys = skillCooldowns.Keys.ToList();
        foreach (var _idx in _keys)
        {
            skillCooldowns[_idx] -= Time.deltaTime;
        }
    }
}