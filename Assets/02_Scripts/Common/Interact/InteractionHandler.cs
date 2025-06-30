using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public void HandleInteraction(Collider2D target, InteractionCommandType type)
    {
        if (target == null) return;

        if (target.TryGetComponent<IInteractable>(out var interactable))
        {
            interactable.Interact(type);
        }
    }
}
