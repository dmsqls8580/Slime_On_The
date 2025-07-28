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
                // 이전에 실행중인 코루틴이 있다면 중지 (안전장치).
                if (coroutine != null)
                {
                    weatherManager.StopCoroutine(coroutine);
                }
                Logger.Log("날씨: 눈 켜짐.");
                coroutine = weatherManager.StartCoroutine(Fade(true));
                break;
            case 2:
                Logger.Log("연속으로 눈이 내려 땅이 더 미끄러워집니다!");
                // 플레이어 미끄러짐 효과 적용 (예: PlayerController의 friction 값 변경)
                break;
        }
    }

    protected override void UpdateEffect()
    {
        // 플레이어 체력 지속 감소 로직
        // player.TakeDamage(1 * Time.deltaTime);
    }

    protected override void RemoveEffect()
    {
        // 플레이어 미끄러짐 효과 제거
        switch (currentLevel)
        {
            case 2:
                // TODO: 플레이어 이동속도 증가(되돌리기).
                goto case 1;
            case 1:
                if (coroutine != null)
                {
                    weatherManager.StopCoroutine(coroutine);
                }
                Logger.Log("날씨: 눈 꺼짐.");
                coroutine = weatherManager.StartCoroutine(Fade(false));
                break;
        }
    }

    // 비를 서서히 내리거나 멈추게 하는 코루틴.
    private IEnumerator Fade(bool fadeIn)
    {
        var emission = particle.emission;
        float currentRate = emission.rateOverTime.constant;
        float targetRate = fadeIn ? 25f : 0f;

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
