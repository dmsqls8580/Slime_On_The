using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifierObject : MonoBehaviour
{
    private bool onEnabled = false;
    private void OnEnable()
    {
        if (!onEnabled)
        {
            onEnabled = true;
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
