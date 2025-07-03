using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectPreview : MonoBehaviour
{
    private bool canPlace = false;
    public bool CanPlace => canPlace;

    private Collider2D previewCollider;
    private SpriteRenderer previewSpriteRenderer;

    private Tilemap tilemap;

    private void Awake()
    {
        previewCollider = GetComponent<Collider2D>();
        previewSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdatePreview()
    {
        PreviewPosition();
        CheckCanPlace();
        PreviewColor();
    }

    // 프리뷰가 마우스 따라다니게 하기.
    private void PreviewPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPos);
        Vector3 cellCenterWorld = tilemap.GetCellCenterWorld(cellPosition);

        transform.position = new Vector3(cellCenterWorld.x, cellCenterWorld.y - 0.5f, 0f);
    }

    // 설치 가능한지 불가능한지 판단.
    private void CheckCanPlace()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.DefaultRaycastLayers);

        Collider2D[] results = new Collider2D[5];
        int count = previewCollider.OverlapCollider(filter, results);

        canPlace = count == 0;
    }


    // 판단에 맞춰 색 바꾸기.
    private void PreviewColor()
    {
        if (previewSpriteRenderer != null)
        {
            Color color = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            previewSpriteRenderer.color = color;
        }
    }

    public void Initialize(Tilemap _tilemap)
    {
        tilemap = _tilemap;
    }
}
