using UnityEngine;

public class PreviewTile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetValid(bool isValid)
    {
        spriteRenderer.color = isValid ? new Color(0, 1, 0, 0.25f) : new Color(1, 0, 0, 0.25f);
    }
}
