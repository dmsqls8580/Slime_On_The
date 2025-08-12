using System.Collections.Generic;
using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    // NPC, Building, 자원들(Mineral, Tree, Flower, Crop) || 통합으로 Resource.
    [SerializeField] private LayerMask interactableMask;

    [Header("스크립트 참조")]
    [SerializeField] private CraftingStationManager craftingStationManager;

    private HashSet<Collider2D> interactables = new HashSet<Collider2D>();
    public HashSet<Collider2D> Interactables => interactables;

    private bool IsInteractable(Collider2D _collision)
    {
        return (interactableMask & (1 << _collision.gameObject.layer)) != 0;
    }

    private void OnTriggerEnter2D(Collider2D _collision)
    {
        if (IsInteractable(_collision) && !interactables.Contains(_collision))
        {
            interactables.Add(_collision);
            if (_collision.TryGetComponent(out IStationType type))
            {
                craftingStationManager.UpdateCurrentCraftingStation(type.GetStationType());
                InventoryManager.Instance.UpdateCraftingUI();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D _collision)
    {
        if (interactables.Contains(_collision))
        {
            interactables.Remove(_collision);
            if (_collision.TryGetComponent(out IStationType type))
            {
                craftingStationManager.UpdateCurrentCraftingStation(CraftingStation.Normal);
                InventoryManager.Instance.UpdateCraftingUI();
            }
        }
    }
}
