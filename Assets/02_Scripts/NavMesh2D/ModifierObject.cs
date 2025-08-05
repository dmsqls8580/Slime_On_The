using UnityEngine;

public class ModifierObject : MonoBehaviour
{
    private void OnEnable()
    {
        if (GameManager.Instance.IsLoading)
        {
            return;
        }
        if (NavMesh2DManager.Instance != null)
        {
            NavMesh2DManager.Instance.UpdateThisNavMesh();
        }
    }

    private void OnDisable()
    {
        if (NavMesh2DManager.Instance != null)
        {
            NavMesh2DManager.Instance.UpdateThisNavMesh();
        }
    }

}
