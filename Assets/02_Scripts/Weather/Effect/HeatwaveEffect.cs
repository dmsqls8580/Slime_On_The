using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class HeatwaveEffect : WeatherEffectBase
{
    protected override int MaxLevel => 2;

    private readonly WeatherManager weatherManager;
    private readonly Volume volume;

    private Coroutine coroutine = null;

    public HeatwaveEffect(WeatherManager _weatherManager, Volume _volume)
    {
        weatherManager = _weatherManager;
        volume = _volume;
    }

    protected override void ApplyEffect()
    {
        switch (++currentLevel)
        {
            case 1:
                Logger.Log("날씨: 폭염 켜짐");
                if (coroutine == null)
                {
                    coroutine = weatherManager.StartCoroutine(Loop());
                }
                break;
            case 2:
                // 플레이어 이동속도 감소.
                break;
        }
    }

    protected override void UpdateEffect()
    {
        // 서서히 체력 감소.
    }

    protected override void RemoveEffect()
    {
        switch (currentLevel)
        {
            case 2:
                // 플레이어 이동속도 되돌리기.
                goto case 1;
            case 1:
                Logger.Log("날씨: 폭염 꺼짐");
                if (coroutine != null)
                {
                    weatherManager.StopCoroutine(coroutine);
                    coroutine = null;
                }
                break;
        }
    }

    private IEnumerator Loop()
    {
        float duration = 2f;
        float timer = 0f;
        bool forward = true;

        while (true)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float weight = forward
                ? Mathf.Lerp(0f, 1f, t)
                : Mathf.Lerp(1f, 0f, t);

            volume.weight = weight;

            if (t >= 1f)
            {
                timer = 0f;
                forward = !forward;
            }

            yield return null;
        }
    }
}
