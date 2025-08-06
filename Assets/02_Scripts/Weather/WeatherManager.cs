using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class WeatherManager : MonoBehaviour
{
    [Header("이펙트 참조")]
    [SerializeField] private ParticleSystem rainParticle;
    [SerializeField] private ParticleSystem snowParticle;
    [SerializeField] private Volume fogVolume;
    [SerializeField] private Volume heatwaveVolume;
    [SerializeField] private GameObject lightningPrefab;
    [SerializeField] private GameObject lightningMark;

    private int dayCount = 0;
    private int maxDayCount = 4;
    private WeatherType currentWeatherType = WeatherType.Clear;
    private HashSet<WeatherType> currentWeatherTypes = new() { WeatherType.Clear };
    public HeatwaveEffect heatwave = null;
    public RainEffect rain = null;
    public TimeOfDay currentTimeOfDay;

    // 나올 수 있는 이펙트들.
    private readonly List<WeatherType> weatherPatterns = new()
    {
        WeatherType.Clear,
        WeatherType.Heatwave,
        WeatherType.Rain,
        WeatherType.Storm,
        WeatherType.Snow
    };
    // 날씨 이펙트 저장소.
    private Dictionary<WeatherType, IWeatherEffect> weatherEffects;
    // 작동중인 이펙트들.
    private List<IWeatherEffect> activeEffects = new();

    public void UpdateDayCount() => dayCount++;

    private void Awake()
    {
        // 미리 모든 날씨 효과 객체를 생성하여 Dictionary에 저장.
        weatherEffects = new Dictionary<WeatherType, IWeatherEffect>
        {
            { WeatherType.Clear, new ClearEffect() },
            { WeatherType.Heatwave, new HeatwaveEffect(this, heatwaveVolume, PlayerStatusManager.Instance) },
            { WeatherType.Rain, new RainEffect(this, rainParticle, PlayerStatusManager.Instance) },
            { WeatherType.Storm, new StormEffect(PlayerStatusManager.Instance, lightningPrefab, lightningMark) },
            { WeatherType.Snow, new SnowEffect(this, snowParticle, PlayerStatusManager.Instance) },
        };

        // 초기 날씨 설정.
        IWeatherEffect clearEffect = weatherEffects[WeatherType.Clear];
        activeEffects.Add(clearEffect);
        clearEffect.OnEnter();
    }

    private void Start()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    private void Update()
    {
        UpdateWeather();
    }

    private void UpdateWeather()
    {
        // 날씨 변경.
        if (dayCount >= maxDayCount)
        {
            dayCount = 0;
            maxDayCount = Random.Range(3, 5);
            ChangeWeather();
        }

        // 현재 활성화된 모든 효과의 Update를 실행.
        foreach (IWeatherEffect effect in activeEffects)
        { effect.OnUpdate(); }
    }

    private void ChangeWeather()
    {
        // 다음 날씨 상태 결정.
        WeatherType nextWeatherType = GetNextWeatherType();
        HashSet<WeatherType> nextWeatherTypes = new HashSet<WeatherType> { nextWeatherType };
        if (nextWeatherType == WeatherType.Rain && rain.CurrentLevel == 1)
        { nextWeatherTypes.Add(WeatherType.Storm); }

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

        currentWeatherType = nextWeatherType;
        currentWeatherTypes = nextWeatherTypes;
        foreach (var type in currentWeatherTypes)
        {
            IWeatherEffect newEffect = weatherEffects[type];
            newEffect.OnEnter();
            activeEffects.Add(newEffect);
        }
    }

    private WeatherType GetNextWeatherType()
    {
        List<WeatherType> selectableWeathers = new();

        foreach (WeatherType type in weatherPatterns)
        {
            // 현재 날씨가 Rain 또는 Heatwave일 때, 다음 날씨로 Snow는 불가능.
            if ((currentWeatherType == WeatherType.Rain || currentWeatherType == WeatherType.Heatwave) && type == WeatherType.Snow)
            { continue; }
            // 현재 날씨가 Snow일 때, 다음 날씨로 Rain 또는 Heatwave는 불가능.
            if (currentWeatherType == WeatherType.Snow && (type == WeatherType.Rain || type == WeatherType.Heatwave))
            { continue; }
            // Storm은 단독으로 선택되지 않도록 제외.
            if (type == WeatherType.Storm)
            { continue; }
            selectableWeathers.Add(type);
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
