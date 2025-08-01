using PlayerStates;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InteractionSelector : MonoBehaviour
{
    [SerializeField] private LayerMask npcMask;
    [SerializeField] private LayerMask buildingMask;
    [SerializeField] private LayerMask resourceMask;

    [Header("스크립트 참조")]
    [SerializeField] private InteractionZone interactionZone;
    [SerializeField] private UIQuickSlot uiQuickSlot;
    [SerializeField] private CraftingStationManager craftingStationManager;

    // F 키 전용.
    private Collider2D fInteractable;
    // Space bar 키 전용.
    private Collider2D spaceInteractable;
    public Collider2D FInteractable => fInteractable;
    public Collider2D SpaceInteractable => spaceInteractable;

    private HashSet<Collider2D> interactables;

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

        // TODO.
        // 방법1: HashSet말고 List로 쓰기(정렬 지원 안함).
        // 방법2: HashSet을 List로 변환 후 정렬하기.
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

                    if (collider.TryGetComponent(out IStationType type))
                    {
                        craftingStationManager.UpdateCurrentCraftingStation(type.GetStationType());
                    }
                    // 빌딩은 "F", "Space bar" 둘 다 할당.
                    break;
                }
            }
            else if (IsInLayerMask(layer, npcMask))
            {
                if (newFInteractable == null)
                {
                    newFInteractable = collider;
                    if (newSpaceInteractable != null)
                    {
                        // 둘 다 찾았으면 종료.
                        break;
                    }
                }
            }
            else if (IsInLayerMask(layer, resourceMask))
            {
                if (newSpaceInteractable == null && IsCompatibleWithCurrentQuickSlotItem(collider))
                {
                    newSpaceInteractable = collider;
                    
                    if (newFInteractable != null)
                    {
                        // 둘 다 찾았으면 종료.
                        break;
                    }
                }
            }
            else
            {
                Logger.Log("Collider의 Layer 오류 입니다.");
            }
        }

        // 변경된 경우에만 할당.
        if (newFInteractable != fInteractable || newSpaceInteractable != spaceInteractable)
        {
            // TODO.
            // UI 갱신.
            fInteractable = newFInteractable;
            spaceInteractable = newSpaceInteractable;
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private bool IsCompatibleWithCurrentQuickSlotItem(Collider2D _collider)
    {

        if (_collider.TryGetComponent(out DestroyableObject resource))
        {
            var selectedSlot = uiQuickSlot?.GetSelectedSlot();
            
            if (selectedSlot.IsUnityNull())
            {
                return false;
            }

            if (!selectedSlot.IsUnityNull())
            {
                PlayerController playerController = GetComponentInParent<PlayerController>();
                ToolType toolType = selectedSlot.GetToolType();
                ToolType requiredToolType = resource.GetRequiredToolType();
                return toolType == requiredToolType;
            }
        }

        return false;
    }
}
