using PlayerStates;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public void HandleInteraction(Collider2D _target, InteractionCommandType _type, PlayerController _playerController = null)
    {
        if (_target == null) return;

        if (_target.TryGetComponent<IInteractable>(out var interactable))
            interactable.Interact(_type, _playerController);
    }
}
