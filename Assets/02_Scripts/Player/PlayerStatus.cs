using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine;

public class PlayerStatus: MonoBehaviour
{
    [SerializeField] private Image SlimeGaugeImage;
    private StatManager statManager;
    
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
    
    public float CurrentSlimeGauge=>statManager.GetStat<ResourceStat>(StatType.CurrentSlimeGauge).CurrentValue;
    public float MaxSlimeGauge => statManager.GetStat<ResourceStat>(StatType.CurrentSlimeGauge).MaxValue;
    
    public float MoveSpeed => statManager.GetValue(StatType.MoveSpeed);

    public void Init(IStatProvider _statProvider, IDamageable _owner = null)
    {
        statManager.Init(_statProvider, _owner);
    }

    public void ConsumeSlimeGauge(float _amount)
    {
        statManager.Consume(StatType.CurrentSlimeGauge,StatModifierType.Base, _amount);
        UpdateSlimeGaugeUI();
    }
    
    public void RecoverSlimeGauge(float _amount)
    {
        statManager.Recover(StatType.CurrentSlimeGauge, StatModifierType.Base, _amount);
        UpdateSlimeGaugeUI();
    }    private void UpdateSlimeGaugeUI()
    
    {
        float cur = statManager.GetValue(StatType.CurrentSlimeGauge);
        float max = statManager.GetValue(StatType.MaxSlimeGauge);
        SlimeGaugeImage.fillAmount = max > 0f ? cur / max : 0f;
    }

    public void ConsumeHp(float _amount)
    {
        statManager.Consume(StatType.CurrentHp, StatModifierType.Base, _amount);
    }

    public void RecoverHp(float _amount)
    {
        statManager.Recover(StatType.CurrentHp, StatModifierType.Base, _amount);
    }
    
}
