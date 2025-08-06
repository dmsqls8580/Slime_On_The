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

    private readonly float moveSpeed = 1f;

    private readonly float effectInterval = 5f;
    private float effectTimer = 0f;

    private bool isNight = false;
    private bool wasSpeedModified = false;

    public HeatwaveEffect(WeatherManager _weatherManager, Volume _volume, PlayerStatusManager _playerStatusManager)
    {
        weatherManager = _weatherManager;
        volume = _volume;
        playerStatusManager = _playerStatusManager;

        weatherManager.heatwave = this;
    }

    protected override void ApplyEffect()
    {
        switch (++currentLevel)
        {
            case 1:
                StartHeatwave();
                break;
            case 2:
                // 밤이 아닐 때만 이동속도 감소.
                if (!isNight)
                {
                    playerStatusManager.UpdateMoveSpeed = -moveSpeed;
                    wasSpeedModified = true;
                }
                break;
        }
    }

    protected override void UpdateEffect()
    {
        if (weatherManager.currentTimeOfDay == TimeOfDay.Night ||
            weatherManager.currentTimeOfDay == TimeOfDay.Dawn) return;

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
                // 이동속도를 감소시켰던 경우에만 복구
                if (wasSpeedModified)
                {
                    playerStatusManager.UpdateMoveSpeed = moveSpeed;
                    wasSpeedModified = false;
                }
                goto case 1;
            case 1:
                StopHeatwave();
                break;
        }
    }

    private void StartHeatwave()
    {
        shouldStop = false;

        if (coroutine != null)
            weatherManager.StopCoroutine(coroutine);

        weatherManager.StartCoroutine(Fade());
    }

    private void StopHeatwave()
    {
        shouldStop = true;
    }

    private IEnumerator Fade()
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

    public void OnNightStarted()
    {
        isNight = true;

        // 현재 폭염 중이면 이동속도를 원래대로 돌림
        if (currentLevel >= 2 && wasSpeedModified)
        {
            playerStatusManager.UpdateMoveSpeed = moveSpeed;
            wasSpeedModified = false;
        }
    }

    public void OnNightEnded()
    {
        isNight = false;

        // 폭염 중이면 다시 이동속도를 줄여야 함
        if (currentLevel >= 2)
        {
            playerStatusManager.UpdateMoveSpeed = -moveSpeed;
            wasSpeedModified = true;
        }
    }
}
