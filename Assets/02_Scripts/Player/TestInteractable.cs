using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public void Interact(InteractionCommandType type)
    {
        Debug.Log($"Interacting with {type}");
    }
}
