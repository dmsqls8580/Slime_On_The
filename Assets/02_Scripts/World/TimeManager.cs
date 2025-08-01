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

    [Header("스크립트 참조")]
    [SerializeField] private WeatherManager weatherManager;

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
        TimeOfDay previousTimeOfDay = GetCurrentTimeOfDay();

        if (previousTimeOfDay != currentTimeOfDay)
        {
            currentTimeOfDay = previousTimeOfDay;
            weatherManager.UpdateDayCount();
        }
    }

    private TimeOfDay GetCurrentTimeOfDay()
    {
        float hour = Hours;
        if (hour >= 5f && hour < 9f) return TimeOfDay.Dawn;
        else if (hour >= 9f && hour < 12f) return TimeOfDay.Morning;
        else if (hour >= 12f && hour < 17f) return TimeOfDay.Noon;
        else if (hour >= 17f && hour < 21f) return TimeOfDay.Evening;
        else return TimeOfDay.Night;
    }

    IEnumerator NextDay()
    {
        days++;
        weatherManager.UpdateDayCount();
        time = 0;
        Debug.Log($"[TimeManager] 현재 날짜: {days}일차");
        yield break;
    }
}
