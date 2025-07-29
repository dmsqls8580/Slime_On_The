public class ClearEffect : WeatherEffectBase
{
    protected override int MaxLevel => 1;

    protected override void ApplyEffect()
    {
        Logger.Log("날씨 Clear On");
        currentLevel++;
    }
    protected override void UpdateEffect() { }
    protected override void RemoveEffect() 
    {
        Logger.Log("날씨 Clear Off");
    }
}
