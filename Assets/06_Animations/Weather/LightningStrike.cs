using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    private readonly float lifeTime = 2f;
    private readonly float lightningDamage = 20f;

    private bool canDealDamage = false;
    private bool hasDealtDamage = false;

    private void Start()
    {
        SoundManager.Instance.PlaySFX(SFX.WeatherStormSound);
        Destroy(gameObject, lifeTime);
    }

    private void EnableDamageWindow()
    {
        canDealDamage = true;
    }

    private void DisableDamageWindow()
    {
        canDealDamage = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (canDealDamage && !hasDealtDamage &&
            other.TryGetComponent(out PlayerStatusManager playerStatusManager))
        {
            Logger.Log("날씨: 전기맞음");
            playerStatusManager.TakeDamage(lightningDamage);
            hasDealtDamage = true;
            return;
        }
    }
}
