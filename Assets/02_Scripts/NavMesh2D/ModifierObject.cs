using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierObject : MonoBehaviour
{
    private void OnEnable()
    {
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
