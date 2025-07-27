using UnityEngine;

public class StormEffect : WeatherEffectBase
{
    private float lightningTimer;

    protected override int MaxLevel => 1;

    // private GameObject lightningPrefab; // OnEnter에서 미리 로드

    protected override void ApplyEffect()
    {
        switch (++currentLevel)
        {
            case 1:
                Debug.Log("번개 효과 시작!");
                // lightningPrefab = Resources.Load<GameObject>("Prefabs/Lightning");
                // AudioManger.Instance.PlaySound("Storm_Start");

                // 첫 번째 번개가 칠 때까지의 시간 설정
                SetNextLightningTimer();
                break;
        }
    }

    protected override void UpdateEffect()
    {
        lightningTimer -= Time.deltaTime;

        if (lightningTimer <= 0)
        {
            StrikeLightning();
            SetNextLightningTimer();
        }
    }

    private void StrikeLightning()
    {
        // 화면 내 랜덤한 위치에 번개 생성
        Debug.Log("번쩍! 번개가 내리칩니다!");
        // Vector2 randomPosition = ...;
        // Object.Instantiate(lightningPrefab, randomPosition, Quaternion.identity);
    }

    private void SetNextLightningTimer()
    {
        // 다음 번개까지 3~8초 사이의 랜덤한 시간 부여
        lightningTimer = Random.Range(3f, 8f);
    }

    protected override void RemoveEffect()
    {
        Debug.Log("번개 효과 종료.");
        // AudioManger.Instance.StopSound("Storm_Loop");
    }
}
