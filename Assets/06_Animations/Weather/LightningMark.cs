using System.Collections;
using UnityEngine;

public class LightningMark : MonoBehaviour
{
    private float lifeTime = 1f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < lifeTime)
        {
            float alpha = Mathf.Lerp(0.5f, 0f, elapsed / lifeTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
