using System.Collections;
using UnityEngine;

public class RainEffect : WeatherEffectBase
{
    protected override int MaxLevel => 2;

    private readonly WeatherManager weatherManager;
    private readonly ParticleSystem particle;
    private readonly PlayerStatusManager playerStatusManager;

    private Coroutine coroutine;

    // 비가 내리거나 시작할때 까지 걸리는 시간.
    private readonly float transitionDuration = 3f;
    // 날씨로 인한 이동속도 변화.
    private readonly float moveSpeed = 2f;

    private readonly float effectInterval = 1.5f;
    private float effectTimer = 0f;

    public RainEffect(WeatherManager _weatherManager, ParticleSystem _particle, PlayerStatusManager _playerStatusManager)
    {
        weatherManager = _weatherManager;
        particle = _particle;
        playerStatusManager = _playerStatusManager;
    }

    protected override void ApplyEffect()
    {
        if (particle == null) return;

        switch (++currentLevel)
        {
            case 1:
                break;
            case 2:
                playerStatusManager.UpdateMoveSpeed = -moveSpeed;
                break;
        }

        if (coroutine != null)
        {
            weatherManager.StopCoroutine(coroutine);
        }
        Logger.Log($"날씨: 비 {currentLevel}단계.");
        coroutine = weatherManager.StartCoroutine(Fade(true));
    }

    protected override void UpdateEffect()
    {
        effectTimer += Time.deltaTime;

        if (effectTimer >= effectInterval)
        {
            effectTimer = 0f;

            switch (currentLevel)
            {
                case 1:
                    playerStatusManager.RecoverSlimeGauge(1f);
                    break;
                case 2:
                    playerStatusManager.RecoverSlimeGauge(4f);
                    break;
            }
        }
    }

    protected override void RemoveEffect()
    {
        switch (currentLevel)
        {
            case 2:
                playerStatusManager.UpdateMoveSpeed = moveSpeed;
                goto case 1;
            case 1:
                break;
        }

        if (coroutine != null)
        {
            weatherManager.StopCoroutine(coroutine);
        }
        Logger.Log("날씨: 비 꺼짐.");
        coroutine = weatherManager.StartCoroutine(Fade(false));
    }

    private IEnumerator Fade(bool fadeIn)
    {
        var emission = particle.emission;
        float currentRate = emission.rateOverTime.constant;
        float targetRate = 0f;

        if (fadeIn)
        {
            switch (currentLevel)
            {
                case 1:
                    targetRate = 100f;
                    break;
                case 2:
                    targetRate = 400f;
                    break;
            }
        }

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            // Lerp를 사용하여 현재 값에서 목표 값으로 부드럽게 변경.
            emission.rateOverTime = Mathf.Lerp(currentRate, targetRate, timer / transitionDuration);
            yield return null;
        }

        // 루프가 끝나면 목표 값으로 확실하게 설정.
        emission.rateOverTime = targetRate;
    }
}
