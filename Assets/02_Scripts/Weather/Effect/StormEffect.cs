using System.Collections;
using UnityEngine;

public class StormEffect : WeatherEffectBase
{
    protected override int MaxLevel => 1;

    private float lightningTimer;

    private readonly PlayerStatusManager playerStatusManager;
    private readonly GameObject lightningPrefab;
    private readonly GameObject lightningMark;

    public StormEffect(PlayerStatusManager _playerStatusManager, GameObject _lightningPrefab, GameObject _lightningMark)
    {
        playerStatusManager = _playerStatusManager;
        lightningPrefab = _lightningPrefab;
        lightningMark = _lightningMark;
    }

    protected override void ApplyEffect()
    {
        switch (++currentLevel)
        {
            case 1:
                SoundManager.Instance.PlaySFX(SFX.WeatherLightningStartSound);
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
