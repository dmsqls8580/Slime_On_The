using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : MonoBehaviour
{
    [SerializeField] private InteractionSelector interactionSelector;
    [SerializeField] private InteractionHandler interactionHandler;

    public void OnFKey(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        interactionHandler.HandleInteraction(interactionSelector.FInteractable, InteractionCommandType.F);
    }

    public void OnSpaceKey(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        interactionHandler.HandleInteraction(interactionSelector.SpaceInteractable, InteractionCommandType.Space);
    }
}
