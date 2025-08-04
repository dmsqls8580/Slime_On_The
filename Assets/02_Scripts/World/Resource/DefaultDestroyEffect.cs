using UnityEngine;

public class DefaultDestroyEffect : MonoBehaviour, IDestroyEffect
{
    public void TriggerDestroyEffect(Transform playerTransform)
    {
        Destroy(gameObject);
    }
}
