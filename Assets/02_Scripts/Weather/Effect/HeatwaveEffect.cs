using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class HeatwaveEffect : WeatherEffectBase
{
    protected override int MaxLevel => 2;

    private readonly WeatherManager weatherManager;
    private readonly Volume volume;
    private readonly PlayerStatusManager playerStatusManager;

    private Coroutine coroutine = null;
    private bool shouldStop = false;

    private readonly float moveSpeed = 2f;

    private readonly float effectInterval = 5f;
    private float effectTimer = 0f;

    public HeatwaveEffect(WeatherManager _weatherManager, Volume _volume, PlayerStatusManager _playerStatusManager)
    {
        weatherManager = _weatherManager;
        volume = _volume;
        playerStatusManager = _playerStatusManager;
    }

    protected override void ApplyEffect()
    {
        switch (++currentLevel)
        {
            case 1:
                Logger.Log("날씨: 폭염 켜짐");
                StartHeatwave();
                break;
            case 2:
                playerStatusManager.UpdateMoveSpeed = -moveSpeed;
                break;
        }
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
                    playerStatusManager.ConsumeHp(3f);
                    break;
                case 2:
                    playerStatusManager.ConsumeHp(10f);
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
                Logger.Log("날씨: 폭염 꺼짐");
                StopHeatwave();
                break;
        }
    }

    private void StartHeatwave()
    {
        shouldStop = false;

        if (coroutine != null)
            weatherManager.StopCoroutine(coroutine);

        weatherManager.StartCoroutine(FadeInToLoop());
    }

    private void StopHeatwave()
    {
        shouldStop = true;
    }

    private IEnumerator FadeInToLoop()
    {
        float duration = 3f;
        float timer = 0f;

        float start = 0f;
        float target = 0.75f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            volume.weight = Mathf.Lerp(start, target, t);
            yield return null;
        }

        volume.weight = target;

        coroutine = weatherManager.StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        float loopDuration = 1f;
        float timer = 0f;
        bool forward = true;

        float min = 0.75f;
        float max = 1f;

        while (true)
        {
            timer += Time.deltaTime;
            float t = timer / loopDuration;

            float weight = forward
                ? Mathf.Lerp(min, max, t)
                : Mathf.Lerp(max, min, t);

            volume.weight = weight;

            if (shouldStop)
            {
                float fadeOutDuration = 3f;
                float currentWeight = volume.weight;
                float fadeTimer = 0f;

                while (fadeTimer < fadeOutDuration)
                {
                    fadeTimer += Time.deltaTime;
                    float f = fadeTimer / fadeOutDuration;
                    volume.weight = Mathf.Lerp(currentWeight, 0f, f);
                    yield return null;
                }

                volume.weight = 0f;
                coroutine = null;
                yield break;
            }

            if (t >= 1f)
            {
                timer = 0f;
                forward = !forward;
            }

            yield return null;
        }
    }
}
