using System;
using Unity.VisualScripting;
using UnityEngine;

public enum StatType
{
    MaxHealth,
    CurrentHealth,
    AttackDamage,
    Defense,
    MoveSpeed,
    Critical
}

public class Stat
{
    public StatType Type;
    public float BaseValue { get; private set; }
    public float BuffValue { get; private set; }
    public float EquipmentValue { get; private set; }
    public float PercentValue { get; private set; }
    public float FinalValue => (BaseValue + BuffValue + EquipmentValue)*( 1 + PercentValue);

    public event Action<float> OnStatChanged;

    public bool IsChangeStat = false;

    // 스탯 타입 설정
    public Stat(StatType _type)
    {
        Type = _type;
    }
    
    // 스탯 변경점 초기화
    public void ResetChangeStat()
    {
        BaseValue = 0;
        BuffValue = 0;
        PercentValue = 0;
        IsChangeStat = false;
    }

    // BaseValue를 변경하는 메서드
    public void ChangeBaseValue(float _value, float _min = 0, float _max = float.MaxValue)
    {
        IsChangeStat = true;
        BaseValue = Mathf.Clamp(BaseValue + _value, _min, _max);
        OnStatChanged?.Invoke(FinalValue);
    }

    // BuffValue 변경하는 메서드
    public void ChangeBuffValue(float _value, float _percent = 0)
    {
        IsChangeStat = true;
        BuffValue += _value;
        PercentValue += _percent;
        OnStatChanged?.Invoke(FinalValue);
    }

    // EquipmentValue 변경하는 메서드
    public void ChangeEquipmentValue(float _value, float _percent = 0)
    {
        IsChangeStat = true;
        EquipmentValue += _value;
        PercentValue += _percent;
        OnStatChanged?.Invoke(FinalValue);
    }
    
    // 모든 Value를 변경하는 메서드, 우선순위 : Buff, Equip, Base
    public void ChangeAllValue(float _value, float _percent = 0)
    {
        float remainingvalue = _value;
        
        if (BuffValue > 0)
        {
            float valueToBuff = Mathf.Min(remainingvalue, BuffValue);
            ChangeBuffValue(valueToBuff, _percent);
            remainingvalue -= valueToBuff;
        }

        if (EquipmentValue > 0)
        {
            float valueToEquip = Mathf.Min(remainingvalue, EquipmentValue);
            ChangeEquipmentValue(valueToEquip, _percent);
            remainingvalue -= valueToEquip;
        }

        if (remainingvalue > 0)
        {
            ChangeBaseValue(remainingvalue,0,FinalValue);
        }
    }
    
    

}
