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
    private float secondsOfDay = 86400f; // 하루 총 시간 (24시간 기준)
    [SerializeField] private float timeScale = 180f; // 현실 1초 = 게임 3분

    [Header("조명 설정")]
    [SerializeField] private Color dayLightColor;
    [SerializeField] private Color nightLightColor;
    [SerializeField] private AnimationCurve lightCurve;
    [SerializeField] private Light2D globalLight;

    [Header("디버그/테스트용")]
    [SerializeField] private float time = 21600f; // 오전 6시 시작
    [SerializeField] private int days = 0;
    [SerializeField] private TimeOfDay currentTimeOfDay;

    // 시간대 구간 (초 단위)
    private readonly float dawnEnd = 5400f;       // 0.5분
    private readonly float dayEnd = 37800f;       // 3분
    private readonly float eveningEnd = 59400f;   // 2분

    // 현재 시각 계산용
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

    /// 현재 time 값에 따라 TimeOfDay enum 갱신
    private void AutoUpdateTimeOfDay()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        float t = time;

        if (t < dawnEnd)
            currentTimeOfDay = TimeOfDay.Dawn;
        else if (t < dayEnd)
            currentTimeOfDay = TimeOfDay.Day;
        else if (t < eveningEnd)
            currentTimeOfDay = TimeOfDay.Evening;
        else
            currentTimeOfDay = TimeOfDay.Night;
    }

    IEnumerator NextDay()
    {
        days++;
        time = 0;
        Debug.Log($"[TimeManager] 현재 날짜: {days}일차");
        yield break;
    }
}