using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour
{
    [Header("날씨 데이터")]
    public List<WeatherDataSO> weatherPatterns;

    // 현재 날씨 효과.
    private IWeatherEffect currentEffect;
    // 모든 날씨 효과들을 저장.
    private Dictionary<WeatherType, IWeatherEffect> weatherEffects;

    private void Awake()
    {
        // 미리 모든 날씨 효과 객체를 생성하여 Dictionary에 저장.
        weatherEffects = new Dictionary<WeatherType, IWeatherEffect>
        {
            { WeatherType.Clear, new ClearEffect() },
            { WeatherType.Rain, new RainEffect() },
            { WeatherType.Heatwave, new HeatwaveEffect() }
        };

        // 초기 날씨 설정.
        currentEffect = weatherEffects[WeatherType.Clear];
    }

    private void Start()
    {
        StartCoroutine(WeatherLoop());
    }

    private void Update()
    {
        currentEffect?.OnUpdate();
    }

    private IEnumerator WeatherLoop()
    {
        while (true)
        {
            // 다음 날씨를 랜덤하게 선택.
            WeatherDataSO nextWeather = GetRandomWeather();
            float duration = Random.Range(nextWeather.durationMin, nextWeather.durationMax);

            // 현재 날씨 효과 종료.
            currentEffect?.OnExit();

            // 새로운 날씨 효과로 교체하고 시작.
            currentEffect = weatherEffects[nextWeather.type];
            currentEffect.OnEnter();

            // duration 동안 대기.
            yield return new WaitForSeconds(duration);
        }
    }

    private WeatherDataSO GetRandomWeather()
    {
        int index = Random.Range(0, weatherPatterns.Count);
        return weatherPatterns[index];
    }
}
