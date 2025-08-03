using _02_Scripts.Manager;
using UnityEngine;

public class CookPotCollider : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var cookUI = UIManager.Instance.GetUIComponent<UICookPot>();
        var cookIndex = GetComponentInParent<CookPotObject>().CookIndex;
        if (cookUI != null && cookUI.IsOpen && cookUI.CookIndex == cookIndex)
        {
            UIManager.Instance.Close<UICookPot>();
            if (UIManager.Instance.GetUIComponent<UIInventory>().IsOpen)
            {
                UIManager.Instance.Toggle<UIInventory>();
            }
        }
    }
}
