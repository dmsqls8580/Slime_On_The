using System.Collections.Generic;
using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    // NPC, Building, 자원들(Mineral, Tree, Flower, Crop) || 통합으로 Resource.
    [SerializeField] private LayerMask interactableMask;

    private HashSet<Collider2D> interactables = new HashSet<Collider2D>();
    public HashSet<Collider2D> Interactables => interactables;

    private bool IsInteractable(Collider2D collision)
    {
        return (interactableMask & (1 << collision.gameObject.layer)) != 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsInteractable(collision) && !interactables.Contains(collision))
        {
            interactables.Add(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (interactables.Contains(collision))
        {
            interactables.Remove(collision);
        }
    }
}
