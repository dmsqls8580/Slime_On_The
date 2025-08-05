using _02_Scripts.Manager;
using UnityEngine;

public class ChestCollider : MonoBehaviour
{
    private UIManager uiManager;

    private void Awake()
    {
        uiManager = UIManager.Instance;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var uiChest = uiManager.GetUIComponent<UIChest>();
        var chestIndex = GetComponentInParent<ChestObject>().ChestIndex;
        if (uiChest != null && uiChest.IsOpen && uiChest.ChestIndex == chestIndex)
        {
            uiManager.Close<UIChest>();
            if (uiManager.GetUIComponent<UIInventory>().IsOpen)
            {
                uiManager.Toggle<UIInventory>();
            }
        }
    }
}
