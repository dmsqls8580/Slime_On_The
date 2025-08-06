using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogEffect : WeatherEffectBase
{
    protected override int MaxLevel => 2;
    private readonly float transitionDuration = 4f;

    private readonly WeatherManager weatherManager;
    private readonly Vignette vignette;
    private readonly ColorAdjustments colorAdjustments;

    private Coroutine coroutine;

    public FogEffect(WeatherManager _weatherManager, Volume _volume)
    {
        weatherManager = _weatherManager;

        if (_volume == null || _volume.profile == null)
        {
            Logger.LogError("Volume 또는 Volume Profile이 null입니다.");
            return;
        }

        if (!_volume.profile.TryGet(out vignette))
        {
            Logger.LogError("Vignette를 찾을 수 없습니다.");
        }

        if (!_volume.profile.TryGet(out colorAdjustments))
        {
            Logger.LogError("Color Adjustments를 찾을 수 없습니다.");
        }
    }

    protected override void ApplyEffect()
    {
        float targetIntensity = 0f;
        float targetContrast = 0f;

        switch (++currentLevel)
        {
            case 1:
                targetIntensity = 0.45f;
                targetContrast = 0f;
                break;
            case 2:
                targetIntensity = 0.45f;
                targetContrast = -25f;
                break;
            case 3:
                targetIntensity = 0.45f;
                targetContrast = 0f;
                break;
        }

        if (coroutine != null)
        {
            weatherManager.StopCoroutine(coroutine);
        }
        coroutine = weatherManager.StartCoroutine(Fade(targetIntensity, targetContrast));
    }

    protected override void UpdateEffect() { }

    protected override void RemoveEffect()
    {
        if (coroutine != null)
        {
            weatherManager.StopCoroutine(coroutine);
        }
        Logger.Log("날씨: 안개 꺼짐");
        coroutine = weatherManager.StartCoroutine(Fade(0f, 0f));
    }

    private IEnumerator Fade(float targetIntensity, float targetContrast)
    {
        float startIntensity = vignette.intensity.value;
        float startContrast = colorAdjustments.contrast.value;

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            vignette.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, progress);
            colorAdjustments.contrast.value = Mathf.Lerp(startContrast, targetContrast, progress);

            yield return null;
        }

        // 루프가 끝나면 목표값으로 확실하게 설정.
        vignette.intensity.value = targetIntensity;
        colorAdjustments.contrast.value = targetContrast;
    }
}
