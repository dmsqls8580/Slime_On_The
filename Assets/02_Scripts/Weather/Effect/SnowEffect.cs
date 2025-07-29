using System.Collections;
using UnityEngine;

public class SnowEffect : WeatherEffectBase
{
    protected override int MaxLevel => 2;

    private readonly WeatherManager weatherManager;
    private readonly ParticleSystem particle;
    private Coroutine coroutine;

    // 눈이 내리거나 멈출때까지 걸리는 시간.
    private readonly float transitionDuration = 3f;

    public SnowEffect(WeatherManager _weatherManager, ParticleSystem _particle)
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
                break;
            case 2:
                // 플레이어 미끄러짐 효과 적용 (예: PlayerController의 friction 값 변경)
                break;
        }

        if (coroutine != null)
        {
            weatherManager.StopCoroutine(coroutine);
        }
        Logger.Log($"날씨: 눈 {currentLevel}단계");
        coroutine = weatherManager.StartCoroutine(Fade(true));
    }

    protected override void UpdateEffect()
    {
        // player.TakeDamage(1 * Time.deltaTime);
    }

    protected override void RemoveEffect()
    {
        switch (currentLevel)
        {
            case 2:
                // 플레이어 미끄러짐 효과 제거.
                goto case 1;
            case 1:
                break;
        }

        if (coroutine != null)
        {
            weatherManager.StopCoroutine(coroutine);
        }
        Logger.Log("날씨: 눈 꺼짐.");
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
                    targetRate = 25f;
                    break;
                case 2:
                    targetRate = 100f;
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
