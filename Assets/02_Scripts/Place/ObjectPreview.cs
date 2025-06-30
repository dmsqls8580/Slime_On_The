using UnityEngine;

public class ObjectPreview : MonoBehaviour
{
    [SerializeField] private TileManager tileManager;

    private GameObject currentVisual;

    public void SetVisual(GameObject prefab)
    {
        if (currentVisual != null)
            Destroy(currentVisual);

        currentVisual = Instantiate(prefab, transform);
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void SetPosition(Vector3Int gridPos)
    {
        transform.position = tileManager.GetWorldPosition(gridPos);
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    // TODO.
    // ��ġ ����, �Ұ��ɿ� ���� ������ �� �ٲٱ�.
}
