using UnityEngine;

public class WindEffect : WeatherEffectBase
{
    private Vector2 windDirection;
    private float windTimer;
    private bool isBlowing;

    protected override int MaxLevel => 1;

    protected override void ApplyEffect()
    {
        switch (++currentLevel)
        {
            case 1:
                // 사이클 동안 불 바람의 방향.
                windDirection = Random.insideUnitCircle.normalized;
                // Public static 변수나 이벤트 시스템을 통해 바람 방향 전파
                // WindSystem.CurrentWindDirection = windDirection;
                break;
        }
    }

    protected override void UpdateEffect()
    {
        // 바람이 불고/멈추는 것을 랜덤하게 처리
        windTimer -= Time.deltaTime;
        if (windTimer <= 0)
        {
            isBlowing = !isBlowing;
            windTimer = Random.Range(2f, 5f); // 2~5초마다 상태 변경
            // WindSystem.IsBlowing = isBlowing;
            Debug.Log(isBlowing ? "바람이 붑니다!" : "바람이 잠시 멈춥니다.");
        }
    }

    protected override void RemoveEffect()
    {
        switch (currentLevel)
        {
            case 1:
                // 바람 효과 완전히 종료.
                // WindSystem.IsBlowing = false;
                break;
        }
    }
}
