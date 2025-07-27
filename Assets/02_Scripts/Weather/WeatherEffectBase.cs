public abstract class WeatherEffectBase : IWeatherEffect
{
    protected int currentLevel = 0;
    protected abstract int MaxLevel { get; }

    public void OnEnter()
    {
        if (currentLevel < MaxLevel)
        {
            ApplyEffect();
        }
    }

    public void OnUpdate()
    {
        if (currentLevel > 0)
        {
            UpdateEffect();
        }
    }

    public void OnExit()
    {
        if (currentLevel > 0)
        {
            RemoveEffect();
            currentLevel = 0;
        }
    }

    protected abstract void ApplyEffect();
    protected abstract void UpdateEffect();
    protected abstract void RemoveEffect();
}
