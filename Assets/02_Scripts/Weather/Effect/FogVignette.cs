using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogVignette : MonoBehaviour
{
    [SerializeField] private Volume fogVolume;

    private readonly float transitionDuration = 4f;

    private Vignette vignette;
    private ColorAdjustments colorAdjustments;

    private Coroutine coroutine;

    private void Awake()
    {
        if (fogVolume == null || fogVolume.profile == null)
        {
            Logger.LogError("Volume 또는 Volume Profile이 null입니다.");
            return;
        }

        if (!fogVolume.profile.TryGet(out vignette))
        {
            Logger.LogError("Vignette를 찾을 수 없습니다.");
        }

        if (!fogVolume.profile.TryGet(out colorAdjustments))
        {
            Logger.LogError("Color Adjustments를 찾을 수 없습니다.");
        }
    }

    public void ApplyEffect(TimeOfDay _timeOfDay)
    {
        float targetIntensity = 0f;
        float targetContrast = 0f;

        switch (_timeOfDay)
        {
            case TimeOfDay.Night:
                targetIntensity = 0.45f;
                targetContrast = 0f;
                break;
            case TimeOfDay.Dawn:
                targetIntensity = 0.45f;
                targetContrast = -25f;
                break;
            case TimeOfDay.Day:
                targetIntensity = 0f;
                targetContrast = 0f;
                break;
            default:
                return;
        }

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(Fade(targetIntensity, targetContrast, _timeOfDay));
    }

    private IEnumerator Fade(float targetIntensity, float targetContrast, TimeOfDay _timeOfDay)
    {
        if (_timeOfDay == TimeOfDay.Night)
        {
            fogVolume.weight = 1f;
        }

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
        if (_timeOfDay == TimeOfDay.Day)
        {
            fogVolume.weight = 0f;
        }
    }
}
