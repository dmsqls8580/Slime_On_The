using System.Collections;
using UnityEngine;

public class RainEffect : WeatherEffectBase
{
    protected override int MaxLevel => 2;

    private readonly WeatherManager weatherManager;
    private readonly ParticleSystem particle;
    private Coroutine coroutine;

    // 비가 내리거나 시작할때 까지 걸리는 시간.
    private readonly float transitionDuration = 3f;

    public RainEffect(WeatherManager _weatherManager, ParticleSystem _particle)
    {
        weatherManager = _weatherManager;
        particle = _particle;
    }

    protected override void ApplyEffect()
    {
        if (particle == null) return;

        switch (++currentLevel)
        {
            case 1:
                // TODO: 슬라임 게이지 회복력 증가.
                break;
            case 2:
                // TODO: 플레이어 이동속도 감소.
                break;
        }

        if (coroutine != null)
        {
            weatherManager.StopCoroutine(coroutine);
        }
        Logger.Log($"날씨: 비 {currentLevel}단계.");
        coroutine = weatherManager.StartCoroutine(Fade(true));
    }

    protected override void UpdateEffect() { }

    protected override void RemoveEffect()
    {
        switch (currentLevel)
        {
            case 2:
                // TODO: 플레이어 이동속도 증가(되돌리기).
                goto case 1;
            case 1:
                // TODO: 슬라임 게이지 회복력 감소(되돌리기).
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
