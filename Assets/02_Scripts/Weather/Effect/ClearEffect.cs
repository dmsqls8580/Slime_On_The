public class ClearEffect : WeatherEffectBase
{
    protected override int MaxLevel => 1;
    protected override void ApplyEffect()
    {
        currentLevel++;
        Logger.Log("날씨: 클리어 켜짐");
    }
    protected override void UpdateEffect() { }
    protected override void RemoveEffect()
    {
        Logger.Log("날씨: 클리어 꺼짐");
    }
}
