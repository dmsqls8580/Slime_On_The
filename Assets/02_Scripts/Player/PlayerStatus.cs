using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] private Image SlimeGaugeImage;
    private StatManager statManager;
    public UnityAction<float> OnHpChanged;


    public PlayerStatus(StatManager _statManager)
    {
        _statManager = statManager;
    }

    private void Awake()
    {
        statManager = GetComponent<StatManager>();
        statManager.OnStatChange += UpdateSlimeGaugeUI;
    }


    public float CurrentHp => statManager.GetStat<ResourceStat>(StatType.CurrentHp).CurrentValue;
    public float MaxHp => statManager.GetStat<ResourceStat>(StatType.CurrentHp).MaxValue;
    
    
    public float FinalAttackDamage => statManager.GetValue(StatType.FinalAtk);

    public float CurrentSlimeGauge => statManager.GetStat<ResourceStat>(StatType.CurrentSlimeGauge).CurrentValue;
    public float MaxSlimeGauge => statManager.GetStat<CalculateStat>(StatType.MaxSlimeGauge).FinalValue;

    public float MoveSpeed => statManager.GetValue(StatType.MoveSpeed);

    public void Init(IStatProvider _statProvider, IDamageable _owner = null)
    {
        statManager.Init(_statProvider, _owner);

        float maxSlime = statManager.GetValue(StatType.MaxSlimeGauge);
        statManager.ApplyStat(StatType.MaxSlimeGauge,StatModifierType.Base, maxSlime);

        UpdateSlimeGaugeUI();
    }

    public void ConsumeSlimeGauge(float _amount)
    {
        if (SlimeGaugeImage == null)
        {
            return;
        }

        statManager.Consume(StatType.CurrentSlimeGauge, StatModifierType.Base, _amount);
        UpdateSlimeGaugeUI();
    }

    public void RecoverSlimeGauge(float _amount)
    {
        statManager.Recover(StatType.CurrentSlimeGauge, StatModifierType.Base, _amount);
        
        UpdateSlimeGaugeUI();
    }
    
    public void TakeDamage(float _damage, StatModifierType _modifierType)
    {
        statManager.Consume(StatType.CurrentHp, _modifierType, _damage);
        Debug.Log($"대미지 입음! 현제체력: {statManager.GetStat<ResourceStat>(StatType.CurrentHp).CurrentValue}");
        NotifyHpChanged();
    }
    
    private void UpdateSlimeGaugeUI()
    {
        if (SlimeGaugeImage == null)
        {
            return;
        }

        float cur = CurrentSlimeGauge;
        float max = MaxSlimeGauge;
        SlimeGaugeImage.fillAmount = max > 0f ? cur / max : 0f;
    }

    public void ConsumeHp(float _amount)
    {
        statManager.Consume(StatType.CurrentHp, StatModifierType.Base, _amount);
        NotifyHpChanged();
    }

    public void RecoverHp(float _amount)
    {
        statManager.Recover(StatType.CurrentHp, StatModifierType.Base, _amount);
        NotifyHpChanged();
    }
    public void ConsumeHunger(float _amount)
    {
        statManager.Consume(StatType.CurrentHunger, StatModifierType.Base, _amount);
    }

    public void RecoverHunger(float _amount)
    {
        statManager.Recover(StatType.CurrentHunger, StatModifierType.Base, _amount);
    }
    
    private void NotifyHpChanged()
    {
        float ratio = MaxHp > 0 ? CurrentHp / MaxHp : 0f;
        OnHpChanged?.Invoke(ratio);
    }
}