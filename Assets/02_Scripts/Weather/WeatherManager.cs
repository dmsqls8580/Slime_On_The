using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class WeatherManager : MonoBehaviour
{
    [Header("날씨 데이터")]
    [SerializeField] private List<WeatherDataSO> weatherPatterns;

    [Header("파티클 데이터")]
    [SerializeField] private ParticleSystem rainParticle;
    [SerializeField] private ParticleSystem snowParticle;

    [Header("볼륨 데이터")]
    [SerializeField] private Volume fogVolume;
    [SerializeField] private Volume HeatwaveVolume;

    // 모든 날씨 효과들의 저장소.
    private Dictionary<WeatherType, IWeatherEffect> weatherEffects;
    // 작동중인 이펙트들.
    private List<IWeatherEffect> activeEffects = new List<IWeatherEffect>();
    private WeatherType currentWeatherType = WeatherType.Clear;
    private HashSet<WeatherType> currentWeatherTypes = new HashSet<WeatherType>() { WeatherType.Clear };

    private void Awake()
    {
        // 미리 모든 날씨 효과 객체를 생성하여 Dictionary에 저장.
        weatherEffects = new Dictionary<WeatherType, IWeatherEffect>
        {
            { WeatherType.Clear, new ClearEffect() },
            { WeatherType.Fog, new FogEffect(this, fogVolume) },
            { WeatherType.Heatwave, new HeatwaveEffect(this, HeatwaveVolume) },
            { WeatherType.Rain, new RainEffect(this, rainParticle) },
            { WeatherType.Snow, new SnowEffect(this, snowParticle) },
            //{ WeatherType.Storm, new StormEffect() },
            //{ WeatherType.Wind, new WindEffect() }
        };

        // 초기 날씨 설정.
        IWeatherEffect clearEffect = weatherEffects[WeatherType.Clear];
        clearEffect.OnEnter();
        activeEffects.Add(clearEffect);
    }

    private void Start()
    {
        StartCoroutine(WeatherLoop());
    }

    private void Update()
    {
        // 현재 활성화된 모든 효과의 Update를 실행.
        foreach (IWeatherEffect effect in activeEffects)
        { effect.OnUpdate(); }
    }

    private IEnumerator WeatherLoop()
    {
        while (true)
        {
            // 다음 날씨 상태 결정.
            WeatherDataSO nextWeatherSO = GetNextWeather();
            HashSet<WeatherType> nextWeatherTypes = new HashSet<WeatherType> { nextWeatherSO.type };
            //if (nextWeatherSO.type == WeatherType.Rain && Random.Range(0, 4) == 0)
            //{ nextWeatherTypes.Add(WeatherType.Storm); }

            HashSet<WeatherType> typesToRemove = new HashSet<WeatherType>(currentWeatherTypes);
            // 현재 상태 - 다음 상태 = 제거할 것들.
            typesToRemove.ExceptWith(nextWeatherTypes);

            // 제거.
            if (typesToRemove.Count > 0)
            {
                // 제거해야 할 효과들의 OnExit() 호출 및 리스트에서 제거.
                List<IWeatherEffect> effectsToRemove = activeEffects
                    .Where(effect => typesToRemove.Contains(GetEffectType(effect))).ToList();

                foreach (IWeatherEffect effect in effectsToRemove)
                {
                    effect.OnExit();
                    activeEffects.Remove(effect);
                }
            }
            activeEffects.Clear();

            currentWeatherType = nextWeatherSO.type;
            currentWeatherTypes = nextWeatherTypes;
            foreach (var type in currentWeatherTypes)
            {
                IWeatherEffect newEffect = weatherEffects[type];
                newEffect.OnEnter();
                activeEffects.Add(newEffect);
            }

            float duration = Random.Range(nextWeatherSO.durationMin, nextWeatherSO.durationMax);
            yield return new WaitForSeconds(duration);
        }
    }

    private WeatherDataSO GetNextWeather()
    {
        List<WeatherDataSO> selectableWeathers = new List<WeatherDataSO>();

        foreach (var weatherSO in weatherPatterns)
        {
            // 현재 날씨가 Rain 또는 Heatwave일 때, 다음 날씨로 Snow는 불가능.
            if ((currentWeatherType == WeatherType.Rain || currentWeatherType == WeatherType.Heatwave) && weatherSO.type == WeatherType.Snow)
            { continue; }
            // 현재 날씨가 Snow일 때, 다음 날씨로 Rain 또는 Heatwave는 불가능.
            if (currentWeatherType == WeatherType.Snow && (weatherSO.type == WeatherType.Rain || weatherSO.type == WeatherType.Heatwave))
            { continue; }
            // Storm은 단독으로 선택되지 않도록 제외.
            if (weatherSO.type == WeatherType.Storm)
            { continue; }
            selectableWeathers.Add(weatherSO);
        }

        // 가능한 날씨 목록에서 랜덤 선택.
        int index = Random.Range(0, selectableWeathers.Count);
        return selectableWeathers[index];
    }

    // 인스턴스로부터 WeatherType을 역으로 찾기 위한 헬퍼 함수.
    private WeatherType GetEffectType(IWeatherEffect effect)
    {
        // Dictionary를 순회하며 해당 인스턴스와 일치하는 키(WeatherType)를 반환.
        return weatherEffects.FirstOrDefault(x => x.Value == effect).Key;
    }
}
