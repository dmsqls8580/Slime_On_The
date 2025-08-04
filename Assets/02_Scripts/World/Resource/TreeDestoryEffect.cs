using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TreeDestroyEffect : MonoBehaviour, IDestroyEffect, IHitReactive
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite stumpSprite;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem leafParticle;
    [SerializeField] private float fallDuration = 1f;

    private bool isFalling = false;
    private bool isStumpVisualized = false;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnHit(float damage)
    {
        TryPreviewStump(damage); // 데미지 기반 판별

        animator?.SetTrigger("shake");
        leafParticle?.Play();
    }

    // 마지막 한 대 남았는지 판단 후 스프라이트 교체
    private void TryPreviewStump(float incomingDamage)
    {
        if (isStumpVisualized) return;

        var destroyable = GetComponent<DestroyableObject>();
        if (destroyable != null && destroyable.CurrentHealth <= incomingDamage)
        {
            spriteRenderer.sprite = stumpSprite;
            isStumpVisualized = true;
            Debug.Log("[TreeDestroyEffect] 다음 공격에 파괴 예정 → stumpSprite 전환");
        }
    }

    public void TriggerDestroyEffect(Transform playerTransform)
    {
        if (isFalling) return;
        isFalling = true;

        // Instantiate 안 함 (이미 stump 상태)
        Destroy(gameObject);
    }
}