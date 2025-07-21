using UnityEngine;

public class CookingSlot : MonoBehaviour
{
    [SerializeField] private CookingPanel cookingPanel;

    private ItemSO item;
    public ItemSO Item => item;

    // 정보를 받으면 cookingPanel.CanCook()
}
