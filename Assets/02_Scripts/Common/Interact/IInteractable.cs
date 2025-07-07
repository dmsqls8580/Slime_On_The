using PlayerStates;

public interface IInteractable
{
    public void Interact(InteractionCommandType _type, PlayerController _playerController);
}
