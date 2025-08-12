using UnityEngine;
using UnityEngine.EventSystems;

public class BackgroundClickHandler : MonoBehaviour, IPointerClickHandler
{
    private HoldManager holdManager;

    private void Awake()
    {
        holdManager = HoldManager.Instance;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (holdManager.IsHolding)
            {
                holdManager.DropHeldItem();
            }
        }
    }
}
