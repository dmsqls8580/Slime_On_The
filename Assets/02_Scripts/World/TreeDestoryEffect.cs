using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TreeDestroyEffect : MonoBehaviour, IDestroyEffect, IHitReactive
{
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem leafParticle;
    [SerializeField] private GameObject stumpPrefab;
    [SerializeField] private float fallDuration = 1f;

    private bool isFalling = false;

    private void Awake()
    {
        animator ??= GetComponent<Animator>();
    }

    public void OnHit()
    {
        animator?.SetTrigger("shake");

        leafParticle?.Play();
    }

    public void TriggerDestroyEffect(Transform playerTransform)
    {
        if (isFalling) return;
        isFalling = true;

        // 쓰러질 방향 판단
        Vector3 toPlayer = playerTransform.position - transform.position;

        if (toPlayer.x > 0)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1; // X축 반전
            transform.localScale = scale;
        }

        if (stumpPrefab != null)
        {
            Instantiate(stumpPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}