using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CookingPanel : MonoBehaviour
{
    [SerializeField] private List<CookingSlot> cookingSlots;
    [SerializeField] private Button button;

    private bool canCook = false;

    private void Awake()
    {
        button.onClick.AddListener(OnClickCookButton);
    }

    // 3슬롯의 정보를 Manager한테 보내기.(비교 후 어떤 음식을 만들어야 하는지 팥한테 보내고 시간후에 팥에서 갖게 하기.)
    private void OnClickCookButton()
    {
        if (!canCook) { return; }

    }

    public void CanCook()
    {
        foreach (CookingSlot slot in cookingSlots) { if (slot.Item == null) return; }
        canCook = true;
    }
}
