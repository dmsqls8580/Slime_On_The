using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionSelector : MonoBehaviour
{
    [SerializeField] private LayerMask npcMask;
    [SerializeField] private LayerMask buildingMask;
    [SerializeField] private LayerMask resourceMask;

    //private Item currentQuickSlotItem;

    private Collider2D fInteractable;     // F Ű��
    private Collider2D spaceInteractable; // Space Ű��
    public Collider2D FInteractable => fInteractable;
    public Collider2D SpaceInteractable => spaceInteractable;

    private InteractionZone interactionZone;
    private HashSet<Collider2D> interactables;

    private void Awake()
    {
        interactionZone = GetComponent<InteractionZone>();
    }

    private void Start()
    {
        interactables = interactionZone.Interactables;
    }

    private void Update()
    {
        Select();
    }

    private void Select()
    {
        if (interactables.Count <= 0)
        {
            fInteractable = null;
            spaceInteractable = null;
            return;
        }

        // TODO
        // ���1: HashSet���� List�� ����(���� ���� ����)
        // ���2: HashSet�� List�� ��ȯ �� �����ϱ�
        var sorted = interactables.OrderBy(collider =>
            Vector2.Distance(transform.position, collider.transform.position));

        Collider2D newFInteractable = null;
        Collider2D newSpaceInteractable = null;

        foreach (Collider2D collider in sorted)
        {
            int layer = collider.gameObject.layer;

            if (IsInLayerMask(layer, buildingMask))
            {
                if (newFInteractable == null && newSpaceInteractable == null)
                {
                    newFInteractable = collider;
                    newSpaceInteractable = collider;
                    break;    // ������ "F", "Space bar" �� �� �Ҵ�
                }
            }
            else if (IsInLayerMask(layer, npcMask))
            {
                if (newFInteractable == null)
                {
                    newFInteractable = collider;
                    if (newSpaceInteractable != null) break;    // �� �� ã������ ����
                }
            }
            else if (IsInLayerMask(layer, resourceMask))
            {
                if (newSpaceInteractable == null && IsCompatibleWithCurrentQuickSlotItem(collider))
                {
                    newSpaceInteractable = collider;
                    if (newFInteractable != null) break;    // �� �� ã������ ����
                }
            }
            else
            {
                Debug.Log("Collider�� Layer ���� �Դϴ�.");
            }
        }

        // ����� ��쿡�� �Ҵ�
        if (newFInteractable != fInteractable || newSpaceInteractable != spaceInteractable)
        {
            // TODO
            // UI ����
            fInteractable = newFInteractable;
            spaceInteractable = newSpaceInteractable;
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private bool IsCompatibleWithCurrentQuickSlotItem(Collider2D collider)
    {
        //if (collider.TryGetComponent(out Resource resource))
        //{
        //    return resource.resourceType == currentQuickSlotItem.interactableResourceType;
        //}

        return false;
    }
}
