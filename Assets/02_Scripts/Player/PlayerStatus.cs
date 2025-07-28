using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] private Image slimeGaugeImage;
    [SerializeField] private Image craftSlimeGaugeImage;
    [SerializeField] private Image staminaGaugeImage;
    [SerializeField] private Image hungerGaugeImage;
    
    private StatManager statManager;
    private ISlimeTextOut ISlimeTextOut;
    
    public UnityAction<float> OnHpChanged;

    private void Awake()
    {
        statManager = GetComponent<StatManager>();
        ISlimeTextOut = GetComponent<SlimeTextController>();
        statManager.OnStatChange += UpdateAllGaugeUI;
    }

    public float CurrentHp => statManager.GetStat<ResourceStat>(StatType.CurrentHp) != null
        ? statManager.GetStat<ResourceStat>(StatType.CurrentHp).CurrentValue
        : 0f;

    public float MaxHp => statManager.GetStat<ResourceStat>(StatType.CurrentHp) != null
        ? statManager.GetStat<ResourceStat>(StatType.CurrentHp).MaxValue
        : 1f;

    public float FinalAttackDamage => statManager.GetValue(StatType.FinalAtk);

    public float CurrentSlimeGauge => statManager.GetStat<ResourceStat>(StatType.CurrentSlimeGauge).CurrentValue;
    public float MaxSlimeGauge => statManager.GetStat<CalculateStat>(StatType.MaxSlimeGauge).FinalValue;

    public float CurrentStamina => statManager.GetStat<ResourceStat>(StatType.CurrentStamina)?.CurrentValue ?? 0f;
    public float CurrentHunger => statManager.GetStat<ResourceStat>(StatType.CurrentHunger)?.CurrentValue ?? 0f;
    public float MaxHunger => statManager.GetStat<CalculateStat>(StatType.MaxHunger)?.FinalValue ?? 1f;

    // 스테미나 최대치는 "배고픔 현재치"로 제한
    public float MaxStaminaByHunger => CurrentHunger;

    public float MoveSpeed => statManager.GetValue(StatType.MoveSpeed);

    public void Init(IStatProvider _statProvider, IDamageable _owner = null)
    {
        statManager.Init(_statProvider, _owner);

        float maxSlime = statManager.GetValue(StatType.MaxSlimeGauge);
        statManager.ApplyStat(StatType.MaxSlimeGauge, StatModifierType.Base, maxSlime);

        UpdateAllGaugeUI();
    }

    //슬라임게이지
    public void ConsumeSlimeGauge(float _amount)
    {
        if (slimeGaugeImage == null)
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
   
    private void UpdateSlimeGaugeUI()
    {
        if (slimeGaugeImage.IsUnityNull())
        {
            return;
        }

        float cur = CurrentSlimeGauge;
        float max = MaxSlimeGauge;
        slimeGaugeImage.fillAmount = max > 0f ? cur / max : 0f;
        craftSlimeGaugeImage.fillAmount = slimeGaugeImage.fillAmount;

        if (!ISlimeTextOut.IsUnityNull())
        {
            Vector3 textPos= new Vector3(transform.position.x + 1f,transform.position.y + 1.6f,transform.position.z);
            ISlimeTextOut.OnSlimeGaugeChanged(cur,max,textPos);

        }
    }
    //------------

    //체력 스탯
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
    
    private void UpdateStaminaGaugeUI()
    {
        if (staminaGaugeImage.IsUnityNull())
        {
            return;
        }

        float cur = CurrentStamina;
        float max = MaxHunger;
        staminaGaugeImage.fillAmount = max > 0f ? cur / max : 0f;
    }

    //---------------------

    //배고픔 스탯
    public void ConsumeHunger(float _amount)
    {
        statManager.Consume(StatType.CurrentHunger, StatModifierType.Base, _amount);
        ClampStaminaByHunger();
        UpdateHungerGaugeUI();
    }

    public void RecoverHunger(float _amount)
    {
        statManager.Recover(StatType.CurrentHunger, StatModifierType.Base, _amount);
        ClampStaminaByHunger();
        UpdateHungerGaugeUI();
    }
    
    private void UpdateHungerGaugeUI()
    {
        if (hungerGaugeImage.IsUnityNull())
        {
            return;
        }

        float cur = CurrentHunger;
        float max = MaxHunger;
        hungerGaugeImage.fillAmount = max > 0f ? cur / max : 0f;
    }

    //----------------------

    //스테미나 스탯
    public void ConsumeStamina(float _amount)
    {
        statManager.Consume(StatType.CurrentStamina, StatModifierType.Base, _amount);
        ClampStaminaByHunger();
        UpdateStaminaGaugeUI();
    }

    public void RecoverStamina(float _amount)
    {
        var stamina = statManager.GetStat<ResourceStat>(StatType.CurrentStamina);
        float max = MaxStaminaByHunger;
        float cur = CurrentStamina;
        float to = Mathf.Min(cur + _amount, max);
        float sum = to - cur;
        if (sum > 0)
        {
            statManager.Recover(StatType.CurrentStamina, StatModifierType.Base, sum);
        }

        if (stamina.GetCurrent() > max)
        {
            stamina.SetCurrentValue(max);
        }

        UpdateStaminaGaugeUI();
    }
    //----------------------

    //배고픔현재치 = 스테미나 최대치
    private void ClampStaminaByHunger()
    {
        var stamina = statManager.GetStat<ResourceStat>(StatType.CurrentStamina);
        float hunger = CurrentHunger;

        if (!stamina.IsUnityNull() && stamina.CurrentValue > hunger)
        {
            stamina.SetCurrentValue(hunger);
        }

        UpdateAllGaugeUI();
    }
    //----------------------

    public void UpdateAllGaugeUI()
    {
        UpdateSlimeGaugeUI();
        UpdateHungerGaugeUI();
        UpdateStaminaGaugeUI();
    }

    private void NotifyHpChanged()
    {
        float ratio = MaxHp > 0 ? CurrentHp / MaxHp : 0f;
        OnHpChanged?.Invoke(ratio);
    }
    
    public void TakeDamage(float _damage, StatModifierType _modifierType)
    {
        statManager.Consume(StatType.CurrentHp, _modifierType, _damage);
        Debug.Log($"대미지 입음! 현제체력: {statManager.GetStat<ResourceStat>(StatType.CurrentHp).CurrentValue}");
        NotifyHpChanged();
    }

}