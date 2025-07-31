using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum TimeOfDay
{
    Dawn, Morning, Noon, Evening, Night
}

public class TimeManager : MonoBehaviour
{
    [Header("시간 설정")]
    private float secondsOfDay = 86400f;
    [SerializeField] private float timeScale = 180f;

    [Header("조명 설정")]
    [SerializeField] private Color dayLightColor;
    [SerializeField] private Color nightLightColor;
    [SerializeField] private AnimationCurve lightCurve;
    [SerializeField] private Light2D globalLight;

    [Header("디버그/테스트용")]
    [SerializeField] private float time = 21600f;
    [SerializeField] private int days = 0;
    [SerializeField] private TimeOfDay currentTimeOfDay;

    public float Hours => time / 3600f;
    public float Minutes => (time % 3600f) / 60f;
    public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;

    private void Update()
    {
        time += Time.deltaTime * timeScale;

        float curve = lightCurve.Evaluate(Hours);
        Color lightColor = Color.Lerp(dayLightColor, nightLightColor, curve);
        globalLight.color = lightColor;

        AutoUpdateTimeOfDay();

        if (time > secondsOfDay)
            StartCoroutine(NextDay());
    }

    private void AutoUpdateTimeOfDay()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        float hour = Hours;
        if (hour >= 5f && hour < 9f) currentTimeOfDay = TimeOfDay.Dawn;
        else if (hour >= 9f && hour < 12f) currentTimeOfDay = TimeOfDay.Morning;
        else if (hour >= 12f && hour < 17f) currentTimeOfDay = TimeOfDay.Noon;
        else if (hour >= 17f && hour < 21f) currentTimeOfDay = TimeOfDay.Evening;
        else currentTimeOfDay = TimeOfDay.Night;
    }

    IEnumerator NextDay()
    {
        days++;
        time = 0;
        Debug.Log($"[TimeManager] 현재 날짜: {days}일차");
        yield break;
    }
}
