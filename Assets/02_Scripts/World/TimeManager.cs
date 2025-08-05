using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum TimeOfDay
{
    Dawn, Day, Evening, Night
}

public class TimeManager : MonoBehaviour
{
    [Header("시간 설정")]
    [SerializeField] private float secondsOfDay = 480f; // 게임 내 하루는 8분
    [SerializeField] private float timeScale = 1f; // 현실 1초 = 게임 1초 (배속은 필요시 조절)

    [Header("조명 설정")]
    [SerializeField] private Color dayLightColor;
    [SerializeField] private Color nightLightColor;
    [SerializeField] private AnimationCurve lightCurve;
    [SerializeField] private Light2D globalLight;

    [Header("스크립트 참조")]
    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private FogVignette fogVignette;

    [Header("디버그/테스트용")]
    [SerializeField] private float time = 120f; // 6:00 AM 시작 = 120초
    [SerializeField] private int days = 0;
    [SerializeField] private TimeOfDay currentTimeOfDay;

    // 시간 계산 (1시간 = 20초 기준)
    public float Hours => time / 20f;
    public float Minutes => (time % 20f) / (20f / 60f);
    public int Days => days;
    public TimeOfDay CurrentTimeOfDay => currentTimeOfDay;

    private void Update()
    {
        time += Time.deltaTime * timeScale;

        float normalizedTime = time / secondsOfDay; // 0 ~ 1
        float curve = lightCurve.Evaluate(normalizedTime);
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
            OnTimeChanged(currentTimeOfDay);
            fogVignette.ApplyEffect(CurrentTimeOfDay);
            weatherManager.UpdateDayCount();
        }
    }

    private TimeOfDay GetCurrentTimeOfDay()
    {
        float t = time;

        // Night: 21:00 ~ 04:30 (390~480 + 0~60)
        if (t < 60f || t >= 390f)
            return TimeOfDay.Night;

        // Dawn: 04:30 ~ 07:30 (60~120)
        if (t < 120f)
            return TimeOfDay.Dawn;

        // Day: 07:30 ~ 16:30 (120~300)
        if (t < 300f)
            return TimeOfDay.Day;

        // Evening: 16:30 ~ 21:00 (300~390)
        return TimeOfDay.Evening;
    }

    IEnumerator NextDay()
    {
        days++;
        time = 0;
        Debug.Log($"[TimeManager] 현재 날짜: {days}일차");
        yield break;
    }

    public void OnTimeChanged(TimeOfDay _newTime)
    {
        weatherManager.currentTimeOfDay = _newTime;

        if (_newTime == TimeOfDay.Night)
            weatherManager.heatwave.OnNightStarted();
        else if (_newTime == TimeOfDay.Day)
            weatherManager.heatwave.OnNightEnded();
    }
}
