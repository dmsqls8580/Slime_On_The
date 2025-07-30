using System.Collections;
using UnityEngine;

public class StormEffect : WeatherEffectBase
{
    protected override int MaxLevel => 1;

    private float lightningTimer;

    private GameObject lightningPrefab;
    private GameObject lightningMark;
    private readonly PlayerStatusManager playerStatusManager;

    public StormEffect(GameObject _lightningPrefab, GameObject _lightningMark, PlayerStatusManager _playerStatusManager)
    {
        lightningPrefab = _lightningPrefab;
        lightningMark = _lightningMark;
        playerStatusManager = _playerStatusManager;
    }

    protected override void ApplyEffect()
    {
        switch (++currentLevel)
        {
            case 1:
                Logger.Log("날씨: 스톰켜짐");
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
        Vector2 playerPos = playerStatusManager.transform.position;
        float radius = 5f;
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        Vector2 spawnPosition = playerPos + randomOffset;

        Object.Instantiate(lightningMark, spawnPosition, Quaternion.identity);

        playerStatusManager.StartCoroutine(SpawnLightningAfterDelay(spawnPosition, 0.9f));
    }

    private IEnumerator SpawnLightningAfterDelay(Vector2 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        Object.Instantiate(lightningPrefab, position, Quaternion.identity);
    }

    private void SetNextLightningTimer()
    {
        lightningTimer = Random.Range(3f, 8f);
    }

    protected override void RemoveEffect()
    {
        Logger.Log("날씨: 번개 꺼짐");
    }
}
