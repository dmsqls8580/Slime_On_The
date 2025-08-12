using _02_Scripts.Manager;
using UnityEngine;

public class CookPotCollider : MonoBehaviour
{
    private UIManager uiManager;

    private void Awake()
    {
        uiManager = UIManager.Instance;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var cookUI = uiManager.GetUIComponent<UICookPot>();
        var cookIndex = GetComponentInParent<CookPotObject>().CookIndex;
        if (cookUI != null && cookUI.IsOpen && cookUI.CookIndex == cookIndex)
        {
            uiManager.Close<UICookPot>();
            if (uiManager.GetUIComponent<UIInventory>().IsOpen)
            {
                uiManager.Toggle<UIInventory>();
            }
        }
    }
}
