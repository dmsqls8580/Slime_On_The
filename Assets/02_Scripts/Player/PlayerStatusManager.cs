using PlayerStates;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerStatusManager : MonoBehaviour
{
    [SerializeField] private Image hpGaugeImage;
    [SerializeField] private Image slimeGaugeImage;
    [SerializeField] private Image craftSlimeGaugeImage;
    [SerializeField] private Image staminaGaugeImage;
    [SerializeField] private Image hungerGaugeImage;

    private StatManager statManager;
    private PlayerController playerController;
    private ISlimeTextOut ISlimeTextOut;
    private Coroutine daySlimeRoutine;
    private Coroutine staminaRecoverRoutine;
    private float slimeConsumeAmount = 0.05f;
    public float SlimeConsumeAmount => slimeConsumeAmount;

    public UnityAction<float> OnHpChanged;

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


    private void Awake()
    {
        statManager = GetComponent<StatManager>();
        ISlimeTextOut = GetComponent<SlimeTextController>();
        statManager.OnStatChange += UpdateAllGaugeUI;
    }

    private void Start()
    {
        daySlimeRoutine  = StartCoroutine(DaySlimeGaugeRoutine(slimeConsumeAmount));
        playerController =  GetComponent<PlayerController>();
    }

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

    private IEnumerator DaySlimeGaugeRoutine(float _amount)
    {
        _amount = slimeConsumeAmount;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Logger.Log("daySlimeGauge");
            switch (TimeManager.Instance.CurrentTimeOfDay)
            {
                case TimeOfDay.Morning:
                case TimeOfDay.Noon:
                case TimeOfDay.Evening:
                    ConsumeSlimeGauge(_amount);
                    break;
                case TimeOfDay.Night:
                case TimeOfDay.Dawn:
                    RecoverSlimeGauge(_amount);
                    break;
            }
        }
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
            Vector3 textPos = new Vector3(transform.position.x + 1f, transform.position.y + 1.6f, transform.position.z);
            ISlimeTextOut.OnSlimeGaugeChanged(cur, max, textPos);
        }
    }
    //------------

    //체력 스탯
    public void ConsumeHp(float _amount)
    {
        statManager.Consume(StatType.CurrentHp, StatModifierType.Base, _amount);

        NotifyHpChanged();      
        UpdateHpUI();
    }

    public void RecoverHp(float _amount)
    {
        statManager.Recover(StatType.CurrentHp, StatModifierType.Base, _amount);
  
        NotifyHpChanged();
        UpdateHpUI();
    }
    private void UpdateHpUI()
    {
        if (hpGaugeImage.IsUnityNull())
        {
            Logger.Log("hpGaugeImage is nullasa");
            return;
        }

        float cur = CurrentHp;
        float max = MaxHp;
        hpGaugeImage.fillAmount = max > 0f ? cur / max : 0f;
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

        if (!staminaRecoverRoutine.IsUnityNull())
        {
            StopCoroutine(staminaRecoverRoutine);
        }

        staminaRecoverRoutine = StartCoroutine(StartDefaultStaminaRecover());
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

    private IEnumerator StartDefaultStaminaRecover()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            float cur = CurrentStamina;
            float max = MaxStaminaByHunger;

            if (cur < max)
            {
                RecoverStamina(5f);
            }

            if (CurrentStamina >= max)
            {
                staminaRecoverRoutine = null;
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
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

        UpdateHungerGaugeUI();
        UpdateStaminaGaugeUI();
    }
    //----------------------

    public void UpdateAllGaugeUI()
    {
        UpdateSlimeGaugeUI();
        UpdateHungerGaugeUI();
        UpdateStaminaGaugeUI();
        UpdateHpUI();
    }

    private void NotifyHpChanged()
    {
        float ratio = MaxHp > 0 ? CurrentHp / MaxHp : 0f;
        OnHpChanged?.Invoke(ratio);
    }

    public void TakeDamage(float _damage)
    {
        if(CurrentHp > 0)
        {
            ConsumeHp(_damage);
        }
        
        if(CurrentHp <= 0)
        {
            playerController.Dead();
        }
        Debug.Log($"대미지 입음! 현제체력: {statManager.GetStat<ResourceStat>(StatType.CurrentHp).CurrentValue}");
        
    }
}