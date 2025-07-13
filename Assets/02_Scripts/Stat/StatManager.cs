using System;
using System.Collections.Generic;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public Dictionary<StatType, StatBase> Stats { get;private set; }= new Dictionary<StatType, StatBase>();

    public event Action OnStatChange;
    public IDamageable Owner { get; private set; }

    public void Init(IStatProvider _statProvider, IDamageable _owner = null)
    {
        Owner = _owner;
        foreach (var stat in _statProvider.Stats)
        {
            Stats[stat.StatType] = BaseStatFactory(stat.StatType, stat.Value);
        }
        
        if (Stats.TryGetValue(StatType.MaxSlimeGauge, out var maxSlimeStat) && maxSlimeStat is CalculateStat calc)
        {
            if (Stats.TryGetValue(StatType.CurrentSlimeGauge, out var curSlimeStat) && curSlimeStat is ResourceStat cur)
            {
                cur.SetMaxValue(calc.FinalValue);
            }
        }
        OnStatChange?.Invoke();
    }
    
    /// <summary>
    /// Stat을 생성해주는 팩토리
    /// </summary>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private StatBase BaseStatFactory(StatType _statType, float _value)
    {
        return _statType switch
        {
            StatType.CurrentHp => new ResourceStat(_statType, _value),
            StatType.CurrentHunger => new ResourceStat(_statType, _value),
            StatType.CurrentSlimeGauge=> new ResourceStat(_statType, _value),
            StatType.Defense=> new ResourceStat(_statType, _value),
            //////////////////////////////////////////////////////////////////
            _ => new CalculateStat(_statType, _value),
        };
    }

    public T GetStat<T>(StatType _statType) where T : StatBase
    {
        return Stats[_statType] as T;
    }
    
    public float GetValue(StatType _statType)
    {
        return Stats[_statType].GetCurrent();
    }
    
    public bool TryGetValue(StatType type, out float value)
    {
        if (Stats.TryGetValue(type, out var stat))
        {
            value = stat.Value;
            return true;
        }
        value = 0f;
        return false;
    }
    
    public void Recover(StatType _statType, StatModifierType _modifierType, float _value)
    {
        if (Stats[_statType] is ResourceStat res)
        {
            if (res.CurrentValue < res.MaxValue)
            {
                switch (_modifierType)
                {
                    case StatModifierType.Base:
                        res.Recover(_value);
                        break;
                    case StatModifierType.BasePercent:
                        res.RecoverPercent(_value);
                        break;
                }
            }
        }
    }

    public void Consume(StatType _statType, StatModifierType _modifierType, float _value)
    {
        if (Stats[_statType] is ResourceStat res)
        {
            if (res.CurrentValue > 0)
            {
                switch (_modifierType)
                {
                    case StatModifierType.Base:
                        res.Consume(_value);
                        break;
                    case StatModifierType.BasePercent:
                        res.ConsumePercent(_value);
                        break;
                }
                
                //todo: Dead 판정은 TakeDamage를 주는 부분의 코드에 작성 
                // if (_statType == StatType.CurrentHp && res.CurrentValue <= 0)
                // {
                //     Owner?.Dead();
                // }
            }
        }
    }
    
    /// <summary>
    /// 증가되는 스탯에 따라 해당 스탯을 증감시켜주는 메서드
    /// </summary>
    /// <param name="_statType"></param>
    /// <param name="_modifierType"></param>
    /// <param name="_value"></param>
    public void ApplyStat(StatType _statType, StatModifierType _modifierType, float _value)
    {
        if (Stats[_statType] is not CalculateStat stat) return;

        switch (_modifierType)
        {
            case StatModifierType.Base:
                stat.ModifyBaseValue(_value);
                break;
            case StatModifierType.Equipment:
                stat.ModifyEquipValue(_value);
                break;
        }

        switch (_statType)
        {
            case StatType.MaxHp:
                SyncCurrentWithMax(StatType.CurrentHp,stat);
                break;
            case StatType.MaxHunger:
                SyncCurrentWithMax(StatType.CurrentHunger, stat);
                break;
            case StatType.MaxSlimeGauge :
                SyncCurrentWithMax(StatType.CurrentSlimeGauge, stat);
                break;
        }
        OnStatChange?.Invoke();
        
        Logger.Log($"Stat : {_statType} Modify Value {_value}, FinalValue : {stat.Value}");
    }
    

    private void SyncCurrentWithMax(StatType _statType, CalculateStat _stat)
    {
        if (Stats.TryGetValue(_statType, out var res) && res is ResourceStat resCurStat)
        {
            resCurStat.SetMaxValue(_stat.FinalValue);
        }
    }
    
}