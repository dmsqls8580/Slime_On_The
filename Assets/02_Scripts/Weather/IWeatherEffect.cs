public interface IWeatherEffect
{
    // 날씨가 시작될 때 호출될 함수.
    void OnEnter();
    // 날씨가 지속되는 동안 매 프레임 호출될 함수.
    void OnUpdate();
    // 날씨가 끝날 때 호출될 함수.
    void OnExit();
}
