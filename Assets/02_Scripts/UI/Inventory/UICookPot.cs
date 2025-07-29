using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UICookPot : UIBase
{
    [SerializeField] private List<InventorySlot> inputSlots; // 3칸
    [SerializeField] private InventorySlot resultSlot;       // 1칸
    [SerializeField] private AnimationCurve JellyAnimationCurve;

    public void Initialize(int cookPotIndex)
    {
        int inputStart = SlotIndexScheme.GetCookInputStart(cookPotIndex);
        for (int i = 0; i < inputSlots.Count; i++)
        {
            inputSlots[i].Initialize(inputStart + i);
        }

        int resultIndex = SlotIndexScheme.GetCookResultIndex(cookPotIndex);
        resultSlot.Initialize(resultIndex);
    }

    public void Refresh()
    {
        foreach (var slot in inputSlots)
        {
            slot.Refresh();
        }
        resultSlot.Refresh();
    }

    public override void Open()
    {
        base.Open();
        Contents.localScale = Vector3.zero;
        Contents.DOScale(Vector3.one, 0.3f).SetEase(JellyAnimationCurve).SetUpdate(true);
    }

    public override void Close()
    {
        base.Close();
    }
}