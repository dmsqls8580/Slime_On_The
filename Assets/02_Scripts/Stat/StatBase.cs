using System;
using UnityEngine;


public abstract class StatBase
{
    public StatType Type { get; private set; }
    public abstract float Value { get; }
    public Action<float> OnValueChanged;

    public StatBase(StatType _type)
    {
        Type = _type;
    }

    public abstract float GetCurrent();
}

public class CalculateStat : StatBase
{
    public float BaseValue { get; private set; }
    public float BuffFlat { get; private set; }
    public float BuffPercent { get; private set; }
    public float EquipValue { get; private set; }

    public float FinalValue => Mathf.Max((BaseValue + BuffFlat + EquipValue) * (1 + BuffPercent), 0);

    public override float Value => FinalValue;

    public CalculateStat(StatType _type, float _baseValue) : base(_type)
    {
        BaseValue = _baseValue;
    }

    public void ModifyBaseValue(float _value)
    {
        BaseValue += _value;
        OnValueChanged?.Invoke(FinalValue);
    }

    public void ModifyBuffFlat(float _value)
    {
        BuffFlat += _value;
        OnValueChanged?.Invoke(FinalValue);
    }

    public void ModifyBuffPercent(float _value)
    {
        BuffPercent += _value;
        OnValueChanged?.Invoke(FinalValue);
    }

    public void ModifyEquipValue(float _value)
    {
        EquipValue += _value;
        OnValueChanged?.Invoke(FinalValue);
    }

    public override float GetCurrent() => FinalValue;
}

public class ResourceStat : StatBase
{
    public float CurrentValue { get; set; }
    public float MaxValue { get; private set; }
    
    public override float Value => CurrentValue;
    
    public ResourceStat(StatType _type, float _maxValue) : base(_type)
    {
        CurrentValue = _maxValue;
        MaxValue = _maxValue;
    }

    public void Recover(float _value)
    {
        CurrentValue =Mathf.Min(CurrentValue + _value, MaxValue);
        OnValueChanged?.Invoke(CurrentValue);
    }

    public void Consume(float _value)
    {
        CurrentValue = Mathf.Max(CurrentValue - _value, 0);
        OnValueChanged?.Invoke(CurrentValue);
    }

    public void RecoverPercent(float _percent)
    {
        float amount = MaxValue * _percent;
        CurrentValue = Mathf.Min(CurrentValue + amount, MaxValue);
        OnValueChanged?.Invoke(CurrentValue);
    }

    public void ConsumePercent(float _percent)
    {
        float amount = MaxValue * _percent;
        CurrentValue = Mathf.Min(CurrentValue - amount, MaxValue);
        OnValueChanged?.Invoke(CurrentValue);
    }

    public void SetCurrentValue(float _value)
    {
        CurrentValue = Mathf.Clamp(_value, 0, MaxValue);
        OnValueChanged?.Invoke(CurrentValue);
    }

    public void SetMaxValue(float _max)
    {
        MaxValue = _max;
        CurrentValue = Mathf.Min(CurrentValue, MaxValue);
        OnValueChanged?.Invoke(CurrentValue);
    }
    
    public override float GetCurrent()=>CurrentValue;
}