using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStatus : MonoBehaviour
{
    public Stat MaxHealth = new Stat(StatType.MaxHealth);
    public Stat CurrentHealth = new Stat(StatType.CurrentHealth);
    public Stat AttackDamage = new Stat(StatType.AttackDamage);
    public Stat Defense = new Stat(StatType.Defense);
    public Stat MoveSpeed = new Stat(StatType.MoveSpeed);
    public Stat Critical = new Stat(StatType.Critical);

    public Dictionary<StatType, Stat> Stats = new Dictionary<StatType, Stat>();

    protected virtual void Start()
    {
        Stats.Add(StatType.MaxHealth, MaxHealth);
        Stats.Add(StatType.CurrentHealth, CurrentHealth);
        Stats.Add(StatType.AttackDamage, AttackDamage);
        Stats.Add(StatType.Defense, Defense);
        Stats.Add(StatType.MoveSpeed, MoveSpeed);
        Stats.Add(StatType.Critical, Critical);

    }

    public void ApplyBuff(StatType _type, float _float, float _percent)
    {
        Stat targetStat = GetStat(_type);
        targetStat.ChangeBuffValue(_float,  _percent);
    }

    public void RemoveBuff(StatType _type, float _float, float _percent)
    {
        Stat targetStat = GetStat(_type);
        targetStat.ChangeBuffValue(-_float,  -_percent);
    }

    // 현재 체력 초기화, 변화 메서드
    public virtual void ChangeHealth(float _value)
    {
        CurrentHealth.ChangeBaseValue(_value,0, MaxHealth.FinalValue);
    }
        
    public virtual Stat GetStat(StatType _type)
    {
        switch (_type)
        {
            case StatType.MaxHealth:
                return MaxHealth;
            case StatType.AttackDamage:
                return AttackDamage;
            case StatType.Defense:
                return Defense;
            case StatType.MoveSpeed:
                return MoveSpeed;
            case StatType.Critical:
                return Critical;
            default:
                return null;
            
        }
        
    }
    
}
